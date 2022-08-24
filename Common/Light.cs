using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Nebula;

public abstract class Light {
    public static readonly Shader ShadowMapShader = Shader.Load("Shaders/shadowmap.glsl");

    public abstract void AddToShader(Shader shader, int index);
    public abstract void DrawShadowMap(int viewportWidth, int viewportHeight, Window window);
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

    public override void DrawShadowMap(int viewportWidth, int viewportHeight, Window window) {
        // Point light shadows
    }
}

public class DirectionalLight : Light {
    public Vector3 direction;
    public Vector3 color;
    public float intensity;

    public int shadowMapWidth;
    public int shadowMapHeight;
    private int depthMapFBO;

    public DirectionalLight(Vector3 direction, Vector3 color, float intensity, int shadowMapWidth = 2048, int shadowMapHeight = 2048)
    {
        this.direction = direction;
        this.color = color;
        this.intensity = intensity;
        this.shadowMapWidth = shadowMapWidth;
        this.shadowMapHeight = shadowMapHeight;

        initialize();
    }

    private void initialize() {
        GL.GenFramebuffer();

        int depthMap = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, depthMap);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, shadowMapWidth, shadowMapHeight, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.Repeat);

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
    }

    public override void DrawShadowMap(int viewportWidth, int viewportHeight, Window window) {
        GL.Viewport(0, 0, shadowMapWidth, shadowMapHeight);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, depthMapFBO);
        GL.Clear(ClearBufferMask.DepthBufferBit);

        float nearPlane = 1.0f, farPlane = 7.5f;
        Matrix4 projection = Matrix4.CreateOrthographicOffCenter(-10.0f, 10.0f, -10.0f, 10.0f, nearPlane, farPlane);
        Matrix4 view = Matrix4.LookAt(-direction * 10, Vector3.Zero, Vector3.UnitY);
        Light.ShadowMapShader.SetUMatrix4("matrix_viewProjection", view * projection);

        window.Rend();
        //Scene.Active.RenderForShadowMap()
        
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.Viewport(0, 0, viewportWidth, viewportHeight);
        //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    }
}