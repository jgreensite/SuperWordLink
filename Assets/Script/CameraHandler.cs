using UnityEngine;

//code heavily influenced by the following
//https://kylewbanks.com/blog/unity3d-panning-and-pinch-to-zoom-camera-with-touch-and-mouse-input

namespace Script
{
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

        private Camera myCam;
        public Camera playerCam;
        public Camera callerCam;

        public bool isPanZoom;

        private Vector3 lastPanPosition;
        private Vector2[] lastZoomPositions; // Touch mode only
        private int panFingerId; // Touch mode only

        private bool wasZoomingLastFrame; // Touch mode only
        public float[] ZoomBounds = new float[2];
        //private List<GameObject> allObjectsInScene;
    
        private void Start()
        {
            myCam = GetComponent<Camera>();
            //allObjectsInScene = Helper.GetAllObjectsInScene();
            switch (myCam.name)
            {
                case "Player Camera":
                    myCam.transform.LookAt(GameBoard.Instance.gameBoardPlayer.transform);
                    break;
                case "Caller Camera":
                    myCam.CopyFrom(playerCam);
                    //myCam.CopyFrom((allObjectsInScene.FirstOrDefault(x => x.name == "Player Camera").GetComponent<Camera>()));
                    //cam.CopyFrom(GameObject.Find("Player Camera").GetComponent<Camera>());
                    myCam.transform.position += GameBoard.Instance.callerCardOffset;
                    myCam.transform.LookAt(GameBoard.Instance.gameBoardCaller.transform);
                    break;
                default:
                    Debug.Log("Error - unknown Camera " + myCam.name);
                    break;
            } 
        }

        private void Update()
        {
            switch (myCam.name)
            {
                case "Player Camera":
                    myCam.transform.LookAt(GameBoard.Instance.gameBoardPlayer.transform);
                    break;
                case "Caller Camera":
                    myCam.transform.LookAt(GameBoard.Instance.gameBoardCaller.transform);
                    break;
                default:
                    Debug.Log("Error - unknown Camera " + myCam.name);
                    break;
            }
        
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
            var offset = myCam.ScreenToViewportPoint(lastPanPosition - newPanPosition);
            var move = new Vector3(offset.x * PanSpeed, 0, offset.y * PanSpeed);

            // Perform the movement
            transform.Translate(move, Space.World);

            // Ensure the camera remains within bounds.
            var pos = transform.position;
            pos.x = Mathf.Clamp(transform.position.x, BoundsX[0], BoundsX[1]);
            if (myCam.name == "Player Camera") pos.z = Mathf.Clamp(transform.position.z, BoundsZ[0], BoundsZ[1]);
            else if (myCam.name == "Caller Camera") pos.z = Mathf.Clamp(transform.position.z, BoundsZ[0]+CS.CALLERCARDOFFSET, BoundsZ[1]+CS.CALLERCARDOFFSET);
            transform.position = pos;
            Debug.Log("x:" + pos.x + " " + "z:" + pos.z);

            // Cache the position
            lastPanPosition = newPanPosition;
        }

        private void ZoomCamera(float offset, float speed)
        {
            if (offset == 0) return;

            myCam.fieldOfView = Mathf.Clamp(myCam.fieldOfView - offset * speed, ZoomBounds[0], ZoomBounds[1]);
        }
    }
}