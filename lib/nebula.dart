library nebula;

import 'dart:ffi';

import 'bindings/angle/egl.dart';
import 'bindings/angle/gl.dart';
import 'bindings/glfw/glfw.dart';

export 'bindings/angle/gl.dart';

final glfw = GLFW(DynamicLibrary.open('ffi/glfw/win-x64/glfw3.dll'));
final egl = AngleEGL(DynamicLibrary.open('ffi/angle/win-x64/libEGL.dll'));
final gl = AngleGLES3(DynamicLibrary.open('ffi/angle/win-x64/libGLESv2.dll'));
