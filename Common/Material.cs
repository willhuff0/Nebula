using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Nebula;

public class Material {
    public Shader shader;
    public Texture[] textures;

    public Material(Shader shader, Texture[] textures)
    {
        this.shader = shader;
        this.textures = textures;
    }

    public void Bind() {
        shader.Bind();
        
        for (int i = 0; i < textures.Length; i++) {
            textures[i].Bind(textureUnitLookup[i]);
            shader.SetUInt($"material.texture_{textures[i].type}", i);
        }
    }

    public void SetMatrices(Matrix4 transform, Matrix4 VPM) {
        shader.SetUMatrix4("matrix_transform", transform);
        shader.SetUMatrix4("matrix_viewProjection", VPM);
    }

    public void SetUniforms(Vector3 viewPos, int directionalLightCount, int pointLightCount) {
        shader.SetUVector3("viewPos", viewPos);
        shader.SetUInt("directionalLightCount", directionalLightCount);
        shader.SetUInt("pointLightCount", pointLightCount);
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
