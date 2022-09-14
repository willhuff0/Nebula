import 'dart:ffi';
import 'dart:io';

import 'package:nebula/bindings/assimp/assimp.dart';
import 'package:nebula/bindings/wgpu/wgpu.dart';

import 'angle/egl.dart';
import 'angle/gl.dart';
import 'glfw/glfw.dart';

export 'angle/gl.dart';

final glfw = GLFW(DynamicLibrary.open(Platform.isWindows
    ? 'ffi/glfw/win-x64/glfw3.dll'
    : Platform.isMacOS
        ? 'ffi/glfw/osx-arm/libglfw.3.4.dylib'
        : throw Exception('Platform \'${Platform.operatingSystem}\' is not supported by GLFW.')));
final wgpu = WGPU(DynamicLibrary.open(Platform.isWindows
    ? 'ffi/wgpu/win-x64/wgpu.dll'
    : Platform.isMacOS
        ? 'ffi/wgpu/osx-arm/libwgpu.dylib'
        : throw Exception('Platform \'${Platform.operatingSystem}\' is not supported by Dawn.')));
final egl = AngleEGL(DynamicLibrary.open(Platform.isWindows
    ? 'ffi/angle/win-x64/libEGL.dll'
    : Platform.isMacOS
        ? 'ffi/angle/osx-arm/libEGL.dylib'
        : throw Exception('Platform \'${Platform.operatingSystem}\' is not supported by EGL.')));
final gl = AngleGLES3(DynamicLibrary.open(Platform.isWindows
    ? 'ffi/angle/win-x64/libGLESv2.dll'
    : Platform.isMacOS
        ? 'ffi/angle/osx-arm/libGLES.dylib'
        : throw Exception('Platform \'${Platform.operatingSystem}\' is not supported by GLES3.')));
final assimp = Assimp(DynamicLibrary.open(Platform.isWindows
    ? 'ffi/assimp/win-x64/assimp.dll'
    : Platform.isMacOS
        ? 'ffi/assimp/osx-arm/libassimp.dylib'
        : throw Exception('Platform \'${Platform.operatingSystem}\' is not supported by Assimp.')));
