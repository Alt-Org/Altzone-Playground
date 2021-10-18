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

            Debug.Log($"Instantiate pos={playerPos} team={teamIndex} prefab={playerPrefab.name} {PhotonNetwork.LocalPlayer.GetDebugLabel()}");

            var instantiationPosition = playerStartPos[playerPos].position;
            PhotonNetwork.Instantiate(playerPrefab.name, instantiationPosition, Quaternion.identity);
            // ... rest of instantiation is done in PlayerActor (or elsewhere) because local and remote requirements can be different.
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