using UnityEngine;
using UnityEngine.InputSystem;

namespace Scripts.Presentation
{
  public sealed class CameraController : MonoBehaviour
  {
    [Header("Speed Settings")]
    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private float _rotateSpeed = 2f;
    [SerializeField] private float _zoomSpeed = 10f;
    [SerializeField] private float _minZoom = 5f;
    [SerializeField] private float _maxZoom = 50f;

    private Vector2 _moveInput;
    private Vector2 _rotateInput;
    private float _zoomInput;
    private bool _isRightMouseDown;

    private CityBuilderInput _input;

    private float _yaw = 0f;
    private float _pitch = 0f;

    private void Awake()
    {
      _input = new CityBuilderInput();

      _input.Camera.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
      _input.Camera.Move.canceled += ctx => _moveInput = Vector2.zero;

      _input.Camera.Pan.performed += ctx =>
      {
        if (_isRightMouseDown)
          _rotateInput = ctx.ReadValue<Vector2>();
      };
      _input.Camera.Pan.canceled += ctx => _rotateInput = Vector2.zero;

      _input.Camera.Zoom.performed += ctx =>
      {
        Vector2 delta = ctx.ReadValue<Vector2>();
        _zoomInput = delta.y;
      };
      _input.Camera.Zoom.canceled += ctx => _zoomInput = 0f;
    }

    private void OnEnable() => _input.Enable();
    private void OnDisable() => _input.Disable();

    private void Update()
    {
      _isRightMouseDown = Mouse.current.rightButton.isPressed;

      if (_isRightMouseDown)
      {
        Vector2 delta = Mouse.current.delta.ReadValue();
        _yaw += delta.x * _rotateSpeed * Time.deltaTime;
        _pitch -= delta.y * _rotateSpeed * Time.deltaTime;
        _pitch = Mathf.Clamp(_pitch, -80f, 80f);

        transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
      }

      Vector3 forward = transform.forward;
      Vector3 right = transform.right;
      Vector3 move = (forward * _moveInput.y + right * _moveInput.x) * _moveSpeed * Time.deltaTime;
      transform.position += move;

      Vector2 scroll = Mouse.current.scroll.ReadValue();
      if (scroll.y != 0)
      {
        Vector3 pos = transform.position;
        pos += transform.forward * scroll.y * _zoomSpeed * Time.deltaTime;
        pos.y = Mathf.Clamp(pos.y, _minZoom, _maxZoom);
        transform.position = pos;
      }
    }
  }
}