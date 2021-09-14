using System;
using UnityEngine;

namespace Altzone.Nelinpeli
{
    public class ShieldTurns : MonoBehaviour
    {
        [Header("Transform Settings")] public Transform _transform;
        public Transform _otherTransform;

        [Header("Rotation Constants")] public float sqrMinPlayerRotationDistance;
        public float sqrMaxPlayerRotationDistance;
        public float sqrShieldDissappearDistance;
        public float playerRotationZ;
        [Range(0, 180)] public float minPlayerRotationAngle;
        [Range(0, 180)] public float maxPlayerRotationAngle;

        [Header("Live Data"), SerializeField]  private float sqrDistanceBetween;
        [SerializeField] private float zSide;
        [SerializeField] private float zAngle;
        [SerializeField] private Vector3 myPrevPosition;
        [SerializeField] private Vector3 otherPrevPosition;
        public GameObject shield;

        private bool isReversedRotation => playerRotationZ != 0f; // Lower size rotation is zero, upper side 180

        private void Awake()
        {
            // Sensible defaults
            if (maxPlayerRotationAngle == 0f)
            {
                maxPlayerRotationAngle = 180f;
            }
        }

        private void Update()
        {
            rotatePlayer();
        }

        private void rotatePlayer()
        {
            if (_otherTransform == null)
            {
                // Other player has left!
                this.enabled = false;
                return;
            }
            // TODO: check that x,y has changed on me and/or other before doing more calculations!
            var myPosition = _transform.position;
            var otherPosition = _otherTransform.position;
            if (myPosition == myPrevPosition && otherPosition == otherPrevPosition)
            {
                return;
            }
            myPrevPosition = myPosition;
            otherPrevPosition = otherPosition;
            sqrDistanceBetween = Mathf.Abs((myPosition - otherPosition).sqrMagnitude);
            zSide = otherPosition.x < myPosition.x ? -1f : 1f;
            if (sqrDistanceBetween > sqrShieldDissappearDistance) {
                shield.SetActive(false);
            } else {
                shield.SetActive(true);
            }
            if (sqrDistanceBetween > sqrMinPlayerRotationDistance)
            {
                if (sqrDistanceBetween < sqrMaxPlayerRotationDistance)
                {
                    zAngle = distanceToAngle(sqrDistanceBetween);
                    if (isReversedRotation)
                    {
                        zSide = -zSide;
                        zAngle += playerRotationZ;
                    }
                    // Side affects only when inside "rotation range"!
                    // Flipping from side to side when rotation angle is big causes kind of "glitch" as shield changes side very quickly :-(
                    _transform.rotation = Quaternion.Euler(0f, 0f, zAngle * zSide);
                }
                else
                {
                    zAngle = minPlayerRotationAngle;
                    if (isReversedRotation)
                    {
                        zAngle += playerRotationZ;
                    }
                    _transform.rotation = Quaternion.Euler(0f, 0f, zAngle);
                }
            }
            else
            {
                zAngle = maxPlayerRotationAngle;
                if (isReversedRotation)
                {
                    zAngle += playerRotationZ;
                }
                _transform.rotation = Quaternion.Euler(0f, 0f, zAngle);
            }
        }

        private float distanceToAngle(float sqrDistance)
        {
            // Linear conversion formula - could be optimized a bit!
            return (sqrDistance - sqrMinPlayerRotationDistance) / (sqrMaxPlayerRotationDistance - sqrMinPlayerRotationDistance) *
                Mathf.Abs(minPlayerRotationAngle - maxPlayerRotationAngle) + Mathf.Max(minPlayerRotationAngle, maxPlayerRotationAngle);
        }
    }
}