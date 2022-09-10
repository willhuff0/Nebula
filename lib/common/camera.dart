import 'dart:math';

import 'package:nebula/common/common.dart';

class Camera {
  static const PI_OVER_2 = pi / 2;

  static const NEAR_CLIP = 0.1;
  static const FAR_CLIP = 100.0;

  Vector3 _forward = Vector3(0, -1, 0);
  Vector3 _up = Vector3(0, 1, 0);
  Vector3 _right = Vector3(1, 0, 0);

  double _pitch = 0.0;
  double _yaw = -PI_OVER_2;

  double _fov = PI_OVER_2;

  Camera(this.position, this.aspectRatio);

  Vector3 position;
  double aspectRatio;

  Vector3 get forward => _forward;
  Vector3 get up => _up;
  Vector3 get right => _right;

  double get pitch => degrees(_pitch);
  set pitch(double value) {
    _pitch = degrees(value.clamp(-89.9, 89.9));
    _updateVectors();
  }

  double get yaw => degrees(_yaw);
  set yaw(double value) {
    _yaw = degrees(value);
    _updateVectors();
  }

  double get fov => degrees(_fov);
  set fov(double value) => _fov = degrees(value.clamp(1.0, 179.0));

  Matrix4 getViewMatrix() => makeViewMatrix(position, position + _forward, _up);
  Matrix4 getProjectionMatrix() => makePerspectiveMatrix(_fov, aspectRatio, NEAR_CLIP, FAR_CLIP);
  Matrix4 getViewProjectionMatrix() => getProjectionMatrix() * getViewMatrix();

  void _updateVectors() {
    _forward.x = cos(_pitch) * cos(_yaw);
    _forward.y = sin(_pitch);
    _forward.z = cos(_pitch) * sin(_yaw);
    _forward.normalize();

    cross3(_forward, Vector3(0, 1, 0), _right);
    _right.normalize();
    cross3(_right, _forward, _up);
    _up.normalize();
  }
}
