import 'package:nebula/nebula.dart';

import 'common.dart';

class Material {
  late final List<Mesh> meshes;
  late final List<Texture> textures;

  Material(this.meshes, this.textures);

  void bindTexturesAndDraw(Shader shader) {
    for (int i = 0; i < textures.length; i++) {
      textures[i].bind(textureUnitLookup[i]!);
      shader.setUInt('nebula_material.texture_${textures[i].type}', i);
    }

    draw();
  }

  void draw() {
    for (final mesh in meshes) {
      mesh.draw();
    }
  }
}

const textureUnitLookup = {
  0: GL_TEXTURE0,
  1: GL_TEXTURE1,
  2: GL_TEXTURE2,
  3: GL_TEXTURE3,
  4: GL_TEXTURE4,
  5: GL_TEXTURE5,
  6: GL_TEXTURE6,
  7: GL_TEXTURE7,
  8: GL_TEXTURE8,
  9: GL_TEXTURE9,
};
