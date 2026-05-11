using System;
using Hellion.Core;
using Hellion.Core.IO;

namespace Hellion.World
{
    public partial class WorldServer
    {
        /// <summary>
        /// Load the world server data (game resources: mobs, items, maps).
        /// Missing files are tolerated so the server can boot without the
        /// proprietary FlyFF client resources.
        /// </summary>
        private void LoadData()
        {
            DateTime start = DateTime.Now;
            Log.Info("Loading world data from '{0}'...", Global.DataPath);

            try
            {
                this.Resources.LoadAll(Global.DataPath);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to load world data: {0}", ex.Message);
            }

            Log.Done("World data loaded in {0:F2}s.", (DateTime.Now - start).TotalSeconds);
        }
    }
}
