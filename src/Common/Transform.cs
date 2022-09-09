using Newtonsoft.Json.Linq;
using OpenTK.Mathematics;

namespace Nebula;

public class Transform : ISerializable {
    private Vector3 position;
    private Quaternion rotation;
    private Vector3 scale;

    public Transform()
    {
        Position = Vector3.Zero;
        Rotation = Quaternion.Identity;
        Scale = Vector3.One;
    }
    public Transform(Vector3 position)
    {
        Position = position;
        Rotation = Quaternion.Identity;
        Scale = Vector3.One;
    }
    public Transform(Vector3 position, Quaternion rotation)
    {
        Position = position;
        Rotation = rotation;
        Scale = Vector3.One;
    }
    public Transform(Vector3 position, Vector3 scale)
    {
        Position = position;
        Rotation = Quaternion.Identity;
        Scale = scale;
    }
    public Transform(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        Position = position;
        Rotation = rotation;
        Scale = scale;
    }

    public Vector3 Position {
        get => position;
        set => position = value;
    }

    public Quaternion Rotation {
        get => rotation;
        set => rotation = value;
    }

    public Vector3 Scale {
        get => scale;
        set => scale = value;
    }

    public Vector3 EulerAngles {
        get => Rotation.ToEulerAngles();
        set => Rotation = Quaternion.FromEulerAngles(value);
    }

    public Matrix4 GetMatrix() {
        Matrix4 matrix = Matrix4.Identity;
        matrix *= Matrix4.CreateScale(Scale);
        matrix *= Matrix4.CreateTranslation(Position);
        matrix *= Matrix4.CreateFromQuaternion(Rotation);
        return matrix;
    }

    public JObject Serialize() => new JObject(
        new JProperty("position", position.X, position.Y, position.Z),
        new JProperty("rotation", rotation.X, rotation.Y, rotation.Z, rotation.W ),
        new JProperty("scale", scale.X, scale.Y, scale.Z));

    public void Deserialize(JObject from) {
        position = new Vector3(from.Value<float[]>("position")[0], from.Value<float[]>("position")[1], from.Value<float[]>("position")[2]);
        rotation = new Quaternion(from.Value<float[]>("rotation")[0], from.Value<float[]>("rotation")[1], from.Value<float[]>("rotation")[2], from.Value<float[]>("rotation")[3]);
        scale = new Vector3(from.Value<float[]>("scale")[0], from.Value<float[]>("scale")[1], from.Value<float[]>("scale")[2]);
    }
}