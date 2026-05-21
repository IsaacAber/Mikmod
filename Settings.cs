using MelonLoader;

namespace Mikmod
{
    internal static class Settings
    {
        private static MelonPreferences_Category _cat;

        public static MelonPreferences_Entry<bool> DisableVsync;
        public static MelonPreferences_Entry<bool> UnlimitedFps;
        public static MelonPreferences_Entry<float> MessageLifetime;

        public static void Init()
        {
            _cat = MelonPreferences.CreateCategory("Mikmod", "Mikmod Settings");

            DisableVsync = _cat.CreateEntry("DisableVsync", false);
            UnlimitedFps = _cat.CreateEntry("UnlimitedFps", false);
            MessageLifetime = _cat.CreateEntry("MessageLifetime", 5f);

            MelonPreferences.Save();
        }
    }
}