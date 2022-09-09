using System.Collections.Generic;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace Nebula;

public class Texture {
    public readonly int handle;
    public string type;

    public static Dictionary<string, Texture> textureCache = new Dictionary<string, Texture>();

    public static Texture LoadFromFile(string path, string type) { // = "default") {
        if (textureCache.ContainsKey(path)) return new Texture(textureCache[path].handle, type);

        int handle = GL.GenTexture();

        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, handle);

        StbImage.stbi_set_flip_vertically_on_load(1);

        using (Stream stream = File.OpenRead(path)) {
            ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
        }

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

        Texture texture = new Texture(handle, type);
        textureCache.Add(path, texture);
        return texture;
    }

    public Texture(int handle, string type)
    {
        this.handle = handle;
        this.type = type;
    }

    public static Texture GetDefaultAlbedo() => Texture.LoadFromFile("Resources/default/default_albedo_ao.png", "albedo");
    public static Texture GetDefaultNormal() => Texture.LoadFromFile("Resources/default/default_normal.png", "normal");
    public static Texture GetDefaultMetallic() => Texture.LoadFromFile("Resources/default/default_metallic_roughness.png", "metallic");
    public static Texture GetDefaultRoughness() => Texture.LoadFromFile("Resources/default/default_metallic_roughness.png", "roughness");
    public static Texture GetDefaultAO() => Texture.LoadFromFile("Resources/default/default_albedo_ao.png", "ao");

    public void Bind(TextureUnit unit)
    {
        GL.ActiveTexture(unit);
        GL.BindTexture(TextureTarget.Texture2D, handle);
    }
}

public class DefaultTextures {
    public Texture albedo;
    public Texture normal;
    public Texture metallic;
    public Texture roughness;
    public Texture ao;

    public DefaultTextures(Texture albedo = null, Texture normal = null, Texture metallic = null, Texture roughness = null, Texture ao = null)
    {
        this.albedo = albedo;
        this.normal = normal;
        this.metallic = metallic;
        this.roughness = roughness;
        this.ao = ao;
    }

    public Texture GetAlbedoOrDefault() => albedo != null ? albedo : Texture.GetDefaultAlbedo();
    public Texture GetNormalOrDefault() => normal != null ? normal : Texture.GetDefaultNormal();
    public Texture GetMetallicOrDefault() => metallic != null ? metallic : Texture.GetDefaultMetallic();
    public Texture GetRoughnessOrDefault() => roughness != null ? roughness : Texture.GetDefaultRoughness();
    public Texture GetAOOrDefault() => ao != null ? ao : Texture.GetDefaultAO();
}