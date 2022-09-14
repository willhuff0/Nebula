import 'dart:ffi';

import 'package:ffi/ffi.dart';
import 'package:nebula/bindings/glfw/glfw.dart';
import 'package:nebula/bindings/wgpu/wgpu.dart';
import 'package:nebula/nebula.dart';

void main() {
  glfw.glfwSetErrorCallback(Pointer.fromFunction(glfwErrorCallback));
  if (glfw.glfwInit() == GLFW_FALSE) return;

  glfw.glfwWindowHint(GLFW_CLIENT_API, GLFW_NO_API);
  glfw.glfwWindowHint(GLFW_COCOA_RETINA_FRAMEBUFFER, GLFW_FALSE);
  late final Pointer<GLFWwindow> window;
  using((arena) => window = glfw.glfwCreateWindow(1280, 960, 'Dawn Window'.toNativeUtf8(allocator: arena).cast(), nullptr, nullptr));
  if (window == nullptr) return;

  late final WGPUSurface surface;
  using((arena) {
    final nsWindow = glfw.glfwGetCocoaWindow(window);

    final surfaceDescriptorPointer = arena<WGPUSurfaceDescriptor>();
    final surfaceDescriptor = surfaceDescriptorPointer.ref;
    surfaceDescriptor.label = nullptr;

    final nextInChainPointer = arena<WGPUSurfaceDescriptorFromMetalLayer>();
    final nextInChain = nextInChainPointer.ref;

    final chainPointer = arena<WGPUChainedStruct>();
    final chain = chainPointer.ref;
    chain.next = nullptr;
    chain.sType = WGPUSType.WGPUSType_SurfaceDescriptorFromMetalLayer;

    nextInChain.chain = chain;
    nextInChain.layer = nsWindow.cast();

    surface = wgpu.wgpuInstanceCreateSurface(nullptr, arena<WGPUSurfaceDescriptor>());
  });

  final WGPUAdapter adapter = calloc();
  using((arena) {
    final requestAdapterOptionsPointer = arena<WGPURequestAdapterOptions>();
    final requestAdapterOptions = requestAdapterOptionsPointer.ref;
    requestAdapterOptions.nextInChain = nullptr;
    requestAdapterOptions.compatibleSurface = surface;

    wgpu.wgpuInstanceRequestAdapter(nullptr, requestAdapterOptionsPointer, Pointer.fromFunction(requestAdapterCallback), adapter.cast());
  });

  final WGPUDevice device = calloc();
  using((arena) {
    final deviceDescriptorPointer = arena<WGPUDeviceDescriptor>();
    final deviceDescriptor = deviceDescriptorPointer.ref;
    deviceDescriptor.nextInChain = nullptr;
    deviceDescriptor.label = 'Device'.toNativeUtf8(allocator: arena).cast();

    final requiredLimitsPointer = arena<WGPURequiredLimits>();
    final requiredLimits = requiredLimitsPointer.ref;
    requiredLimits.nextInChain = nullptr;

    final limitsPointer = arena<WGPULimits>();
    final limits = limitsPointer.ref;
    limits.maxBindGroups = 1;

    requiredLimits.limits = limits;

    deviceDescriptor.requiredLimits = requiredLimitsPointer;

    final queueDescriptorPointer = arena<WGPUQueueDescriptor>();
    final queueDescriptor = queueDescriptorPointer.ref;
    queueDescriptor.nextInChain = nullptr;
    queueDescriptor.label = nullptr;

    deviceDescriptor.defaultQueue = queueDescriptor;

    wgpu.wgpuAdapterRequestDevice(adapter, deviceDescriptorPointer, Pointer.fromFunction(requestDeviceCallback), device.cast());
  });

  wgpu.wgpuDeviceSetUncapturedErrorCallback(device, Pointer.fromFunction(handleUncapturedError), nullptr);
  wgpu.wgpuDeviceSetDeviceLostCallback(device, Pointer.fromFunction(handleDeviceLost), nullptr);

  late final WGPUShaderModule shader;
  using((arena) {
    final wgslDescriptorPointer = arena<WGPUShaderModuleWGSLDescriptor>();
    final wgslDescriptor = wgslDescriptorPointer.ref;
    wgslDescriptor.chain.next = nullptr;
    wgslDescriptor.chain.sType = WGPUSType.WGPUSType_ShaderModuleWGSLDescriptor;
    wgslDescriptor.code = '''
      
    '''
        .toNativeUtf8(allocator: arena)
        .cast();

    final wgpuShaderModuleDescriptorPointer = arena<WGPUShaderModuleDescriptor>();
    final wgpuShaderModuleDescriptor = wgpuShaderModuleDescriptorPointer.ref;
    wgpuShaderModuleDescriptor.nextInChain = wgslDescriptorPointer.cast();
    wgpuShaderModuleDescriptor.label = 'shader.wgsl'.toNativeUtf8(allocator: arena).cast();

    shader = wgpu.wgpuDeviceCreateShaderModule(device, wgpuShaderModuleDescriptorPointer);
  });

  int swapChainFormat = wgpu.wgpuSurfaceGetPreferredFormat(surface, adapter);

  late final WGPURenderPipeline pipeline;
  using((arena) {
    final renderPipelineDescriptorPointer = arena<WGPURenderPipelineDescriptor>();
    final renderPipelineDescriptor = renderPipelineDescriptorPointer.ref;
    renderPipelineDescriptor.label = 'Render Pipeline'.toNativeUtf8(allocator: arena).cast();

    final vertexState = arena<WGPUVertexState>().ref
      ..module = shader
      ..entryPoint = 'vs_main'.toNativeUtf8(allocator: arena).cast()
      ..bufferCount = 0
      ..buffers = nullptr;
    renderPipelineDescriptor.vertex = vertexState;

    final primitiveState = arena<WGPUPrimitiveState>().ref
      ..topology = WGPUPrimitiveTopology.WGPUPrimitiveTopology_TriangleList
      ..stripIndexFormat = WGPUIndexFormat.WGPUIndexFormat_Undefined
      ..cullMode = WGPUCullMode.WGPUCullMode_None;
    renderPipelineDescriptor.primitive = primitiveState;

    final multisample = arena<WGPUMultisampleState>().ref
      ..count = 1
      ..mask = ~0
      ..alphaToCoverageEnabled = false;
    renderPipelineDescriptor.multisample = multisample;

    final fragmentPointer = arena<WGPUFragmentState>();
    final fragment = fragmentPointer.ref;
    fragment.module = shader;
    fragment.entryPoint = 'fs_main'.toNativeUtf8(allocator: arena).cast();
    fragment.targetCount = 1;

    final colorTargetsPointer = arena<WGPUColorTargetState>(1);
    final colorTarget = colorTargetsPointer[0]
      ..format = swapChainFormat
      ..writeMask = WGPUColorWriteMask.WGPUColorWriteMask_All;

    final blendPointer = arena<WGPUBlendState>();
    final blend = blendPointer.ref
      ..color = (arena<WGPUBlendComponent>().ref
        ..srcFactor = WGPUBlendFactor.WGPUBlendFactor_One
        ..dstFactor = WGPUBlendFactor.WGPUBlendFactor_Zero
        ..operation = WGPUBlendOperation.WGPUBlendOperation_Add)
      ..alpha = (arena<WGPUBlendComponent>().ref
        ..srcFactor = WGPUBlendFactor.WGPUBlendFactor_One
        ..dstFactor = WGPUBlendFactor.WGPUBlendFactor_Zero
        ..operation = WGPUBlendOperation.WGPUBlendOperation_Add);

    colorTarget.blend = blendPointer;

    fragment.targets = colorTargetsPointer;
    renderPipelineDescriptor.fragment = fragmentPointer;

    renderPipelineDescriptor.depthStencil = nullptr;

    pipeline = wgpu.wgpuDeviceCreateRenderPipeline(device, renderPipelineDescriptorPointer);
  });

  late WGPUSwapChain swapChain;
  using((arena) {
    final swapChainDescriptorPointer = arena<WGPUSwapChainDescriptor>();
    final swapChainDescriptor = swapChainDescriptorPointer.ref;
    swapChainDescriptor.usage = WGPUTextureUsage.WGPUTextureUsage_RenderAttachment;
    swapChainDescriptor.format = swapChainFormat;
    swapChainDescriptor.width = 1280;
    swapChainDescriptor.height = 960;
    swapChainDescriptor.presentMode = WGPUPresentMode.WGPUPresentMode_Mailbox;

    swapChain = wgpu.wgpuDeviceCreateSwapChain(device, surface, swapChainDescriptorPointer);
  });

  while (glfw.glfwWindowShouldClose(window) == GLFW_FALSE) {
    WGPUTextureView? nextTexture;

    for (int attempt = 0; attempt < 2; attempt++) {
      // Only if size changes
      // using((arena) {
      //   final swapChainDescriptorPointer = arena<WGPUSwapChainDescriptor>();
      //   final swapChainDescriptor = swapChainDescriptorPointer.ref;
      //   swapChainDescriptor.usage = WGPUTextureUsage.WGPUTextureUsage_RenderAttachment;
      //   swapChainDescriptor.format = swapChainFormat;
      //   swapChainDescriptor.width = 1280;
      //   swapChainDescriptor.height = 960;
      //   swapChainDescriptor.presentMode = WGPUPresentMode.WGPUPresentMode_Mailbox;

      //   swapChain = wgpu.wgpuDeviceCreateSwapChain(device, surface, swapChainDescriptorPointer);
      // });

      nextTexture = wgpu.wgpuSwapChainGetCurrentTextureView(swapChain);

      if (attempt == 0 && nextTexture == nullptr) {
        print('gpuSwapChainGetCurrentTextureView() failed; trying to create a new swap chain...');
        // prevWidth = 0;
        // prevHeight = 0;
        continue;
      }

      break;
    }

    if (nextTexture == null || nextTexture == nullptr) {
      print('Cannot aquire next swap chain texture.');
      return;
    }

    late final WGPUCommandEncoder commandEncoder;
    using((arena) {
      final commandEncoderDescriptorPointer = arena<WGPUCommandEncoderDescriptor>();
      final commandEncoderDescriptor = commandEncoderDescriptorPointer.ref;
      commandEncoderDescriptor.label = 'Command Encoder'.toNativeUtf8(allocator: arena).cast();

      commandEncoder = wgpu.wgpuDeviceCreateCommandEncoder(device, commandEncoderDescriptorPointer);
    });

    late final WGPURenderPassEncoder renderPass;
    using((arena) {
      final renderPassDescriptorPointer = arena<WGPURenderPassDescriptor>();
    });
  }
}

void requestAdapterCallback(int status, WGPUAdapter received, Pointer<Char> message, Pointer<Void> userdata) {
  userdata = received.cast();
}

void requestDeviceCallback(int status, WGPUDevice received, Pointer<Char> message, Pointer<Void> userdata) {
  userdata = received.cast();
}

void handleUncapturedError(int type, Pointer<Char> message, Pointer<Void> userdata) {
  print('WGPU uncaptured error ($type): ${message.cast<Utf8>().toDartString()}');
}

void handleDeviceLost(int reason, Pointer<Char> message, Pointer<Void> userdata) {
  print('WGPU device lost ($reason): ${message.cast<Utf8>().toDartString()}');
}

// import 'dart:ffi';
// import 'dart:io';

// import 'package:ffi/ffi.dart';
// import 'package:nebula/bindings/angle/egl.dart';
// import 'package:nebula/bindings/bindings.dart';
// import 'package:nebula/bindings/glfw/glfw.dart';
// import 'package:nebula/common/common.dart';

// void main() {
//   glfw.glfwSetErrorCallback(Pointer.fromFunction(glfwErrorCallback));

//   glfw.glfwInitHint(GLFW_ANGLE_PLATFORM_TYPE, GLFW_ANGLE_PLATFORM_TYPE_OPENGL);

//   if (glfw.glfwInit() == GLFW_FALSE) {
//     print('Could not init GLFW');
//     return;
//   }

//   egl.eglBindAPI(EGL_OPENGL_ES_API);

//   glfw.glfwWindowHint(GLFW_CLIENT_API, GLFW_OPENGL_ES_API);
//   glfw.glfwWindowHint(GLFW_CONTEXT_CREATION_API, GLFW_EGL_CONTEXT_API);
//   glfw.glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
//   glfw.glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 1);
//   glfw.glfwWindowHint(GLFW_SAMPLES, 4);
//   glfw.glfwWindowHint(GLFW_RESIZABLE, GLFW_FALSE);

//   late Pointer<GLFWwindow> window;
//   using((arena) => window = glfw.glfwCreateWindow(1280, 960, 'Tests'.toNativeUtf8(allocator: arena).cast(), nullptr, nullptr), malloc);
//   if (window == nullptr) {
//     print('Could not create window');
//     glfw.glfwTerminate();
//     return;
//   }

//   glfw.glfwMakeContextCurrent(window);

//   print('GLFW ${glfw.glfwGetVersionString().cast<Utf8>().toDartString()}');
//   print(gl.glGetString(GL_RENDERER).cast<Utf8>().toDartString());
//   print(gl.glGetString(GL_VERSION).cast<Utf8>().toDartString());
//   print(gl.glGetString(GL_SHADING_LANGUAGE_VERSION).cast<Utf8>().toDartString());

//   gl.glViewport(0, 0, 1280, 960);
//   gl.glClearColor(.2, .3, .3, 1);
//   gl.glEnable(GL_DEPTH_TEST);
//   gl.glEnable(GL_CULL_FACE);
//   gl.glCullFace(GL_BACK);
//   gl.glClear(GL_COLOR_BUFFER_BIT);

//   Light.shadowMapShader = Shader.load('shaders/shadowmap.glsl');

//   final lights = [
//     DirectionalLight(Vector3(0.4, -1.0, 0.4), Vector3(1.0, 0.96, 0.9), 1.0),
//   ];

//   final model = Model.load('resources/untitled.glb', Shader.load('shaders/standard.glsl'))!;

//   print('${Texture.textureCache.length} textures were cached.');

//   final camera = Camera(Vector3(0, 1, 3), 1280 / 960);

//   //glfw.glfwSetInputMode(window, GLFW_CURSOR, GLFW_CURSOR_DISABLED);
//   //glfw.glfwSetKeyCallback(window, Pointer.fromFunction(glfwKeyCallback));

//   //print(glfw.glfwWindowShouldClose(window));

//   drawForShadowMaps() {
//     model.drawForShadowMaps();
//   }

//   final stopwatch = Stopwatch()..start();
//   while (glfw.glfwWindowShouldClose(window) == GLFW_FALSE) {
//     //print(1.0 / stopwatch.elapsedMilliseconds * 1000.0);
//     //stopwatch.reset();

//     /*
//       UPDATE
//     */

//     /*
//       RENDER
//     */

//     final vpm = camera.getProjectionMatrix() * camera.getViewMatrix();
//     final viewPos = camera.position;
//     final uniforms = MeshStandardUniforms(vpm: vpm, viewPos: viewPos);

//     gl.glCullFace(GL_FRONT);
//     final shadowMaps = <int>[];
//     for (final light in lights) {
//       final map = light.drawShadowMap(drawForShadowMaps);
//       if (map != -1) shadowMaps.add(map);
//     }
//     gl.glCullFace(GL_BACK);

//     gl.glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

//     model.draw(uniforms);

//     glfw.glfwSwapBuffers(window);
//     glfw.glfwPollEvents();
//   }

//   glfw.glfwTerminate();
// }

// void glfwKeyCallback(Pointer<GLFWwindow> window, int key, int scancode, int action, int mods) {}

void glfwErrorCallback(int code, Pointer<Char> error) {
  print('GLFW ERROR ($code): ${error.cast<Utf8>().toDartString()}');
}
