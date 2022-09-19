import 'dart:ffi';
import 'dart:typed_data';

import 'package:ffi/ffi.dart';
import 'package:nebula/nebula.dart';
import 'package:vector_math/vector_math.dart';

class Mesh {
  final Float32List vertices;
  final Uint32List? indices;
  final int stide;
  final int materialIndex;

  final PrimitiveType primitiveType;

  late int _vao, _vbo, _ebo;

  Mesh(this.vertices, {this.indices, this.materialIndex = -1, this.stide = 8, this.primitiveType = PrimitiveType.triangles}) {
    using((arena) {
      final arrays = arena<UnsignedInt>();
      gl.glGenVertexArrays(1, arrays);
      _vao = arrays[0];

      final buffers = arena<UnsignedInt>(2);
      gl.glGenBuffers(2, buffers);
      _vbo = buffers[0];
      _ebo = buffers[1];

      gl.glBindVertexArray(_vao);

      print(gl.glGetError());

      gl.glBindBuffer(GL_ARRAY_BUFFER, _vbo);
      gl.glBufferData(GL_ARRAY_BUFFER, vertices.length * 4, (arena<Float>(vertices.length)..asTypedList(vertices.length).setAll(0, vertices)).cast(), GL_STATIC_DRAW);

      print(gl.glGetError());

      if (indices != null) {
        gl.glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, _ebo);
        gl.glBufferData(GL_ELEMENT_ARRAY_BUFFER, indices!.length * 4, (arena<Uint32>(indices!.length)..asTypedList(indices!.length).setAll(0, indices!)).cast(), GL_STATIC_DRAW);
      }

      print(gl.glGetError());

      gl.glEnableVertexAttribArray(0);
      gl.glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, stide * 4, (arena<Int>()..value = 0).cast());
      print(gl.glGetError());

      gl.glEnableVertexAttribArray(1);
      gl.glVertexAttribPointer(1, 3, GL_FLOAT, GL_FALSE, stide * 4, (arena<Int>()..value = 3 * 4).cast());
      print(gl.glGetError());

      gl.glEnableVertexAttribArray(2);
      gl.glVertexAttribPointer(2, 3, GL_FLOAT, GL_FALSE, stide * 4, (arena<Int>()..value = 6 * 4).cast());
      print(gl.glGetError());

      gl.glBindVertexArray(0);
      print(gl.glGetError());
    }, malloc);
  }

  static Float32List makeVerticesArrayFromComponents(int count, List<Vector3> positions, List<Vector3> normals, List<Vector2> uvs) => Float32List.fromList(List<dynamic>.generate(count, (index) => index).expand((index) => [...positions[index].storage, ...normals[index].storage, ...uvs[index].storage]).toList());

  void draw() {
    gl.glBindVertexArray(_vao);
    if (indices == null) {
      gl.glDrawArrays(primitiveType.glMode, 0, vertices.length);
    } else {
      gl.glDrawElements(GL_TRIANGLES, indices!.length, GL_UNSIGNED_INT, nullptr);
    }
    print(gl.glGetError());
    gl.glBindVertexArray(0);
  }
}

enum PrimitiveType {
  points(GL_POINTS),
  lines(GL_LINES),
  lineLoop(GL_LINE_LOOP),
  lineStrip(GL_LINE_STRIP),
  triangles(GL_TRIANGLES),
  triangleStrip(GL_TRIANGLE_STRIP),
  triangleFan(GL_TRIANGLE_FAN),
  quads(GL_QUADS);

  const PrimitiveType(this.glMode);
  final int glMode;
}
