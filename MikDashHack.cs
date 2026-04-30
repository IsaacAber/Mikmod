using HarmonyLib;
using System;
using static Code.Core.Utils.Globals;

namespace Mikmod
{

    [HarmonyPatch(typeof(General), nameof(General.IsDebugBuild))]
    public static class IsDebugBuild_Patch
    {
        static bool Prefix(Action onDebug = null, Action onNotDebug = null)
        {
            if (Settings.MikDashHackEnabled.Value)
                onDebug?.Invoke();
            else
                onNotDebug?.Invoke();

            return false; // always skip original
        }
    }
}