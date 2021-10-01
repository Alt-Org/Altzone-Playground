using UnityEngine;

namespace Examples.Game.Scripts.Config
{
    /// <summary>
    /// Editable persistent settings for the game.
    /// </summary>
    /// <remarks>
    /// Create these in <c>Resources</c> folder with name "GameSettings" so they can be loaded when needed first time.
    /// </remarks>
    [CreateAssetMenu(menuName = "ALT-Zone/GameSettings")]
    public class GameSettings : ScriptableObject
    {
        [Header("Game Features")] public GameFeatures features;
        [Header("Game Variables")] public GameVariables variables;
    }
}