#if UNITY_EDITOR || FORCE_LOG || DEVELOPMENT_BUILD
using Examples.Game.Scripts.Battle.SlingShot;
using Photon.Pun;
using System.Linq;
using UnityEngine;

namespace Examples.Game.Scripts.Battle.Test
{
    public class BallSlingShotTest : MonoBehaviour
    {
        public KeyCode controlKey = KeyCode.F3;

        private void Awake()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(controlKey))
            {
                startTheBall();
                gameObject.SetActive(false);
            }
        }

        private static void startTheBall()
        {
            // Get slingshot with longest distance and start it.
            var ballSlingShot = FindObjectsOfType<BallSlingShot>()
                .Cast<IBallSlingShot>()
                .OrderByDescending(x => x.currentDistance)
                .FirstOrDefault();

            ballSlingShot?.startBall();
        }
    }
}
#endif