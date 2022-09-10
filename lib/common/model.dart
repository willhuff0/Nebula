import 'dart:ffi';
import 'dart:typed_data';

import 'package:ffi/ffi.dart';
import 'package:nebula/bindings/assimp/assimp.dart';
import 'package:nebula/nebula.dart';

import 'common.dart';

import 'package:path/path.dart' as p;

class Model {
  final List<Material> materials;
  final Shader shader;
  late final Transform transform;

  Model({required this.materials, required this.shader, Transform? transform}) : transform = transform ?? Transform();

  void draw(MeshStandardUniforms uniforms, [List<Light>? lights]) {
    shader.bind();

    if (lights != null) {
      int directionalLightCount = 0;
      int pointLightCount = 0;
      for (int i = 0; i < lights.length; i++) {
        Light light = lights[i];
        switch (light.runtimeType) {
          case DirectionalLight:
            light.addToShader(shader, directionalLightCount);
            directionalLightCount++;
            break;
          case PointLight:
            light.addToShader(shader, pointLightCount);
            pointLightCount++;
            break;
        }
      }
      uniforms.directionalLightCount = directionalLightCount;
      uniforms.pointLightCount = pointLightCount;
    }

    uniforms.transform = transform.getMatrix();
    shader.setStandardMeshUniforms(uniforms);

    for (final material in materials) {
      material.bindTexturesAndDraw(shader);
    }
  }

  void drawForShadowMaps() {
    Light.shadowMapShader.setUMatrix4('nebula_matrix_transform', transform.getMatrix());
    for (final material in materials) {
      material.draw();
    }
  }

  static Model? load(String path, Shader shader, [double scale = 1.0, DefaultTextures? defaultTextures]) {
    defaultTextures ??= DefaultTextures();
    print('t100');
    final scenePointer = using((arena) => assimp.aiImportFile(path.toNativeUtf8(allocator: arena).cast(), aiPostProcessSteps.aiProcess_Triangulate | aiPostProcessSteps.aiProcess_GenNormals | aiPostProcessSteps.aiProcess_GenUVCoords | aiPostProcessSteps.aiProcess_TransformUVCoords | aiPostProcessSteps.aiProcess_PreTransformVertices), malloc);
    if (scenePointer == nullptr) return null;
    final scene = scenePointer.ref;

    /*
      Meshes
    */

    final meshes = <Mesh>[];
    for (int i = 0; i < scene.mNumMeshes; i++) {
      final assimpMesh = scene.mMeshes.elementAt(i).value.ref;

      final positions = List.generate(assimpMesh.mNumVertices, (index) => assimpMesh.mVertices[index].toDartVector3());
      final normals = List.generate(assimpMesh.mNumVertices, (index) => assimpMesh.mNormals[index].toDartVector3());
      final _textureCoords = assimpMesh.mTextureCoords[0];
      final uvs = _textureCoords == nullptr ? List.filled(assimpMesh.mNumVertices, Vector2(0.0, 0.0)) : List.generate(assimpMesh.mNumVertices, (index) => _textureCoords[index].toDartVector2());
      final indices = List.generate(assimpMesh.mNumFaces, (index) => assimpMesh.mFaces[0]).expand((face) => List.generate(face.mNumIndices, (index) => face.mIndices[index])).toList();

      meshes.add(Mesh(Mesh.makeVerticesArrayFromComponents(positions.length, positions, normals, uvs), indices: Uint32List.fromList(indices), materialIndex: assimpMesh.mMaterialIndex));
    }

    /*
      Materials
    */

    final directory = p.dirname(path);
    final materials = <Material>[];
    for (int i = 0; i < scene.mNumMaterials; i++) {
      final assimpMaterial = scene.mMaterials[i];
      using((arena) {
        final texturePointers = List.generate(5, (index) => arena<aiString>());
        assimp.aiGetMaterialTextureCount(assimpMaterial, aiTextureType.aiTextureType_DIFFUSE) > 0 ? assimp.aiGetMaterialTexture(assimpMaterial, aiTextureType.aiTextureType_DIFFUSE, 0, texturePointers[0], nullptr, nullptr, nullptr, nullptr, nullptr, nullptr) : Texture.getDefaultAlbedo();
        assimp.aiGetMaterialTextureCount(assimpMaterial, aiTextureType.aiTextureType_NORMALS) > 0 ? assimp.aiGetMaterialTexture(assimpMaterial, aiTextureType.aiTextureType_NORMALS, 0, texturePointers[0], nullptr, nullptr, nullptr, nullptr, nullptr, nullptr) : Texture.getDefaultNormal();
        assimp.aiGetMaterialTextureCount(assimpMaterial, aiTextureType.aiTextureType_METALNESS) > 0 ? assimp.aiGetMaterialTexture(assimpMaterial, aiTextureType.aiTextureType_METALNESS, 0, texturePointers[0], nullptr, nullptr, nullptr, nullptr, nullptr, nullptr) : Texture.getDefaultMetallic();
        assimp.aiGetMaterialTextureCount(assimpMaterial, aiTextureType.aiTextureType_DIFFUSE_ROUGHNESS) > 0 ? assimp.aiGetMaterialTexture(assimpMaterial, aiTextureType.aiTextureType_DIFFUSE_ROUGHNESS, 0, texturePointers[0], nullptr, nullptr, nullptr, nullptr, nullptr, nullptr) : Texture.getDefaultRoughness();
        assimp.aiGetMaterialTextureCount(assimpMaterial, aiTextureType.aiTextureType_AMBIENT_OCCLUSION) > 0 ? assimp.aiGetMaterialTexture(assimpMaterial, aiTextureType.aiTextureType_AMBIENT_OCCLUSION, 0, texturePointers[0], nullptr, nullptr, nullptr, nullptr, nullptr, nullptr) : Texture.getDefaultAO();
        final textures = [
          Texture.loadFromFile(p.join(directory, texturePointers[0].ref.toDartString()), 'albedo'),
          Texture.loadFromFile(p.join(directory, texturePointers[1].ref.toDartString()), 'normal'),
          Texture.loadFromFile(p.join(directory, texturePointers[2].ref.toDartString()), 'metallic'),
          Texture.loadFromFile(p.join(directory, texturePointers[3].ref.toDartString()), 'roughness'),
          Texture.loadFromFile(p.join(directory, texturePointers[4].ref.toDartString()), 'ao'),
        ];
        final materialMeshes = meshes.where((e) => e.materialIndex == i).toList();
        materials.add(Material(materialMeshes, textures));
      });
    }

    return Model(materials: materials, shader: shader, transform: Transform(scale: Vector3(scale, scale, scale)));
  }
}

extension AssimpVector3DConversion on aiVector3D {
  Vector3 toDartVector3() => Vector3(x, y, z);
  Vector2 toDartVector2() => Vector2(x, y);
}

extension AssimpVector2DConversion on aiVector2D {
  Vector2 toDartVector2() => Vector2(x, y);
}

extension AssimpStringConversion on aiString {
  String toDartString() => String.fromCharCodes(List.generate(length, (index) => data[index]));
}
