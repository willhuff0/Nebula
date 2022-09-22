using WGPU.NET;

namespace Nebula.Graphics;

public class Shader {
    public Wgpu.ShaderModuleImpl impl;

    public Shader(string wgsl) {
        var wgslDescriptor = new Wgpu.ShaderModuleWGSLDescriptor() { chain = new Wgpu.ChainedStruct() { sType = Wgpu.SType.ShaderModuleWGSLDescriptor }, code = wgsl };
        var wgslDescriptorPtr = Utils.MarshalAndBox(wgslDescriptor);
        var shaderDescriptor = new Wgpu.ShaderModuleDescriptor() { nextInChain = wgslDescriptorPtr, label = "shader.wgsl" };
        impl = Wgpu.DeviceCreateShaderModule(State.device, shaderDescriptor);
    }
}