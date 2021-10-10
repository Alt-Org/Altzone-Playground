using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Examples.Model.Scripts
{
    /// <summary>
    /// Model view.
    /// </summary>
    public class ModelView : MonoBehaviour
    {
        public Text titleText;
        public InputField playerName;
        public Button continueButton;

        [SerializeField] private Transform leftPane;
        [SerializeField] private Transform rightPane;

        public List<Button> getButtons()
        {
            return leftPane.GetComponentsInChildren<Button>().ToList();
        }

        public List<Text> getTextLabels()
        {
            return rightPane.GetComponentsInChildren<Text>().ToList();
        }
    }
}