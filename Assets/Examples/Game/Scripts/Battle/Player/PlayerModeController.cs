using Examples.Game.Scripts.Battle.Ball;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.PubSub;
using UnityEngine;

namespace Examples.Game.Scripts.Battle.Player
{
    /// <summary>
    /// Keep all players synchronized: play mode and color.
    /// </summary>
    public class PlayerModeController : MonoBehaviour
    {
        private const int msgSetActiveTeam = PhotonEventDispatcher.eventCodeBase + 1;

        private PhotonEventDispatcher photonEventDispatcher;

        private void Awake()
        {
            photonEventDispatcher = PhotonEventDispatcher.Get();
            photonEventDispatcher.registerEventListener(msgSetActiveTeam, data => { onSetActiveTeam(data.CustomData); });
        }

        private void OnEnable()
        {
            this.Subscribe<BallActor.ActiveTeamEvent>(onActiveTeamEvent);
        }

        private void OnDisable()
        {
            this.Unsubscribe();
        }

        private void onActiveTeamEvent(BallActor.ActiveTeamEvent data)
        {
            if (data.newTeamIndex == -1)
            {
                return; // Ignore indeterminate state
            }
            if (PhotonNetwork.IsMasterClient)
            {
                sendSetActiveTeam(data.newTeamIndex);
            }
        }

        private void sendSetActiveTeam(int activeTeam)
        {
            photonEventDispatcher.RaiseEvent(msgSetActiveTeam, activeTeam);
        }

        private void onSetActiveTeam(object data)
        {
            var activeTeam = (int)data;
            foreach (var playerActor in PlayerActor.playerActors)
            {
                if (playerActor.TeamIndex == activeTeam)
                {
                    playerActor.setFrozenMode();
                }
                else
                {
                    playerActor.setNormalMode();
                }
            }
        }
    }
}