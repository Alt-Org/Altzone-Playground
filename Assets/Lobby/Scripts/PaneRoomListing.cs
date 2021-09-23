using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby.Scripts
{
    public class PaneRoomListing : MonoBehaviour
    {
        private const string JOIN_PREFIX = "Join: ";

        [SerializeField] private Text title;
        [SerializeField] private Button templateButton;
        [SerializeField] private List<Button> buttons;

        private void Start()
        {
            title.text = $"Welcome to {Application.productName}";
            templateButton.onClick.AddListener(createRoomForMe);
            buttons = new List<Button>();
            buttons.Add(templateButton);
            buttons.Add(duplicate(templateButton, "TEST ROOM"));
        }

        private static void createRoomForMe()
        {
            Debug.Log("createRoomForMe");
        }

        private static void joinRoom(string roomName)
        {
            Debug.Log($"joinRoom '{roomName}'");
        }

        private static Button duplicate(Button template, string roomName)
        {
            var templateParent = template.gameObject;
            var instance = Instantiate(templateParent, templateParent.transform.parent);
            var button = instance.GetComponent<Button>();
            var text = button.GetComponentInChildren<Text>();
            text.text = $"{JOIN_PREFIX}{roomName}";
            button.onClick.AddListener(() => joinRoom(roomName));
            return button;
        }
    }
}
