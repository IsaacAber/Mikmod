using MelonLoader;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Mikmod
{
    internal static class ExceptionHandler
    {
        public static void Init()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Handle("AppDomain", e.ExceptionObject as Exception);
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                Handle("Task", e.Exception);
                e.SetObserved();
            };

            Application.logMessageReceived += (condition, stackTrace, type) =>
            {
                if (type == LogType.Exception)
                {
                    Handle("Unity", new Exception(condition + "\n" + stackTrace));
                }
            };
            MelonLogger.Msg("Exception handler initialized!");
        }

        private static void Handle(string source, Exception ex)
        {
            if (ex == null) return;

            MelonLogger.Error($"[EXCEPTION:{source}] {ex}");
        }
    }
}