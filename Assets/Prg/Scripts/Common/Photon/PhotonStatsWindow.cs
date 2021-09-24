using Photon.Pun;
using System;
using System.Linq;
using UnityEngine;

namespace Prg.Scripts.Common.Photon
{
    public class PhotonStatsWindow : MonoBehaviour
    {
        public bool Visible = true;
        public KeyCode controlKey = KeyCode.F2;

        private int WindowId;
        private Rect WindowRect;
        private string WindowTitle;
        private bool hasStyles;
        private GUIStyle guiButtonStyle;
        private GUIStyle guiLabelStyle;

        private void Start()
        {
            WindowId = (int) DateTime.Now.Ticks;
            WindowRect = new Rect(0, 0, Screen.width, Screen.height);
            WindowTitle = $"({controlKey}) Photon";
        }

        private void Update()
        {
            if (Input.GetKeyDown(controlKey))
            {
                toggleWindowState();
            }
        }

        private void toggleWindowState()
        {
            Visible = !Visible;
        }

        private void OnGUI()
        {
            if (!Visible)
            {
                return;
            }
            if (!hasStyles)
            {
                hasStyles = true;
                guiButtonStyle = new GUIStyle(GUI.skin.button) { fontSize = 20 };
                guiLabelStyle = new GUIStyle(GUI.skin.label) { fontSize = 24 };
            }
            WindowRect = GUILayout.Window(WindowId, WindowRect, DebugWindow, WindowTitle);
        }

        private void DebugWindow(int windowId)
        {
            string label;
            var inRoom = PhotonNetwork.InRoom;
            if (inRoom)
            {
                var room = PhotonNetwork.CurrentRoom;
                label = $"{PhotonNetwork.LocalPlayer.NickName} | {room.Name}" +
                        $"{(room.IsVisible ? "" : ",hidden")}" +
                        $"{(room.IsOpen ? "" : ",closed")} " +
                        $"{(room.PlayerCount == 1 ? "1 player" : $"{room.PlayerCount} players")}" +
                        $"{(room.MaxPlayers == 0 ? "" : $" (max {room.MaxPlayers})")}";
            }
            else if (PhotonNetwork.InLobby)
            {
                label = $"Lobby: rooms {PhotonNetwork.CountOfRooms}, players {PhotonNetwork.CountOfPlayers}";
            }
            else
            {
                label = $"Photon: {PhotonNetwork.NetworkClientState}";
            }
            if (GUILayout.Button(label, guiButtonStyle))
            {
                toggleWindowState();
            }
            if (inRoom)
            {
                label = "Props:";
                var room = PhotonNetwork.CurrentRoom;
                var props = room.CustomProperties;
                var keys = props.Keys.ToList();
                keys.Sort((a, b) => String.Compare(a.ToString(), b.ToString(), StringComparison.Ordinal));
                foreach (var key in keys)
                {
                    var propValue = props[key].ToString();
                    label += $"\r\n{key}={propValue}";
                }
                label += "\r\nPlayers:";
                foreach (var player in room.GetPlayersByActorNumber())
                {
                    var text = player.GetDebugLabel(verbose: false);
                    label += $"\r\n{text}";
                    props = player.CustomProperties;
                    if (props.Count > 0)
                    {
                        keys = props.Keys.ToList();
                        keys.Sort((a, b) => String.Compare(a.ToString(), b.ToString(), StringComparison.Ordinal));
                        foreach (var key in keys)
                        {
                            var propValue = props[key].ToString();
                            label += $"\r\n{key}={propValue}";
                        }
                    }
                }
            }
            label += $"\r\nPhoton v='{PhotonLobby.gameVersion()}'";
            GUILayout.Label(label, guiLabelStyle);
        }
    }
}