using System;
using Hellion.Core.Data;
using Hellion.Core.Structures;

namespace Hellion.World.Structures
{
    public abstract class Mover : WorldObject
    {
        public bool IsDead { get; set; }

        public bool IsFlying { get; set; }

        public bool IsFighting { get; set; }

        public bool IsFollowing { get; set; }

        public bool IsReseting { get; set; }

        /// <summary>Units of world space the mover can travel per second.</summary>
        public float Speed { get; set; }

        public int Level { get; }

        public float Angle { get; set; }

        public int MaxHp { get; set; } = 1;

        public int AttackDamage { get; set; } = 1;

        public int Defense { get; set; }

        /// <summary>Set by the move handler each time the mover's position is updated.</summary>
        public DateTime LastMoveAt { get; set; } = DateTime.UtcNow;

        public WorldPosition? SpawnPoint { get; set; }

        public int Hp { get; set; } = 1;

        public override WorldObjectType Type
        {
            get { return WorldObjectType.Mover; }
        }

        public Mover(int modelId)
            : base(modelId)
        {
            this.Speed = 12.0f;
            this.Level = 1;
        }
    }
}
