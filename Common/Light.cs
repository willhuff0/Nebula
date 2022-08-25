using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Nebula;

public abstract class Light {
    public const int DEFAULT_SHADOWMAP_WIDTH = 2048;
    public const int DEFAULT_SHADOWMAP_HEIGHT = 2048;

    public static Shader ShadowMapShader;

    public abstract void AddToShader(Shader shader, int index);
    public abstract int DrawShadowMap(Window window);
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

    public override int DrawShadowMap(Window window) {
        // Point light shadows
        return -1;
    }
}

public class DirectionalLight : Light {
    public Vector3 direction;
    public Vector3 color;
    public float intensity;
    private Matrix4 shadowMatrix;

    public int shadowMapWidth;
    public int shadowMapHeight;

    private int depthMapFBO;
    private int depthMap;

    public DirectionalLight(Vector3 direction, Vector3 color, float intensity, int shadowMapWidth = DEFAULT_SHADOWMAP_WIDTH, int shadowMapHeight = DEFAULT_SHADOWMAP_HEIGHT)
    {
        this.direction = direction;
        this.color = color;
        this.intensity = intensity;
        this.shadowMapWidth = shadowMapWidth;
        this.shadowMapHeight = shadowMapHeight;

        initialize();
    }

    private void initialize() {
        depthMapFBO = GL.GenFramebuffer();

        depthMap = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, depthMap);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent16, shadowMapWidth, shadowMapHeight, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureWrapMode.ClampToBorder);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureWrapMode.ClampToBorder);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, new float[] { 1.0f, 1.0f, 1.0f, 1.0f });

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, depthMapFBO);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, depthMap, 0);
        GL.DrawBuffer(DrawBufferMode.None);
        GL.ReadBuffer(ReadBufferMode.None);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public override void AddToShader(Shader shader, int index) {
        shader.SetUVector3($"directionalLights[{index}].direction", -direction);
        shader.SetUVector3($"directionalLights[{index}].color", color);
        shader.SetUFloat($"directionalLights[{index}].intensity", intensity);

        shader.SetUInt("shadowCasterCount", 1);
        GL.ActiveTexture(TextureUnit.Texture19);
        GL.BindTexture(TextureTarget.Texture2D, depthMap);
        shader.SetUInt($"shadowMaps[{index}]", 19);
        shader.SetUMatrix4($"shadowMatrices[{index}]", shadowMatrix);
    }

    public override int DrawShadowMap(Window window) {
        Matrix4 projection = Matrix4.CreateOrthographic(30.0f, 30.0f, 0.2f, 30.0f);
        Matrix4 view = Matrix4.LookAt(-direction * 15, Vector3.Zero, new Vector3(0, 1, 0));
        shadowMatrix = view * projection;
        Light.ShadowMapShader.Bind();
        Light.ShadowMapShader.SetUMatrix4("matrix_viewProjection", shadowMatrix);

        GL.Viewport(0, 0, shadowMapWidth, shadowMapHeight);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, depthMapFBO);
        GL.Clear(ClearBufferMask.DepthBufferBit);

        window.Rend();
        //Scene.Active.RenderForShadowMap()
        
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        return depthMap;
    }
}