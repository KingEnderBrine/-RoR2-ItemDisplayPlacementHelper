using System.Runtime.InteropServices;

namespace ItemDisplayPlacementHelper.Native.Structs
{
#warning return sequential?
    [StructLayout(LayoutKind.Explicit, Pack = 8)]
    public struct StringStorageDefaultV2
    {
        [FieldOffset(0x0)]
        public StringStorageDefaultV2Union union;
        [FieldOffset(0x20)]
        public StringRepresentation data_repr;
        [FieldOffset(0x24)]
        public int label;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 8)]
    public struct StringStorageDefaultV2Union
    {
        [FieldOffset(0)]
        public StackAllocatedRepresentationV2 embedded;
        [FieldOffset(0)]
        public HeapAllocatedRepresentationV2 heap;
    }

    public struct StackAllocatedRepresentationV2
    {
        public unsafe fixed byte data[25];
    }

    public struct HeapAllocatedRepresentationV2
    {
        public nint data;
        public ulong capacity;
        public ulong size;
    }

    public enum StringRepresentation : byte
    {
        Heap,
        Embedded,
        External
    }
}
