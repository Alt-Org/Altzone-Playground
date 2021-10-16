using Examples.Config.Scripts;
using Examples.Game.Scripts.Battle.Scene;
using Photon.Pun;
using UnityEngine;

namespace Examples.Game.Scripts.Battle.Room
{
    /// <summary>
    /// Setup arena for Battle gameplay.
    /// </summary>
    public class RoomSetup : MonoBehaviour
    {
        public void OnEnable()
        {
            var player = PhotonNetwork.LocalPlayer;
            PhotonBattle.getPlayerProperties(player, out var playerPos, out var teamIndex);
            Debug.Log($"OnEnable pos={playerPos} team={teamIndex} {player.GetDebugLabel()}");
            var sceneConfig = SceneConfig.Get();
            var features = RuntimeGameConfig.Get().features;
            if (features.isRotateGameCamera)
            {
                if (teamIndex == 1)
                {
                    // Rotate game camera for upper team
                    var _camera = sceneConfig._camera;
                    var cameraTransform = _camera.transform;
                    cameraTransform.rotation = Quaternion.Euler(0f, 0f, 180f); // Upside down
                }
            }
            if (features.isLocalPLayerOnTeamBlue)
            {
                if (teamIndex == 1)
                {
                    // c# swap via deconstruction
                    (sceneConfig.upperTeamSprite.color, sceneConfig.lowerTeamSprite.color) = (sceneConfig.lowerTeamSprite.color, sceneConfig.upperTeamSprite.color);
                }
            }
        }
    }
}