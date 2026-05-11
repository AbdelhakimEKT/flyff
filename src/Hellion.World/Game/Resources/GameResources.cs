using System.Collections.Generic;
using System.IO;
using Hellion.Core.IO;
using Newtonsoft.Json;

namespace Hellion.World.Game.Resources
{
    /// <summary>
    /// Default in-memory implementation of <see cref="IGameResources"/>.
    /// Loads catalogs from JSON files under <c>data/</c> at startup. Missing
    /// files are logged but never throw — the server boots with empty catalogs
    /// so smoke tests work without the proprietary FlyFF client resources.
    /// </summary>
    public class GameResources : IGameResources
    {
        private readonly Dictionary<int, MobData> _mobs = new();
        private readonly Dictionary<int, ItemData> _items = new();
        private readonly Dictionary<int, MapData> _maps = new();

        public IReadOnlyDictionary<int, MobData> Mobs => this._mobs;
        public IReadOnlyDictionary<int, ItemData> Items => this._items;
        public IReadOnlyDictionary<int, MapData> Maps => this._maps;

        public void LoadAll(string dataRoot)
        {
            LoadInto(_mobs, Path.Combine(dataRoot, "mobs.json"), m => m.Id, "mobs");
            LoadInto(_items, Path.Combine(dataRoot, "items.json"), i => i.Id, "items");
            LoadInto(_maps, Path.Combine(dataRoot, "maps.json"), m => m.Id, "maps");

            Log.Done("Game resources loaded: {0} mobs, {1} items, {2} maps.",
                this._mobs.Count, this._items.Count, this._maps.Count);
        }

        private static void LoadInto<T>(Dictionary<int, T> target, string path, System.Func<T, int> keySelector, string label)
        {
            if (!File.Exists(path))
            {
                Log.Warning("Resource file '{0}' not found; '{1}' catalog will be empty.", path, label);
                return;
            }

            try
            {
                List<T>? entries = JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(path));
                if (entries == null) return;
                foreach (T entry in entries)
                {
                    target[keySelector(entry)] = entry;
                }
            }
            catch (System.Exception ex)
            {
                Log.Error("Failed to parse '{0}': {1}", path, ex.Message);
            }
        }
    }
}
