library nebula.common;

import 'dart:ffi';
import 'dart:io';

import 'package:ffi/ffi.dart';
import 'package:nebula/nebula.dart';
import 'package:vector_math/vector_math.dart';

class Shader {
  late final int handle;
  late final Map<String, int> _uniformLocations;

  Shader(String vert, String frag) {
    Pointer<Int> status;
    int _status;

    print('t1');

    using((arena) {
      print('t20');
      final vertexShader = gl.glCreateShader(GL_VERTEX_SHADER);
      print('t21');
      gl.glShaderSource(vertexShader, 1, malloc<Pointer<Char>>()..value = vert.toNativeUtf8(allocator: arena).cast<Char>(), nullptr);
      print('t22');
      gl.glCompileShader(vertexShader);
      gl.glGetShaderiv(vertexShader, GL_COMPILE_STATUS, status = malloc<Int>());
      _status = status.value;
      malloc.free(status);
      if (_status != GL_TRUE) {
        throw Exception('An error occurred while compiling vertex shader ($vertexShader):\n\n${() {
          return using((arena) {
            final length = arena<Int>();
            final infoLog = arena<Char>(1024);
            gl.glGetShaderInfoLog(vertexShader, 1024, length, infoLog);
            return infoLog.cast<Utf8>().toDartString();
          });
        }()}');
      }

      print('t2');

      final fragmentShader = gl.glCreateShader(GL_FRAGMENT_SHADER);
      gl.glShaderSource(fragmentShader, 1, malloc<Pointer<Char>>()..value = frag.toNativeUtf8(allocator: arena).cast<Char>(), nullptr);
      gl.glCompileShader(fragmentShader);
      gl.glGetShaderiv(fragmentShader, GL_COMPILE_STATUS, status = malloc<Int>());
      _status = status.value;
      malloc.free(status);
      if (_status != GL_TRUE) {
        throw Exception('An error occurred while compiling fragment shader ($fragmentShader):\n\n${() {
          return using((arena) {
            final length = arena<Int>();
            final infoLog = arena<Char>(1024);
            gl.glGetShaderInfoLog(fragmentShader, 1024, length, infoLog);
            return infoLog.cast<Utf8>().toDartString();
          });
        }()}');
      }

      handle = gl.glCreateProgram();
      gl.glAttachShader(handle, vertexShader);
      gl.glAttachShader(handle, fragmentShader);
      gl.glLinkProgram(handle);
      gl.glGetProgramiv(handle, GL_LINK_STATUS, status = malloc<Int>());
      _status = status.value;
      malloc.free(status);
      if (_status != GL_TRUE) {
        throw Exception('An error occurred while linking program ($handle):\n\n${() {
          return using((arena) {
            final length = arena<Int>();
            final infoLog = arena<Char>(1024);
            gl.glGetProgramInfoLog(handle, 1024, length, infoLog);
            return infoLog.cast<Utf8>().toDartString();
          });
        }()}');
      }
    }, malloc);

    print('t3');

    _uniformLocations = <String, int>{};
    Pointer<Int> uniformCount;
    gl.glGetProgramiv(handle, GL_ACTIVE_UNIFORMS, uniformCount = malloc<Int>());
    for (int i = 0; i < uniformCount.value; i++) {
      final name = malloc<Char>(1024);
      gl.glGetActiveUniform(handle, i, 1024, nullptr, nullptr, nullptr, name);
      final location = gl.glGetUniformLocation(handle, name);
      _uniformLocations[name.cast<Utf8>().toDartString()] = location;
      malloc.free(name);
    }
  }

  static Shader load(String path) {
    print('t45');
    final sourceLines = File(path).readAsLinesSync();
    print('t456');
    final vertexSource = (sourceLines.skipWhile((value) => !value.startsWith('##VERTEX')).skip(1).takeWhile((value) => !value.startsWith('##FRAGMENT')).toList()..removeLast()).join('\n');
    print('t457');
    final fragmentSource = sourceLines.skipWhile((value) => !value.startsWith('##FRAGMENT')).takeWhile((value) => !value.startsWith('##FRAGMENT')).skip(1).join('\n');
    print('t459');
    return Shader(vertexSource, fragmentSource);
  }

  static Future<Shader> loadSplit(String vertexPath, String fragmentPath) async => Shader(await File(vertexPath).readAsString(), await File(fragmentPath).readAsString());

  //

  int getAttribLocation(String name) => using((arena) => gl.glGetAttribLocation(handle, name.toNativeUtf8(allocator: arena).cast<Char>()));

  void bind() => gl.glUseProgram(handle);

  void setUFloat(String name, double value) => gl.glProgramUniform1f(handle, _uniformLocations[name]!, value);
  void setUInt(String name, int value) => gl.glProgramUniform1i(handle, _uniformLocations[name]!, value);
  void setUVector3(String name, Vector3 value) => gl.glProgramUniform3f(handle, _uniformLocations[name]!, value.x, value.y, value.z);
  void setUVector4(String name, Vector4 value) => gl.glProgramUniform4f(handle, _uniformLocations[name]!, value.x, value.y, value.z, value.w);
  void setUMatrix4(String name, Matrix4 value) => using((arena) => gl.glProgramUniformMatrix4fv(handle, _uniformLocations[name]!, 1, GL_FALSE, arena<Float>(16)..asTypedList(16).setAll(0, value.storage)));

  void setStandardMeshUniforms(MeshStandardUniforms uniforms) {
    if (uniforms.transform != null) setUMatrix4("nebula_matrix_transform", uniforms.transform!);
    if (uniforms.vpm != null) setUMatrix4("nebula_matrix_viewProjection", uniforms.vpm!);
    if (uniforms.viewPos != null) setUMatrix4("nebula_matrix_viewProjection", uniforms.vpm!);
    if (uniforms.directionalLightCount != null) setUInt("nebula_int_directionalLightCount", uniforms.directionalLightCount!);
    if (uniforms.pointLightCount != null) setUInt("nebula_int_pointLightCount", uniforms.pointLightCount!);
  }
}

class MeshStandardUniforms {
  Matrix4? vpm;
  Vector3? viewPos;
  Matrix4? transform;
  int? directionalLightCount;
  int? pointLightCount;

  MeshStandardUniforms({this.vpm, this.viewPos, this.transform, this.directionalLightCount, this.pointLightCount});
}
