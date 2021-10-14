using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Prg.Scripts.Common.Unity.ScoreFlash
{
    public class ScoreFlash : MonoBehaviour
    {
        public string displayTextFormat;
        public float countDownTime;
        [SerializeField] private TMP_Text textMeshPro;

        private GameObject parent;
        private Transform parentTransform;
        private Vector3 endPosition;

        private void OnEnable()
        {
            parent = textMeshPro.gameObject;
            parentTransform = parent.transform;
            parent.SetActive(true);

            Debug.Log($"start countDownTime={countDownTime:0.0}");
            endPosition = Vector3.one;
            move();
        }

        private void move()
        {
            // http://dotween.demigiant.com/documentation.php

            parentTransform.DOPunchScale(endPosition, 1f).OnComplete(() =>
            {
                countDownTime -= 1f;
                textMeshPro.text = string.Format(displayTextFormat, countDownTime);
                Debug.Log($"start countDownTime={countDownTime:0.0}");
                if (countDownTime < 0)
                {
                    parent.SetActive(false);
                }
                else
                {
                    move();
                }
            });
        }
    }
}