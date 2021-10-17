using Examples.Config.Scripts;
using Examples.Game.Scripts.Battle.Player;
using Examples.Game.Scripts.Battle.Scene;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Examples.Game.Scripts.Battle.Room
{
    /// <summary>
    /// Setup arena for Battle gameplay.
    /// </summary>
    /// <remarks>
    /// Wait that all players has been instantiated properly.
    /// </remarks>
    public class RoomSetup : MonoBehaviour
    {
        [Header("Settings"), SerializeField] private GameObject[] objectsToManage;

        [Header("Live Data"), SerializeField] private List<PlayerActor> playerActors;

        private void Awake()
        {
            Debug.Log($"Awake: {PhotonNetwork.NetworkClientState}");
            prepareCurrentStage();
        }

        private void OnEnable()
        {
            setupLocalPlayer();
            StartCoroutine(setupAllPlayers());
        }

        private void prepareCurrentStage()
        {
            // Disable game objects until this room stage is ready
            Array.ForEach(objectsToManage, x => x.SetActive(false));
        }

        private void continueToNextStage()
        {
            enabled = false;
            // Enable game objects when this room stage is ready to play
            Array.ForEach(objectsToManage, x => x.SetActive(true));
        }

        private void setupLocalPlayer()
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
                    (sceneConfig.upperTeamSprite.color, sceneConfig.lowerTeamSprite.color) =
                        (sceneConfig.lowerTeamSprite.color, sceneConfig.upperTeamSprite.color);
                }
            }
        }

        private IEnumerator setupAllPlayers()
        {
            var playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            playerActors = FindObjectsOfType<PlayerActor>().ToList();
            while (playerActors.Count != playerCount && PhotonNetwork.InRoom)
            {
                Debug.Log($"setupAllPlayers playerCount={playerCount} playerActors={playerActors.Count} wait");
                yield return null;
                playerActors = FindObjectsOfType<PlayerActor>().ToList();
            }
            if (!PhotonNetwork.InRoom)
            {
                yield break;
            }
            foreach (var playerActor in playerActors)
            {
                ((IPlayerActor)playerActor).setGhosted();
            }
            Debug.Log($"setupAllPlayers playerCount={playerCount} playerActors={playerActors.Count} ready");
            continueToNextStage();
        }
    }
}