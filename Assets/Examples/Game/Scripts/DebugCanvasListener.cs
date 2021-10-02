using Prg.Scripts.Common.PubSub;
using UnityEngine;
using UnityEngine.UI;

namespace Examples.Game.Scripts
{
    /// <summary>
    /// Simple temporary UI for debugging.
    /// </summary>
    public class DebugCanvasListener : MonoBehaviour
    {
        private static readonly string[] teamName = { "Blue", "Red" };

        public Text leftText;
        public Text rightText;

        private void OnEnable()
        {
            leftText.text = "initializing";
            rightText.text = "initializing";
            this.Subscribe<GameManager.TeamScoreEvent>(OnTeamScoreEvent);
        }

        private void OnDisable()
        {
            leftText.text = "";
            rightText.text = "";
            this.Unsubscribe();
        }

        private void OnTeamScoreEvent(GameManager.TeamScoreEvent data)
        {
            Debug.Log($"OnGameDataUpdate {data}");
            var score = data.score;
            var text = score.teamIndex == 0 ? leftText : rightText;
            text.text = $"<b>{teamName[score.teamIndex]}</b> head {score.headCollisionCount} wall {score.wallCollisionCount}";
        }
    }
}