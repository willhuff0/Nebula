using AustinHarris.JsonRpc;
using Silk.NET.GLFW;

namespace Nebula.Interop.Services;

public unsafe class WindowingService : JsonRpcService {
    public static Glfw glfw = GlfwProvider.GLFW.Value;

    public static Dictionary<long, (int, int)> windowSizes = new Dictionary<long, (int, int)>();

    public static (int, int) GetWindowSize(long window) {
        glfw.GetWindowSize((WindowHandle*)new IntPtr(window).ToPointer(), out int width, out int height);
        windowSizes[window] = (width, height);
        return (width, height);
    }

    [JsonRpcMethod] public bool init() => glfw.Init();
    [JsonRpcMethod] public void windowHint(int hint, int value) => glfw.WindowHint((WindowHintInt)hint, value);
    [JsonRpcMethod] public long createWindow(int width, int height, string name) { 
        var handle = new IntPtr(glfw.CreateWindow(width, height, name, null, null)).ToInt64();
        windowSizes[handle] = (width, height);
        return handle;
    }
    [JsonRpcMethod] public bool windowShoudClose(long window) => glfw.WindowShouldClose((WindowHandle*)new IntPtr(window).ToPointer());
    [JsonRpcMethod] public void destroyWindow(long window) => glfw.DestroyWindow((WindowHandle*)new IntPtr(window).ToPointer());
    [JsonRpcMethod] public void terminate() => glfw.Terminate();
}