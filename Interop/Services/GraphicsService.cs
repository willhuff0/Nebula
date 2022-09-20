using System.Diagnostics;
using System.Runtime.InteropServices;
using AustinHarris.JsonRpc;
using Silk.NET.GLFW;
using WGPU.NET;

namespace Nebula.Interop.Services;

public unsafe class GraphicsService : JsonRpcService {
    Instance instance = new Instance();
    Surface surface;
    Adapter adapter;
    Device device;

    [JsonRpcMethod] private void CreateSurfaceAdapterDevice(int window) {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            var nativeWindow = new GlfwNativeWindow(WindowingService.glfw, (WindowHandle*)new IntPtr(window).ToPointer()).Win32.Value;
            surface = instance.CreateSurfaceFromWindowsHWND(nativeWindow.HInstance, nativeWindow.Hwnd);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
            var nativeWindow = new GlfwNativeWindow(WindowingService.glfw, (WindowHandle*)new IntPtr(window).ToPointer()).Cocoa.Value;
            surface = instance.CreateSurfaceFromMetalLayer(nativeWindow);
        } else {
            var nativeWindow = new GlfwNativeWindow(WindowingService.glfw, (WindowHandle*)new IntPtr(window).ToPointer()).Wayland.Value;
            surface = instance.CreateSurfaceFromWaylandSurface(nativeWindow.Display);
        }

        instance.RequestAdapter(surface, default, default, (s, a, m) => adapter = a);
        adapter.GetProperties(out Wgpu.AdapterProperties properties);

        adapter.RequestDevice((s, d, m) => device = d, 
            limits: new RequiredLimits {
                Limits = new Wgpu.Limits { maxBindGroups = 1 }   
            },
            deviceExtras: new DeviceExtras { Label = "Device" });

        device.SetUncapturedErrorCallback(ErrorCallback);
    }

    public static void ErrorCallback(Wgpu.ErrorType type, string message) => JRPCServer.Notify("error", new string[] {Enum.GetName(type), message});

    
}