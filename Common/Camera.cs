using System;
using OpenTK.Mathematics;

namespace Nebula;

public class Camera {
    public const float NEAR_CLIP = 0.01f;
    public const float FAR_CLIP = 100.0f;

    private Vector3 _forward = -Vector3.UnitZ;
    private Vector3 _up = Vector3.UnitY;
    private Vector3 _right = Vector3.UnitX;

    private float _pitch;
    private float _yaw = -MathHelper.PiOver2;

    private float _fov = MathHelper.PiOver2;

    public Camera(Vector3 position, float aspectRatio) {
        Position = position;
        AspectRatio = aspectRatio;
    }

    public Vector3 Position { get; set; }

    public float AspectRatio { private get; set; }

    public Vector3 Forward => _forward;
    public Vector3 Up => _up;
    public Vector3 Right => _right;

    public float Pitch {
        get => MathHelper.RadiansToDegrees(_pitch);
        set {
            var angle = MathHelper.Clamp(value, -89f, 89f);
            _pitch = MathHelper.DegreesToRadians(angle);
            UpdateVectors();
        }
    }

    public float Yaw {
        get => MathHelper.RadiansToDegrees(_yaw);
        set {
            _yaw = MathHelper.DegreesToRadians(value);
            UpdateVectors();
        }
    }

    public float Fov {
        get => MathHelper.RadiansToDegrees(_fov);
        set {
            var angle = MathHelper.Clamp(value, 1.0f, 179.0f);
            _fov = MathHelper.DegreesToRadians(angle);
        }
    }

    public Matrix4 GetViewMatrix() => Matrix4.LookAt(Position, Position + _forward, _up);
    public Matrix4 GetProjectionMatrix() => Matrix4.CreatePerspectiveFieldOfView(_fov, AspectRatio, NEAR_CLIP, FAR_CLIP);

    private void UpdateVectors() {
        _forward.X = MathF.Cos(_pitch) * MathF.Cos(_yaw);
        _forward.Y = MathF.Sin(_pitch);
        _forward.Z = MathF.Cos(_pitch) * MathF.Sin(_yaw);
        _forward = Vector3.Normalize(_forward);

        _right = Vector3.Normalize(Vector3.Cross(_forward, Vector3.UnitY));
        _up = Vector3.Normalize(Vector3.Cross(_right, _forward));
    }
}