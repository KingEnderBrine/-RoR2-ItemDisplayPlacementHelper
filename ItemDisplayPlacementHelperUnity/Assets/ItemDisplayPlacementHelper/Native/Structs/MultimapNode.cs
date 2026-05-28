using System.Runtime.InteropServices;

namespace ItemDisplayPlacementHelper.Native.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct MultimapNode<TKey, TValue> where TKey : unmanaged where TValue : unmanaged
    {
        public MultimapNode<TKey, TValue>* left;
        public MultimapNode<TKey, TValue>* parent;
        public MultimapNode<TKey, TValue>* right;
        public byte color;
        public bool isnil;
        public fixed byte offset[6];
        public MultimapPair<TKey, TValue> value;
    }
}
