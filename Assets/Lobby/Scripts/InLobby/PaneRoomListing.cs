using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.Photon;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby.Scripts.InLobby
{
    /// <summary>
    /// Shows list of (open/closed) rooms and buttons for creating a new room or joining existing one.
    /// </summary>
    public class PaneRoomListing : MonoBehaviour
    {
        [SerializeField] private Text title;
        [SerializeField] private Button templateButton;

        private PhotonRoomList photonRoomList;

        private readonly List<Button> buttons = new List<Button>();

        private void Start()
        {
            title.text = $"Welcome to {Application.productName}";
            templateButton.onClick.AddListener(createRoomForMe);
        }

        private void OnEnable()
        {
            photonRoomList = FindObjectOfType<PhotonRoomList>();
            if (photonRoomList != null)
            {
                if (PhotonNetwork.InLobby)
                {
                    updateStatus();
                }
                photonRoomList.roomsUpdated += updateStatus;
            }
        }

        private void OnDisable()
        {
            if (photonRoomList != null)
            {
                photonRoomList.roomsUpdated -= updateStatus;
                photonRoomList = null;
            }
            buttons.Clear();
        }

        private void updateStatus()
        {
            if (!PhotonNetwork.InLobby)
            {
                deleteExtraButtons(buttons, 0);
                return;
            }
            var rooms = photonRoomList.currentRooms.ToList();
            rooms.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
            Debug.Log($"updateStatus enter {PhotonNetwork.NetworkClientState} buttons: {buttons.Count} rooms: {rooms.Count}");

            // Synchronize button count with room count.
            while (buttons.Count < rooms.Count)
            {
                buttons.Add(duplicate(templateButton));
            }
            deleteExtraButtons(buttons, rooms.Count);
            // Update button captions
            for (var i = 0; i < rooms.Count; ++i)
            {
                var room = rooms[i];
                var button = buttons[i];
                update(button, room);
            }
            Debug.Log($"updateStatus exit {PhotonNetwork.NetworkClientState} buttons: {buttons.Count} rooms: {rooms.Count}");
        }

        private static void createRoomForMe()
        {
            Debug.Log("createRoomForMe");
            PhotonLobby.Get().createRoom($"Room{DateTime.Now.Second:00}");
        }

        private void joinRoom(string roomName)
        {
            Debug.Log($"joinRoom '{roomName}'");
            var rooms = photonRoomList.currentRooms.ToList();
            foreach (var roomInfo in rooms)
            {
                if (roomInfo.Name == roomName && !roomInfo.RemovedFromList && roomInfo.IsOpen)
                {
                    PhotonLobby.Get().joinRoom(roomInfo);
                }
            }
        }

        private static Button duplicate(Button template)
        {
            var templateParent = template.gameObject;
            var instance = Instantiate(templateParent, templateParent.transform.parent);
            var button = instance.GetComponent<Button>();
            Debug.Log("duplicate");
            return button;
        }

        private void update(Button button, RoomInfo room)
        {
            var text = button.GetComponentInChildren<Text>();
            var roomText = $"{room.Name}";
            if (room.IsOpen)
            {
                roomText += $" ({room.PlayerCount})";
                roomText = $"<color=blue>{roomText}</color>";
            }
            else
            {
                roomText += " (closed)";
                roomText = $"<color=brown>{roomText}</color>";
            }
            Debug.Log($"update '{text.text}' -> '{roomText}'");
            text.text = roomText;
            button.onClick.RemoveAllListeners();
            if (room.IsOpen)
            {
                button.onClick.AddListener(() => joinRoom(room.Name));
            }
        }

        private static void deleteExtraButtons(List<Button> buttons, int buttonsToKeep)
        {
            while (buttons.Count > buttonsToKeep)
            {
                var lastButton = buttons[buttons.Count - 1];
                if (!buttons.Remove(lastButton))
                {
                    throw new UnityException("can not remove button");
                }
                Debug.Log($"Destroy '{lastButton.GetComponentInChildren<Text>().text}'");
                Destroy(lastButton.gameObject);
            }
        }
    }
}