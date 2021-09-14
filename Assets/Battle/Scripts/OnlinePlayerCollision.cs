using UnityEngine;

namespace Altzone.Nelinpeli
{
    public class OnlinePlayerCollision : MonoBehaviour
    {
        public string myColliderName;
        public string otherColliderName;
        public GameObject lastHit;

        private void OnCollisionEnter2D(Collision2D other)
        {
            myColliderName = other.otherCollider.name;
            otherColliderName = other.collider.name;
            lastHit = other.gameObject;
            Debug.Log($"HIT {myColliderName} <- {otherColliderName}");
        }
    }
}
