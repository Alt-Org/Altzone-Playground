using UnityEngine;
using UnityEngine.EventSystems;

namespace Prg.Scripts.Common.Unity.Input
{
    public class TouchHandler : BaseHandler
    {
        [Header("Debug")] [SerializeField] private int touchCount;
        [SerializeField] private Vector3 firstPanPosition;
        [SerializeField] private Vector3 lastPanPosition;
        [SerializeField] private int panFingerId;
        [SerializeField] private bool isFingerDown;
        [SerializeField] private bool zoomActive;

        [SerializeField] private Vector2[] lastZoomPositions;

        private void Update()
        {
            switch (UnityEngine.Input.touchCount)
            {
                case 1: // Panning
                    // If the touch began, capture its position and its finger ID.
                    // Otherwise, if the finger ID of the touch doesn't match, skip it.
                    zoomActive = false;
                    var touch = UnityEngine.Input.GetTouch(0);
                    if (touch.phase == TouchPhase.Began)
                    {
                        // Check if finger is over a UI element
                        IsPointerOverGameObject = EventSystem.current.IsPointerOverGameObject(touch.fingerId);
                        if (IsPointerOverGameObject)
                        {
                            return;
                        }
                        isFingerDown = true;
                        firstPanPosition = touch.position;
                        touchCount = 1;
                        SendMouseDown(firstPanPosition, touchCount);
                        panFingerId = touch.fingerId;
                    }
                    else if (touch.fingerId == panFingerId && touch.phase == TouchPhase.Moved)
                    {
                        lastPanPosition = touch.position;
                        touchCount += 1;
                        SendMouseDown(lastPanPosition, touchCount);
                        PanCamera((firstPanPosition - lastPanPosition) * panSpeed);
                    }
                    break;

                case 2: // Zooming
                    var newPositions = new[] { UnityEngine.Input.GetTouch(0).position, UnityEngine.Input.GetTouch(1).position };
                    if (!zoomActive)
                    {
                        lastZoomPositions = newPositions;
                        zoomActive = true;
                    }
                    else
                    {
                        // Zoom based on the distance between the new positions compared to the distance between the previous positions.
                        var newDistance = Vector2.Distance(newPositions[0], newPositions[1]);
                        var oldDistance = Vector2.Distance(lastZoomPositions[0], lastZoomPositions[1]);
                        var offset = newDistance - oldDistance;
                        ZoomCamera(offset * zoomSpeed);
                        lastZoomPositions = newPositions;
                    }
                    break;

                default:
                    zoomActive = false;
                    if (isFingerDown)
                    {
                        isFingerDown = false;
                        SendMouseUp();
                    }
                    break;
            }
        }
    }
}