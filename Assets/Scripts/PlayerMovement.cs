
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public Camera gameplayCamera;

    [Tooltip("Velocidad maxima de movimiento del jugador.")]
    public float speed = 6f;

    [Tooltip("Que tan rapido alcanza la velocidad maxima al empezar a moverse.")]
    public float acceleration = 12f;

    [Tooltip("Que tan rapido se frena al soltar el movimiento.")]
    public float deceleration = 16f;

    [Tooltip("Que tan rapido gira visualmente hacia la direccion de movimiento.")]
    public float rotationSpeed = 12f;

    public float gravity = -9.81f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    Vector3 velocity;
    Vector3 currentMove;
    bool isGrounded;

    void Update()
    {
        Camera activeCamera = gameplayCamera != null
            ? gameplayCamera
            : Camera.main;

        isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundDistance,
            groundMask
        );

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        Vector2 input = ReadMovementInput();

        Vector3 targetMove = GetCameraRelativeMove(activeCamera, input.x, input.y);
        float smoothing = targetMove.sqrMagnitude > 0.001f
            ? acceleration
            : deceleration;

        currentMove = Vector3.MoveTowards(
            currentMove,
            targetMove,
            smoothing * Time.deltaTime
        );

        if (currentMove.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(currentMove);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        controller.Move(currentMove * speed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    Vector3 GetCameraRelativeMove(Camera activeCamera, float x, float z)
    {
        if (activeCamera == null)
        {
            Vector3 fallbackMove = Vector3.right * x + Vector3.forward * z;

            return fallbackMove.sqrMagnitude > 1f
                ? fallbackMove.normalized
                : fallbackMove;
        }

        Vector3 cameraForward = Vector3.ProjectOnPlane(
            activeCamera.transform.forward,
            Vector3.up
        ).normalized;

        Vector3 cameraRight = Vector3.ProjectOnPlane(
            activeCamera.transform.right,
            Vector3.up
        ).normalized;

        Vector3 move = cameraRight * x + cameraForward * z;

        return move.sqrMagnitude > 1f
            ? move.normalized
            : move;
    }

    Vector2 ReadMovementInput()
    {
        Vector2 input = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed ||
                Keyboard.current.leftArrowKey.isPressed)
            {
                input.x -= 1f;
            }

            if (Keyboard.current.dKey.isPressed ||
                Keyboard.current.rightArrowKey.isPressed)
            {
                input.x += 1f;
            }

            if (Keyboard.current.sKey.isPressed ||
                Keyboard.current.downArrowKey.isPressed)
            {
                input.y -= 1f;
            }

            if (Keyboard.current.wKey.isPressed ||
                Keyboard.current.upArrowKey.isPressed)
            {
                input.y += 1f;
            }
        }

        if (Gamepad.current != null)
        {
            input += Gamepad.current.leftStick.ReadValue();
        }

        return input.sqrMagnitude > 1f
            ? input.normalized
            : input;
    }
}
