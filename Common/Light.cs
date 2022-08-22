using OpenTK.Mathematics;

namespace Nebula;

public abstract class Light {
    public abstract void AddToShader(Shader shader, int index);
}

public class PointLight : Light {
    public Vector3 position;
    public Vector3 color;
    public float intensity;

    public PointLight(Vector3 position, Vector3 color, float intensity)
    {
        this.position = position;
        this.color = color;
        this.intensity = intensity;
    }

    public override void AddToShader(Shader shader, int index) {
        shader.SetUVector3($"pointLights[{index}].position", position);
        shader.SetUVector3($"pointLights[{index}].color", color);
        shader.SetUFloat($"pointLights[{index}].intensity", intensity);
    }
}

public class DirectionalLight : Light {
    public Vector3 direction;
    public Vector3 color;
    public float intensity;

    public DirectionalLight(Vector3 direction, Vector3 color, float intensity)
    {
        this.direction = direction;
        this.color = color;
        this.intensity = intensity;
    }

    public override void AddToShader(Shader shader, int index) {
        shader.SetUVector3($"directionalLights[{index}].direction", direction);
        shader.SetUVector3($"directionalLights[{index}].color", color);
        shader.SetUFloat($"directionalLights[{index}].intensity", intensity);
    }
}