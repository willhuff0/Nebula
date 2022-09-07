import 'common.dart';

class Model {
  late final List<Material> materials;
  late final Shader shader;
  late final Transform transform;

  Model({required this.materials, required this.shader, this.transform = Transform()});

  void draw(MeshStandardUniforms uniforms, Light[] lights) {
    shader.bind();

    int directionalLightCount = -1;
    int pointLightCount = -1;

    

    shader.setStandardMeshUniforms(uniforms);
  }
}
