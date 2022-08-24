using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Assimp;
using OpenTK.Mathematics;

namespace Nebula;

public class Model {
    public Material[] materials;
    public Shader shader;
    public Transform transform;

    public Model(Material[] materials, Shader shader)
    {
        this.materials = materials;
        this.shader = shader;
        this.transform = new Transform();
    }
    public Model(Material[] materials, Shader shader, Transform transform)
    {
        this.materials = materials;
        this.shader = shader;
        this.transform = transform;
    }

    public void Draw(Matrix4? VPM = null, Vector3? viewPos = null, Light[] lights = null) {
        shader.Bind();
        shader.SetUsualMatrices(transform.GetMatrix(), VPM);
        
        int directionalLightCount = -1;
        int pointLightCount = -1;
        if (lights != null) {
            directionalLightCount = 0;
            pointLightCount = 0;
            for(int i = 0; i < lights.Length; i++) {
                Light light = lights[i];

                switch(light) {
                    case DirectionalLight directionalLight:
                        light.AddToShader(shader, directionalLightCount);
                        directionalLightCount++;
                        break;
                    case PointLight pointLight:
                        light.AddToShader(shader, pointLightCount);
                        pointLightCount++;
                        break;
                }
            }
        }
        shader.StandardMaterialSetUniforms(viewPos, directionalLightCount, pointLightCount);

        foreach(Material material in materials) material.BindTexturesAndDraw(shader);
    }

    public void DrawForShadowMaps() {
        Light.ShadowMapShader.Bind();
        shader.SetUMatrix4("matrix_transform", transform.GetMatrix());
        foreach(Material material in materials) material.Draw();
    }

    public static Model Load (string path, Shader shader, float scale = 1.0f, DefaultTextures defaultTextures = null) {
        if (defaultTextures == null) defaultTextures = new DefaultTextures();
        Assimp.AssimpContext importer = new Assimp.AssimpContext();
        Assimp.Scene scene = importer.ImportFile(path, Assimp.PostProcessSteps.Triangulate | Assimp.PostProcessSteps.GenerateNormals | Assimp.PostProcessSteps.GenerateUVCoords | Assimp.PostProcessSteps.TransformUVCoords | Assimp.PostProcessSteps.PreTransformVertices);
        if (scene == null) return null;

        Mesh[] meshes = new Mesh[scene.MeshCount];
        for (int i = 0; i < scene.MeshCount; i++) {
            Assimp.Mesh assimpMesh = scene.Meshes[i];
            Vector3[] positions = assimpMesh.Vertices.Select((e) => new Vector3(e.X, e.Y, e.Z)).ToArray();
            Vector3[] normals = assimpMesh.Normals.Select((e) => new Vector3(e.X, e.Y, e.Z)).ToArray();
            Vector2[] uvs = assimpMesh.TextureCoordinateChannels[0].Select((e) => new Vector2(e.X, e.Y)).ToArray();
            uint[] indicies = assimpMesh.GetUnsignedIndices();

            Debug.WriteLine($"Loaded {positions.Length} vertex positions");
            Debug.WriteLine($"Loaded {normals.Length} vertex normals");
            Debug.WriteLine($"Loaded {uvs.Length} vertex uvs");
            
            meshes[i] = new Mesh(Vertex.CreateVertexArrayFromComponents(positions.Length, positions, normals, uvs), indicies, assimpMesh.MaterialIndex);
        }

        string directory = Path.GetDirectoryName(path);
        Material[] materials = new Material[scene.MaterialCount];
        for (int i = 0; i < scene.MaterialCount; i++) {
            Assimp.Material assimpMaterial = scene.Materials[i];
            bool hasAlbedo = assimpMaterial.GetMaterialTexture(Assimp.TextureType.Diffuse, 0, out TextureSlot albedo);
            bool hasNormal = assimpMaterial.GetMaterialTexture(Assimp.TextureType.Normals, 0, out TextureSlot normal);
            bool hasMetallic = assimpMaterial.GetMaterialTexture(Assimp.TextureType.Metalness, 0, out TextureSlot metallic);
            bool hasRoughness = assimpMaterial.GetMaterialTexture(Assimp.TextureType.Roughness, 0, out TextureSlot roughness);
            bool hasAO = assimpMaterial.GetMaterialTexture(Assimp.TextureType.AmbientOcclusion, 0, out TextureSlot ao);
            Texture[] textures = new Texture[] { 
                hasAlbedo ? Texture.LoadFromFile(Path.Combine(directory, albedo.FilePath), "albedo") : defaultTextures.GetAlbedoOrDefault(),
                hasNormal ? Texture.LoadFromFile(Path.Combine(directory, normal.FilePath), "normal") : defaultTextures.GetNormalOrDefault(), 
                hasMetallic ? Texture.LoadFromFile(Path.Combine(directory, metallic.FilePath), "metallic") : defaultTextures.GetMetallicOrDefault(), 
                hasRoughness ? Texture.LoadFromFile(Path.Combine(directory, roughness.FilePath), "roughness") : defaultTextures.GetRoughnessOrDefault(), 
                hasAO ? Texture.LoadFromFile(Path.Combine(directory, ao.FilePath), "ao") : defaultTextures.GetAOOrDefault(), 
            };
            Mesh[] materialMeshes = Array.FindAll(meshes, (e) => e.materialIndex == i);
            materials[i] = new Material(materialMeshes, textures);
        }

        return new Model(materials, shader, new Transform(Vector3.Zero, new Vector3(scale)));
    }
}