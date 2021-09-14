using Prg.Scripts.Common.PubSub;
using Prg.Scripts.Common.Unity.Input;
using Prg.Scripts.Common.Util;
using UnityEngine;

namespace Altzone.Scripts.Apu
{
    public class LongPressSprite : MonoBehaviour
    {
        [Header("UI Settings")] public Camera _worldCamera;
        public GameObject effectParent;
        public SpriteRenderer spriteEffect;
        public SpriteRenderer spriteEffectBackground;
        public Color flashingColor;

        [Header("Time Settings"), Min(0f)] public float _longPressDuration;
        [Min(0f)] public float _effectStartDelay;
        [Min(0f)] public float _flashDuration;

        [Header("Event Settings"), SerializeField] private Vector2 eventThreshold;
        public UnityEventVector2 longPress;

        [Header("Live Data"), SerializeField] private float minScaleFactor;
        [SerializeField] private float maxScaleFactor;
        public Color normalColor;

        [Header("Debug"), SerializeField] private float firstClickTime;
        [SerializeField] private Vector3 firstWorldPosition;
        [SerializeField] private float curDuration;
        [SerializeField] private float curScaleFactor;
        [SerializeField] private Vector3 curWorldPosition;
        [SerializeField] private Vector3 curScreenPosition;
        [SerializeField] private float curFlashDuration;
        [SerializeField] private float deltaX;
        [SerializeField] private float deltaY;
        [SerializeField] private bool isCancelled;

        private bool isInitialized;
        private Transform effectParentTransform;
        private Transform effectTransform;
        private ILinearConverter<float> linearConverter;

        // To keep liner conversion routine consistent
        private float maxLongPressDuration => _longPressDuration;
        private float maxFlashDuration => _flashDuration;

        private void OnEnable()
        {
            if (!isInitialized)
            {
                isInitialized = true;
                effectParentTransform = effectParent.transform;
                effectTransform = spriteEffect.transform;
                minScaleFactor = effectTransform.localScale.x;
                maxScaleFactor = spriteEffectBackground.transform.localScale.x;
                normalColor = spriteEffect.color;
                hideSprite();
                linearConverter = LinearConverter.Get(0f, maxLongPressDuration, minScaleFactor, maxScaleFactor);
            }
            this.Subscribe<InputManager.ClickDownEvent>(onClickDownEvent);
            this.Subscribe<InputManager.ClickUpEvent>(onClickUpEvent);
        }

        private void OnDisable()
        {
            this.Unsubscribe<InputManager.ClickDownEvent>(onClickDownEvent);
            this.Unsubscribe<InputManager.ClickUpEvent>(onClickUpEvent);
        }

        private void onClickDownEvent(InputManager.ClickDownEvent data)
        {
            curScreenPosition = data.ScreenPosition;
            if (data.ClickCount == 1)
            {
                showSprite(_worldCamera.ScreenToWorldPoint(curScreenPosition), Time.unscaledTime);
                return;
            }
            if (isCancelled)
            {
                return;
            }
            curDuration = Time.unscaledTime - firstClickTime;
            curScaleFactor = linearConverter.mapValue(curDuration);
            curWorldPosition = _worldCamera.ScreenToWorldPoint(curScreenPosition);
            if (eventThreshold.x > 0f)
            {
                deltaX = Mathf.Abs(firstWorldPosition.x - curWorldPosition.x);
                if (deltaX > eventThreshold.x)
                {
                    Debug.Log($"Long press SKIP deltaX {deltaX:F2}");
                    isCancelled = true;
                    hideSprite();
                    return;
                }
            }
            if (eventThreshold.y > 0f)
            {
                deltaY = Mathf.Abs(firstWorldPosition.y - curWorldPosition.y);
                if (deltaY > eventThreshold.y)
                {
                    Debug.Log($"Long press SKIP deltaY {deltaY:F2}");
                    isCancelled = true;
                    hideSprite();
                    return;
                }
            }
            updateSprite();
        }

        private void onClickUpEvent(InputManager.ClickUpEvent data)
        {
            if (isCancelled)
            {
                return;
            }
            hideSprite();
            if (!(curDuration < maxLongPressDuration))
            {
                Debug.Log($"Long press SENT {curDuration:F1} s X={curScreenPosition.x},Y={curScreenPosition.x}");
                longPress.Invoke(curScreenPosition);
            }
        }

        private void resetSprite(Vector3 position, float time)
        {
            isCancelled = false;
            firstWorldPosition = position;
            firstClickTime = time;
            curDuration = 0f;
            curScaleFactor = minScaleFactor;
            curWorldPosition = position;
            curFlashDuration = 0f;
            spriteEffect.color = normalColor;
            updateSprite();
        }

        private bool isSpriteVisible => effectParent.activeSelf;

        private void showSprite(Vector3 position, float time)
        {
            resetSprite(position, time);
            var isShowImmediately = _effectStartDelay == 0f;
            effectParent.SetActive(isShowImmediately);
       }

        private void hideSprite()
        {
            effectParent.SetActive(false);
        }

        private void updateSprite()
        {
            var localScale = effectTransform.localScale;
            localScale.x = curScaleFactor;
            localScale.y = curScaleFactor;
            effectTransform.localScale = localScale;

            var position = effectParentTransform.position;
            position.x = curWorldPosition.x;
            position.y = curWorldPosition.y;
            effectParentTransform.position = position;

            if (!isCancelled && !isSpriteVisible && _effectStartDelay > 0f && curDuration > _effectStartDelay)
            {
                effectParent.SetActive(true);
            }
            if (curScaleFactor < maxScaleFactor || maxFlashDuration == 0f)
            {
                return;
            }
            curFlashDuration += Time.deltaTime;
            if (curFlashDuration > maxFlashDuration)
            {
                curFlashDuration = 0f;
                if (spriteEffect.color != flashingColor)
                {
                    // Just change color on first time
                    spriteEffect.color = flashingColor;
                }
                else
                {
                    // Then start flashing
                    effectParent.SetActive(!effectParent.activeSelf);
                }
            }
        }
    }
}