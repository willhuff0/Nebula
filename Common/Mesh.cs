using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Nebula;

public class Mesh {
    public Vertex[] vertices;
    public uint[] indices;

    private int VAO, VBO, EBO;

    Mesh(Vertex[] vertices, uint[] indices) {
        this.vertices = vertices;
        this.indices = indices;

        initialize();
    }

    private void initialize() {
        VAO = GL.GenVertexArray();
        VBO = GL.GenBuffer();
        EBO = GL.GenBuffer();

        GL.BindVertexArray(VAO);

        GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * 8 * sizeof(float), vertices, BufferUsageHint.StaticDraw);
    
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);

        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));

        GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
    
        GL.BindVertexArray(0);
    }

    public void Draw() {
        GL.BindVertexArray(VAO);
        GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);
        GL.BindVertexArray(0);
    }
}

public struct Vertex {
    Vector3 Position;
    Vector3 Normal;
    Vector2 TexCoords;
}