using UnityEngine;

namespace Examples.Game.Scripts.Battle.Scene
{
    public class SceneConfig : MonoBehaviour
    {
        /// <summary>
        /// Camera for this scene.
        /// </summary>
        public Camera _camera;

        /// <summary>
        /// Nice actors can put themselves here - not to pollute top <c>GameObject</c> hierarchy.
        /// </summary>
        public GameObject actorParent;

        /// <summary>
        /// Parent <c>GameObject</c> for the ball and its related components.
        /// </summary>
        public GameObject ballParent;

        /// <summary>
        /// Player start (instantiation) positions on game arena.
        /// </summary>
        public Transform[] playerStartPos = new Transform[4];

        /// <summary>
        /// Ball needs to know where it travels and collides, this is area for upper team activity.
        /// </summary>
        public Collider2D upperTeamCollider;

        /// <summary>
        /// Ball needs to know where it travels and collides, this is area for lower team activity.
        /// </summary>
        public Collider2D lowerTeamCollider;

        /// <summary>
        /// Ball needs to know where it travels and collides, this is area for upper team activity.
        /// </summary>
        public SpriteRenderer upperTeamSprite;

        /// <summary>
        /// Ball needs to know where it travels and collides, this is area for lower team activity.
        /// </summary>
        public SpriteRenderer lowerTeamSprite;

        public static SceneConfig Get()
        {
            if (_Instance == null)
            {
                _Instance = FindObjectOfType<SceneConfig>();
                if (_Instance == null)
                {
                    throw new UnityException("SceneConfig not found");
                }
            }
            return _Instance;
        }

        private static SceneConfig _Instance;
    }
}