using Examples.Game.Scripts.Battle.Player;
using System.Collections.Generic;
using UnityEngine;

namespace Examples.Game.Scripts.Battle.Ball
{
    public class BallSlingShotConfigurer : MonoBehaviour
    {
        public BallSlingShot upperSlingShot;
        public BallSlingShot lowerSlingShot;

        [SerializeField] private BallActor ballActor;
        [SerializeField] private PlayerActor[] playerActors;
        [SerializeField] private List<PlayerActor> teamUpper;
        [SerializeField] private List<PlayerActor> teamLower;

        private void OnEnable()
        {
            ballActor = FindObjectOfType<BallActor>();
            playerActors = FindObjectsOfType<PlayerActor>();
            teamUpper.Clear();
            teamLower.Clear();
            foreach (var playerActor in playerActors)
            {
                if (playerActor.TeamIndex == 0)
                {
                    teamLower.Add(playerActor);
                }
                else
                {
                    teamUpper.Add(playerActor);
                }
            }
            configure(lowerSlingShot, ballActor, teamLower);
            configure(upperSlingShot, ballActor, teamUpper);
        }

        private static void configure(BallSlingShot slingShot, BallActor ball, List<PlayerActor> playerActors)
        {
            if (playerActors.Count == 0)
            {
                slingShot.gameObject.SetActive(false);
            }
        }
    }
}