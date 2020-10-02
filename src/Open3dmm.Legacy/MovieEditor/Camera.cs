using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace Open3dmm
{
    //public class Camera
    //{
    //    private float _fov = 1f;
    //    private float _near = 1f;
    //    private float _far = 1000f;

    //    private Matrix4x4 _viewMatrix;
    //    private Matrix4x4 _projectionMatrix;

    //    private Vector3 _position = new Vector3(0, 3, 0);
    //    private Vector3 _lookDirection = new Vector3(0, -.3f, -1f);
    //    private float _moveSpeed = 10.0f;

    //    private float _yaw;
    //    private float _pitch;

    //    private Vector2 _previousMousePos;
    //    private float _windowWidth;
    //    private float _windowHeight;

    //    public event Action<Matrix4x4> ProjectionChanged;
    //    public event Action<Matrix4x4> ViewChanged;

    //    public Camera(float width, float height)
    //    {
    //        _windowWidth = width;
    //        _windowHeight = height;
    //        UpdatePerspectiveMatrix();
    //        UpdateViewMatrix();
    //    }

    //    public Matrix4x4 ViewMatrix => _viewMatrix;
    //    public Matrix4x4 ProjectionMatrix => _projectionMatrix;

    //    public Vector3 Position { get => _position; set { _position = value; UpdateViewMatrix(); } }

    //    public float FarDistance { get => _far; set { _far = value; UpdatePerspectiveMatrix(); } }
    //    public float FieldOfView => _fov;
    //    public float NearDistance { get => _near; set { _near = value; UpdatePerspectiveMatrix(); } }

    //    public float AspectRatio => _windowWidth / _windowHeight;

    //    public float Yaw { get => _yaw; set { _yaw = value; UpdateViewMatrix(); } }
    //    public float Pitch { get => _pitch; set { _pitch = value; UpdateViewMatrix(); } }

    //    public float MoveSpeed { get => _moveSpeed; set => _moveSpeed = value; }
    //    public Vector3 Forward => GetLookDir();

    //    public void Update(float deltaSeconds)
    //    {
    //        float sprintFactor = InputTracker.GetKey(Key.ControlLeft)
    //            ? 0.1f
    //            : InputTracker.GetKey(Key.ShiftLeft)
    //                ? 2.5f
    //                : 1f;
    //        Vector3 motionDir = Vector3.Zero;
    //        if (InputTracker.GetKey(Key.A))
    //        {
    //            motionDir += -Vector3.UnitX;
    //        }
    //        if (InputTracker.GetKey(Key.D))
    //        {
    //            motionDir += Vector3.UnitX;
    //        }
    //        if (InputTracker.GetKey(Key.W))
    //        {
    //            motionDir += -Vector3.UnitZ;
    //        }
    //        if (InputTracker.GetKey(Key.S))
    //        {
    //            motionDir += Vector3.UnitZ;
    //        }
    //        if (InputTracker.GetKey(Key.Q))
    //        {
    //            motionDir += -Vector3.UnitY;
    //        }
    //        if (InputTracker.GetKey(Key.E))
    //        {
    //            motionDir += Vector3.UnitY;
    //        }

    //        if (motionDir != Vector3.Zero)
    //        {
    //            Quaternion lookRotation = Quaternion.CreateFromYawPitchRoll(Yaw, Pitch, 0f);
    //            motionDir = Vector3.Transform(motionDir, lookRotation);
    //            _position += motionDir * MoveSpeed * sprintFactor * deltaSeconds;
    //            UpdateViewMatrix();
    //        }

    //        Vector2 mouseDelta = InputTracker.MousePosition - _previousMousePos;
    //        _previousMousePos = InputTracker.MousePosition;

    //        if (InputTracker.GetMouseButton(MouseButton.Left) || InputTracker.GetMouseButton(MouseButton.Right))
    //        {
    //            Yaw += -mouseDelta.X * 0.01f;
    //            Pitch += -mouseDelta.Y * 0.01f;
    //            Pitch = Clamp(Pitch, -1.55f, 1.55f);

    //            UpdateViewMatrix();
    //        }
    //    }

    //    private float Clamp(float value, float min, float max)
    //    {
    //        return value > max
    //            ? max
    //            : value < min
    //                ? min
    //                : value;
    //    }

    //    public void WindowResized(float width, float height)
    //    {
    //        _windowWidth = width;
    //        _windowHeight = height;
    //        UpdatePerspectiveMatrix();
    //    }

    //    private void UpdatePerspectiveMatrix()
    //    {
    //        _projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(_fov, _windowWidth / _windowHeight, _near, _far);
    //        ProjectionChanged?.Invoke(_projectionMatrix);
    //    }

    //    private void UpdateViewMatrix()
    //    {
    //        Vector3 lookDir = GetLookDir();
    //        _lookDirection = lookDir;
    //        _viewMatrix = Matrix4x4.CreateLookAt(_position, _position + _lookDirection, Vector3.UnitY);
    //        ViewChanged?.Invoke(_viewMatrix);
    //    }

    //    private Vector3 GetLookDir()
    //    {
    //        Quaternion lookRotation = Quaternion.CreateFromYawPitchRoll(Yaw, Pitch, 0f);
    //        Vector3 lookDir = Vector3.Transform(-Vector3.UnitZ, lookRotation);
    //        return lookDir;
    //    }

    //    public CameraInfo GetCameraInfo() => new CameraInfo
    //    {
    //        CameraPosition_WorldSpace = _position,
    //        CameraLookDirection = _lookDirection
    //    };
    //}

    //[StructLayout(LayoutKind.Sequential)]
    //public struct CameraInfo
    //{
    //    public Vector3 CameraPosition_WorldSpace;
    //    private float _padding1;
    //    public Vector3 CameraLookDirection;
    //    private float _padding2;
    //}

    //public static class InputTracker
    //{
    //    private static HashSet<Key> _currentlyPressedKeys = new HashSet<Key>();
    //    private static HashSet<Key> _newKeysThisFrame = new HashSet<Key>();

    //    private static HashSet<MouseButton> _currentlyPressedMouseButtons = new HashSet<MouseButton>();
    //    private static HashSet<MouseButton> _newMouseButtonsThisFrame = new HashSet<MouseButton>();

    //    public static Vector2 MousePosition;
    //    public static InputSnapshot FrameSnapshot { get; private set; }

    //    public static bool GetKey(Key key)
    //    {
    //        return _currentlyPressedKeys.Contains(key);
    //    }

    //    public static bool GetKeyDown(Key key)
    //    {
    //        return _newKeysThisFrame.Contains(key);
    //    }

    //    public static bool GetMouseButton(MouseButton button)
    //    {
    //        return _currentlyPressedMouseButtons.Contains(button);
    //    }

    //    public static bool GetMouseButtonDown(MouseButton button)
    //    {
    //        return _newMouseButtonsThisFrame.Contains(button);
    //    }

    //    public static void UpdateFrameInput(InputSnapshot snapshot)
    //    {
    //        FrameSnapshot = snapshot;
    //        _newKeysThisFrame.Clear();
    //        _newMouseButtonsThisFrame.Clear();

    //        MousePosition = snapshot.MousePosition;
    //        for (int i = 0; i < snapshot.KeyEvents.Count; i++)
    //        {
    //            KeyEvent ke = snapshot.KeyEvents[i];
    //            if (ke.Down)
    //            {
    //                KeyDown(ke.Key);
    //            }
    //            else
    //            {
    //                KeyUp(ke.Key);
    //            }
    //        }
    //        for (int i = 0; i < snapshot.MouseEvents.Count; i++)
    //        {
    //            MouseEvent me = snapshot.MouseEvents[i];
    //            if (me.Down)
    //            {
    //                MouseDown(me.MouseButton);
    //            }
    //            else
    //            {
    //                MouseUp(me.MouseButton);
    //            }
    //        }
    //    }

    //    private static void MouseUp(MouseButton mouseButton)
    //    {
    //        _currentlyPressedMouseButtons.Remove(mouseButton);
    //        _newMouseButtonsThisFrame.Remove(mouseButton);
    //    }

    //    private static void MouseDown(MouseButton mouseButton)
    //    {
    //        if (_currentlyPressedMouseButtons.Add(mouseButton))
    //        {
    //            _newMouseButtonsThisFrame.Add(mouseButton);
    //        }
    //    }

    //    private static void KeyUp(Key key)
    //    {
    //        _currentlyPressedKeys.Remove(key);
    //        _newKeysThisFrame.Remove(key);
    //    }

    //    private static void KeyDown(Key key)
    //    {
    //        if (_currentlyPressedKeys.Add(key))
    //        {
    //            _newKeysThisFrame.Add(key);
    //        }
    //    }
    //}
}
