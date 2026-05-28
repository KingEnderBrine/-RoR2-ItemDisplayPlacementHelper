using System.Runtime.InteropServices;

namespace ItemDisplayPlacementHelper.Native.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DirectoryInfo
    {
        public Vector<PathNode> vector;
    }
}
