using OpenTK.Mathematics;

namespace Nebula;

public class Model {
    public Mesh mesh;
    public Material material;
    public Transform transform;

    public Model(Mesh mesh, Material material, Transform transform)
    {
        this.mesh = mesh;
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

        mesh.Draw();
    }
}