using System.Runtime.InteropServices;

namespace ItemDisplayPlacementHelper.Native.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct AssetInfo
    {
        public int preloadIndex;
        public int preloadSize;
        public int instanceId;
    }
}
