using UnityEngine;

namespace Examples.Game.Scripts.Battle.Ball
{
    /// <summary>
    ///  Puts the ball on the game using "sling shot" method between two team mates in position A and B.
    /// </summary>
    /// <remarks>
    /// Position A is end point of aiming and position B is start point of aiming.<br />
    /// Vector A-B provides direction and relative speed (increase or decrease) to the ball when it is started to the game.
    /// </remarks>
    public class BallSlingShot : MonoBehaviour
    {
        [SerializeField] private Transform positionA;
        [SerializeField] private Transform positionB;
        [SerializeField] private SpriteRenderer spriteA;
        [SerializeField] private SpriteRenderer spriteB;
        [SerializeField] private IBallControl ballControl;

        public void setup(IBallControl ballControl, Transform positionA, Transform positionB, SpriteRenderer spriteA, SpriteRenderer spriteB)
        {
            this.positionA = positionA;
            this.positionB = positionB;
            this.spriteA = spriteA;
            this.spriteB = spriteB;
            this.ballControl = ballControl;
        }
    }
}
