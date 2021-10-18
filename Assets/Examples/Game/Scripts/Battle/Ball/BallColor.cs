using Examples.Config.Scripts;
using Photon.Pun;
using Prg.Scripts.Common.PubSub;
using UnityEngine;

namespace Examples.Game.Scripts.Battle.Ball
{
    public class BallColor : MonoBehaviour
    {
        [Header("Settings"), SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private Color neutralColor;
        [SerializeField] private Color upperTeamColor;
        [SerializeField] private Color lowerTeamColor;

        private void Awake()
        {
            Debug.Log("Awake");
        }

        private void OnEnable()
        {
            var player = PhotonNetwork.LocalPlayer;
            PhotonBattle.getPlayerProperties(player, out var playerPos, out var teamIndex);
            if (teamIndex == 1)
            {
                // c# swap via deconstruction
                (upperTeamColor, lowerTeamColor) = (lowerTeamColor, upperTeamColor);
            }
            this.Subscribe<BallActor.ActiveTeamEvent>(onActiveTeamEvent);
        }

        private void OnDisable()
        {
            this.Unsubscribe();
        }

        private void onActiveTeamEvent(BallActor.ActiveTeamEvent data)
        {
            switch (data.newTeamIndex)
            {
                case 0:
                    _sprite.color = lowerTeamColor;
                    break;
                case 1:
                    _sprite.color = upperTeamColor;
                    break;
                default:
                    _sprite.color = neutralColor;
                    break;
            }
        }
    }
}