using OpenTK.Mathematics;

namespace Nebula;

public class Model {
    public Mesh mesh;
    public Material material;

    public void Draw(Matrix4 VPM) {
        material.Bind();



        mesh.Draw();
    }
}