using System.Runtime.InteropServices;

namespace ItemDisplayPlacementHelper.Native.Structs
{
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct NativeAssetBundle
    {
        [FieldOffset(0x68)]
        public Multimap<StringStorageDefaultV2, AssetInfo> pathContainer;
        [FieldOffset(0xd8)]
        public ArchiveStorageReader* archiveStorage;
        [FieldOffset(0x110)]
        public char* assetBundleName;
    }
}
