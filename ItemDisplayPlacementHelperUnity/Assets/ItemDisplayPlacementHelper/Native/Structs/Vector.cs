using System.Runtime.InteropServices;

namespace ItemDisplayPlacementHelper.Native.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct Vector<T> where T : unmanaged
    {
        public T* first;
        public T* last;
        public T* end;
    }
}
