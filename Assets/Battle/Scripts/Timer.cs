using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Altzone.Scripts.Apu
{
    public class Timer : MonoBehaviour
    {
        [Header("Event Settings"), SerializeField, Min(1)] private int timerDelayMs;
        [SerializeField] private bool isContinuous;
        public UnityEvent timer;

        private void onTimer()
        {
            Debug.Log($"onTimer isContinuous={isContinuous} timerDelayMs={timerDelayMs}");
            timer.Invoke();
            if (!isContinuous)
            {
                enabled = false;
            }
        }

        private WaitForSeconds delay;
        private int curDelay;
        private Coroutine unityTimer;

        private void OnEnable()
        {
            Debug.Log("OnEnable");
            if (unityTimer != null)
            {
                StopCoroutine(unityTimer);
            }
            if (delay == null || curDelay != timerDelayMs)
            {
                curDelay = timerDelayMs;
                delay = new WaitForSeconds(timerDelayMs / 1000f);
            }
            unityTimer = StartCoroutine(_onTimer());
        }

        private IEnumerator _onTimer()
        {
            for (;;)
            {
                yield return delay;
                onTimer();
            }
        }

        private void OnDisable()
        {
            Debug.Log("OnDisable");
            StopCoroutine(unityTimer);
            unityTimer = null;
        }
    }
}