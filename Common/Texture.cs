using System.Collections.Generic;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace Nebula;

public class Texture {
    public readonly int handle;
    public string type;

    public static Dictionary<string, Texture> textureCache = new Dictionary<string, Texture>();

    public static Texture LoadFromFile(string path, string type = "default") {
        if (textureCache.ContainsKey(path)) return textureCache[path];

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

    public static Texture GetDefaultAlbedo() => Texture.LoadFromFile("Resources/default/default_albedo.png");
    public static Texture GetDefaultNormal() => Texture.LoadFromFile("Resources/default/default_normal.png");
    public static Texture GetDefaultMetallic() => Texture.LoadFromFile("Resources/default/default_metallic.png");
    public static Texture GetDefaultRoughness() => Texture.LoadFromFile("Resources/default/default_roughness.png");
    public static Texture GetDefaultAO() => Texture.LoadFromFile("Resources/default/default_ao.png");

    public void Bind(TextureUnit unit)
    {
        GL.ActiveTexture(unit);
        GL.BindTexture(TextureTarget.Texture2D, handle);
    }
}