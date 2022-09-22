using WGPU.NET;

namespace Nebula.Graphics;

public unsafe class Model {
    public Wgpu.RenderPipelineImpl impl;

    public Model(Shader shader, int cullMode = (int)Wgpu.CullMode.Back) {
        var pipelineLayoutDescriptor = new Wgpu.PipelineLayoutDescriptor() { bindGroupLayoutCount = 0 };
        var pipelineLayout = Wgpu.DeviceCreatePipelineLayout(State.device, pipelineLayoutDescriptor);

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
            format = State.swapChainFormat,
            blend = (IntPtr)(&blendState),
            writeMask = (uint)Wgpu.ColorWriteMask.All
        };

        var fragmentState = new Wgpu.FragmentState() {
            module = shader.impl,
            entryPoint = "fs_main",
            targetCount = 1,
            targets = (IntPtr)(&colorTargetState)
        };
        var framgentStatePtr = Utils.MarshalAndBox(fragmentState);

        var bufferDescriptor = new Wgpu.BufferDescriptor() {
            
        };

        var renderPipelineDescriptor = new Wgpu.RenderPipelineDescriptor() {
            layout = pipelineLayout,
            vertex = new Wgpu.VertexState() {
                module = shader.impl,
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

        impl = Wgpu.DeviceCreateRenderPipeline(State.device, renderPipelineDescriptor);
    }
}