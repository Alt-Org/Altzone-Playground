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
            view.continueButton.onClick.AddListener(() =>
            {
                // Save player settings if changed before continuing!
                if (view.playerName.text != player.PlayerName)
                {
                    player.PlayerName = view.playerName.text;
                }
                if (currentCharacterId != player.CharacterModelId)
                {
                    player.CharacterModelId = currentCharacterId;
                }
                manager.Continue();
            });
            currentCharacterId = player.CharacterModelId;
            var buttons = view.getButtons();
            var characters = Models.GetAll<CharacterModel>();
            characters.Sort((a, b) => string.Compare(a.sortValue(), b.sortValue(), StringComparison.Ordinal));
            for (var i = 0; i < characters.Count; ++i)
            {
                var button = buttons[i];
                var character = characters[i];
                button.SetCaption(character.Name);
                button.onClick.AddListener(() =>
                {
                    currentCharacterId = character.Id;
                    showCharacter(character);
                });
                if (currentCharacterId == character.Id)
                {
                    showCharacter(character);
                }
            }
        }

        private void showCharacter(CharacterModel character)
        {
            var labels = view.getTextLabels();
            var i = -1;
            labels[++i].text = $"<b>{character.Name}</b>";
            labels[++i].text = $"MainDefence:\r\n{character.MainDefence}";
            labels[++i].text = $"Speed:\r\n{character.Speed}";
            labels[++i].text = $"Resistance:\r\n{character.Resistance}";
            labels[++i].text = $"Attack:\r\n{character.Attack}";
            labels[++i].text = $"Defence:\r\n{character.Defence}";
        }
    }
}