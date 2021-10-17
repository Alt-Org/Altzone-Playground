using Prg.Scripts.Common.PubSub;
using UnityEngine;
using UnityEngine.UI;

namespace Examples.Game.Scripts.Battle.UI
{
    /// <summary>
    /// Simple temporary UI for debugging.
    /// </summary>
    public class DebugCanvasListener : MonoBehaviour
    {
        private static readonly string[] teamName = { "Blue", "Red" };

        public GameObject roomStartPanel;
        public Text titleText;
        public Text countdownText;
        public GameObject scorePanel;
        public Text leftText;
        public Text rightText;

        private void OnEnable()
        {
            roomStartPanel.SetActive(false);
            scorePanel.SetActive(false);
            this.Subscribe<GameManager.TeamScoreEvent>(OnTeamScoreEvent);
            this.Subscribe<GameStartPlaying.CountdownEvent>(OnCountdownEvent);
        }

        private void OnDisable()
        {
            leftText.text = "";
            rightText.text = "";
            this.Unsubscribe();
        }

        private void OnCountdownEvent(GameStartPlaying.CountdownEvent data)
        {
            Debug.Log($"OnCountdownEvent {data}");
            if (data.maxCountdownValue == data.curCountdownValue)
            {
                roomStartPanel.SetActive(true);
                titleText.text = "Wait for game start:";
            }
            countdownText.text = data.curCountdownValue.ToString("N0");
            if (data.curCountdownValue <= 0)
            {
                this.executeAsCoroutine(new WaitForSeconds(0.67f), () =>
                {
                    roomStartPanel.SetActive(false);
                    scorePanel.SetActive(true);
                });
            }
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