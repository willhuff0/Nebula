using OpenTK.Mathematics;

namespace Nebula;

public class Transform {
    private Matrix4 matrix;

    public Transform()
    {
        matrix = Matrix4.Identity;
        Position = Vector3.Zero;
        Rotation = Quaternion.Identity;
        Scale = Vector3.One;
    }
    public Transform(Vector3 position)
    {
        matrix = Matrix4.Identity;
        Position = position;
        Rotation = Quaternion.Identity;
        Scale = Vector3.One;
    }
    public Transform(Vector3 position, Quaternion rotation)
    {
        matrix = Matrix4.Identity;
        Position = position;
        Rotation = rotation;
        Scale = Vector3.One;
    }
    public Transform(Vector3 position, Vector3 scale)
    {
        matrix = Matrix4.Identity;
        Position = position;
        Rotation = Quaternion.Identity;
        Scale = scale;
    }
    public Transform(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        matrix = Matrix4.Identity;
        Position = position;
        Rotation = rotation;
        Scale = scale;
    }

    public Vector3 Position {
        get => matrix.ExtractTranslation();
        set {
            matrix = matrix.ClearTranslation();
            matrix *= Matrix4.CreateTranslation(value);
        }
    }

    public Quaternion Rotation {
        get => matrix.ExtractRotation();
        set {
            matrix = matrix.ClearRotation();
            matrix *= Matrix4.CreateFromQuaternion(value);
        }
    }

    public Vector3 Scale {
        get => matrix.ExtractScale();
        set {
            matrix = matrix.ClearScale();
            matrix *= Matrix4.CreateScale(value);
        }
    }

    public Vector3 EulerAngles {
        get => Rotation.ToEulerAngles();
        set => Rotation = Quaternion.FromEulerAngles(value);
    }

    public Matrix4 GetMatrix() => matrix;
}