using Examples.Config.Scripts;
using Prg.Scripts.Common.Photon;
using UnityEngine;

namespace Examples.Model.Scripts
{
    /// <summary>
    /// UI controller for model view.
    /// </summary>
    public class ModelController : MonoBehaviour
    {
        [SerializeField] private ModelManager manager;
        [SerializeField] private ModelView view;

        private void Start()
        {
            Debug.Log("Start");
            view.titleText.text = $"<b>Choose your character</b>\r\nfor {Application.productName} {PhotonLobby.gameVersion}";
            var player = RuntimeGameConfig.Get().playerDataCache;
            view.playerName.text = player.PlayerName;
            view.continueButton.onClick.AddListener(() =>
            {
                if (view.playerName.text != player.PlayerName)
                {
                    // Name has been changed - save it
                    player.PlayerName = view.playerName.text;
                }
                manager.Continue();
            });
        }
    }
}
