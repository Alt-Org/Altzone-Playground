using Prg.Scripts.Common.PubSub;
using UnityEngine;
using UnityEngine.UI;

namespace Examples.Game.Scripts
{
    public class DebugCanvasListener : MonoBehaviour
    {
        private static string[] teamName = { "Blue", "Red" };

        public Text leftText;
        public Text rightText;

        private void OnEnable()
        {
            leftText.text = "initializing";
            rightText.text = "initializing";
            this.Subscribe<GameManager.Event>(OnGameDataUpdate);
        }

        private void OnDisable()
        {
            leftText.text = "";
            rightText.text = "";
            this.Unsubscribe<GameManager.Event>(OnGameDataUpdate);
        }

        private void OnGameDataUpdate(GameManager.Event data)
        {
            Debug.Log($"OnGameDataUpdate {data}");
            var score = data.score;
            var text = score.teamIndex == 0 ? leftText : rightText;
            text.text = $"<b>{teamName[score.teamIndex]}</b> head {score.headCollisionCount} wall {score.wallCollisionCount}";
        }
    }
}