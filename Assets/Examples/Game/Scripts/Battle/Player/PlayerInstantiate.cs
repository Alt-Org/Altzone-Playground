using Examples.Config.Scripts;
using Examples.Game.Scripts.Battle.Scene;
using Examples.Model.Scripts.Model;
using Photon.Pun;
using System;
using UnityEngine;

namespace Examples.Game.Scripts.Battle.Player
{
    /// <summary>
    /// Instantiates local networked player.
    /// </summary>
    public class PlayerInstantiate : MonoBehaviour
    {
        private void OnEnable()
        {
            var sceneConfig = SceneConfig.Get();
            var playerStartPos = sceneConfig.playerStartPos;
            var player = PhotonNetwork.LocalPlayer;
            PhotonBattle.getPlayerProperties(PhotonNetwork.LocalPlayer, out var playerPos, out var teamIndex);
            if (playerPos < 0 || playerPos >= playerStartPos.Length)
            {
                throw new UnityException($"invalid player position '{playerPos}' for player {player.GetDebugLabel()}");
            }
            if (teamIndex < 0 || teamIndex > 1)
            {
                throw new UnityException($"invalid team index '{teamIndex}' for player {player.GetDebugLabel()}");
            }
            var playerDataCache = RuntimeGameConfig.Get().playerDataCache;
            var defence = playerDataCache.CharacterModel.MainDefence;
            var playerPrefab = getPlayerPrefab(defence);

            Debug.Log($"OnEnable pos={playerPos} team={teamIndex} {PhotonNetwork.LocalPlayer.GetDebugLabel()}");

            var instantiationPosition = playerStartPos[playerPos].position;
            var instance = _instantiateLocalPlayer(playerPrefab.name, instantiationPosition);

            // Setup input system to move player around
            var playerMovement = instance.AddComponent<PlayerMovement>();
            var playArea = getPlayArea(playerPos);
            ((IRestrictedPlayer)playerMovement).setPlayArea(playArea);
            var playerInput = instance.AddComponent<PlayerInput>();
            playerInput.Camera = sceneConfig._camera;
            playerInput.PlayerMovement = playerMovement;
            if (!Application.isMobilePlatform)
            {
                var keyboardInput = instance.AddComponent<PlayerInputKeyboard>();
                keyboardInput.PlayerMovement = playerMovement;
            }
        }

        private static GameObject _instantiateLocalPlayer(string prefabName, Vector3 instantiationPosition)
        {
            var instance = PhotonNetwork.Instantiate(prefabName, instantiationPosition, Quaternion.identity);
            return instance;
        }

        private static Rect getPlayArea(int playerPos)
        {
            return new Rect(-10, -10, 20, 20);
        }

        private static GameObject getPlayerPrefab(Defence defence)
        {
            var prefabs = RuntimeGameConfig.Get().prefabs;
            switch (defence)
            {
                case Defence.Desensitisation:
                    return prefabs.playerForDes;
                case Defence.Deflection:
                    return prefabs.playerForDef;
                case Defence.Introjection:
                    return prefabs.playerForInt;
                case Defence.Projection:
                    return prefabs.playerForPro;
                case Defence.Retroflection:
                    return prefabs.playerForRet;
                case Defence.Egotism:
                    return prefabs.playerForEgo;
                case Defence.Confluence:
                    return prefabs.playerForCon;
                default:
                    throw new ArgumentOutOfRangeException(nameof(defence), defence, null);
            }
        }
    }
}