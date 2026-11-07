//==============================================================
// PersonalizationColors — Windows personalization/theme settings reader (registry)
//
// GitHub: https://github.com/SevenKeyboard/personalization-colors
// Author: SevenKeyboard Ltd. (2025-2026)
// License: The Unlicense
//==============================================================

using System.Runtime.InteropServices;

namespace SevenKeyboard
{
    public enum ThemeMode
    {
        Dark,
        Light
    }

    public static class PersonalizationColors
    {
        private const string PersonalizeKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        private const string DwmKey = @"SOFTWARE\Microsoft\Windows\DWM";
        private static readonly nint CurrentUser = unchecked((nint)(int)0x80000001);
        private const uint DwordOnly = 0x00000010; // RRF_RT_REG_DWORD (excludes REG_BINARY).
        private const int ErrorFileNotFound = 2;
        private const int ErrorPathNotFound = 3;

        // Each query is independent. Use the out value only when the method returns true.
        // Missing keys/values succeed with the original defaults: Dark modes and false flags.
        public static bool TryGetDefaultWindowsMode(out ThemeMode mode) =>
            TryGetThemeMode("SystemUsesLightTheme", out mode);

        public static bool TryGetDefaultAppMode(out ThemeMode mode) =>
            TryGetThemeMode("AppsUseLightTheme", out mode);

        public static bool TryGetTransparencyEffects(out bool enabled) =>
            TryReadFlag(PersonalizeKey, "EnableTransparency", out enabled);

        public static bool TryGetShowAccentColorOnStartAndTaskbar(out bool enabled) =>
            TryReadFlag(PersonalizeKey, "ColorPrevalence", out enabled);

        public static bool TryGetShowAccentColorOnTitleBarsAndWindowsBorders(out bool enabled) =>
            TryReadFlag(DwmKey, "ColorPrevalence", out enabled);

        private static bool TryGetThemeMode(string valueName, out ThemeMode mode)
        {
            mode = default;
            if (!TryReadFlag(PersonalizeKey, valueName, out bool light))
                return false;

            mode = light ? ThemeMode.Light : ThemeMode.Dark;
            return true;
        }

        internal static bool TryReadFlag(string keyPath, string valueName, out bool enabled)
        {
            enabled = false;
            uint size = sizeof(int);
            // GetValue can hide native read errors behind its default value; keep their status here.
            int result = RegGetValueW(CurrentUser, keyPath, valueName, DwordOnly,
                out uint type, out int value, ref size);
            if (result is ErrorFileNotFound or ErrorPathNotFound)
                return true;
            if (result != 0 || type != 4 || size != sizeof(int))
                return false;

            enabled = value != 0;
            return true;
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int RegGetValueW(nint key, string subKey, string valueName,
            uint flags, out uint type, out int data, ref uint dataSize);
    }
}
