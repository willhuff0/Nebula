import 'dart:ffi';

import 'package:ffi/ffi.dart';
import 'package:nebula/common/common.dart';
import 'package:nebula/nebula.dart';

abstract class Light {
  static const DEFAULT_SHADOWMAP_WIDTH = 2048;
  static const DEFAULT_SHADOWMAP_HEIGHT = 2048;

  static late Shader shadowMapShader;

  void addToShader(Shader shader, int index);
  int drawShadowMap();
}

class PointLight extends Light {
  Vector3 position;
  Vector3 color;
  double intensity;

  PointLight(this.position, this.color, [this.intensity = 1.0]);

  @override
  void addToShader(Shader shader, int index) {
    shader.setUVector3('nebula_pointLights[$index].position', position);
    shader.setUVector3('nebula_pointLights[$index].color', color);
    shader.setUFloat('nebula_pointLights[$index].intensity', intensity);
  }

  @override
  int drawShadowMap() {
    return -1;
  }
}

class DirectionalLight extends Light {
  Vector3 direction;
  Vector3 color;
  double intensity;

  int shadowMapWidth;
  int shadowMapHeight;

  Matrix4? shadowMatrix;
  late int _depthMapFBO;
  late int _depthMap;

  DirectionalLight(this.direction, this.color, [this.intensity = 1.0, this.shadowMapWidth = Light.DEFAULT_SHADOWMAP_WIDTH, this.shadowMapHeight = Light.DEFAULT_SHADOWMAP_HEIGHT]) {
    using((arena) {
      final _depthMapFBOPointer = arena<UnsignedInt>();
      gl.glGenFramebuffers(1, _depthMapFBOPointer);
      _depthMapFBO = _depthMapFBOPointer.value;
    }, malloc);

    using((arena) {
      final _depthMapPointer = arena<UnsignedInt>();
      gl.glGenTextures(1, _depthMapPointer);
      _depthMap = _depthMapPointer.value;
    }, malloc);

    gl.glBindTexture(GL_TEXTURE_2D, _depthMap);
    gl.glTexImage2D(GL_TEXTURE_2D, 0, GL_DEPTH_COMPONENT16, shadowMapWidth, shadowMapHeight, 0, GL_DEPTH_COMPONENT, GL_FLOAT, nullptr);
    gl.glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
    gl.glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
    gl.glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_BORDER);
    gl.glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_BORDER);
    using((arena) => gl.glTexParameterfv(GL_TEXTURE_2D, GL_TEXTURE_BORDER_COLOR, malloc<Float>()..asTypedList(4).setAll(0, [1.0, 1.0, 1.0, 1.0])), malloc);

    gl.glBindFramebuffer(GL_FRAMEBUFFER, _depthMapFBO);
    gl.glFramebufferTexture2D(GL_FRAMEBUFFER, GL_DEPTH_ATTACHMENT, GL_TEXTURE_2D, _depthMap, 0);
    using((arena) => gl.glDrawBuffers(1, malloc<UnsignedInt>()..elementAt(0).value = GL_NONE), malloc);
    gl.glReadBuffer(GL_NONE);
    gl.glBindFramebuffer(GL_FRAMEBUFFER, 0);
  }

  @override
  void addToShader(Shader shader, int index) {
    shader.setUVector3('nebula_directionalLights[$index].direction', -direction);
    shader.setUVector3('nebula_directionalLights[$index].color', color);
    shader.setUFloat('nebula_directionalLights[$index].intensity', intensity);

    shader.setUInt('nebula_shadowCasterCount', 1);
    gl.glActiveTexture(GL_TEXTURE10);
    gl.glBindTexture(GL_TEXTURE_2D, _depthMap);
    shader.setUInt('nebula_shadowMaps[$index]', 10);
    shader.setUMatrix4('nebula_shadowMatrices[$index]', shadowMatrix!);
  }

  @override
  int drawShadowMap() {
    Matrix4 projection = makeOrthographicMatrix(-15.0, 15.0, -15.0, 15.0, 0.2, 30.0);
    Matrix4 view = makeViewMatrix(-direction * 15.0, Vector3.zero(), Vector3(0, 1, 0));
    shadowMatrix = view * projection;
    Light.shadowMapShader.bind();
    Light.shadowMapShader.setUMatrix4('nebula_matrix_viewProjection', shadowMatrix!);

    gl.glViewport(0, 0, shadowMapWidth, shadowMapHeight);
    gl.glBindFramebuffer(GL_FRAMEBUFFER, _depthMapFBO);
    gl.glClear(GL_DEPTH_BUFFER_BIT);

    //Render();

    gl.glBindFramebuffer(GL_FRAMEBUFFER, 0);
    return _depthMap;
  }
}
