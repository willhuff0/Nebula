using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Nebula;

public class Shader {
    public readonly int handle;
    private readonly Dictionary<string, int> _uniformLocations;

    public Shader(string vert, string frag) {
        var vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, vert);
        GL.CompileShader(vertexShader);
        GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out var status);
        if (status != (int)All.True) throw new Exception($"An error occurred while compiling vertex shader ({vertexShader}):\n\n{GL.GetShaderInfoLog(vertexShader)}");

        var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, frag);
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

    public static Shader Load(string path) {
        string[] sourceLines = File.ReadAllLines(path);
        string vertexSource = string.Join('\n', sourceLines.SkipWhile((e) => !e.StartsWith("##VERTEX")).Skip(1).TakeWhile((e) => !e.StartsWith("##FRAGMENT")).SkipLast(1));
        string fragmentSource = string.Join('\n', sourceLines.SkipWhile((e) => !e.StartsWith("##FRAGMENT")).Skip(1));
        return new Shader(vertexSource, fragmentSource);
    }
    public static Shader Load(string vertexPath, string fragmentPath) {
        return new Shader(File.ReadAllText(vertexPath), File.ReadAllText(fragmentPath));
    }

    public int GetAttribLocation(string name) => GL.GetAttribLocation(handle, name);

    public void Bind() => GL.UseProgram(handle);
    public void SetUFloat(String name, float value) => GL.ProgramUniform1(handle, _uniformLocations[name], value);
    public void SetUInt(String name, int value) => GL.ProgramUniform1(handle, _uniformLocations[name], value);
    public void SetUVector3(String name, Vector3 value) => GL.ProgramUniform3(handle, _uniformLocations[name], value);
    public void SetUVector4(String name, Vector4 value) => GL.ProgramUniform4(handle, _uniformLocations[name], value);
    public void SetUMatrix4(String name, Matrix4 value) => GL.ProgramUniformMatrix4(handle, _uniformLocations[name], true, ref value);

    public void SetUsualMatrices(Matrix4 transform, Matrix4? VPM) {
        SetUMatrix4("matrix_transform", transform);
        if (VPM != null) SetUMatrix4("matrix_viewProjection", (Matrix4)VPM);
    }

    public void StandardMaterialSetUniforms(Vector3? viewPos, int directionalLightCount, int pointLightCount) {
        if (viewPos != null) SetUVector3("viewPos", (Vector3)viewPos);
        if (directionalLightCount != -1) SetUInt("directionalLightCount", directionalLightCount);
        if (pointLightCount != -1) SetUInt("pointLightCount", pointLightCount);
    }
}