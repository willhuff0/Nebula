using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Nebula;

public class Mesh {
    public float[] vertices;
    public uint[] indices;
    private int stride;
    public Texture[] textures;

    public PrimitiveType primitiveType;

    private int VAO, VBO, EBO;

    public Mesh(float[] vertices, uint[] indices, Texture[] textures, int stride = 8, PrimitiveType primitiveType = PrimitiveType.Triangles) {
        this.vertices = vertices;
        this.indices = indices;
        this.textures = textures;
        this.stride = stride;
        this.primitiveType = primitiveType;

        if (vertices != null) initialize();
    }

    public Mesh(Vertex[] vertices, uint[] indices, Texture[] textures, int stride = 8, PrimitiveType primitiveType = PrimitiveType.Triangles) {
        this.vertices = new float[vertices.Length * stride];
        for(int i = 0; i < vertices.Length; i++) {
            Vertex vertex = vertices[i];
            this.vertices[i * stride] = vertex.Position.X;
            this.vertices[i * stride + 1] = vertex.Position.Y;
            this.vertices[i * stride + 2] = vertex.Position.Z;

            if (stride > 3) {
                this.vertices[i * stride + 3] = vertex.Normal.X;
                this.vertices[i * stride + 4] = vertex.Normal.Y;
                this.vertices[i * stride + 5] = vertex.Normal.Z;

                if (stride > 6) {
                    this.vertices[i * stride + 6] = vertex.TexCoord.X;
                    this.vertices[i * stride + 7] = vertex.TexCoord.Y;
                }
            }
        }

        this.indices = indices;
        this.textures = textures;
        this.stride = stride;
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
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride * sizeof(float), 0);

        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride * sizeof(float), 3 * sizeof(float));

        GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, stride * sizeof(float), 6 * sizeof(float));
    
        GL.BindVertexArray(0);
    }

    public void BindTexturesAndDraw(Shader shader) {
        for (int i = 0; i < textures.Length; i++) {
            textures[i].Bind(textureUnitLookup[i]);
            shader.SetUInt($"material.texture_{textures[i].type}", i);
        }

        GL.BindVertexArray(VAO);
        if (indices == null) GL.DrawArrays(primitiveType, 0, vertices.Length);
        else GL.DrawElements(primitiveType, indices.Length, DrawElementsType.UnsignedInt, 0);
        GL.BindVertexArray(0);
    }

    private readonly Dictionary<int, TextureUnit> textureUnitLookup = new Dictionary<int, TextureUnit>() {
        { 0, TextureUnit.Texture0 },
        { 1, TextureUnit.Texture1 },
        { 2, TextureUnit.Texture2 },
        { 3, TextureUnit.Texture3 },
        { 4, TextureUnit.Texture4 },
        { 5, TextureUnit.Texture5 },
        { 6, TextureUnit.Texture6 },
        { 7, TextureUnit.Texture7 },
        { 8, TextureUnit.Texture8 },
        { 9, TextureUnit.Texture9 }
    };
}

public struct Vertex {
    public Vector3 Position;
    public Vector3 Normal;
    public Vector2 TexCoord;

    public Vertex(Vector3 position, Vector3 normal, Vector2 texCoord)
    {
        Position = position;
        Normal = normal;
        TexCoord = texCoord;
    }
}