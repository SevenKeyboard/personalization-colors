using System;
using Microsoft.Win32;
using SevenKeyboard;

string testPath = @"Software\SevenKeyboard.PersonalizationColors.Tests\" + Guid.NewGuid().ToString("N");
try
{
    Check(!PersonalizationColors.TryReadFlag(testPath, "Flag", out _), "Missing key");
    using (RegistryKey key = Registry.CurrentUser.CreateSubKey(testPath))
    {
        Check(!PersonalizationColors.TryReadFlag(testPath, "Flag", out _), "Missing value");
        foreach (int value in new[] { 0, 1, 2, -1 })
        {
            key.SetValue("Flag", value, RegistryValueKind.DWord);
            Check(PersonalizationColors.TryReadFlag(testPath, "Flag", out bool flag) && flag == (value != 0), "DWORD " + value);
        }

        object[] invalidValues = { "1", "1", 1L, new byte[] { 1 }, new[] { "1" }, new byte[] { 1 } };
        RegistryValueKind[] invalidKinds = { RegistryValueKind.String, RegistryValueKind.ExpandString,
            RegistryValueKind.QWord, RegistryValueKind.Binary, RegistryValueKind.MultiString, RegistryValueKind.None };
        for (int i = 0; i < invalidValues.Length; i++)
        {
            key.SetValue("Flag", invalidValues[i], invalidKinds[i]);
            Check(!PersonalizationColors.TryReadFlag(testPath, "Flag", out bool flag) && !flag, "Reject " + invalidKinds[i]);
        }

        key.SetValue("Other", 1, RegistryValueKind.DWord);
        Check(PersonalizationColors.TryReadFlag(testPath, "Other", out bool other) && other, "Independent success");
    }
}
finally
{
    Registry.CurrentUser.DeleteSubKeyTree(testPath, throwOnMissingSubKey: false);
}

Console.WriteLine($"Windows mode: {PersonalizationColors.TryGetDefaultWindowsMode(out var windowsMode)}, {windowsMode}");
Console.WriteLine($"App mode: {PersonalizationColors.TryGetDefaultAppMode(out var appMode)}, {appMode}");
Console.WriteLine($"Transparency: {PersonalizationColors.TryGetTransparencyEffects(out bool transparency)}, {transparency}");
Console.WriteLine($"Start/taskbar: {PersonalizationColors.TryGetShowAccentColorOnStartAndTaskbar(out bool start)}, {start}");
Console.WriteLine($"Title/borders: {PersonalizationColors.TryGetShowAccentColorOnTitleBarsAndWindowsBorders(out bool borders)}, {borders}");
Console.WriteLine("Smoke checks passed.");

static void Check(bool passed, string name)
{
    if (!passed)
        throw new InvalidOperationException(name);
}
