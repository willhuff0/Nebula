using System;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Nebula;

public class Nebula {
    public static void Main(string[] args) {
        var nativeWindowSettings = new NativeWindowSettings()
        {
            Size = new Vector2i(800, 600),
            Title = "Nebula",
            APIVersion = new Version(4, 1),
            Flags = ContextFlags.ForwardCompatible,
            NumberOfSamples = 8
        };

        using (var window = new Window(GameWindowSettings.Default, nativeWindowSettings))
        {
            window.Run();
        }
    }
}

public class Window : GameWindow
{
    private readonly float[] _vertices =
        {
             // Position          Normal
             -0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  0.0f, 0.0f,
             0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  1.0f, 0.0f,
             0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  1.0f, 1.0f,
             0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  1.0f, 1.0f,
            -0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  0.0f, 1.0f,
            -0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  0.0f, 0.0f,

            -0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f,  0.0f, 0.0f,
             0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f,  1.0f, 0.0f,
             0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,  1.0f, 1.0f,
             0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,  1.0f, 1.0f,
            -0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,  0.0f, 1.0f,
            -0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f,  0.0f, 0.0f,

            -0.5f,  0.5f,  0.5f, -1.0f,  0.0f,  0.0f,  1.0f, 0.0f,
            -0.5f,  0.5f, -0.5f, -1.0f,  0.0f,  0.0f,  1.0f, 1.0f,
            -0.5f, -0.5f, -0.5f, -1.0f,  0.0f,  0.0f,  0.0f, 1.0f,
            -0.5f, -0.5f, -0.5f, -1.0f,  0.0f,  0.0f,  0.0f, 1.0f,
            -0.5f, -0.5f,  0.5f, -1.0f,  0.0f,  0.0f,  0.0f, 0.0f,
            -0.5f,  0.5f,  0.5f, -1.0f,  0.0f,  0.0f,  1.0f, 0.0f,

             0.5f,  0.5f,  0.5f,  1.0f,  0.0f,  0.0f,  1.0f, 0.0f,
             0.5f,  0.5f, -0.5f,  1.0f,  0.0f,  0.0f,  1.0f, 1.0f,
             0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f,  0.0f, 1.0f,
             0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f,  0.0f, 1.0f,
             0.5f, -0.5f,  0.5f,  1.0f,  0.0f,  0.0f,  0.0f, 0.0f,
             0.5f,  0.5f,  0.5f,  1.0f,  0.0f,  0.0f,  1.0f, 0.0f,

            -0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,  0.0f, 1.0f,
             0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,  1.0f, 1.0f,
             0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,  1.0f, 0.0f,
             0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,  1.0f, 0.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,  0.0f, 0.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,  0.0f, 1.0f,

            -0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f,  0.0f, 1.0f,
             0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f,  1.0f, 1.0f,
             0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,  1.0f, 0.0f,
             0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,  1.0f, 0.0f,
            -0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,  0.0f, 0.0f,
            -0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f,  0.0f, 1.0f
        };

    private Vector3[] lightPositions = new Vector3[] {
        new Vector3(0.0f, 0.0f, 10.0f),
    };
    private Vector3[] lightColors = new Vector3[] {
        new Vector3(150.0f, 150.0f, 150.0f),
    };
    private int nrRows = 7;
    private int nrColumns = 7;
    private float spacing = 2.5f;

    private int _vertexBufferObject;

    private int _vaoModel;
    private int _vaoLamp;

    private Shader _modelShader;
    private Shader _lampShader;

    private Texture albedoMap;
    private Texture normalMap;
    private Texture metallicMap;
    private Texture roughnessMap;
    private Texture aoMap;

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

        GL.ClearColor(.2f, .3f, .3f, 1);
        GL.Enable(EnableCap.Multisample);
        GL.Enable(EnableCap.DepthTest);

        _vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

        _modelShader = Shader.Load("Shaders/standard.glsl");
        _lampShader = Shader.Load("Shaders/light.glsl");

       {
            _vaoModel = GL.GenVertexArray();
            GL.BindVertexArray(_vaoModel);

            // Positions
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // Normals
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            // Texcoords
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
            GL.EnableVertexAttribArray(2);
        }
        {
            _vaoLamp = GL.GenVertexArray();
            GL.BindVertexArray(_vaoLamp);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
        }

        // _elementBufferObject = GL.GenBuffer();
        // GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
        // GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

        albedoMap = Texture.LoadFromFile("Resources/rustediron2_basecolor.png");
        normalMap = Texture.LoadFromFile("Resources/rustediron2_normal.png");
        metallicMap = Texture.LoadFromFile("Resources/rustediron2_metallic.png");
        roughnessMap = Texture.LoadFromFile("Resources/rustediron2_roughness.png");
        aoMap = Texture.LoadFromFile("Resources/rustediron2_ao.png");

        _camera = new Camera(Vector3.UnitZ * 3, Size.X / (float)Size.Y);

        CursorState = CursorState.Grabbed;

        _stopwatch = new Stopwatch();
        _stopwatch.Start();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        GL.BindVertexArray (_vaoModel);
        //GL.Enable(EnableCap.CullFace);
        //GL.CullFace(CullFaceMode.Back);

        albedoMap.Bind(TextureUnit.Texture0);
        normalMap.Bind(TextureUnit.Texture1);
        metallicMap.Bind(TextureUnit.Texture2);
        roughnessMap.Bind(TextureUnit.Texture3);
        aoMap.Bind(TextureUnit.Texture4);
        _modelShader.Bind();

        _modelShader.SetUMatrix4("view", _camera.GetViewMatrix());
        _modelShader.SetUMatrix4("projection", _camera.GetProjectionMatrix());

        _modelShader.SetUVector3("camPos", _camera.Position);

        _modelShader.SetUInt("albedoMap", 0);
        _modelShader.SetUInt("normalMap", 1);
        _modelShader.SetUInt("metallicMap", 2);
        _modelShader.SetUInt("roughnessMap", 3);
        _modelShader.SetUInt("aoMap", 4);

        for (int row = 0; row < nrRows; row++) {
            for (int col = 0; col < nrColumns; col++) {
                Matrix4 modelMatrix = Matrix4.Identity;
                modelMatrix *= Matrix4.CreateTranslation((float)(col - (nrColumns / 2)) * spacing, (float)(row - (nrRows / 2)) * spacing, 0.0f);
                _modelShader.SetUMatrix4("model", modelMatrix);
                GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            }
        }

        _lampShader.Bind();

        for(int i = 0; i < lightPositions.Length; ++i) {
            Vector3 newPos = lightPositions[i] + new Vector3(MathF.Sin((float)_stopwatch.Elapsed.TotalSeconds * 5.0f) * 5.0f, 0.0f, 0.0f) * (float)args.Time;
            lightPositions[i] = newPos;
            _modelShader.SetUVector3($"lightPositions[{i}]", newPos);
            _modelShader.SetUVector3($"lightColors[{i}]", lightColors[i]);

            Matrix4 lightMatrix = Matrix4.Identity;
            lightMatrix *= Matrix4.CreateTranslation(newPos);
            lightMatrix *= Matrix4.CreateScale(0.2f);

            _lampShader.SetUMatrix4("model", lightMatrix);
            _lampShader.SetUMatrix4("view", _camera.GetViewMatrix());
            _lampShader.SetUMatrix4("projection", _camera.GetProjectionMatrix());

            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
        }

        //GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

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

            const float cameraSpeed = 1.5f;
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

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        GL.Viewport(0, 0, Size.X, Size.Y);
        _camera.AspectRatio = Size.X / (float)Size.Y;
    }
}
