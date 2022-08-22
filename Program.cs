using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using glTFLoader;
using glTFLoader.Schema;
using Nebual.Utils;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using static glTFLoader.Schema.Accessor;
using static glTFLoader.Schema.MeshPrimitive;

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

    private Light[] lights = new Light[] {
        new DirectionalLight(new Vector3(0.4f, -1.0f, 0.4f), new Vector3(1.0f, 0.96f, 0.9f), 1.0f),
        new PointLight(new Vector3(5.0f, 2.0f, 3.0f), new Vector3(1.0f, 1.0f, 0.0f), 1.0f),
    };

    private Model model;

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

        Material material =new Material(Shader.Load("Shaders/standard.glsl"), new Texture[] {
            Texture.LoadFromFile("Resources/rustediron2_basecolor.png", "albedo"),
            Texture.LoadFromFile("Resources/rustediron2_normal.png", "normal"),
            Texture.LoadFromFile("Resources/rustediron2_metallic.png", "metallic"),
            Texture.LoadFromFile("Resources/rustediron2_roughness.png", "roughness"),
            Texture.LoadFromFile("Resources/rustediron2_ao.png", "ao"),
        });

        Gltf gltf = Interface.LoadModel("Resources/untitled.gltf");

        // cache
        Dictionary<int, dynamic[]> buffers = new Dictionary<int, dynamic[]>(gltf.Buffers.Length);
        Mesh[] meshes = new Mesh[gltf.Meshes.Length];
        Model[] models = new Model[gltf.Nodes.Length];

        foreach(Scene scene in gltf.Scenes) {
            foreach(int nodeIndex in scene.Nodes) {

                Node node = gltf.Nodes[nodeIndex];
                glTFLoader.Schema.Mesh gltfMesh = gltf.Meshes[(int)node.Mesh];
                
                Dictionary<int, Vertex> vertices = new Dictionary<int, Vertex>();

                foreach(MeshPrimitive primitive in gltfMesh.Primitives) {
                    foreach(KeyValuePair<string, int> accessorIndex in primitive.Attributes.ToArray()) {
                        Accessor accessor = gltf.Accessors[accessorIndex.Value];
                        switch(accessorIndex.Key) {
                            case "POSITION": {
                                BufferView view = gltf.BufferViews[(int)accessor.BufferView];
                                dynamic[] buffer;
                                if (!buffers.TryGetValue(view.Buffer, out buffer)) {
                                
                                    Stream input;
                                    if (PathValidator.IsValidPath(buffer.Uri)) { 
                                        input = File.OpenRead(buffer.Uri);
                                        input.Seek((long)view.ByteOffset, SeekOrigin.Begin);
                                        input.SetLength((long)view.ByteLength);
                                    }
                                    else input = new MemoryStream(Convert.FromBase64String(buffer.Uri), view.ByteOffset, view.ByteLength);

                                    dynamic[] values;
                                    using(BinaryReader reader = new BinaryReader(input)) {
                                        switch(accessor.ComponentType) {
                                            case ComponentTypeEnum.FLOAT: {
                                                int count = accessor.Count / 4;
                                                values = new dynamic[count];
                                                for (int i = 0; i < count; i++) values[i] = reader.ReadSingle();
                                                break;
                                            }
                                            case ComponentTypeEnum.UNSIGNED_INT: {
                                                int count = accessor.Count / 4;
                                                values = new dynamic[count];
                                                for (int i = 0; i < count; i++) values[i] = reader.ReadUInt32();
                                                break;
                                            }
                                        }
                                    }

                                    break;
                                }
                            }
                        }
                    }
                }
                
                mesh
            }
        }

        model = new Model(new Mesh(_vertices, Enumerable.Range(0, _vertices.Length / 8).Select((value) => (uint)value).ToArray()), material, new Transform());

        _camera = new Camera(Vector3.UnitZ * 3, Size.X / (float)Size.Y);

        CursorState = CursorState.Grabbed;

        _stopwatch = new Stopwatch();
        _stopwatch.Start();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        //GL.Enable(EnableCap.CullFace);
        //GL.CullFace(CullFaceMode.Back);

        Matrix4 VPM = _camera.GetViewProjectionMatrix();
        Vector3 viewPos = _camera.Position;

        model.Draw(VPM, viewPos, lights);

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
