using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ItemDisplayPlacementHelper.Native.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct Multimap<TKey, TValue> : IEnumerable<MultimapNode<TKey, TValue>> where TKey : unmanaged where TValue : unmanaged
    {
        public MultimapNode<TKey, TValue>* root;
        public int count;

        public IEnumerator<MultimapNode<TKey, TValue>> GetEnumerator()
        {
            return new MultimapEnumerator<TKey, TValue>(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
