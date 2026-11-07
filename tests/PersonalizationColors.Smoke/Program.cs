using System;
using System.Security;
using System.Security.AccessControl;
using System.Security.Principal;
using Microsoft.Win32;
using SevenKeyboard;

string testPath = @"Software\SevenKeyboard.PersonalizationColors.Tests\" + Guid.NewGuid().ToString("N");
try
{
    ExpectDefault(testPath, "Missing key");
    using (RegistryKey key = Registry.CurrentUser.CreateSubKey(testPath))
    {
        ExpectDefault(testPath, "Missing value");
        key.SetValue("Unrelated", 1, RegistryValueKind.DWord);
        ExpectDefault(testPath, "Unrelated value does not replace the default");
        foreach (int value in new[] { 0, 1, 2, -1, int.MinValue, int.MaxValue })
        {
            key.SetValue("Flag", value, RegistryValueKind.DWord);
            Check(PersonalizationColors.TryReadFlag(testPath, "Flag", out bool flag) && flag == (value != 0), "DWORD " + value);
        }

        object[] invalidValues = { "", "1", 1L, new byte[] { 1, 0, 0, 0 }, new[] { "1" }, new byte[] { 1 } };
        RegistryValueKind[] invalidKinds = { RegistryValueKind.String, RegistryValueKind.ExpandString,
            RegistryValueKind.QWord, RegistryValueKind.Binary, RegistryValueKind.MultiString, RegistryValueKind.None };
        for (int i = 0; i < invalidValues.Length; i++)
        {
            key.SetValue("Flag", invalidValues[i], invalidKinds[i]);
            Check(!PersonalizationColors.TryReadFlag(testPath, "Flag", out bool flag) && !flag, "Reject " + invalidKinds[i]);
        }

        key.SetValue("Other", 1, RegistryValueKind.DWord);
        Check(PersonalizationColors.TryReadFlag(testPath, "Other", out bool other) && other, "Independent success");

        key.SetValue("Flag", 1, RegistryValueKind.DWord);
        Check(PersonalizationColors.TryReadFlag(testPath, "Flag", out bool recovered) && recovered,
            "Recovery after invalid types");
        key.DeleteValue("Flag");
        ExpectDefault(testPath, "Deleted value is not cached");

        ExpectDenied(testPath, "Denied read with a missing value");
        ExpectDefault(testPath, "Permissions restored with a missing value");
        key.SetValue("Flag", 1, RegistryValueKind.DWord);
        ExpectDenied(testPath, "Denied read with an existing value");
        Check(PersonalizationColors.TryReadFlag(testPath, "Flag", out bool restored) && restored,
            "Permissions restored with an existing value");
    }
    Registry.CurrentUser.DeleteSubKeyTree(testPath);
    ExpectDefault(testPath, "Deleted key");
    using (RegistryKey recreated = Registry.CurrentUser.CreateSubKey(testPath))
        recreated.SetValue("Flag", 1, RegistryValueKind.DWord);
    Check(PersonalizationColors.TryReadFlag(testPath, "Flag", out bool changed) && changed,
        "Recreated key is read afresh");
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

static void ExpectDefault(string keyPath, string name)
{
    bool enabled = true;
    Check(PersonalizationColors.TryReadFlag(keyPath, "Flag", out enabled) && !enabled, name);
}

static void ExpectDenied(string keyPath, string name)
{
    // Only this GUID-owned fixture is changed. Retain WRITE_DAC for unconditional restoration.
    using RegistryKey permissions = Registry.CurrentUser.OpenSubKey(keyPath,
        RegistryKeyPermissionCheck.ReadWriteSubTree,
        RegistryRights.ReadPermissions | RegistryRights.ChangePermissions)!;
    RegistrySecurity original = permissions.GetAccessControl(AccessControlSections.Access);
    using WindowsIdentity identity = WindowsIdentity.GetCurrent();
    try
    {
        RegistrySecurity denied = permissions.GetAccessControl(AccessControlSections.Access);
        denied.AddAccessRule(new RegistryAccessRule(identity.User!, RegistryRights.QueryValues,
            InheritanceFlags.None, PropagationFlags.None, AccessControlType.Deny));
        permissions.SetAccessControl(denied);
        bool accessDenied = false;
        try { using RegistryKey? unexpected = Registry.CurrentUser.OpenSubKey(keyPath); }
        catch (Exception exception) when (exception is UnauthorizedAccessException or SecurityException)
        { accessDenied = true; }
        Check(accessDenied, "Fixture must actually deny reads");
        bool enabled = true;
        Check(!PersonalizationColors.TryReadFlag(keyPath, "Flag", out enabled) && !enabled, name);
    }
    finally
    {
        var restored = new RegistrySecurity();
        restored.SetSecurityDescriptorBinaryForm(original.GetSecurityDescriptorBinaryForm(),
            AccessControlSections.Access);
        permissions.SetAccessControl(restored);
    }
}
