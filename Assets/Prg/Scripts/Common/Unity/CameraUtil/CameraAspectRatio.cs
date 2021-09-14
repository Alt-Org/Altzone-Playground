using UnityEngine;

namespace Prg.Scripts.Common.Unity.CameraUtil
{
    /// <summary>
    /// Crops camera to desired aspect ratio adding letterbox / pillarbox bars if required.
    /// See: https://gamedev.stackexchange.com/questions/144575/how-to-force-keep-the-aspect-ratio-and-specific-resolution-without-stretching-th/144578#144578
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraAspectRatio : MonoBehaviour
    {
        // Set this to your target aspect ratio, eg. (16, 9) or (4, 3).
        public Vector2 targetAspectRatio = new Vector2(9, 16);

        private Camera _camera;

        private void Awake()
        {
            var allowedPlatform = Application.platform == RuntimePlatform.Android ||
                                  Application.platform == RuntimePlatform.IPhonePlayer ||
                                  Application.platform == RuntimePlatform.WindowsPlayer ||
                                  Application.platform == RuntimePlatform.WindowsEditor;
            if (!allowedPlatform)
            {
                enabled = false;
            }
        }

        private void OnEnable()
        {
            _camera = GetComponent<Camera>();
            updateCrop();
        }

#if UNITY_EDITOR
        private int width;
        private int height;

        private void Update()
        {
            if (height != Screen.height || width != Screen.width)
            {
                height = Screen.height;
                width = Screen.width;
                updateCrop();
            }
        }

#endif

        private void OnPreCull()
        {
            // https://forum.unity.com/threads/force-camera-aspect-ratio-16-9-in-viewport.385541/
            var wp = _camera.rect;
            var nr = new Rect(0, 0, 1, 1);

            _camera.rect = nr;
            GL.Clear(true, true, Color.black);
            _camera.rect = wp;
        }

        private void updateCrop()
        {
            // Determine ratios of screen/window & target, respectively.
            var screenRatio = Screen.width / (float) Screen.height;
            var targetRatio = targetAspectRatio.x / targetAspectRatio.y;

            if (Mathf.Approximately(screenRatio, targetRatio))
            {
                // Screen or window is the target aspect ratio: use the whole area.
                _camera.rect = new Rect(0, 0, 1, 1);
            }
            else if (screenRatio > targetRatio)
            {
                // Screen or window is wider than the target: pillarbox.
                var normalizedWidth = targetRatio / screenRatio;
                var barThickness = (1f - normalizedWidth) / 2f;
                _camera.rect = new Rect(barThickness, 0, normalizedWidth, 1);
            }
            else
            {
                // Screen or window is narrower than the target: letterbox.
                var normalizedHeight = screenRatio / targetRatio;
                var barThickness = (1f - normalizedHeight) / 2f;
                _camera.rect = new Rect(0, barThickness, 1, normalizedHeight);
            }
        }
    }
}