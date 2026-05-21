using MelonLoader;
using System.Reflection;
using UnityEngine;

[assembly: MelonInfo(typeof(Mikmod.Mikmod), "Mikmod", "0.2.0", "IsaacAber")]
[assembly: MelonGame("DIC NetworkTechnologies", "Mikmak2")]

namespace Mikmod
{
    public class Mikmod : MelonMod
    {
        public override void OnInitializeMelon()
        {

            ExceptionHandler.Init();
            Settings.Init();
            Application.logMessageReceived += OnUnityLog;
            LoggerInstance.Msg("Mikmod loaded!");
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            LoggerInstance.Msg("Harmony patches applied!");
            LoggerInstance.Msg($"VSync {(Settings.DisableVsync.Value ? "disabled" : "enabled")}, unlimited FPS mode {(Settings.UnlimitedFps.Value ? "enabled" : "disabled")}!");
            LoggerInstance.Msg("SFS patches applied!");
            LoggerInstance.Msg("Current SFSPatches debugLevel: " + SFSPatches.debugLevel);
        }

        public override void OnUpdate()
        {
            WindowTweaks.OnUpdate();
            ModMenu.OnUpdate();
            DebugOverlay.OnUpdate();
            FSIngameHooks.OnUpdate();
            if (UnityEngine.Time.frameCount % 300 == 0)
            {
                FreeSpeech.CleanupDeadHandlers();
            }
        }

        public override void OnGUI()
        {
            ModMenu.OnGUI();
            DebugOverlay.OnGUI();
        }

        private void OnUnityLog(string condition, string stackTrace, LogType type)
        {
            if (condition.Contains("onDebugMessage")) return;
            if (condition.Contains("worldPositionStays")) return;

            switch (type)
            {
                case LogType.Error:
                case LogType.Exception:
                    LoggerInstance.Error("[Unity] " + condition);
                    break;
                case LogType.Warning:
                    LoggerInstance.Warning("[Unity] " + condition);
                    break;
                default:
                    LoggerInstance.Msg("[Unity] " + condition);
                    break;
            }
        }
    }
}