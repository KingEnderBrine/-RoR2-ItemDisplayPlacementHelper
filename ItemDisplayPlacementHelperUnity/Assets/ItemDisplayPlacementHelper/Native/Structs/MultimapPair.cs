using System.Runtime.InteropServices;

namespace ItemDisplayPlacementHelper.Native.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MultimapPair<TKey, TValue>
    {
        public TKey first;
        public TValue second;
    }
}
