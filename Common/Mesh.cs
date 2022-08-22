using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using static glTFLoader.Schema.MeshPrimitive;

namespace Nebula;

public class Mesh {
    public float[] vertices;
    public uint[] indices;

    public PrimitiveType primitiveType;

    private int VAO, VBO, EBO;

    public Mesh(float[] vertices = null, uint[] indices = null, PrimitiveType primitiveType = PrimitiveType.Triangles) {
        this.vertices = vertices;
        this.indices = indices;
        this.primitiveType = primitiveType;

        if (vertices != null) initialize();
    }

    public Mesh(Vertex[] vertices = null, uint[] indices = null, PrimitiveType primitiveType = PrimitiveType.Triangles) {
        this.vertices = new float[vertices.Length * 8];
        for(int i = 0; i < vertices.Length; i++) {
            Vertex vertex = vertices[i];
            this.vertices[i * 8] = vertex.Position.X;
            this.vertices[i * 8 + 1] = vertex.Position.Y;
            this.vertices[i * 8 + 2] = vertex.Position.Z;

            this.vertices[i * 8 + 3] = vertex.Normal.X;
            this.vertices[i * 8 + 4] = vertex.Normal.Y;
            this.vertices[i * 8 + 5] = vertex.Normal.Z;

            this.vertices[i * 8 + 6] = vertex.TexCoord.X;
            this.vertices[i * 8 + 7] = vertex.TexCoord.Y;
        }

        this.indices = indices;
        this.primitiveType = primitiveType;

        if (vertices != null) initialize();
    }

    private void initialize() {
        VAO = GL.GenVertexArray();
        VBO = GL.GenBuffer();
        EBO = GL.GenBuffer();

        GL.BindVertexArray(VAO);

        GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
    
        if (indices != null) {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
        }

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
        if (indices == null) GL.DrawArrays(primitiveType, 0, vertices.Length);
        else GL.DrawElements(primitiveType, indices.Length, DrawElementsType.UnsignedInt, 0);
        GL.BindVertexArray(0);
    }
}

public struct Vertex {
    public Vector3 Position;
    public Vector3 Normal;
    public Vector2 TexCoord;
}