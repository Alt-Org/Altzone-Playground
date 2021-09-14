using UnityEngine;

namespace Altzone.Nelinpeli
{
    public class OnlinePlayerShield : MonoBehaviour
    {
        [Header("Player Settings"), SerializeField, Range(0f, 90f)] private float minAngle;
        [SerializeField, Range(0f, 90f)] private float maxAngle;
        [SerializeField] private float initialAngle;
        [SerializeField] private Transform leftShieldTransform;
        [SerializeField] private Transform rightShieldTransform;
        [SerializeField] private OnlinePlayer player;
        [SerializeField] private SpriteRenderer head;
        [SerializeField] private CircleCollider2D headcollider;
        [SerializeField] private Sprite hp4psrite;
        [SerializeField] private Sprite hp3psrite;
        [SerializeField] private Sprite hp2psrite;
        [SerializeField] private Sprite hp1psrite;
        [SerializeField] private Sprite hp0psrite;
        [SerializeField] private float[] radi = new float[]{0.6f, 0.7f, 0.85f, 1.05f, 1.2f};


        [Header("Live Data"), SerializeField] private float curAngle;
        [SerializeField] private Collider2D leftCollider;
        [SerializeField] private Collider2D rightCollider;
        [SerializeField] private float angle = 0f;
        [SerializeField] private float health = 4f;
        [SerializeField] private Sprite[] sprites;

        private Vector3 localEulerAngles = new Vector3(0, 0, 0);
        private bool isInitialized;

        private void OnEnable()
        {
            if (!isInitialized)
            {
                isInitialized = true;
                curAngle = initialAngle;
                leftCollider = leftShieldTransform.GetComponent<Collider2D>();
                rightCollider = rightShieldTransform.GetComponent<Collider2D>();
                createSlave(leftCollider.gameObject, this, 9);
                createSlave(rightCollider.gameObject, this, 9);
                health = 4f;
                sprites = new Sprite[]{hp0psrite, hp1psrite, hp2psrite, hp3psrite, hp4psrite};
            }
        }

        public void onCollisionEnter2D(Collision2D collision)
        {
            if(collision.gameObject.layer == 9)
            {
                if (health == 0)
                {
                    player.playerStop(1);
                } else {
                    health -= 1;
                    angle += -15f;
                    head.sprite = sprites[(int)health];
                    headcollider.radius = radi[(int)health];
                }
            }
        }

        private void Update()
        {

            curAngle = Mathf.Clamp(curAngle, minAngle, maxAngle);
            localEulerAngles.z = -curAngle;
            leftShieldTransform.localEulerAngles  = localEulerAngles;
            localEulerAngles.z = curAngle;
            rightShieldTransform.localEulerAngles  = localEulerAngles;
            leftShieldTransform.Rotate(0.0f, 0.0f, 0f + (angle), Space.Self);
            rightShieldTransform.Rotate(0.0f, 0.0f, 0f + (angle*-1f), Space.Self);

        }
        
        private static void createSlave(GameObject parent, OnlinePlayerShield master, int layer)
        {
            var slave = parent.gameObject.AddComponent<CollisionSlave>();
            slave.master = master;
            slave.layer = layer;
        }
    }
}
