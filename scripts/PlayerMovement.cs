using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 0.1f;
    public float gravity = -20f;
    public Transform cameraTransform;

    CharacterController controller;
    float xRotation = 0f;
    float yVelocity = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Look around with the mouse
        if (Mouse.current != null)
        {
            Vector2 look = Mouse.current.delta.ReadValue() * mouseSensitivity;
            xRotation = Mathf.Clamp(xRotation - look.y, -80f, 80f);
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            transform.Rotate(Vector3.up * look.x);
        }

        // Walk with W A S D
        float x = 0f;
        float z = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) z = 1f;
            if (Keyboard.current.sKey.isPressed) z = -1f;
            if (Keyboard.current.dKey.isPressed) x = 1f;
            if (Keyboard.current.aKey.isPressed) x = -1f;
        }

        Vector3 move = transform.right * x + transform.forward * z;

        // Gravity
        if (controller.isGrounded && yVelocity < 0f) yVelocity = -2f;
        yVelocity += gravity * Time.deltaTime;

        controller.Move((move * moveSpeed + Vector3.up * yVelocity) * Time.deltaTime);

        // Press Esc to get your mouse back
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}