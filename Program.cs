using CommandLine;
using Nebula.Interop;
using Nebula.Interop.Services;
using Silk.NET.GLFW;
using WGPU.NET;

namespace Nebula;

public class Options {
    [Option('i', "interopPassword")]
    public string InteropPassword { get; set; }
}

public class Nebula {
    public static void Main(string[] args) {
        Options options = Parser.Default.ParseArguments<Options>(args).Value;
        JRPCServer.Start(options.InteropPassword);

        GraphicsService gs = (GraphicsService)Interop.Interop.Services[1];
        WindowingService ws = (WindowingService)Interop.Interop.Services[2];

        ws.windowHint((int)WindowHintClientApi.ClientApi, (int)ClientApi.NoApi);
        var window = ws.createWindow(800, 600, "WGPU!");
        gs.createInstanceSurfaceAdapterDeviceAndSetState(window);

        var shader = gs.createShader(@"
            @stage(vertex)
            fn vs_main(@builtin(vertex_index) in_vertex_index: u32) -> @builtin(position) vec4<f32> {
                let x = f32(i32(in_vertex_index) - 1);
                let y = f32(i32(in_vertex_index & 1u) * 2 - 1);
                return vec4<f32>(x, y, 0.0, 1.0);
            }

            @stage(fragment)
            fn fs_main() -> @location(0) vec4<f32> {
                return vec4<f32>(1.0, 0.0, 0.0, 1.0);
            }
        ", (int)Wgpu.CullMode.None);

        while(!ws.windowShoudClose(window)) {
            gs.beginRenderPass(window);
            
            gs.bindShader(shader);
            gs.draw(3);

            gs.submitRenderPass();
        }
    }
}