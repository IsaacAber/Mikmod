using UnityEngine;

namespace Mikmod
{
    public static class WindowTweaks
    {
        public static void OnUpdate()
        {
            if (Settings.DisableVsync.Value)
            {
                QualitySettings.vSyncCount = 0;
            }
            if (Settings.UnlimitedFps.Value)
            {
                Application.targetFrameRate = -1;
            }
        }
    }
}