library nebula.common;

import 'dart:ffi';

import 'package:ffi/ffi.dart';
import 'package:nebula/nebula.dart';

class Shader {
  final int handle;
  final Map<String, int> _uniformLocations;

  Shader(String vert, String frag) {
    Pointer<Int> status;

    final vertexShader = gl.glCreateShader(GL_VERTEX_SHADER);
    gl.glShaderSource(vertexShader, 1, vert.toNativeUtf8().cast(), nullptr);
    gl.glCompileShader(vertexShader);
    gl.glGetShaderiv(vertexShader, GL_COMPILE_STATUS, status = malloc<Int>());
    if (status.value != GL_TRUE)
      throw Exception('An error occurred while compiling vertex shader ($vertexShader):\n\n${gl.glGetShaderInfoLog(
        vertexShader,
        1024,
      )}');
  }
}
