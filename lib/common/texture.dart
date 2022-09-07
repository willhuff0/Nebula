import 'dart:ffi';
import 'dart:io';

import 'package:ffi/ffi.dart';
import 'package:image/image.dart';
import 'package:nebula/nebula.dart';

class Texture {
  late final int handle;
  late String type;

  static Map<String, Texture> textureCache = <String, Texture>{};

  Texture(this.handle, this.type);

  static Future<Texture> loadFromFile(String path, String type) async {
    if (textureCache.containsKey(path)) return Texture(textureCache[path]!.handle, type);

    int handle = 0;
    using((arena) {
      final textures = arena<UnsignedInt>();
      gl.glGenTextures(1, textures);
      handle = textures.elementAt(0).value;
    });

    gl.glActiveTexture(GL_TEXTURE0);
    gl.glBindTexture(GL_TEXTURE_2D, handle);

    final image = decodeImage(await File(path).readAsBytes())!;
    using((arena) => gl.glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, image.width, image.height, 0, GL_RGBA, GL_UNSIGNED_BYTE, (arena<Uint8>(image.length)..asTypedList(image.length).setAll(0, image.getBytes())).cast()));

    gl.glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
    gl.glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);

    gl.glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
    gl.glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);

    gl.glGenerateMipmap(GL_TEXTURE_2D);

    final texture = Texture(handle, type);
    textureCache[path] = texture;
    return texture;
  }

  static Future<Texture> getDefaultAlbedo() => Texture.loadFromFile('Resources/default/default_albedo.png', 'albedo');
  static Future<Texture> getDefaultNormal() => Texture.loadFromFile('Resources/default/default_normal.png', 'normal');
  static Future<Texture> getDefaultMetallic() => Texture.loadFromFile('Resources/default/default_metallic_roughness.png', 'metallic');
  static Future<Texture> getDefaultRoughness() => Texture.loadFromFile('Resources/default/default_metallic_roughness.png', 'roughness');
  static Future<Texture> getDefaultAO() => Texture.loadFromFile('Resources/default/default_ao.png', 'ao');

  void bind(int unit) {
    gl.glActiveTexture(unit);
    gl.glBindTexture(GL_TEXTURE_2D, handle);
  }
}

class DefaultTextures {
  Texture? albedo;
  Texture? normal;
  Texture? metallic;
  Texture? roughness;
  Texture? ao;

  DefaultTextures({this.albedo, this.normal, this.metallic, this.roughness, this.ao});

  Future<Texture> getAlbedoOrDefault() async => albedo ?? await Texture.getDefaultAlbedo();
  Future<Texture> getNormalOrDefault() async => normal ?? await Texture.getDefaultNormal();
  Future<Texture> getMetallicOrDefault() async => metallic ?? await Texture.getDefaultMetallic();
  Future<Texture> getRoughnessOrDefault() async => roughness ?? await Texture.getDefaultRoughness();
  Future<Texture> getAOOrDefault() async => ao ?? await Texture.getDefaultAO();
}
