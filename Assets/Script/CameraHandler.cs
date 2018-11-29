﻿using UnityEngine;

//code heavily influenced by the following
//https://kylewbanks.com/blog/unity3d-panning-and-pinch-to-zoom-camera-with-touch-and-mouse-input

public class CameraHandler : MonoBehaviour
{
    //TODO - When zooming you need to make the UI non-clickable, possible by putting a toggle on the UI that determines if it is clickable

    private static readonly float PanSpeed = 5f;
    private static readonly float ZoomSpeedTouch = 0.1f;
    private static readonly float ZoomSpeedMouse = 0.5f;

    public float[] BoundsX = new float[2];
    public float[] BoundsZ = new float[2];

//	public float[] BoundsX = new float[]{-0.5f, 0.5f};
//	public float[] BoundsZ = new float[]{-2.5f, -1.5f};
//	public float[] ZoomBounds = new float[]{13f, 37f};

    private Camera cam;

    public bool isPanZoom;

    private Vector3 lastPanPosition;
    private Vector2[] lastZoomPositions; // Touch mode only
    private int panFingerId; // Touch mode only

    private bool wasZoomingLastFrame; // Touch mode only
    public float[] ZoomBounds = new float[2];

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        if (isPanZoom)
        {
            if (Input.touchSupported && Application.platform != RuntimePlatform.WebGLPlayer)
                HandleTouch();
            else
                HandleMouse();
        }
    }

    private void HandleTouch()
    {
        switch (Input.touchCount)
        {
            case 1: // Panning
                wasZoomingLastFrame = false;

                // If the touch began, capture its position and its finger ID.
                // Otherwise, if the finger ID of the touch doesn't match, skip it.
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    lastPanPosition = touch.position;
                    panFingerId = touch.fingerId;
                }
                else if (touch.fingerId == panFingerId && touch.phase == TouchPhase.Moved)
                {
                    PanCamera(touch.position);
                }

                break;

            case 2: // Zooming
                Vector2[] newPositions = {Input.GetTouch(0).position, Input.GetTouch(1).position};
                if (!wasZoomingLastFrame)
                {
                    lastZoomPositions = newPositions;
                    wasZoomingLastFrame = true;
                }
                else
                {
                    // Zoom based on the distance between the new positions compared to the 
                    // distance between the previous positions.
                    var newDistance = Vector2.Distance(newPositions[0], newPositions[1]);
                    var oldDistance = Vector2.Distance(lastZoomPositions[0], lastZoomPositions[1]);
                    var offset = newDistance - oldDistance;

                    ZoomCamera(offset, ZoomSpeedTouch);

                    lastZoomPositions = newPositions;
                }

                break;

            default:
                wasZoomingLastFrame = false;
                break;
        }
    }

    private void HandleMouse()
    {
        // On mouse down, capture it's position.
        // Otherwise, if the mouse is still down, pan the camera.
        if (Input.GetMouseButtonDown(0))
            lastPanPosition = Input.mousePosition;
        else if (Input.GetMouseButton(0)) PanCamera(Input.mousePosition);

        // Check for scrolling to zoom the camera
        var scroll = Input.GetAxis("Mouse ScrollWheel");
        ZoomCamera(scroll, ZoomSpeedMouse);
    }

    private void PanCamera(Vector3 newPanPosition)
    {
        // Determine how much to move the camera
        var offset = cam.ScreenToViewportPoint(lastPanPosition - newPanPosition);
        var move = new Vector3(offset.x * PanSpeed, 0, offset.y * PanSpeed);

        // Perform the movement
        transform.Translate(move, Space.World);

        // Ensure the camera remains within bounds.
        var pos = transform.position;
        pos.x = Mathf.Clamp(transform.position.x, BoundsX[0], BoundsX[1]);
        pos.z = Mathf.Clamp(transform.position.z, BoundsZ[0], BoundsZ[1]);
        transform.position = pos;
        Debug.Log("x:" + pos.x + " " + "z:" + pos.z);

        // Cache the position
        lastPanPosition = newPanPosition;
    }

    private void ZoomCamera(float offset, float speed)
    {
        if (offset == 0) return;

        cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - offset * speed, ZoomBounds[0], ZoomBounds[1]);
    }
}