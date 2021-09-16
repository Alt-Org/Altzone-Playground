using Prg.Scripts.Common.Unity;
using Prg.Scripts.Common.Util;
using Prg.Scripts.Config;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using UnityEngine;

namespace Prg.Scripts
{
    public class BootLoader : MonoBehaviour
    {
        [Conditional("FORCE_LOG"), Conditional("DEVELOPMENT_BUILD")]
        private static void checkDevelopmentStatus()
        {
            // This is just for debugging to get strings (numbers) formatted consistently
            // - everything that goes to UI should go through Localizer using player's locale preferences
            var ci = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void beforeSceneLoad()
        {
            checkDevelopmentStatus();

            var localDevConfig = ResourceLoader.Get().LoadAsset<LocalDevConfig>(nameof(LocalDevConfig));
            LocalDevConfig.Instance = Instantiate(localDevConfig);

            var folderConfig = ResourceLoader.Get().LoadAsset<FolderConfig>(nameof(FolderConfig));
            var resourceLoader = ResourceLoader.Get(folderConfig.primaryConfigFolder, localDevConfig.developmentConfigFolder);

            var loggerConfig = resourceLoader.LoadAsset<LoggerConfig>(nameof(LoggerConfig));
            LoggerConfig.createLoggerConfig(loggerConfig);
        }
  }
}