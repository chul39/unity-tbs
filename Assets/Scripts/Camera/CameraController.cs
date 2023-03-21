using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{

    private const float MIN_FOLLOW_Y_OFFSET = 2f;
    private const float MAX_FOLLOW_Y_OFFSET = 12f;

    [SerializeField] CinemachineVirtualCamera cinemachineVirtualCamera;

    private CinemachineTransposer cinemachineTransposer;
    private Vector3 targetFollowOffset;

    private void Start()
    {
        cinemachineTransposer = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        targetFollowOffset = cinemachineTransposer.m_FollowOffset;
    }

    private void Update()
    {
        // Camera position
        Vector3 inputMoveDir = new Vector3(0, 0, 0);
        float moveSpeed = 10f;
        if (Input.GetKey(KeyCode.W)) inputMoveDir.z = +1f;
        if (Input.GetKey(KeyCode.S)) inputMoveDir.z = -1f;
        if (Input.GetKey(KeyCode.A)) inputMoveDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) inputMoveDir.x = +1f;
        Vector3 moveVector = transform.forward * inputMoveDir.z + transform.right * inputMoveDir.x;
        transform.position += moveVector * moveSpeed * Time.deltaTime;

        // Camera rotation
        Vector3 rotationVector = new Vector3(0, 0, 0);
        float rotationSpeed = 100f;
        if (Input.GetKey(KeyCode.Q)) rotationVector.y = +1f;
        if (Input.GetKey(KeyCode.E)) rotationVector.y = -1f;
        if (Input.GetMouseButton(1)) rotationVector.y += 2f * Input.GetAxis("Mouse X");
        //rotationVector.y += Input.GetAxis("Mouse Y");
        transform.eulerAngles += rotationVector * rotationSpeed * Time.deltaTime;

        // Camera zoom
        float zoomAmount = 1f;
        float zoomSpeed = 5f;
        if (Input.mouseScrollDelta.y < 0) targetFollowOffset.y += zoomAmount;
        if (Input.mouseScrollDelta.y > 0) targetFollowOffset.y -= zoomAmount;
        targetFollowOffset.y = Mathf.Clamp(targetFollowOffset.y, MIN_FOLLOW_Y_OFFSET, MAX_FOLLOW_Y_OFFSET);
        cinemachineTransposer.m_FollowOffset = Vector3.Lerp(cinemachineTransposer.m_FollowOffset, targetFollowOffset, Time.deltaTime * zoomSpeed);
    }
}
