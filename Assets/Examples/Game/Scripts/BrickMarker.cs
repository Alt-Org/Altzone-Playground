using UnityEngine;

namespace Examples.Game.Scripts
{
    /// <summary>
    /// Unique marker for a brick so it can be identified over network and destroyed with proper effects.
    /// </summary>
    public class BrickMarker : MonoBehaviour
    {
        [SerializeField] private int brickId;

        public int BrickId
        {
            get => brickId;
            set => brickId = value;
        }

        public void destroyBrick()
        {
            Debug.Log($"destroyBrick {this}");
            if (!gameObject.activeSelf)
            {
                throw new UnityException("brick is not active: " + this);
            }
            gameObject.SetActive(false);
        }

        public override string ToString()
        {
            return $"BrickMarker #:{brickId} {gameObject.name}";
        }
    }
}