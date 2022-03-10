using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTransform : MonoBehaviour
{
    public Transform targetTransform;
    public Vector3 position;
    public Vector3 rotation;
    public float tweenDuration = 1.0f;
    public bool useWorldSpace = true;
    private Coroutine currentCoroutine = null;


    public void TweenToRotation()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }

        if (gameObject.activeInHierarchy && targetTransform != null)
        {
            Quaternion targetRotation = Quaternion.Euler(rotation);
            currentCoroutine = StartCoroutine(TweenRotate(tweenDuration, targetRotation, useWorldSpace));
        }
    }


    private IEnumerator TweenRotate(float duration, Quaternion targetRotation, bool worldSpace)
    {
        Quaternion startRotation = worldSpace ? targetTransform.rotation : targetTransform.localRotation;

        float t = 0;
        float timeElapsed = 0;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            t = timeElapsed / duration;

            if (worldSpace)
            {
                targetTransform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
            }
            else
            {
                targetTransform.localRotation = Quaternion.Lerp(startRotation, targetRotation, t);
            }
            yield return null;
        }

        if (worldSpace)
        {
            targetTransform.rotation = targetRotation;
        }
        else
        {
            targetTransform.localRotation = targetRotation;
        }
    }

}






