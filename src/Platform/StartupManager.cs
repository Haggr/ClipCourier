using System;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ClipCourier.Platform
{
    internal static class StartupManager
    {
        private const string RunKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";
        private const string ValueName = "ClipCourier";

        public static void SetEnabled(bool enabled)
        {
            RegistryKey writableKey = Registry.CurrentUser.OpenSubKey(RunKey, true);
            if (enabled && writableKey == null) writableKey = Registry.CurrentUser.CreateSubKey(RunKey);
            if (enabled && writableKey == null) throw new InvalidOperationException("The Windows startup registry key could not be opened.");
            if (writableKey == null) return;

            using (RegistryKey key = writableKey)
            {
                if (enabled) key.SetValue(ValueName, "\"" + Application.ExecutablePath + "\"");
                else key.DeleteValue(ValueName, false);
            }
        }
    }

}
