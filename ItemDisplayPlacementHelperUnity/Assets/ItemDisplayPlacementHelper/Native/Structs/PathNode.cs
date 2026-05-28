using System.Runtime.InteropServices;

namespace ItemDisplayPlacementHelper.Native.Structs
{
    [StructLayout(LayoutKind.Explicit)]
    public struct PathNode
    {
        [FieldOffset(0x18)]
        public StringStorageDefaultV2 path;
    }
}
