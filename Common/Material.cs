using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;

namespace Nebula;

public class Material {
    public Mesh[] meshes;
    public Texture[] textures;

    public Material(Mesh[] meshes, Texture[] textures) {
        this.meshes = meshes;
        this.textures = textures;
    }

    public void BindTexturesAndDraw(Shader shader) {
        for (int i = 0; i < textures.Length; i++) {
            textures[i].Bind(textureUnitLookup[i]);
            shader.SetUInt($"material.texture_{textures[i].type}", i);
        }

        foreach(Mesh mesh in meshes) mesh.Draw();
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