#define USE_NEW_INPUT_SYSTEM
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{

    public static InputManager Instance { get; private set; }

    private PlayerInputActions playerInputActions;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError($"Only a single instance of InputManager can exist at a time. ({transform} - {Instance})");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
    }
    
    public Vector2 GetMouseScreenPosition()
    {
        #if USE_NEW_INPUT_SYSTEM
            return Mouse.current.position.ReadValue();
        #else
            return Input.mousePosition;
        #endif
    }

    public bool IsMouseButtonDownThisFrame()
    {
        #if USE_NEW_INPUT_SYSTEM
            return playerInputActions.Player.LeftClick.WasPressedThisFrame();
        #else
            return Input.GetMouseButtonDown(0);
        #endif
    }

    public Vector2 GetCameraMoveVector()
    {
        #if USE_NEW_INPUT_SYSTEM
            return playerInputActions.Player.CameraMovement.ReadValue<Vector2>();
        #else
            Vector2 inputMoveDir = new Vector3(0, 0);
            if (Input.GetKey(KeyCode.W)) inputMoveDir.y = +1f;
            if (Input.GetKey(KeyCode.S)) inputMoveDir.y = -1f;
            if (Input.GetKey(KeyCode.A)) inputMoveDir.x = -1f;
            if (Input.GetKey(KeyCode.D)) inputMoveDir.x = +1f;
            return inputMoveDir;
        #endif
    }

    public float GetCameraRotateAmount()
    {
        #if USE_NEW_INPUT_SYSTEM
            float rotateAmount = 0f;
            rotateAmount += playerInputActions.Player.CameraRotate.ReadValue<float>();
            if (playerInputActions.Player.RightClick.IsPressed())
            {
                rotateAmount += playerInputActions.Player.CameraRotateMouse.ReadValue<float>();
            }
            return rotateAmount;
        #else
            float rotateAmount = 0f;
            if (Input.GetKey(KeyCode.Q)) rotateAmount = +1f;
            if (Input.GetKey(KeyCode.E)) rotateAmount = -1f;
            if (Input.GetMouseButton(1)) rotateAmount += 2f * Input.GetAxis("Mouse X");
            return rotateAmount;
        #endif
    }

    public float GetCameraZoomAmount()
    {
        #if USE_NEW_INPUT_SYSTEM
            return playerInputActions.Player.CameraZoom.ReadValue<float>();
        #else
            float zoomAmount = 0f;
            if (Input.mouseScrollDelta.y < 0) zoomAmount = 1f;
            if (Input.mouseScrollDelta.y > 0) zoomAmount = -1f;
            return zoomAmount;
        #endif
    }

}
