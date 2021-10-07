using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.Unity;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Examples.Lobby.Scripts.LootLocker
{
    /// <summary>
    /// Manage LootLocker login UI.
    /// </summary>
    public class LootLockerUI : MonoBehaviour
    {
        [SerializeField] private Text titleText;

        [SerializeField] private Text loginText;
        [SerializeField] private InputField username;
        [SerializeField] private Button loginButton;

        [SerializeField] private Text continueText;
        [SerializeField] private Button continueButton;

        [SerializeField] private LootLockerManager manager;
        [SerializeField] private UnitySceneName lobbyScene;

        private void Start()
        {
            // NOTE that code below is utterly rubbish!

            titleText.text = $"Welcome to {Application.productName} {PhotonLobby.gameVersion}";
            loginText.text = "Login using following username:";
            var deviceId = PlayerPrefs.GetString("lootLocker.deviceId", "");
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                deviceId = System.Guid.NewGuid().ToString();
                PlayerPrefs.SetString("lootLocker.deviceId", deviceId);
            }
            var playerName = PlayerPrefs.GetString("lootLocker.playerName", "");
            if (string.IsNullOrWhiteSpace(playerName))
            {
                playerName = $"Player{DateTime.Now.Second:00}";
                PlayerPrefs.SetString("lootLocker.playerName", playerName);
            }
            username.text = playerName;
            username.interactable = false;

            manager.StartSession(deviceId, (success) =>
            {
                Debug.Log($"StartSession for {deviceId}: {success}");
                if (success)
                {
                    continueToLobby();
                }
                else
                {
                    registerNewPlayer();
                }
            });
        }

        private void registerNewPlayer()
        {
            continueText.gameObject.SetActive(false);
            continueButton.gameObject.SetActive(false);
            loginButton.onClick.AddListener(() =>
            {
                var deviceId = PlayerPrefs.GetString("lootLocker.deviceId");
            });
        }

        private void continueToLobby()
        {
            loginText.gameObject.SetActive(false);
            username.gameObject.SetActive(false);
            loginButton.gameObject.SetActive(false);

            continueText.text = $"Continue to lobby as '{username.text}'";
            continueButton.onClick.AddListener(() =>
            {
                Debug.Log($"LoadScene {lobbyScene.sceneName}");
                SceneManager.LoadScene(lobbyScene.sceneName);
            });
        }
    }
}