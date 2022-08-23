using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using OpenTK.Mathematics;

namespace Nebula;

public class Model {
    public Mesh[] meshes;
    public Shader shader;
    public Transform transform;

    public Model(Mesh[] meshes, Shader shader)
    {
        this.meshes = meshes;
        this.shader = shader;
        this.transform = new Transform();
    }
    public Model(Mesh[] meshes, Shader shader, Transform transform)
    {
        this.meshes = meshes;
        this.shader = shader;
        this.transform = transform;
    }

    public void Draw(Matrix4 VPM, Vector3 viewPos, Light[] lights) {
        shader.Bind();
        shader.MaterialSetMatrices(transform.GetMatrix(), VPM);
        
        int directionalLightCount = 0;
        int pointLightCount = 0;
        for(int i = 0; i < lights.Length; i++) {
            Light light = lights[i];
            light.AddToShader(shader, i);

            switch(light) {
                case DirectionalLight directionalLight:
                    directionalLightCount++;
                    break;
                case PointLight pointLight:
                    pointLightCount++;
                    break;
            }
        }
        shader.MaterialSetUniforms(viewPos, directionalLightCount, pointLightCount);

        foreach(Mesh mesh in meshes) mesh.BindTexturesAndDraw(shader);
    }

    public static Model Load (string path, Shader shader, float scale = 1.0f) {
        Assimp.AssimpContext importer = new Assimp.AssimpContext();
        Assimp.Scene scene = importer.ImportFile(path, Assimp.PostProcessSteps.Triangulate);

        List<Mesh> meshes = new List<Mesh>();

        if (scene == null || scene.RootNode == null) return null;
        _Load_ProcessNode(scene.RootNode, scene, Path.GetDirectoryName(path), ref meshes);

        return new Model(meshes.ToArray(), shader, new Transform(Vector3.Zero, new Vector3(scale)));
    }

    private static void _Load_ProcessNode(Assimp.Node node, Assimp.Scene scene, string workingDirectory, ref List<Mesh> meshes) {
        for (int i = 0; i < node.MeshCount; i++) meshes.Add(_Load_ProcessMesh(scene.Meshes[node.MeshIndices[i]], scene, workingDirectory));
        for (int i = 0; i < node.ChildCount; i++) _Load_ProcessNode(node.Children[i], scene, workingDirectory, ref meshes);
    }

    private static Mesh _Load_ProcessMesh(Assimp.Mesh mesh, Assimp.Scene scene, string workingDirectory) {
        Vertex[] vertices = new Vertex[mesh.VertexCount];
        List<uint> indices = new List<uint>();

        for(int i = 0; i < mesh.VertexCount; i++) {
            Assimp.Vector3D position = mesh.Vertices[i];
            Assimp.Vector3D normal = mesh.Normals[i];
            Assimp.Vector3D texCoords;

            if (mesh.HasTextureCoords(0)) texCoords = mesh.TextureCoordinateChannels[0][i];
            else texCoords = new Assimp.Vector3D(0.0f, 0.0f, 0.0f);

            vertices[i] = new Vertex(new Vector3(position.X, position.Y, position.Z), new Vector3(normal.X, normal.Y, normal.Z), new Vector2(texCoords.X, texCoords.Y));
        }

        for(int i = 0; i< mesh.FaceCount; i++) {
            Assimp.Face face = mesh.Faces[i];
            for(int j = 0; j < face.IndexCount; j++) indices.Add((uint)face.Indices[j]);
        }

        List<Texture> _textures = new List<Texture>();
        if (mesh.MaterialIndex >= 0) {
            Assimp.Material material = scene.Materials[mesh.MaterialIndex];
            Assimp.TextureSlot[] assimpTextures = material.GetAllMaterialTextures();
            for(int i = 0; i < assimpTextures.Length; i++) {
                Assimp.TextureSlot assimpTexture = assimpTextures[i];
                string type;
                if (!assimpTextureTypeLookup.TryGetValue(assimpTexture.TextureType, out type)) continue;
                _textures.Add(Texture.LoadFromFile(Path.Join(workingDirectory, assimpTexture.FilePath), type));
            }
        }

        Debug.WriteLine($"Mesh {mesh.Name} loaded {_textures.Count} textures");
        return new Mesh(vertices, indices.ToArray(), _textures.ToArray());
    }

    private static readonly Dictionary<Assimp.TextureType, string> assimpTextureTypeLookup = new Dictionary<Assimp.TextureType, string>() {
        { Assimp.TextureType.Diffuse, "albedo" },
        { Assimp.TextureType.Normals, "normal" },
        { Assimp.TextureType.Metalness, "metallic" },
        { Assimp.TextureType.Roughness, "roughness" },
        { Assimp.TextureType.AmbientOcclusion, "ao" },
    };
}