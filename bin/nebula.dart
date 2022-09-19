import 'dart:ffi';

import 'package:ffi/ffi.dart';
import 'package:nebula/bindings/angle/egl.dart';
import 'package:nebula/bindings/bindings.dart';
import 'package:nebula/bindings/glfw/glfw.dart';
import 'package:nebula/common/common.dart';

void main() {
  glfw.glfwSetErrorCallback(Pointer.fromFunction(glfwErrorCallback));

  glfw.glfwInitHint(GLFW_ANGLE_PLATFORM_TYPE, GLFW_ANGLE_PLATFORM_TYPE_OPENGL);

  if (glfw.glfwInit() == GLFW_FALSE) {
    print('Could not init GLFW');
    return;
  }

  egl.eglBindAPI(EGL_OPENGL_ES_API);

  glfw.glfwWindowHint(GLFW_CLIENT_API, GLFW_OPENGL_ES_API);
  glfw.glfwWindowHint(GLFW_CONTEXT_CREATION_API, GLFW_EGL_CONTEXT_API);
  glfw.glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
  glfw.glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 1);
  glfw.glfwWindowHint(GLFW_SAMPLES, 4);
  glfw.glfwWindowHint(GLFW_RESIZABLE, GLFW_FALSE);

  late Pointer<GLFWwindow> window;
  using((arena) => window = glfw.glfwCreateWindow(1280, 960, 'Tests'.toNativeUtf8(allocator: arena).cast(), nullptr, nullptr), malloc);
  if (window == nullptr) {
    print('Could not create window');
    glfw.glfwTerminate();
    return;
  }

  glfw.glfwMakeContextCurrent(window);

  print('GLFW ${glfw.glfwGetVersionString().cast<Utf8>().toDartString()}');
  print(gl.glGetString(GL_RENDERER).cast<Utf8>().toDartString());
  print(gl.glGetString(GL_VERSION).cast<Utf8>().toDartString());
  print(gl.glGetString(GL_SHADING_LANGUAGE_VERSION).cast<Utf8>().toDartString());

  Light.shadowMapShader = Shader.load('shaders/shadowmap.glsl');

  final lights = [
    DirectionalLight(Vector3(0.4, -1.0, 0.4), Vector3(1.0, 0.96, 0.9), 1.0),
  ];

  gl.glViewport(0, 0, 1280, 960);
  gl.glClearColor(.2, .3, .3, 1);
  gl.glEnable(GL_DEPTH_TEST);
  gl.glEnable(GL_CULL_FACE);
  gl.glCullFace(GL_BACK);

  final model = Model.load('resources/untitled.glb', Shader.load('shaders/standard.glsl'))!;

  print('${Texture.textureCache.length} textures were cached.');

  final camera = Camera(Vector3(0, 1, 3), 1280 / 960);

  //glfw.glfwSetInputMode(window, GLFW_CURSOR, GLFW_CURSOR_DISABLED);
  //glfw.glfwSetKeyCallback(window, Pointer.fromFunction(glfwKeyCallback));

  //print(glfw.glfwWindowShouldClose(window));

  drawForShadowMaps() {
    model.drawForShadowMaps();
  }

  final stopwatch = Stopwatch()..start();
  while (glfw.glfwWindowShouldClose(window) == GLFW_FALSE) {
    //print(1.0 / stopwatch.elapsedMilliseconds * 1000.0);
    //stopwatch.reset();

    /*
      UPDATE
    */

    /*
      RENDER
    */

    final vpm = camera.getViewMatrix() * camera.getProjectionMatrix();
    final viewPos = camera.position;
    final uniforms = MeshStandardUniforms(vpm: vpm, viewPos: viewPos);

    gl.glCullFace(GL_FRONT);
    final shadowMaps = <int>[];
    for (final light in lights) {
      final map = light.drawShadowMap(drawForShadowMaps);
      if (map != -1) shadowMaps.add(map);
    }
    gl.glCullFace(GL_BACK);

    gl.glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

    model.draw(uniforms, lights);

    glfw.glfwSwapBuffers(window);
    glfw.glfwPollEvents();
  }

  glfw.glfwTerminate();
}

void glfwKeyCallback(Pointer<GLFWwindow> window, int key, int scancode, int action, int mods) {}

void glfwErrorCallback(int code, Pointer<Char> error) {
  print('GLFW ERROR ($code): ${error.cast<Utf8>().toDartString()}');
}
