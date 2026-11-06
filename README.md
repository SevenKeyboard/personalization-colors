# personalization-colors
Windows theme, transparency and accent-color settings for .NET 10.

```csharp
if (SevenKeyboard.PersonalizationColors.TryGetDefaultAppMode(out var mode))
    Console.WriteLine(mode);
```

Each query is independent. Missing, unreadable or non-DWORD values return false;
use the output only on success.
