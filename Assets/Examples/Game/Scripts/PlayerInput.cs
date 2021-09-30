using Prg.Scripts.Common.PubSub;
using Prg.Scripts.Common.Unity.Input;
using UnityEngine;

namespace Examples.Game.Scripts
{
    /// <summary>
    /// Listens <c>InputManager</c> click down and up events and forwards them to player for processing.
    /// </summary>
    public class PlayerInput : MonoBehaviour
    {
        [Header("Live Data"), SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private Camera _camera;
        [SerializeField] protected Transform _transform;
        [SerializeField] private float playerPositionZ;
        [SerializeField] private Vector3 mousePosition;

        public Camera Camera
        {
            get => _camera;
            set => _camera = value;
        }

        public PlayerMovement PlayerMovement
        {
            get => playerMovement;
            set
            {
                playerMovement = value;
                _transform = transform;
                playerPositionZ = _transform.position.z;
                this.Subscribe<InputManager.ClickDownEvent>(onClickDownEvent);
                this.Subscribe<InputManager.ClickUpEvent>(onClickUpEvent);
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe always as photonView can be destroyed before us
            this.Unsubscribe<InputManager.ClickDownEvent>(onClickDownEvent);
            this.Unsubscribe<InputManager.ClickUpEvent>(onClickUpEvent);
        }

        private void onClickDownEvent(InputManager.ClickDownEvent data)
        {
            movePlayerTo(data.ScreenPosition);
        }

        private void onClickUpEvent(InputManager.ClickUpEvent data)
        {
            movePlayerTo(data.ScreenPosition);
        }

        private void movePlayerTo(Vector3 screenPosition)
        {
            mousePosition = _camera.ScreenToWorldPoint(screenPosition);
            mousePosition.z = playerPositionZ;
            playerMovement.moveTo(mousePosition);
        }
    }
}