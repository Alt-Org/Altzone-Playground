using UnityEngine;

namespace Examples.Game.Scripts.Battle.Ball
{
    public class BallActor : MonoBehaviour
    {
        private void Awake()
        {
            Debug.Log("Awake");

            enabled = false; // Wait until game starts
        }

        private void OnEnable()
        {
            Debug.Log("OnEnable");
        }

        private void OnDisable()
        {
            Debug.Log("OnDisable");
        }
    }
}
