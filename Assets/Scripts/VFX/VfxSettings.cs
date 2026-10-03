using System;
using UnityEngine;

// Whether particle effects play. Set from the settings menu (either scene); the
// effect scripts check it themselves, so nothing has to be wired in the Inspector.
public static class VfxSettings
{
    private const string Key = "vfx";

    private static bool? enabled;

    public static event Action<bool> OnChanged;

    public static bool Enabled
    {
        get
        {
            enabled ??= PlayerPrefs.GetInt(Key, 1) == 1;
            return enabled.Value;
        }
        set
        {
            if (value == Enabled) return;

            enabled = value;
            PlayerPrefs.SetInt(Key, value ? 1 : 0);
            OnChanged?.Invoke(value);
        }
    }
}
