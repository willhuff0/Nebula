using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Nebula;

public class Nebula {
    public static Window activeWindow;

    public static void Main(string[] args) {
        var nativeWindowSettings = new NativeWindowSettings()
        {
            Size = new Vector2i(1280, 720),
            Title = "Nebula",
            APIVersion = new Version(4, 1),
            Flags = ContextFlags.ForwardCompatible,
            //NumberOfSamples = 8
        };

        using (activeWindow = new Window(GameWindowSettings.Default, nativeWindowSettings))
        {
            activeWindow.Run();
        }
    }
}

public class Window : GameWindow
{
    private Light[] lights;

    private readonly float[] quadVertices = {  
            // positions   // texCoords
            -1.0f,  1.0f,  0.0f, 1.0f,
            -1.0f, -1.0f,  0.0f, 0.0f,
            1.0f, -1.0f,  1.0f, 0.0f,

            -1.0f,  1.0f,  0.0f, 1.0f,
            1.0f, -1.0f,  1.0f, 0.0f,
            1.0f,  1.0f,  1.0f, 1.0f
        };	

    private Shader depthMapShader;

    private Model lightModel;
    public Model model;
    private Model model2;

    private int depthDisplayVAO;

    private Camera _camera;

    private Stopwatch _stopwatch;

    private bool _firstMove = true;

    private Vector2 _lastPos;

    //private int _elementBufferObject;

    public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
    {
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        Debug.WriteLine(GL.GetString(StringName.Version));
        Debug.WriteLine(GL.GetString(StringName.ShadingLanguageVersion));
        Debug.WriteLine(GL.GetString(StringName.Renderer));
        Debug.WriteLine(GL.GetString(StringName.Vendor));

        Light.ShadowMapShader = Shader.Load("Shaders/shadowmap.glsl");

        lights = new Light[] {
            new DirectionalLight(new Vector3(0.4f, -1.0f, 0.4f), new Vector3(1.0f, 0.96f, 0.9f), 1.0f),
            //new PointLight(new Vector3(5.0f, 2.0f, 3.0f), new Vector3(1.0f, 1.0f, 1.0f), 4.0f),
        };

        GL.ClearColor(.2f, .3f, .3f, 1);
        //GL.Enable(EnableCap.Multisample);
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.CullFace(CullFaceMode.Back);

        lightModel = new Model(new Material[] { new Material(new Mesh[] { new Mesh(Primitives.Cube) }, new Texture[] {}) }, Shader.Load("Shaders/light.glsl"));

        // model = Model.Load("Resources/Survival_BackPack_2/Survival_BackPack_2.fbx", Shader.Load("Shaders/standard.glsl"), 0.01f, new DefaultTextures(
        //     albedo: Texture.LoadFromFile(@"Resources/Survival_BackPack_2/1001_albedo.jpg", "albedo"),
        //     normal: Texture.LoadFromFile(@"Resources/Survival_BackPack_2/1001_normal.png", "normal"),
        //     metallic: Texture.LoadFromFile(@"Resources/Survival_BackPack_2/1001_metallic.jpg", "metallic"),
        //     roughness: Texture.LoadFromFile(@"Resources/Survival_BackPack_2/1001_roughness.jpg", "roughness"),
        //     ao: Texture.LoadFromFile(@"Resources/Survival_BackPack_2/1001_AO.jpg", "ao")
        // ));

        model2 = Model.Load("Resources/untitled.glb", Shader.Load("Shaders/standard.glsl"));
        //model2.transform.Position = new Vector3(4, -4, 0);

        Debug.WriteLine($"{Texture.textureCache.Count} textures were cached");

        _camera = new Camera(Vector3.UnitZ * 3, Size.X / (float)Size.Y);

        depthDisplayVAO = GL.GenVertexArray();
        GL.BindVertexArray(depthDisplayVAO);
        
        int vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, quadVertices.Length * sizeof(float), quadVertices, BufferUsageHint.StaticDraw);

        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));

        GL.BindVertexArray(0);

        depthMapShader = Shader.Load("Shaders/depthdisplay.glsl");

        CursorState = CursorState.Grabbed;

        _stopwatch = new Stopwatch();
        _stopwatch.Start();
    }

    public void Rend() {
        //model.DrawForShadowMaps();
        model2.DrawForShadowMaps();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        Matrix4 VPM = _camera.GetViewMatrix() * _camera.GetProjectionMatrix();
        Vector3 viewPos = _camera.Position;

        GL.CullFace(CullFaceMode.Front);
        List<int> shadowMaps = new List<int>();
        foreach(Light light in lights) {
            int map = light.DrawShadowMap(this);
            if (map != -1) shadowMaps.Add(map);
        }
        GL.CullFace(CullFaceMode.Back);

        GL.Viewport(0, 0, currentSize.X, currentSize.Y);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        // depthMapShader.Bind();
        // GL.ActiveTexture(TextureUnit.Texture0);
        // GL.BindTexture(TextureTarget.Texture2D, shadowMaps[0]);
        // depthMapShader.SetUInt("depthMap", 0);
        // GL.BindVertexArray(depthDisplayVAO);
        // GL.DrawArrays(PrimitiveType.Triangles, 0, quadVertices.Length);
        // GL.BindVertexArray(0);

        // foreach(Light light in lights) {
        //     if (light is not PointLight) continue;
        //     PointLight pointLight = (PointLight)light;
        //     lightModel.transform.Position = pointLight.position;
        //     lightModel.Draw(VPM);
        // }

        //model.Draw(VPM, viewPos, lights);
        model2.Draw(VPM, viewPos, lights);

        SwapBuffers();
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (!IsFocused)
            {
                return;
            }

            var input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            }

            const float cameraSpeed = 7.0f;
            const float sensitivity = 0.2f;

            if (input.IsKeyDown(Keys.W))
            {
                _camera.Position += _camera.Forward * cameraSpeed * (float)e.Time; // Forward
            }
            if (input.IsKeyDown(Keys.S))
            {
                _camera.Position -= _camera.Forward * cameraSpeed * (float)e.Time; // Backwards
            }
            if (input.IsKeyDown(Keys.A))
            {
                _camera.Position -= _camera.Right * cameraSpeed * (float)e.Time; // Left
            }
            if (input.IsKeyDown(Keys.D))
            {
                _camera.Position += _camera.Right * cameraSpeed * (float)e.Time; // Right
            }
            if (input.IsKeyDown(Keys.Space))
            {
                _camera.Position += _camera.Up * cameraSpeed * (float)e.Time; // Up
            }
            if (input.IsKeyDown(Keys.LeftShift))
            {
                _camera.Position -= _camera.Up * cameraSpeed * (float)e.Time; // Down
            }

            var mouse = MouseState;

            if (_firstMove)
            {
                _lastPos = new Vector2(mouse.X, mouse.Y);
                _firstMove = false;
            }
            else
            {
                var deltaX = mouse.X - _lastPos.X;
                var deltaY = mouse.Y - _lastPos.Y;
                _lastPos = new Vector2(mouse.X, mouse.Y);

                _camera.Yaw += deltaX * sensitivity;
                _camera.Pitch -= deltaY * sensitivity;
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            _camera.Fov -= e.OffsetY;
        }

    Vector2i currentSize;
    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        currentSize = Size * (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? 2 : 1);
        GL.Viewport(0, 0, currentSize.X, currentSize.Y);
        _camera.AspectRatio = currentSize.X / (float)currentSize.Y;
    }
}
