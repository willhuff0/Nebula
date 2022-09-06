import 'dart:ffi';

import 'package:ffi/ffi.dart';
import 'package:nebula/bindings/angle/egl.dart';
import 'package:nebula/bindings/angle/gl.dart';
import 'package:nebula/bindings/glfw/glfw.dart';

void main() async {
  glfw.glfwInitHint(EGL_ANGLE_platform_angle, EGL_ANGLE_platform_angle_vulkan);

  if (glfw.glfwInit() == GLFW_FALSE) {
    print('Could not init GLFW');
    return;
  }

  glfw.glfwWindowHint(GLFW_CLIENT_API, GLFW_OPENGL_ES_API);
  glfw.glfwWindowHint(GLFW_CONTEXT_CREATION_API, GLFW_EGL_CONTEXT_API);
  glfw.glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
  glfw.glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 1);
  glfw.glfwWindowHint(GLFW_SAMPLES, 4);
  glfw.glfwWindowHint(GLFW_RESIZABLE, GLFW_FALSE);

  final window = glfw.glfwCreateWindow(800, 600, 'Tests'.toNativeUtf8().cast(), nullptr, nullptr);
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

  gl.glViewport(0, 0, 800, 600);
  gl.glClearColor(0, 1, 0, 1);

  while (glfw.glfwWindowShouldClose(window) == GLFW_FALSE) {
    gl.glClear(GL_COLOR_BUFFER_BIT);

    glfw.glfwSwapBuffers(window);
    glfw.glfwPollEvents();
  }

  glfw.glfwTerminate();
}
