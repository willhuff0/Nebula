import 'dart:ffi';
import 'package:ffi/ffi.dart';

import 'package:nebula/bindings/angle/angle.dart';

void main() {
  final angle = Angle(DynamicLibrary.open('dll/angle/libGLESv2.dll'));
  print(angle.glGetString(GL_VENDOR));
  print(angle.glGetString(GL_VERSION);
}
