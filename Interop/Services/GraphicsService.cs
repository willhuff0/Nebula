using System.Numerics;
using System.Runtime.InteropServices;
using AustinHarris.JsonRpc;
using Silk.NET.GLFW;
using WGPU.NET;

namespace Nebula.Interop.Services;

public unsafe class GraphicsService : JsonRpcService {
    private State state;

    #region Initialization

    [JsonRpcMethod] public void createInstanceSurfaceAdapterDeviceAndSetState(long window) {
        var windowHandle = (WindowHandle*)new IntPtr(window).ToPointer();

        Wgpu.InstanceImpl instance = new Wgpu.InstanceImpl();
        
        Wgpu.SurfaceDescriptor surfaceDescriptor;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            var nativeWindow = new GlfwNativeWindow(WindowingService.glfw, windowHandle).Win32.Value;
            var surfaceDescriptorInfo = new Wgpu.SurfaceDescriptorFromWindowsHWND() { hwnd = nativeWindow.Hwnd, hinstance = nativeWindow.HInstance, chain = new Wgpu.ChainedStruct() { sType = Wgpu.SType.SurfaceDescriptorFromWindowsHWND } };
			surfaceDescriptor = new Wgpu.SurfaceDescriptor() { nextInChain = (IntPtr)(&surfaceDescriptorInfo) };
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
            var nativeWindow = new GlfwNativeWindow(WindowingService.glfw, windowHandle).Cocoa.Value;
            var surfaceDescriptorInfo = new Wgpu.SurfaceDescriptorFromMetalLayer() { layer = nativeWindow, chain = new Wgpu.ChainedStruct() { sType = Wgpu.SType.SurfaceDescriptorFromMetalLayer } };
            surfaceDescriptor = new Wgpu.SurfaceDescriptor() { nextInChain = (IntPtr)(&surfaceDescriptorInfo) };
        } else {
            var nativeWindow = new GlfwNativeWindow(WindowingService.glfw, windowHandle).Wayland.Value;
            var surfaceDescriptorInfo = new Wgpu.SurfaceDescriptorFromWaylandSurface() { display = nativeWindow.Display, surface = nativeWindow.Surface, chain = new Wgpu.ChainedStruct() { sType = Wgpu.SType.SurfaceDescriptorFromWaylandSurface } };
			surfaceDescriptor = new Wgpu.SurfaceDescriptor() { nextInChain = (IntPtr)(&surfaceDescriptorInfo) };
        }
        Wgpu.SurfaceImpl surface = Wgpu.InstanceCreateSurface(instance, surfaceDescriptor);

        var adapterOptions = new Wgpu.RequestAdapterOptions() { compatibleSurface = surface };
        Wgpu.AdapterImpl adapter = default;
        Wgpu.InstanceRequestAdapter(instance, adapterOptions, (s, a, m, u) => { adapter = a; }, IntPtr.Zero);

        var properties = new Wgpu.AdapterProperties();
        Wgpu.AdapterGetProperties(adapter, ref properties);

        var deviceExtras = new Wgpu.DeviceExtras () { chain = new Wgpu.ChainedStruct () { sType = (Wgpu.SType)Wgpu.NativeSType.STypeDeviceExtras }, label = "Device" };
        IntPtr deviceExtrasPtr = MarshalAndBox(deviceExtras);

        var requiredLimits = new Wgpu.RequiredLimits() { limits = new Wgpu.Limits() { maxBindGroups = 1 } };
        var deviceDescriptor = new Wgpu.DeviceDescriptor() { nextInChain = deviceExtrasPtr, requiredLimits = (IntPtr)(&requiredLimits)};

        Wgpu.DeviceImpl device = default;
        Wgpu.AdapterRequestDevice(adapter, deviceDescriptor, (s, d, m, u) => { device = d; }, IntPtr.Zero);

        WindowingService.glfw.GetWindowSize(windowHandle, out int width, out int height);
        var swapChainFormat = Wgpu.SurfaceGetPreferredFormat(surface, adapter);
        var swapChainDescriptor = new Wgpu.SwapChainDescriptor() {
            usage = (uint) Wgpu.TextureUsage.RenderAttachment,
            format = swapChainFormat,
            width = (uint)width,
            height = (uint)height,
            presentMode = Wgpu.PresentMode.Mailbox
        };
        var swapChain = Wgpu.DeviceCreateSwapChain(device, surface, swapChainDescriptor);

        Console.WriteLine($"Initialized WGPU {Wgpu.GetVersion()}; {properties.name} ({properties.backendType})");

        state = new State() {
            instance = instance,
            surface = surface,
            adapter = adapter,
            device = device,
            swapChainDescriptor = swapChainDescriptor,
            swapChain = swapChain,
            swapChainFormat = swapChainFormat
        };
    }
    // [JsonRpcMethod] private int[] createInstanceSurfaceAdapterDevice(int window) => _createInstanceSurfaceAdapterDevice(window).Serialize();
    // [JsonRpcMethod] private int[] createInstanceSurfaceAdapterDeviceAndSetState(int window) {
    //     state = _createInstanceSurfaceAdapterDevice(window);
    //     return state.Serialize();
    // }
    // [JsonRpcMethod] private void setState(int[] value) => state = new State(value);
    // [JsonRpcMethod] private int[] getState() => state.Serialize();

    public void ErrorCallback(Wgpu.ErrorType type, string message) => JRPCServer.Notify("error", new string[] {Enum.GetName(type), message});

    #endregion

    #region Scene Loading

    [JsonRpcMethod] public int createShader(string wgsl, int cullMode) {
        var wgslDescriptor = new Wgpu.ShaderModuleWGSLDescriptor() { chain = new Wgpu.ChainedStruct() { sType = Wgpu.SType.ShaderModuleWGSLDescriptor }, code = wgsl };
        var wgslDescriptorPtr = MarshalAndBox(wgslDescriptor);
        var shaderDescriptor = new Wgpu.ShaderModuleDescriptor() { nextInChain = wgslDescriptorPtr, label = "shader.wgsl" };
        var shader = Wgpu.DeviceCreateShaderModule(state.device, shaderDescriptor);

        var pipelineLayoutDescriptor = new Wgpu.PipelineLayoutDescriptor() { bindGroupLayoutCount = 0 };
        var pipelineLayout = Wgpu.DeviceCreatePipelineLayout(state.device, pipelineLayoutDescriptor);

        var blendState = new Wgpu.BlendState() {
            color = new Wgpu.BlendComponent() {
                srcFactor = Wgpu.BlendFactor.One,
                dstFactor = Wgpu.BlendFactor.Zero,
                operation = Wgpu.BlendOperation.Add
            },
            alpha = new Wgpu.BlendComponent() {
                srcFactor = Wgpu.BlendFactor.One,
                dstFactor = Wgpu.BlendFactor.Zero,
                operation = Wgpu.BlendOperation.Add
            }
        };

        var colorTargetState = new Wgpu.ColorTargetState() {
            format = state.swapChainFormat,
            blend = (IntPtr)(&blendState),
            writeMask = (uint)Wgpu.ColorWriteMask.All
        };

        var fragmentState = new Wgpu.FragmentState() {
            module = shader,
            entryPoint = "fs_main",
            targetCount = 1,
            targets = (IntPtr)(&colorTargetState)
        };
        var framgentStatePtr = MarshalAndBox(fragmentState);

        var renderPipelineDescriptor = new Wgpu.RenderPipelineDescriptor() {
            layout = pipelineLayout,
            vertex = new Wgpu.VertexState() {
                module = shader,
                entryPoint = "vs_main",
                bufferCount = 3
            },
            primitive = new Wgpu.PrimitiveState() {
                topology = Wgpu.PrimitiveTopology.TriangleList,
                stripIndexFormat = Wgpu.IndexFormat.Undefined,
                frontFace = Wgpu.FrontFace.CCW,
                cullMode = (Wgpu.CullMode)cullMode
            },
            multisample = new Wgpu.MultisampleState() {
                count = 1,
                mask = uint.MaxValue,
                alphaToCoverageEnabled = false
            },
            fragment = framgentStatePtr
        };

        var renderPipeline = Wgpu.DeviceCreateRenderPipeline(state.device, renderPipelineDescriptor);
        return Interop.Store(renderPipeline);
    }

    #endregion

    #region Render Pass

    private Wgpu.CommandEncoderImpl encoder;
    private Wgpu.RenderPassEncoderImpl renderPass;

    [JsonRpcMethod] public void beginRenderPass(long window) {
        // var prevWindowSize = WindowingService.windowSizes[window];
        // var windowSize = WindowingService.GetWindowSize(window);
        // if (windowSize != prevWindowSize) {
        //     state.swapChainDescriptor.width = (uint)windowSize.Item1;
        //     state.swapChainDescriptor.height = (uint)windowSize.Item2;
        //     state.UpdateSwapChain();
        // }

        var nextTexture = Wgpu.SwapChainGetCurrentTextureView(state.swapChain);
        if (nextTexture.Handle == IntPtr.Zero) throw new Exception("Could not acquire next swap chain texture.");

        var encoderDescriptor = new Wgpu.CommandEncoderDescriptor() { };
        encoder = Wgpu.DeviceCreateCommandEncoder(state.device, encoderDescriptor);

        var colorAttachment = new Wgpu.RenderPassColorAttachment()  {
            view = nextTexture,
            resolveTarget = default,
            loadOp = Wgpu.LoadOp.Clear,
            storeOp = Wgpu.StoreOp.Store,
            clearValue = new Wgpu.Color() { r = 0, g = 1, b = 0, a = 1 }
        };

        var renderPassDescriptor = new Wgpu.RenderPassDescriptor() {
            colorAttachments = (IntPtr)(&colorAttachment),
            colorAttachmentCount = 1
        };

        renderPass = Wgpu.CommandEncoderBeginRenderPass(encoder, renderPassDescriptor);
    }

    [JsonRpcMethod] public void bindShader(int shader) => Wgpu.RenderPassEncoderSetPipeline(renderPass, Interop.Retrieve<Wgpu.RenderPipelineImpl>(shader));

    [JsonRpcMethod] public void drawIndexed(uint indexCount) => Wgpu.RenderPassEncoderDrawIndexed(renderPass, indexCount, 1, 0, 0, 0);
    [JsonRpcMethod] public void draw(uint count) => Wgpu.RenderPassEncoderDraw(renderPass, count, 1, 0, 0);

    [JsonRpcMethod] public void submitRenderPass() {
        Wgpu.RenderPassEncoderEnd(renderPass);

        var queue = Wgpu.DeviceGetQueue(state.device);
        var commandBufferDescriptor = new Wgpu.CommandBufferDescriptor() { label = "Command Encoder" };
        var commandBuffer = Wgpu.CommandEncoderFinish(encoder, commandBufferDescriptor);

        Wgpu.QueueSubmit(queue, 1, ref commandBuffer);
        Wgpu.SwapChainPresent(state.swapChain);

        WindowingService.glfw.PollEvents();
    }

    #endregion
}

internal struct State {
    public Wgpu.InstanceImpl instance;
    public Wgpu.SurfaceImpl surface;
    public Wgpu.AdapterImpl adapter;
    public Wgpu.DeviceImpl device;

    public Wgpu.SwapChainDescriptor swapChainDescriptor;
    public Wgpu.SwapChainImpl swapChain;
    public Wgpu.TextureFormat swapChainFormat;

    public void UpdateSwapChain() => swapChain = Wgpu.DeviceCreateSwapChain(device, surface, swapChainDescriptor);

    // public State(int[] value) { 
    //     instance = new Wgpu.InstanceImpl(new IntPtr(value[0]));
    //     surface = new Wgpu.SurfaceImpl(new IntPtr(value[1]));
    //     adapter = new Wgpu.AdapterImpl(new IntPtr(value[2]));
    //     device = new Wgpu.DeviceImpl(new IntPtr(value[3]));
    // }
    // public int[] Serialize() => new int[] { 
    //     instance.Handle.ToInt32(), 
    //     surface.Handle.ToInt32(), 
    //     adapter.Handle.ToInt32(), 
    //     device.Handle.ToInt32() 
    // };
}

public struct Vertex {
    public Vector3 position;
    public Vector3 normal;
    public Vector2 uv;
}