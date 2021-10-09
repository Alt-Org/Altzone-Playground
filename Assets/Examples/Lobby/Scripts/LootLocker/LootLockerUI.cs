using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.Unity;
using System.Collections;
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
        [SerializeField] private Button continueButton;

        [SerializeField] private LootLockerManager manager;
        [SerializeField] private UnitySceneName lobbyScene;

        private IEnumerator Start()
        {
            Debug.Log("Start");
            titleText.text = $"Welcome to {Application.productName} {PhotonLobby.gameVersion}";
            loginText.text = "Your current username is:";
            username.interactable = false;
            continueButton.interactable = false;
            var waitUntil = new WaitUntil(() => manager.isValid);
            yield return waitUntil;
            username.interactable = true;
            continueButton.interactable = true;
            username.text = manager.playerHandle.PlayerName;
            var playerHandle = manager.playerHandle;
            Debug.Log($"LootLocker player is {playerHandle.PlayerName} {playerHandle.PlayerId}");
            continueButton.onClick.AddListener(continueButtonOnClick);
        }

        private async void continueButtonOnClick()
        {
            if (username.text != manager.playerHandle.PlayerName)
            {
                Debug.Log($"Set player {manager.playerHandle.PlayerName} <- {username.text}");
                username.interactable = false;
                continueButton.interactable = false;
                await manager.SetPlayerName(username.text);
            }
            Debug.Log($"LoadScene {lobbyScene.sceneName}");
            SceneManager.LoadScene(lobbyScene.sceneName);
        }
    }
}