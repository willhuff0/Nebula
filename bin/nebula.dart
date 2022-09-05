import 'package:dart_sdl/dart_sdl.dart';
import 'package:dart_sdl/src/sdl_bindings.dart';

void main() {
  final sdl = Sdl(libName: 'dll/sdl/osx-arm/libSDL2.dylib')..init();
  final window = sdl.createWindow('Example Window');
}
