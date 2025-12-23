using UnityEngine;
using UnityEngine.InputSystem;

public class MouseCameraController : MonoBehaviour
{
    public float rotateSpeed = 180f;
    public float panSpeed = 10f;
    public float zoomSpeed = 10f;

    float yaw;
    float pitch;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        if (Mouse.current == null || Keyboard.current == null)
            return;

        var mouse = Mouse.current;
        var keyboard = Keyboard.current;

        bool leftDown = mouse.leftButton.isPressed;
        bool shiftDown = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
        bool altDown = keyboard.altKey.isPressed;

        Vector2 mouseDelta = mouse.delta.ReadValue();
        float dt = Time.deltaTime;

        if (leftDown && altDown && !shiftDown)
        {
            yaw += mouseDelta.x * rotateSpeed * dt;
            pitch -= mouseDelta.y * rotateSpeed * dt;
            pitch = Mathf.Clamp(pitch, -85f, 85f);

            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
            return;
        }

        if (leftDown && shiftDown && !altDown)
        {
            Vector3 move =
                transform.right * (-mouseDelta.x * panSpeed * dt) +
                transform.up    * (-mouseDelta.y * panSpeed * dt);

            transform.position += move;
            return;
        }

        float scroll = mouse.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            transform.position += transform.forward * (scroll * zoomSpeed * dt);
        }
    }
}
