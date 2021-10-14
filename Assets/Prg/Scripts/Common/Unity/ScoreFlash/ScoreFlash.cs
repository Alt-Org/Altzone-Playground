using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Prg.Scripts.Common.Unity.ScoreFlash
{
    public class ScoreFlash : MonoBehaviour
    {
        [SerializeField] private float countDownTime;
        [SerializeField] private Vector3 punchForce;
        [SerializeField] private GameObject parent;
        [SerializeField] private TMP_Text tmpText;

        private Transform textTransform;

        private void OnEnable()
        {
            textTransform = tmpText.transform;
            parent.SetActive(true);
            move();
        }

        private void move()
        {
            // http://dotween.demigiant.com/documentation.php

            textTransform.DOPunchScale(punchForce, 1f).OnComplete(() =>
            {
                countDownTime -= 1f;
                tmpText.text = countDownTime.ToString("N0");
                if (countDownTime < 0)
                {
                    parent.SetActive(false);
                    gameObject.SetActive(false);
                }
                else
                {
                    move();
                }
            });
        }
    }
}