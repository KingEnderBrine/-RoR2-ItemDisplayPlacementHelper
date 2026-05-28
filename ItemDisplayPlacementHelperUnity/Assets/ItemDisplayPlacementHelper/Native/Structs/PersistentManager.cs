using System;
using System.Runtime.InteropServices;

namespace ItemDisplayPlacementHelper.Native.Structs
{
    [StructLayout(LayoutKind.Explicit)]
    public struct PersistentManager
    {
        [FieldOffset(0x58)]
        public IntPtr Remapper;
    }
}
