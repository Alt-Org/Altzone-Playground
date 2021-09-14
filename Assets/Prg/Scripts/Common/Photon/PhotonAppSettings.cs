using Photon.Realtime;
using UnityEngine;

namespace Prg.Scripts.Common.Photon
{
    //[CreateAssetMenu(menuName = "ALT-Zone/PhotonAppSettings")]
    public class PhotonAppSettings : ScriptableObject
    {
        public AppSettings appSettings;

        public override string ToString()
        {
            return appSettings != null ? appSettings.ToStringFull() : "";
        }
    }
}
