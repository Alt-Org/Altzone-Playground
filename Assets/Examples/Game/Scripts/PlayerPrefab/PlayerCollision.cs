using Photon.Pun;
using Prg.Scripts.Common.PubSub;
using UnityEngine;

namespace Examples.Game.Scripts.PlayerPrefab
{
    /// <summary>
    /// Handle collisions with ball and control player colliders state after collision.
    /// </summary>
    /// <remarks>
    /// Colliders can are disabled and enabled depending on game and ball state.
    /// </remarks>
    public class PlayerCollision : MonoBehaviour
    {
        [Header("Settings"), SerializeField] private GameObject playerHead;
        [SerializeField] private Collider2D headCollider;
        [SerializeField] private Collider2D[] shieldColliders;
        [SerializeField] private LayerMask collisionToHeadMask;
        [SerializeField] private int collisionToHead;
        [SerializeField] private PlayerMovement playerMovement;

        [Header("Live Data"), SerializeField] private int playerPos;
        [SerializeField] private int teamIndex;
        [SerializeField] private bool isCollidersEnabled;

        private void Awake()
        {
            collisionToHead = collisionToHeadMask.value;
        }

        private void OnEnable()
        {
            this.Subscribe<BallMovement.ActiveTeamEvent>(OnActiveTeamEvent);
            this.Subscribe<BallMovement.CollisionEvent>(OnCollisionEvent);
            startPlaying();
        }

        private void OnDisable()
        {
            this.Unsubscribe();
        }

        private void OnCollisionEvent(BallMovement.CollisionEvent data)
        {
            // var hasLayer = layerMask == (layerMask | 1 << _layer); // unity3d check if layer mask contains a layer

            var colliderMask = 1 << data.layer;
            var hasLayer = collisionToHead == (collisionToHead | colliderMask);
            if (hasLayer)
            {
                if (data.hitObject.Equals(playerHead))
                {
                    //-Debug.Log($"OnCollisionEvent {data}");
                    disableColliders();
                    playerMovement.enableGhostMove();
                }
            }
        }

        private void OnActiveTeamEvent(BallMovement.ActiveTeamEvent data)
        {
            //-Debug.Log($"OnActiveTeamEvent {data}");
            if (data.teamIndex != teamIndex)
            {
                // Ball has left our side
                enableColliders();
            }
        }

        private void enableColliders()
        {
            setColliders(true);
        }

        private void disableColliders()
        {
            setColliders(false);
      }

        private void setColliders(bool state)
        {
            if (isCollidersEnabled == state)
            {
                return;
            }
            isCollidersEnabled = state;
            headCollider.enabled = state;
            for (var i = 0; i < shieldColliders.Length; i++)
            {
                shieldColliders[i].enabled = state;
            }
        }

        private void startPlaying()
        {
            enableColliders();
            var player = PhotonView.Get(this).Owner;
            GameManager.getPlayerProperties(player, out playerPos, out teamIndex);
        }
    }
}