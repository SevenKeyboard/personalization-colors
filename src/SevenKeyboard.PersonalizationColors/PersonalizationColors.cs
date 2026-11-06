//==============================================================
// PersonalizationColors — Windows personalization/theme settings reader (registry)
//
// GitHub: https://github.com/SevenKeyboard/personalization-colors
// Author: SevenKeyboard Ltd. (2025-2026)
// License: The Unlicense
//==============================================================

using System;
using System.IO;
using System.Security;
using Microsoft.Win32;

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

        // Each query is independent. Use the out value only when the method returns true.
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
            try
            {
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(keyPath, writable: false);
                // GetValue returns Int32 only for REG_DWORD; do not coerce other registry types.
                if (key?.GetValue(valueName) is not int value)
                    return false;

                enabled = value != 0;
                return true;
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or SecurityException)
            {
                return false;
            }
        }
    }
}
