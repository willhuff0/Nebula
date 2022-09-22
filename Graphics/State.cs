using WGPU.NET;

namespace Nebula.Graphics;

public static class State {
    public static Wgpu.InstanceImpl instance;
    public static Wgpu.SurfaceImpl surface;
    public static Wgpu.AdapterImpl adapter;
    public static Wgpu.DeviceImpl device;

    public static Wgpu.SwapChainDescriptor swapChainDescriptor;
    public static Wgpu.SwapChainImpl swapChain;
    public static Wgpu.TextureFormat swapChainFormat;

    public static void UpdateSwapChain() => swapChain = Wgpu.DeviceCreateSwapChain(device, surface, swapChainDescriptor);
}