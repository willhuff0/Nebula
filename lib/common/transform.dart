import 'package:nebula/serialization/serializable.dart';

import 'common.dart';

class Transform with Serializable {
  Vector3 _position;
  Quaternion _rotation;
  Vector3 _scale;

  Transform({Vector3? position, Quaternion? rotation, Vector3? scale})
      : _position = position ?? Vector3.zero(),
        _rotation = rotation ?? Quaternion.identity(),
        _scale = scale ?? Vector3(1, 1, 1);

  Vector3 get position => _position;
  set position(Vector3 value) => _position = value;

  Quaternion get rotation => _rotation;
  set rotation(Quaternion value) => _rotation = value;

  Vector3 get scale => _scale;
  set scale(Vector3 value) => _scale = value;

  Vector3 get eulerAngles => _rotation.rotate(Vector3(0, 0, 0));

  Matrix4 getMatrix() => Matrix4.identity()..setFromTranslationRotationScale(position, rotation, scale);

  @override
  Map<String, dynamic> serialize() => {
        'position': position.storage,
        'rotation': rotation.storage,
        'scale': scale.storage,
      };

  @override
  void deserialize(Map<String, dynamic> from) {
    position = Vector3.fromFloat32List(from['position']);
    rotation = Quaternion.fromFloat32List(from['rotation']);
    scale = Vector3.fromFloat32List(from['scale']);
  }
}
