using System;
using UnityEngine;

namespace Examples.Game.Scripts.Config
{
    /// <summary>
    /// Game features that can be toggled on and off.
    /// </summary>
    [Serializable]
    public class GameFeatures
    {
        /// <summary>
        /// Rotate game camera for upper team so they see their own game area in lower part of the screen.
        /// </summary>
        public bool isRotateGameCamera;

        /// <summary>
        /// Spawn mini ball aka diamonds.
        /// </summary>
        public bool isSPawnMiniBall;

        /// <summary>
        /// Enable or disable team (player) movement depending on ball position on game area, is it on our side or others side.
        /// </summary>
        public bool isActivateTeamWithBall;

        public void CopyFrom(GameFeatures other)
        {
            this.isRotateGameCamera = other.isRotateGameCamera;
            this.isSPawnMiniBall = other.isSPawnMiniBall;
            this.isActivateTeamWithBall = other.isActivateTeamWithBall;
        }
    }

    /// <summary>
    /// Game variables that control game play somehow.
    /// </summary>
    [Serializable]
    public class GameVariables
    {
        [Header("Ball")] public float ballMoveSpeed;
        public float ballLerpSmoothingFactor;
        public float ballTeleportDistance;

        [Header("Player")] public float playerMoveSpeed;

        public void CopyFrom(GameVariables other)
        {
            this.ballMoveSpeed = other.ballMoveSpeed;
            this.ballLerpSmoothingFactor = other.ballLerpSmoothingFactor;
            this.ballTeleportDistance = other.ballTeleportDistance;

            this.playerMoveSpeed = other.playerMoveSpeed;
        }
    }

    /// <summary>
    /// Runtime game config variables that can be changed on the fly and can be referenced from anywhere safely.
    /// </summary>
    /// <remarks>
    /// Note that full or partial <c>RuntimeGameConfig</c> can be synchronized over network and is intended to be used so.
    /// </remarks>
    public class RuntimeGameConfig : MonoBehaviour
    {
        public static RuntimeGameConfig Get()
        {
            if (_Instance == null)
            {
                _Instance = FindObjectOfType<RuntimeGameConfig>();
                if (_Instance == null)
                {
                    _Instance = UnityExtensions.CreateGameObjectAndComponent<RuntimeGameConfig>(nameof(RuntimeGameConfig), isDontDestroyOnLoad: true);
                    _Instance._permanentFeatures = new GameFeatures();
                    _Instance._permanentVariables = new GameVariables();
                    loadGameConfig();
                }
            }
            return _Instance;
        }

        private static RuntimeGameConfig _Instance;

        [SerializeField] private GameFeatures _permanentFeatures;
        [SerializeField] private GameVariables _permanentVariables;

        public GameFeatures features
        {
            get => _permanentFeatures;
            set => _permanentFeatures.CopyFrom(value);
        }

        public GameVariables variables
        {
            get => _permanentVariables;
            set => _permanentVariables.CopyFrom(value);
        }

        private static void loadGameConfig()
        {
            var gameSettings = Resources.Load<PersistentGameSettings>(nameof(PersistentGameSettings));
            _Instance.features = gameSettings.features;
            _Instance.variables = gameSettings.variables;
        }
    }
}