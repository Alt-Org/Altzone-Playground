using Prg.Scripts.Common.Photon;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Examples.Game.Scripts.Config
{
    [Flags] public enum What
    {
        None = 0,
        All = 1,
        Features = 2,
        Variables = 4,
    }

    /// <summary>
    /// Synchronize runtime game config over network.
    /// </summary>
    /// <remarks>
    /// Only Master Client can do this while in a room.
    /// </remarks>
    public class GameConfigSynchronizer : MonoBehaviour
    {
        private const int photonEventCode = PhotonEventDispatcher.eventCodeBase + 4; // synchronize game config
        private const byte endByte = 0xFE;
        private const int overheadBytes = 2;

        public static void listen()
        {
            Get(); // Instantiate our private instance for listening synchronize events
        }

        public static void synchronize(What what)
        {
            if (!PhotonWrapper.InRoom || !PhotonWrapper.IsMasterClient)
            {
                throw new UnityException("only master client can synchronize in a room");
            }
            if (what.HasFlag(What.All) || what.HasFlag(What.Features))
            {
                Get().synchronizeFeatures((byte)What.Features, endByte);
            }
            if (what.HasFlag(What.All) || what.HasFlag(What.Variables))
            {
                Get().synchronizeVariables((byte)What.Variables, endByte);
            }
        }

        private static GameConfigSynchronizer Get()
        {
            if (_Instance == null)
            {
                _Instance = FindObjectOfType<GameConfigSynchronizer>();
                if (_Instance == null)
                {
                    _Instance = UnityExtensions.CreateGameObjectAndComponent<GameConfigSynchronizer>(nameof(GameConfigSynchronizer),
                        isDontDestroyOnLoad: true);
                }
            }
            return _Instance;
        }

        private static GameConfigSynchronizer _Instance;

        private PhotonEventDispatcher photonEventDispatcher;

        private void Awake()
        {
            photonEventDispatcher = PhotonEventDispatcher.Get();
            photonEventDispatcher.registerEventListener(photonEventCode, data =>
            {
                if (data.CustomData is byte[] bytes)
                {
                    if (bytes.Length < 3)
                    {
                        throw new UnityException($"invalid synchronization message length: {bytes.Length}");
                    }
                    var lastByte = bytes[bytes.Length - 1];
                    if (lastByte != endByte)
                    {
                        throw new UnityException($"invalid synchronization message end: {lastByte}");
                    }
                    var firstByte = bytes[0];
                    if (firstByte == (byte) What.Features)
                    {
                        readFeatures(bytes);
                    }
                    else if (firstByte == (byte) What.Variables)
                    {
                        readVariables(bytes);
                    }
                    else
                    {
                        throw new UnityException($"invalid synchronization message start: {firstByte}");
                    }
                }
            });
        }

        private void synchronizeFeatures(byte first, byte last)
        {
            var features = RuntimeGameConfig.Get().features;
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(first);
                    writer.Write(features.isRotateGameCamera);
                    writer.Write(features.isSPawnMiniBall);
                    writer.Write(features.isActivateTeamWithBall);
                    writer.Write(last);
                }
                var bytes = stream.ToArray();
                var type = features.GetType();
                var fieldsLength = countFieldsByteSize(type, out var fieldCount);
                Debug.Log($"synchronizeFeatures data {fieldCount} fields {bytes.Length} bytes");
                Debug.Log($"send data> {string.Join(", ", bytes)}");
                if (bytes.Length != fieldsLength)
                {
                    throw new UnityException($"mismatch in type {type} fields size {fieldsLength} and written fields size {bytes.Length}");
                }
                photonEventDispatcher.RaiseEvent(photonEventCode, bytes);
            }
        }

        private static void readFeatures(byte[] bytes)
        {
            Debug.Log($"readFeatures data length {bytes.Length}");
            Debug.Log($"recv data< {string.Join(", ", bytes)}");
            var features = new GameFeatures();
            using (var stream = new MemoryStream(bytes))
            {
                using (var reader = new BinaryReader(stream))
                {
                    reader.ReadByte(); // skip first
                    features.isRotateGameCamera = reader.ReadBoolean();
                    features.isSPawnMiniBall = reader.ReadBoolean();
                    features.isActivateTeamWithBall = reader.ReadBoolean();
                    reader.ReadByte(); // skip last
                }
            }
            RuntimeGameConfig.Get().features = features;
        }

        private void synchronizeVariables(byte first, byte last)
        {
            var variables = RuntimeGameConfig.Get().variables;
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(first);
                    writer.Write(variables.ballMoveSpeed);
                    writer.Write(variables.ballLerpSmoothingFactor);
                    writer.Write(variables.ballTeleportDistance);
                    writer.Write(variables.playerMoveSpeed);
                    writer.Write(variables.playerSqrMinRotationDistance);
                    writer.Write(variables.playerSqrMaxRotationDistance);
                    writer.Write(last);
                }
                var bytes = stream.ToArray();
                var type = variables.GetType();
                var fieldsLength = countFieldsByteSize(type, out var fieldCount);
                Debug.Log($"synchronizeVariables data {fieldCount} fields {bytes.Length} bytes");
                Debug.Log($"send data> {string.Join(", ", bytes)}");
                if (bytes.Length != fieldsLength)
                {
                    throw new UnityException($"mismatch in type {type} fields size {fieldsLength} and written fields size {bytes.Length}");
                }
                photonEventDispatcher.RaiseEvent(photonEventCode, bytes);
            }
        }

        private static void readVariables(byte[] bytes)
        {
            Debug.Log($"readVariables data length {bytes.Length}");
            Debug.Log($"recv data< {string.Join(", ", bytes)}");
            var variables = new GameVariables();
            using (var stream = new MemoryStream(bytes))
            {
                using (var reader = new BinaryReader(stream))
                {
                    reader.ReadByte(); // skip first
                    variables.ballMoveSpeed = reader.ReadSingle();
                    variables.ballLerpSmoothingFactor = reader.ReadSingle();
                    variables.ballTeleportDistance = reader.ReadSingle();
                    variables.playerMoveSpeed = reader.ReadSingle();
                    variables.playerSqrMinRotationDistance = reader.ReadSingle();
                    variables.playerSqrMaxRotationDistance = reader.ReadSingle();
                    reader.ReadByte(); // skip last
                }
            }
            RuntimeGameConfig.Get().variables = variables;
        }

        private static int countFieldsByteSize(Type type, out int fieldCount)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            var countBytes = overheadBytes; // stream start and end bytes
            foreach (var fieldInfo in fields)
            {
                var fieldTypeName = fieldInfo.FieldType.Name.ToString();
                switch (fieldTypeName)
                {
                    case "Boolean":
                        countBytes += 1;
                        break;
                    case "Single":
                        countBytes += 4;
                        break;
                    default:
                        throw new UnityException($"unknown field type: {fieldTypeName}");
                }
            }
            fieldCount = fields.Length;
            return countBytes;
        }
    }
}