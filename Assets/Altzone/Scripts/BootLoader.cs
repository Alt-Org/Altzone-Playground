using Altzone.Scripts.Config;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.Unity;
using Prg.Scripts.Common.Util;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using UnityEngine;

namespace Altzone.Scripts
{
    public class BootLoader : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void beforeSceneLoad()
        {
            var localDevConfig = ResourceLoader.Get().LoadAsset<LocalDevConfig>(nameof(LocalDevConfig));
            LocalDevConfig.Instance = Instantiate(localDevConfig);

            var folderConfig = ResourceLoader.Get().LoadAsset<FolderConfig>(nameof(FolderConfig));
            var resourceLoader = ResourceLoader.Get(folderConfig.primaryConfigFolder, localDevConfig.developmentConfigFolder);

            var loggerConfig = resourceLoader.LoadAsset<LoggerConfig>(nameof(LoggerConfig));
            LoggerConfig.createLoggerConfig(loggerConfig);

            setDevelopmentStatus();
        }

        [Conditional("FORCE_LOG"), Conditional("DEVELOPMENT_BUILD")]
        private static void setDevelopmentStatus()
        {
            // This is just for debugging to get strings (numbers) formatted consistently
            // - everything that goes to UI should go through Localizer using player's locale preferences
            var ci = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;

            if (!string.IsNullOrWhiteSpace(LocalDevConfig.Instance.photonVersionPrefix))
            {
                PhotonLobby._gameVersion = () => $"{LocalDevConfig.Instance.photonVersionPrefix}{Application.version}";
            }
        }
    }
}