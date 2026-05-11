using System.Threading;

namespace Hellion.Core.Network.Helpers
{
    public static class Helper
    {
        private static int _uniqueId;

        public static int GenerateUniqueId() => Interlocked.Increment(ref _uniqueId);
    }
}
