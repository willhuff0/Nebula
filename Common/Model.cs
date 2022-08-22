using System.Collections.Generic;
using OpenTK.Mathematics;

namespace Nebula;

public class Model {
    public Mesh[] meshes;
    public Material material;
    public Transform transform;

    public Model(Mesh[] meshes, Material material, Transform transform)
    {
        this.meshes = meshes;
        this.material = material;
        this.transform = transform;
    }

    public void Draw(Matrix4 VPM, Vector3 viewPos, Light[] lights) {
        material.Bind();
        material.SetMatrices(transform.GetMatrix(), VPM);
        
        int directionalLightCount = 0;
        int pointLightCount = 0;
        for(int i = 0; i < lights.Length; i++) {
            Light light = lights[i];
            light.AddToShader(material.shader, i);

            switch(light) {
                case DirectionalLight directionalLight:
                    directionalLightCount++;
                    break;
                case PointLight pointLight:
                    pointLightCount++;
                    break;
            }
        }
        material.SetUniforms(viewPos, directionalLightCount, pointLightCount);

        foreach(Mesh mesh in meshes) mesh.Draw();
    }

    public static Model Load (string path) {
        Assimp.AssimpContext importer = new Assimp.AssimpContext();
        Assimp.Scene scene = importer.ImportFile(path);

        List<Mesh> meshes = new List<Mesh>();

        if (scene == null || scene.RootNode == null) return null;
        _Load_ProcessNode(scene.RootNode, scene, ref meshes);
    }

    public static void _Load_ProcessNode(Assimp.Node node, Assimp.Scene scene, ref List<Mesh> meshes) {
        for (int i = 0; i < node.MeshCount; i++) meshes.Add(_Load_ProcessMesh(scene.Meshes[node.MeshIndices[i]], scene));
        for (int i = 0; i < node.ChildCount; i++) _Load_ProcessNode(node.Children[i], scene, ref meshes);
    }

    public static Mesh _Load_ProcessMesh(Assimp.Mesh mesh, Assimp.Scene scene) {
        Vertex[] vertices = new Vertex[mesh.VertexCount];
        uint[] indices = mesh.GetUnsignedIndices();

        for(int i = 0; i < mesh.VertexCount; i++) {
            Assimp.Vector3D position = mesh.Vertices[i];
            Assimp.Vector3D normal = mesh.Normals[i];

            vertices[i] = new Vertex(new Vector3(position.X, position.Y, position.Z), new Vector3(normal.X, normal.Y, normal.Z));
        }
    }
}