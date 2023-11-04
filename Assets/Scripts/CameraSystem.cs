using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class VirtualCameraMovement : MonoBehaviour
{
    public NavMeshAgent agent;

    [Range(1, 2)]
    public float cameraSpeed;
    [Range(1, 10)]
    public float moveSpeed;
    [Range(1, 10)]
    public float rotationSpeed;
    [Range(1, 2)]
    public float dragPanSpeed;
    [Range(0, 2)]
    public int dragMouseButton;
    public int edgeScrollSize;

    [SerializeField] private CinemachineVirtualCamera cinemachineVirtualCamera;
    [SerializeField] private bool edgeScrollingEnabled = false;
    [SerializeField] private bool dragPanEnabled = false;
    [SerializeField] private bool lockCameraOnPlayer = false;
    [SerializeField] private float minFollowOffset = 5f;
    [SerializeField] private float maxFollowOffset = 50f;
    [SerializeField] private float minFollowOffsetY = 2f;
    [SerializeField] private float maxFollowOffsetY = 30f;

    private bool mouseDragActive = false;
    private bool mouseAngleActive = false;
    private float targetFieldOfView = 40f;
    private float minTargetFieldOfView = 10f;
    private float maxTargetFieldOfView = 50f;

    private Vector2 lastMousePosition;
    private Vector3 followOffset;
    private Vector3 inputDirection;

    private void Awake()
    {
        followOffset = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset;
        followOffset.z = -7f;
        followOffset.y = 10f;
    }

    private void Update()
    {
        HandleCameraRotation();
        HandleCameraMovement(inputDirection);
        HandleCameraDragging(inputDirection);
        HandleCameraEdgeScrolling(inputDirection);
        HandleCameraZoom_Y_Position();
        LockCamera();


        // HandleCameraZoom_MoveForward();
        // HandleCameraZoom_FieldOfView();
        // HandleCameraPan();
    }

    private void HandleCameraMovement(Vector3 inputDirection)
    {
        if (Input.GetKey(KeyCode.W))
        {
            inputDirection.z = +moveSpeed;
        }
        if (Input.GetKey(KeyCode.S))
        {
            inputDirection.z = -moveSpeed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            inputDirection.x = +moveSpeed;
        }
        if (Input.GetKey(KeyCode.A))
        {
            inputDirection.x = -moveSpeed;
        }

        TransformCameraPosition(inputDirection);
    }

    private void HandleCameraEdgeScrolling(Vector3 inputDirection)
    {
        if (edgeScrollingEnabled)
        {
            if (Input.mousePosition.x < edgeScrollSize)
            {
                inputDirection.x = -moveSpeed;
            }
            if (Input.mousePosition.y < edgeScrollSize)
            {
                inputDirection.z = -moveSpeed;
            }
            if (Input.mousePosition.x > Screen.width - edgeScrollSize)
            {
                inputDirection.x = +moveSpeed;
            }
            if (Input.mousePosition.y > Screen.height - edgeScrollSize)
            {
                inputDirection.z = +moveSpeed;
            }

            TransformCameraPosition(inputDirection);
        }
    }

    private void HandleCameraRotation()
    {
        float rotationDirection = 0f;

        if (Input.GetKey(KeyCode.Q))
        {
            rotationDirection = -rotationSpeed;
        }
        if (Input.GetKey(KeyCode.E))
        {
            rotationDirection = +rotationSpeed;
        }

        transform.eulerAngles += new Vector3(0, rotationDirection * rotationSpeed * Time.deltaTime, 0);
    }

    private void HandleCameraDragging(Vector3 inputDirection)
    {
        if (dragPanEnabled)
        {
            if (Input.GetMouseButtonDown(dragMouseButton))
            {
                mouseDragActive = true;
                lastMousePosition = Input.mousePosition;
            }
            if (Input.GetMouseButtonUp(dragMouseButton))
            {
                mouseDragActive = false;
            }

            if (mouseDragActive)
            {
                Vector2 mouseMovementDelta = (Vector2)Input.mousePosition - lastMousePosition;

                inputDirection.x = mouseMovementDelta.x * dragPanSpeed;
                inputDirection.z = mouseMovementDelta.y * dragPanSpeed;

                TransformCameraPosition(inputDirection);

                lastMousePosition = Input.mousePosition;
            }
        }
    }

    private void HandleCameraZoom_FieldOfView()
    {
        if (Input.mouseScrollDelta.y > 0)
        {
            targetFieldOfView -= 5;
        }
        if (Input.mouseScrollDelta.y < 0)
        {
            targetFieldOfView += 5;
        }

        targetFieldOfView = Mathf.Clamp(targetFieldOfView, minTargetFieldOfView, maxTargetFieldOfView);

        TransformCameraOffset();
    }

    private void HandleCameraZoom_MoveForward()
    {
        Vector3 zoomDirection = followOffset.normalized;

        if (Input.mouseScrollDelta.y > 0)
        {
            followOffset -= zoomDirection;
        }
        if (Input.mouseScrollDelta.y < 0)
        {
            followOffset += zoomDirection;
        }


        if (followOffset.magnitude < minFollowOffset)
        {
            followOffset = zoomDirection * minFollowOffset;
        }
        if (followOffset.magnitude > maxFollowOffset)
        {
            followOffset = zoomDirection * maxFollowOffset;
        }

        TransformCameraOffset();
    }

    private void HandleCameraPan()
    {
        if (Input.GetMouseButtonDown(2))
        {
            mouseAngleActive = true;
            lastMousePosition = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(2))
        {
            mouseAngleActive = false;
        }

        if (mouseAngleActive)
        {
            Vector2 mouseMovementDelta = (Vector2)Input.mousePosition - lastMousePosition;

            followOffset.x = mouseMovementDelta.x;
            followOffset.z = mouseMovementDelta.y;

            followOffset.x = Mathf.Clamp(followOffset.x, minFollowOffsetY, maxFollowOffsetY);
            followOffset.z = Mathf.Clamp(followOffset.z, minFollowOffsetY, maxFollowOffsetY);

            TransformCameraOffset();

            lastMousePosition = Input.mousePosition;
        }
    }

    private void HandleCameraZoom_Y_Position()
    {
        float zoomAmount = 1.5f;

        if (Input.mouseScrollDelta.y > 0)
        {
            followOffset.y -= zoomAmount;
        }
        if (Input.mouseScrollDelta.y < 0)
        {
            followOffset.y += zoomAmount;
        }

        followOffset.y = Mathf.Clamp(followOffset.y, minFollowOffsetY, maxFollowOffsetY);

        TransformCameraOffset();
    }

    private void LockCamera()
    {
        if (lockCameraOnPlayer)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                followOffset.z = -5f;
                followOffset.y = 7f;

                // cinemachineVirtualCamera.Follow = agent.transform;
                cinemachineVirtualCamera.LookAt = agent.transform;
            }
            if (Input.GetKeyUp(KeyCode.R))
            {
                followOffset.z = -7f;
                followOffset.y = 10f;

                cinemachineVirtualCamera.Follow = transform;
                cinemachineVirtualCamera.LookAt = transform;
            }
        }

    }

    private void TransformCameraPosition(Vector3 inputDirection)
    {
        Vector3 moveAction = (transform.forward * inputDirection.z) + (transform.right * inputDirection.x);
        transform.position += cameraSpeed * Time.deltaTime * moveAction;
    }

    private void TransformCameraOffset()
    {
        Vector3 currentOffset = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset;
        float zoomSpeed = 10f;
        cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset =
            Vector3.Lerp(currentOffset, followOffset, Time.deltaTime * zoomSpeed);
    }
}
