using System.Runtime.InteropServices;

namespace ItemDisplayPlacementHelper.Native.Structs
{
    [StructLayout(LayoutKind.Explicit)]
    public struct ArchiveStorageReader
    {
        [FieldOffset(0xd0)]
        public DirectoryInfo directoryInfo;
    }
}
