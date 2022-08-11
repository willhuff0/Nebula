using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Nebula;

public class Nebula {
    public static void Main(string[] args) {
        var nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(800, 600),
                Title = "LearnOpenTK - Creating a Window",
                Flags = ContextFlags.ForwardCompatible
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
        -0.5f, -0.5f, 0.0f, // Bottom-left vertex
            0.5f, -0.5f, 0.0f, // Bottom-right vertex
            0.0f,  0.5f, 0.0f  // Top vertex
    };

    private int _vertexBufferObject;
    private int _vertexArrayObject;

    private Shader _shader;

    public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
    {
    }

    protected override void OnLoad()
    {
        base.OnLoad();
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
    public void SetUMatrix4(String name, Matrix4 value) => GL.ProgramUniformMatrix4(handle, _uniformLocations[name], true, ref value);
}