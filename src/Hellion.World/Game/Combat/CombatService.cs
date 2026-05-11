using System;
using Hellion.Core.Data.Headers;
using Hellion.Core.IO;
using Hellion.Core.Network;
using Hellion.Core.Structures;
using Hellion.World.Game.Resources;
using Hellion.World.Structures;

namespace Hellion.World.Game.Combat
{
    /// <summary>
    /// Basic combat math + lifecycle. Computes damage with the
    /// <c>(attackerDamage - targetDefense) * uniform(0.9, 1.1)</c> formula
    /// from the roadmap, applies it, and emits a damage packet to the
    /// attacker's chunk neighbourhood.
    /// </summary>
    public class CombatService
    {
        private readonly IGameResources _resources;
        private readonly Random _random = new();

        public CombatService(IGameResources resources)
        {
            this._resources = resources;
        }

        public int CalculateDamage(Mover attacker, Mover target)
        {
            int baseDamage = attacker.AttackDamage - target.Defense;
            if (baseDamage < 1) baseDamage = 1;
            double multiplier = 0.9 + this._random.NextDouble() * 0.2;
            return (int)Math.Round(baseDamage * multiplier);
        }

        public void ProcessAttack(WorldClient attackerClient, Mover target)
        {
            if (attackerClient.Player == null || target.IsDead) return;

            int damage = this.CalculateDamage(attackerClient.Player, target);
            target.Hp = Math.Max(0, target.Hp - damage);

            this.BroadcastDamage(attackerClient, target, damage);

            if (target.Hp == 0)
            {
                target.IsDead = true;
                this.OnDeath(attackerClient, target);
            }
        }

        public void OnDeath(WorldClient killerClient, Mover dead)
        {
            switch (dead)
            {
                case Monster mob:
                    this.HandleMonsterDeath(killerClient, mob);
                    break;
                case Player player:
                    this.HandlePlayerDeath(killerClient, player);
                    break;
            }
        }

        private void HandleMonsterDeath(WorldClient killerClient, Monster mob)
        {
            if (killerClient.Player == null) return;

            int xp = 0;
            if (this._resources.Mobs.TryGetValue(mob.ModelId, out MobData? data) && data != null)
            {
                xp = data.ExpReward;
                // Drops will be rolled here once Phase 5.5 inventory + currency are wired in.
            }

            killerClient.Player.Experience += xp;
            Log.Info("{0} killed monster {1} (+{2} xp).", killerClient.Player.Name, mob.ModelId, xp);
        }

        private void HandlePlayerDeath(WorldClient killerClient, Player player)
        {
            WorldPosition? spawn = player.SpawnPoint ?? new WorldPosition(player.MapId, player.Position.X, player.Position.Y, player.Position.Z);

            player.Position = spawn.ToVector3();
            player.MapId = spawn.MapId;
            player.Angle = spawn.Angle;
            player.Hp = Math.Max(1, player.MaxHp / 2);
            player.IsDead = false;

            Log.Info("Player '{0}' died and respawned at map {1} ({2:F1}, {3:F1}).",
                player.Name, spawn.MapId, spawn.X, spawn.Z);
        }

        private void BroadcastDamage(WorldClient attackerClient, Mover target, int damage)
        {
            if (attackerClient.Player == null) return;

            using var packet = new FFPacket();
            packet.StartNewMergedPacket(attackerClient.Player.ObjectId, WorldHeaders.Outgoing.ShowDamage);
            packet.Write(attackerClient.Player.ObjectId);
            packet.Write(target.ObjectId);
            packet.Write(damage);
            packet.Write(target.Hp);

            var worldMap = attackerClient.Server.Maps.Get(attackerClient.Player.MapId);
            foreach (var observer in worldMap.Neighbours(attackerClient.Player.Position))
            {
                observer.Send(packet);
            }
        }
    }
}
