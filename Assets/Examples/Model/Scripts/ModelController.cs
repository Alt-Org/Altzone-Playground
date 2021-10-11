using Examples.Config.Scripts;
using Examples.Model.Scripts.Model;
using Prg.Scripts.Common.Photon;
using System;
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

        [SerializeField] private int currentCharacterId;

        private void Start()
        {
            Debug.Log("Start");
            view.titleText.text = $"<b>Choose your character</b>\r\nfor {Application.productName} {PhotonLobby.gameVersion}";
            var player = RuntimeGameConfig.Get().playerDataCache;
            view.playerName.text = player.PlayerName;
            view.hideCharacter();
            view.continueButton.onClick.AddListener(() =>
            {
                // Save player settings if changed before continuing!
                if (view.playerName.text != player.PlayerName ||
                    currentCharacterId != player.CharacterModelId)
                {
                    player.BatchSave(() =>
                    {
                        player.PlayerName = view.playerName.text;
                        player.CharacterModelId = currentCharacterId;
                    });
                }
                manager.Continue();
            });
            currentCharacterId = player.CharacterModelId;
            var characters = Models.GetAll<CharacterModel>();
            characters.Sort((a, b) => string.Compare(a.sortValue(), b.sortValue(), StringComparison.Ordinal));
            for (var i = 0; i < characters.Count; ++i)
            {
                var character = characters[i];
                var button = view.getButton(i);
                button.SetCaption(character.Name);
                button.onClick.AddListener(() =>
                {
                    currentCharacterId = character.Id;
                    view.showCharacter(character);
                });
                if (currentCharacterId == character.Id)
                {
                    view.showCharacter(character);
                }
            }
        }
    }
}