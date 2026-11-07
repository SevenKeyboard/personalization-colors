# personalization-colors
Windows theme, transparency and accent-color settings for .NET 10.

```csharp
if (SevenKeyboard.PersonalizationColors.TryGetDefaultAppMode(out var mode))
    Console.WriteLine(mode);
```

Each query is independent. Missing keys or values succeed with the original AHK
defaults: `Dark` for theme modes and `false` for flags. Read errors or non-DWORD
values return `false`; use the output only on success. No caller `try/catch` is
needed for registry read failures.
