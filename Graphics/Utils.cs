using System.Runtime.InteropServices;

namespace Nebula.Graphics;

public static class Utils {
    public static IntPtr MarshalAndBox<T>(T structure) {
        IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(structure));
        Marshal.StructureToPtr(structure, ptr, false);
        return ptr;
    }
}