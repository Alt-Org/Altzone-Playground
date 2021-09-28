using Prg.Scripts.Common.PubSub;
using UnityEngine;
using UnityEngine.UI;

namespace Examples.Game.Scripts
{
    public class DebugCanvasListener : MonoBehaviour
    {
        public Text leftText;
        public Text rightText;

        private void OnEnable()
        {
            leftText.text = "initializing";
            rightText.text = "initializing";
            this.Subscribe<GameManagerExample2.Event>(OnGameDataUpdate);
        }

        private void OnDisable()
        {
            leftText.text = "";
            rightText.text = "";
            this.Unsubscribe<GameManagerExample2.Event>(OnGameDataUpdate);
        }

        private void OnGameDataUpdate(GameManagerExample2.Event data)
        {
            Debug.Log($"OnGameDataUpdate {data}");
            var score = data.score;
            var text = score.teamIndex == 0 ? leftText : rightText;
            text.text = $"team {score.teamIndex} head {score.headCollisionCount} wall {score.wallCollisionCount}";
        }
    }
}