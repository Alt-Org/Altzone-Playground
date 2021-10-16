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
    }
}