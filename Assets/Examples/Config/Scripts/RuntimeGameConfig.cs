using Examples.Model.Scripts.Model;
using Prg.Scripts.Common.Util;
using System;
using UnityEngine;

namespace Examples.Config.Scripts
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
            PropertyCopier<GameFeatures, GameFeatures>.CopyFields(other, this);
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
        public float playerSqrMinRotationDistance;
        public float playerSqrMaxRotationDistance;

        public void CopyFrom(GameVariables other)
        {
            PropertyCopier<GameVariables, GameVariables>.CopyFields(other, this);
        }
    }

    /// <summary>
    /// Well known prefabs for the game.
    /// </summary>
    [Serializable]
    public class GamePrefabs
    {
        [Header("Battle")] public GameObject playerForDes;
        public GameObject playerForDef;
        public GameObject playerForInt;
        public GameObject playerForPro;
        public GameObject playerForRet;
        public GameObject playerForEgo;
        public GameObject playerForCon;

        public void CopyFrom(GamePrefabs other)
        {
            PropertyCopier<GamePrefabs, GamePrefabs>.CopyFields(other, this);
        }
    }

    /// <summary>
    /// Player data cache.
    /// </summary>
    /// <remarks>
    /// Common location for player related data that is persisted elsewhere.<br />
    /// As this class is visible in UNITY Editor it can not be <c>abstract</c> as it should be!
    /// </remarks>
    [Serializable]
    public class PlayerDataCache
    {
        /// <summary>
        /// Player name.
        /// </summary>
        /// <remarks>
        /// This should be validated and sanitized before accepting a new value.
        /// </remarks>
        [SerializeField] protected string _playerName;

        public string PlayerName
        {
            get => _playerName;
            set
            {
                if (_playerName != value)
                {
                    _playerName = value ?? "";
                    Save();
                }
            }
        }

        /// <summary>
        /// Player character model id.
        /// </summary>
        [SerializeField] protected int _characterModelId;

        public int CharacterModelId
        {
            get => _characterModelId;
            set
            {
                if (_characterModelId != value)
                {
                    _characterModelId = value;
                    Save();
                }
            }
        }

        public CharacterModel CharacterModel => Models.GetById<CharacterModel>(_characterModelId);

        /// <summary>
        /// Unique string to identify this player across devices and systems.
        /// </summary>
        /// <remarks>
        /// When new player is detected this should be given and persisted in all external systems in order to identify this player unambiguously.
        /// </remarks>
        [SerializeField] protected string _playerHandle;

        public string PlayerHandle
        {
            get => _playerHandle;
            set
            {
                if (_playerHandle != value)
                {
                    _playerHandle = value ?? "";
                    Save();
                }
            }
        }

        /// <summary>
        /// Protected <c>Save</c> method to handle single property change.
        /// </summary>
        protected virtual void Save()
        {
            // Placeholder for actual implementation in derived class.
        }

        /// <summary>
        /// Public <c>BatchSave</c> method to save several properties at once.
        /// </summary>
        /// <param name="saveSettings"></param>
        public virtual void BatchSave(Action saveSettings)
        {
            // Placeholder for actual implementation in derived class.
        }

        public override string ToString()
        {
            // This is required for actual implementation to detect changes in our properties!
            return $"Name:{PlayerName}, C-ModelId:{CharacterModelId}, GUID:{PlayerHandle}";
        }
    }

    /// <summary>
    /// Runtime game config variables that can be referenced from anywhere safely and optionally can be changed on the fly.
    /// </summary>
    /// <remarks>
    /// Note that some parts of <c>RuntimeGameConfig</c> can be synchronized over network.
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
                    loadGameConfig();
                }
            }
            return _Instance;
        }

        private static RuntimeGameConfig _Instance;

        [SerializeField] private GameFeatures _permanentFeatures;
        [SerializeField] private GameVariables _permanentVariables;
        [SerializeField] private GamePrefabs _permanentPrefabs;
        [SerializeField] private PlayerDataCache _playerDataCache;

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

        public GamePrefabs prefabs
        {
            get => _permanentPrefabs;
            private set => _permanentPrefabs.CopyFrom(value);
        }

        public PlayerDataCache playerDataCache => _playerDataCache;

        private static void loadGameConfig()
        {
            // We can use models
            ModelLoader.LoadModels();
            // Create default values
            _Instance._permanentFeatures = new GameFeatures();
            _Instance._permanentVariables = new GameVariables();
            _Instance._permanentPrefabs = new GamePrefabs();
            // Set persistent values
            var gameSettings = Resources.Load<PersistentGameSettings>(nameof(PersistentGameSettings));
            _Instance.features = gameSettings.features;
            _Instance.variables = gameSettings.variables;
            _Instance.prefabs = gameSettings.prefabs;
            _Instance._playerDataCache = loadPlayerDataCache();
        }

        private static PlayerDataCache loadPlayerDataCache()
        {
            return new PlayerDataCacheLocal();
        }

        private class PlayerDataCacheLocal : PlayerDataCache
        {
            private const string PlayerNameKey = "PlayerData.PlayerName";
            private const string PlayerHandleKey = "PlayerData.PlayerHandle";
            private const string CharacterModelIdKey = "PlayerData.CharacterModelId";

            private bool isBatchSave;
            private string currentState;

            public PlayerDataCacheLocal()
            {
                _playerName = PlayerPrefs.GetString(PlayerNameKey, string.Empty);
                if (string.IsNullOrWhiteSpace(PlayerName))
                {
                    _playerName = $"Player{1000 * (1 + DateTime.Now.Second % 10) + DateTime.Now.Millisecond:00}";
                    PlayerPrefs.SetString(PlayerNameKey, PlayerName);
                }
                _characterModelId = PlayerPrefs.GetInt(CharacterModelIdKey, -1);
                _playerHandle = PlayerPrefs.GetString(PlayerHandleKey, string.Empty);
                if (string.IsNullOrWhiteSpace(PlayerHandle))
                {
                    _playerHandle = Guid.NewGuid().ToString();
                    PlayerPrefs.SetString(PlayerHandleKey, PlayerHandle);
                }
                currentState = ToString();
            }

            public sealed override string ToString()
            {
                // https://www.jetbrains.com/help/rider/VirtualMemberCallInConstructor.html
                return base.ToString();
            }

            protected override void Save()
            {
                internalSave();
            }

            public override void BatchSave(Action saveSettings)
            {
                isBatchSave = true;
                saveSettings?.Invoke();
                isBatchSave = false;
                internalSave();
            }

            private void internalSave()
            {
                if (isBatchSave)
                {
                    return; // Defer saving until later
                }
                if (currentState == ToString())
                {
                    return; // Skip saving when nothing has changed
                }
                PlayerPrefs.SetString(PlayerNameKey, PlayerName);
                PlayerPrefs.SetInt(CharacterModelIdKey, CharacterModelId);
                PlayerPrefs.SetString(PlayerHandleKey, PlayerHandle);
                currentState = ToString();
            }
        }
    }
}