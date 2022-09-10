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

  static Texture loadFromFile(String path, String type) {
    if (textureCache.containsKey(path)) return Texture(textureCache[path]!.handle, type);

    int handle = 0;
    using((arena) {
      final textures = arena<UnsignedInt>();
      gl.glGenTextures(1, textures);
      handle = textures.elementAt(0).value;
    });

    gl.glActiveTexture(GL_TEXTURE0);
    gl.glBindTexture(GL_TEXTURE_2D, handle);

    final image = decodeImage(File(path).readAsBytesSync())!;
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

  static Texture getDefaultAlbedo() => Texture.loadFromFile('Resources/default/default_albedo.png', 'albedo');
  static Texture getDefaultNormal() => Texture.loadFromFile('Resources/default/default_normal.png', 'normal');
  static Texture getDefaultMetallic() => Texture.loadFromFile('Resources/default/default_metallic_roughness.png', 'metallic');
  static Texture getDefaultRoughness() => Texture.loadFromFile('Resources/default/default_metallic_roughness.png', 'roughness');
  static Texture getDefaultAO() => Texture.loadFromFile('Resources/default/default_ao.png', 'ao');

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

  Texture getAlbedoOrDefault() => albedo ?? Texture.getDefaultAlbedo();
  Texture getNormalOrDefault() => normal ?? Texture.getDefaultNormal();
  Texture getMetallicOrDefault() => metallic ?? Texture.getDefaultMetallic();
  Texture getRoughnessOrDefault() => roughness ?? Texture.getDefaultRoughness();
  Texture getAOOrDefault() => ao ?? Texture.getDefaultAO();
}
