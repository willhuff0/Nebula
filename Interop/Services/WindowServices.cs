using AustinHarris.JsonRpc;
using Silk.NET.GLFW;

namespace Nebula.Interop.Services;

public unsafe class WindowingService : JsonRpcService {
    public static Glfw glfw = GlfwProvider.GLFW.Value;

    [JsonRpcMethod] private bool glfwInit() => glfw.Init();
    [JsonRpcMethod] private void glfwWindowHint(int hint, int value) => glfw.WindowHint((WindowHintInt)hint, value);
    [JsonRpcMethod] private int glfwCreateWindow(int width, int height, string name) => new IntPtr(glfw.CreateWindow(800, 600, name, null, null)).ToInt32();
    [JsonRpcMethod] private void glfwTerminate() => glfw.Terminate();
}