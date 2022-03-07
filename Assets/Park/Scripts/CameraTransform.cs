using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTransform : MonoBehaviour
{
    public Transform targetTransform;
    public Vector3 position;
    public Vector3 rotation;
    public bool useWorldSpace = true;
    public float tweenDuration = 1.0f;

    //private Coroutine _currentCoroutine = null;


    /*
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    */

    private IEnumerator TweenMoveRotate(float duration, Transform target)
    {
        Vector3 startPosition = targetTransform.position;
        Quaternion startRotation = targetTransform.rotation;

        float t = 0;
        float timeElapsed = 0;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            t = timeElapsed / duration;

            targetTransform.position = Vector3.Lerp(startPosition, target.position, t);
            targetTransform.rotation = Quaternion.Lerp(startRotation, target.rotation, t);
            yield return null;
        }
        targetTransform.position = target.position;
        targetTransform.rotation = target.rotation;



    }

}
