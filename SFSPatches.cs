using HarmonyLib;
using MelonLoader;
using SmartFoxClientAPI;
using System;
using System.Collections.Generic;


internal static class SFSPatches
{
    public enum DebugLevel
    {
        None = 0,
        ConnectionChange = 1,
        RawMessages = 2
    }

    public static DebugLevel debugLevel = DebugLevel.ConnectionChange;
    public static HashSet<SmartFoxClientAPI.SmartFoxClient> activeClients = new HashSet<SmartFoxClientAPI.SmartFoxClient>();
}

namespace Mikmod
{
    [HarmonyPatch(typeof(SmartFoxClient), "Connect", new Type[] { typeof(string), typeof(int) })]
    public static class SFS_Connect_StringPort_Patch
    {
        static void Prefix(SmartFoxClient __instance, string hostName, int port)
        {
            SFSPatches.activeClients.Add(__instance);
            if (SFSPatches.debugLevel >= SFSPatches.DebugLevel.ConnectionChange)
                MelonLogger.Msg($"[SFS-{SFSPatches.activeClients.Count}] Connect(string, int) → {hostName}:{port}");
        }
    }

    [HarmonyPatch(typeof(SmartFoxClient), "Disconnect")]
    public static class SFS_Disconnect_Patch
    {
        static void Prefix(SmartFoxClient __instance)
        {
            SFSPatches.activeClients.Remove(__instance);
            if (SFSPatches.debugLevel >= SFSPatches.DebugLevel.ConnectionChange)
                MelonLogger.Msg($"[SFS-{SFSPatches.activeClients.Count}] Disconnect() → {__instance.ipAddress}");
        }
    }

    [HarmonyPatch(typeof(SmartFoxClient), "DebugMessage")]
    public static class SFS_DebugMessage_Patch
    {
        static void Prefix(string message, SmartFoxClient __instance)
        {
            if (SFSPatches.debugLevel != SFSPatches.DebugLevel.RawMessages) return;
            MelonLogger.Msg($"[SFS-{SFSPatches.activeClients.Count}-RAW] {message}");
        }
    }
}