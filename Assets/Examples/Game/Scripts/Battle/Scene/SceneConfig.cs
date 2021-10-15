using UnityEngine;

namespace Examples.Game.Scripts.Battle.Scene
{
    public class SceneConfig : MonoBehaviour
    {
        /// <summary>
        /// Nice actors can put themselves here - not to pollute top <c>GameObject</c> hierarchy.
        /// </summary>
        public GameObject actorParent;

        /// <summary>
        /// Ball needs to know where it travels and collides, this is area for upper team activity.
        /// </summary>
        public Collider2D upperTeam;

        /// <summary>
        /// Ball needs to know where it travels and collides, this is area for lower team activity.
        /// </summary>
        public Collider2D lowerTeam;
    }
}