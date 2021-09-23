using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Altzone.Nelinpeli
{
    public class CreateOnlinePlayer : MonoBehaviourPunCallbacks
    {
        private enum PlayerStatus
        {
            Idle = 1,
            Acquiring = 2,
            Playing = 3,
            Releasing = 4,
        }

        // Order of key names is order of allocation of slots for new players
        // - when they join or when
        // - an other player leaves and slot is made free again
        // NOTE that this should be set in player prefab config (VerkkoPelaaja)
        public static readonly string[] roomPosKeyNames = { "P0", "P1", "P2", "P3" };
        public const string playerPosKeyName = "N";

        [Header("Settings"), SerializeField] private string prefabFolder;
        [SerializeField] private string playerPrefabName;
        [SerializeField] private string botPrefabName;
        [SerializeField] private Vector3 safeSpawnPoint;
        [SerializeField] private int pollingTimeMs;
        [SerializeField] private KeyCode createBotKey = KeyCode.F3;

        [Header("Live Data"), SerializeField] private string myKeyName;
        [SerializeField] private int myKeyValue;
        [SerializeField] private PlayerStatus status;
        [SerializeField] private string myBotKeyName;
        [SerializeField] private int myBotKeyValue;

        public override void OnEnable()
        {
            base.OnEnable();
            Debug.Log($"OnEnable {PhotonNetwork.NetworkClientState}");
            status = PlayerStatus.Idle;
            myKeyName = string.Empty;
            myKeyValue = -1;
            myBotKeyName = string.Empty;
            myBotKeyValue = -1;
            StartCoroutine(waitForPlayerPosition(new WaitForSeconds(pollingTimeMs / 1000f)));
        }

        public override void OnDisable()
        {
            base.OnDisable();
            Debug.Log($"OnDisable {PhotonNetwork.NetworkClientState}");
            StopAllCoroutines();
            status = PlayerStatus.Idle;
        }

        private void Update()
        {
            if (!PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient)
            {
                return;
            }
            if (Input.GetKeyDown(createBotKey) && status == PlayerStatus.Playing && myBotKeyName == string.Empty)
            {
                // While master client is playing, new bots can be allocated with keyboard command
                var room = PhotonNetwork.CurrentRoom;
                var player = PhotonNetwork.LocalPlayer;
                setBotPosition(player, room);
            }
        }

        private IEnumerator waitForPlayerPosition(YieldInstruction waitDelay)
        {
            yield return null;
            Debug.Log("waitForPlayerPosition start");
            var room = PhotonNetwork.CurrentRoom;
            var player = PhotonNetwork.LocalPlayer;
            Debug.Log(room.GetDebugLabel());
            Debug.Log(player.GetDebugLabel());
            for (; PhotonNetwork.InRoom;)
            {
                if (checkPlayerPosition(player, room))
                {
                    instantiatePlayer(player, room);
                    break;
                }
                yield return waitDelay;
            }
            Debug.Log("waitForPlayerPosition done");
        }

        private bool checkPlayerPosition(Player player, Room room)
        {
            if (status == PlayerStatus.Playing || status == PlayerStatus.Releasing)
            {
                return true; // We are done
            }
            if (status == PlayerStatus.Idle)
            {
                myKeyName = getFreePlayerPosKey(room);
                if (!string.IsNullOrEmpty(myKeyName))
                {
                    // This position is free now, try to allocate it for us
                    Debug.Log($"ROOM SafeSetCustomProperty player {myKeyName} : 0 <- {player.ActorNumber}");
                    room.SafeSetCustomProperty(myKeyName, (byte) player.ActorNumber, 0);
                    status = PlayerStatus.Acquiring;
                }
            }
            else if (status == PlayerStatus.Acquiring)
            {
                return checkPlayerRoomPosition(player, room);
            }
            return false;
        }

        private void setBotPosition(Player player, Room room)
        {
            if (string.IsNullOrEmpty(myBotKeyName))
            {
                myBotKeyName = getFreePlayerPosKey(room);
                if (!string.IsNullOrEmpty(myBotKeyName))
                {
                    // This position is free now, try to allocate it for us
                    Debug.Log($"ROOM SafeSetCustomProperty bot {myBotKeyName} : 0 <- {player.ActorNumber}");
                    room.SafeSetCustomProperty(myBotKeyName, (byte) player.ActorNumber, 0);
                }
            }
        }

        private void instantiatePlayer(Player player, Room room)
        {
            var prefabName = getPlayerPrefabName(myKeyName);
            Debug.Log($"instantiatePlayer {myKeyName}={myKeyValue} prefabName={prefabName}");
            Debug.Log(room.GetDebugLabel());
            Debug.Log(player.GetDebugLabel());
            var data = new object[0];
            PhotonNetwork.Instantiate(prefabName, safeSpawnPoint, Quaternion.identity, 0, data);
        }

        private void instantiateBot(Player player, Room room)
        {
            var prefabName = getBotPrefabName(myBotKeyName);
            Debug.Log($"instantiateBot {myBotKeyName}={myBotKeyValue} prefabName={prefabName}");
            Debug.Log(room.GetDebugLabel());
            Debug.Log(player.GetDebugLabel());
            var data = new object[0];
            PhotonNetwork.Instantiate(prefabName, safeSpawnPoint, Quaternion.identity, 0, data);
        }

        private bool checkPlayerRoomPosition(Player player, Room room)
        {
            var keyValue = room.GetCustomProperty<byte>(myKeyName);
            if (keyValue == player.ActorNumber)
            {
                status = PlayerStatus.Playing;
                myKeyValue = keyValue;
                player.SetCustomProperties(new Hashtable { { playerPosKeyName, myKeyName } });
                return true;
            }
            return false;
        }

        private bool checkBotRoomPosition(Player player, Room room)
        {
            var keyValue = room.GetCustomProperty<byte>(myBotKeyName);
            if (keyValue == player.ActorNumber)
            {
                myBotKeyValue = keyValue;
                return true;
            }
            return false;
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            var player = PhotonNetwork.LocalPlayer;
            if (otherPlayer.Equals(player))
            {
                Debug.Log($"OnPlayerLeftRoom {otherPlayer.GetDebugLabel()}");
                enabled = false;
            }
        }

        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            // We search for bot creation when room props are updated
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            if (!string.IsNullOrEmpty(myBotKeyName))
            {
                var player = PhotonNetwork.LocalPlayer;
                var room = PhotonNetwork.CurrentRoom;
                if (checkBotRoomPosition(player, room))
                {
                    instantiateBot(player, room);
                    myBotKeyName = string.Empty;
                    myBotKeyValue = -1;
                }
            }
        }

        private static string getFreePlayerPosKey(Room room)
        {
            var props = room.CustomProperties;
            var isPosFree = new[] { false, false, false, false };
            var firstFree = -1;
            for (var keyIndex = 0; keyIndex < roomPosKeyNames.Length; ++keyIndex)
            {
                var keyName = roomPosKeyNames[keyIndex];
                var keyValue = props[keyName].ToString();
                isPosFree[keyIndex] = keyValue == "0";
                if (isPosFree[keyIndex] && firstFree == -1)
                {
                    firstFree = keyIndex;
                    Debug.Log($"getFreePlayerPosKey {string.Join(",", roomPosKeyNames)} firstFree={firstFree} '{keyValue}'");
                }
            }
            return firstFree == -1 ? null : roomPosKeyNames[firstFree];
        }

        private string getPlayerPrefabName(string suffix)
        {
            if (string.IsNullOrEmpty(prefabFolder))
            {
                return $"{playerPrefabName}{suffix}";
            }
            return $"{prefabFolder}/{playerPrefabName}{suffix}";
        }

        private string getBotPrefabName(string suffix)
        {
            if (string.IsNullOrEmpty(prefabFolder))
            {
                return $"{botPrefabName}{suffix}";
            }
            return $"{prefabFolder}/{botPrefabName}{suffix}";
        }
    }
}