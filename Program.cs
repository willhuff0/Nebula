using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            Title = "Sofia",
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
        // positions        // colors
             0.5f, -0.5f, 0.0f,  1.0f, 0.0f, 0.0f,   // bottom right
            -0.5f, -0.5f, 0.0f,  0.0f, 1.0f, 0.0f,   // bottom left
             0.0f,  0.5f, 0.0f,  0.0f, 0.0f, 1.0f    // top 
    };

    private readonly uint[] _indices =
    {
        // Note that indices start at 0!
        0, 1, 2, // The first triangle will be the top-right half of the triangle
        //1, 2, 3  // Then the second will be the bottom-left half of the triangle
    };

    private Stopwatch _timer;

    private int _vertexBufferObject;
    private int _vertexArrayObject;

    private Shader _shader;

    private int _elementBufferObject;

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

        GL.Viewport(0, 0, Size.X, Size.Y);
        GL.Enable(EnableCap.Multisample);
        GL.ClearColor(.2f, .3f, .3f, 1);

        _vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

        _vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(_vertexArrayObject);

        // Positions
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        // Colors
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        _elementBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

        GL.GetInteger(GetPName.MaxVertexAttribs, out int maxAttributeCount);
        Debug.WriteLine($"Maximum number of vertex attributes supported: {maxAttributeCount}");

        _shader = new Shader("Shaders/shader.glsl");
        _shader.Use();

        _timer = new Stopwatch();
        _timer.Start();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        if (KeyboardState.IsKeyDown(Keys.Escape))
        {
            Close();
        }
        
        base.OnUpdateFrame(args);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.Clear(ClearBufferMask.ColorBufferBit);

        _shader.Use();

        double timeValue = _timer.Elapsed.TotalSeconds;
        float greenValue = (float)Math.Sin(timeValue) / 2.0f + 0.5f;

        _shader.SetUVector3("globalColor", new Vector3(0.0f, greenValue, 0.0f));

        GL.BindVertexArray(_vertexArrayObject);
        GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

        SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        GL.Viewport(0, 0, Size.X, Size.Y);
    }

    protected override void OnUnload()
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);
        GL.UseProgram(0);

        GL.DeleteBuffer(_vertexBufferObject);
        GL.DeleteVertexArray(_vertexArrayObject);

        GL.DeleteProgram(_shader.handle);

        base.OnUnload();
    }
}

public class Shader {
    public readonly int handle;
    private readonly Dictionary<string, int> _uniformLocations;

    public Shader(string path) {
        string[] sourceLines = File.ReadAllLines(path);
        string vertexSource = string.Join('\n', sourceLines.SkipWhile((e) => !e.StartsWith("##VERTEX")).Skip(1).TakeWhile((e) => !e.StartsWith("##FRAGMENT")).SkipLast(1));
        string fragmentSource = string.Join('\n', sourceLines.SkipWhile((e) => !e.StartsWith("##FRAGMENT")).Skip(1));

        var vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, vertexSource);
        GL.CompileShader(vertexShader);
        GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out var status);
        if (status != (int)All.True) throw new Exception($"An error occurred while compiling vertex shader ({vertexShader}):\n\n{GL.GetShaderInfoLog(vertexShader)}");

        var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, fragmentSource);
        GL.CompileShader(fragmentShader);
        GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out status);
        if (status != (int)All.True) throw new Exception($"An error occurred while compiling fragment shader ({fragmentShader}):\n\n{GL.GetShaderInfoLog(fragmentShader)}");

        handle = GL.CreateProgram();
        GL.AttachShader(handle, vertexShader);
        GL.AttachShader(handle, fragmentShader);
        GL.LinkProgram(handle);
        GL.GetProgram(handle, GetProgramParameterName.LinkStatus, out status);
        if (status != (int)All.True) throw new Exception($"An error occured while linking program ({handle}):\n\n{GL.GetProgramInfoLog(handle)}");

        GL.DetachShader(handle, vertexShader);
        GL.DetachShader(handle, fragmentShader);
        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);

        _uniformLocations = new Dictionary<string, int>();
        GL.GetProgram(handle, GetProgramParameterName.ActiveUniforms, out var uniformCount);
        for (int i = 0; i < uniformCount; i++)
        {
            var name = GL.GetActiveUniform(handle, i, out _, out _);
            var location = GL.GetUniformLocation(handle, name);
            _uniformLocations.Add(name, location);
        }
    }

    public int GetAttribLocation(string name) => GL.GetAttribLocation(handle, name);

    public void Use() => GL.UseProgram(handle);
    public void SetUFloat(String name, float value) => GL.ProgramUniform1(handle, _uniformLocations[name], value);
    public void SetUVector3(String name, Vector3 value) => GL.ProgramUniform3(handle, _uniformLocations[name], value);
    public void SetUVector4(String name, Vector4 value) => GL.ProgramUniform4(handle, _uniformLocations[name], value);
    public void SetUMatrix4(String name, Matrix4 value) => GL.ProgramUniformMatrix4(handle, _uniformLocations[name], true, ref value);
}