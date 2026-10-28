using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ClipCourier.Platform
{
    internal static class Native
    {
        public const uint ModAlt = 0x0001;
        public const uint ModControl = 0x0002;
        public const uint ModShift = 0x0004;
        public const uint ModNoRepeat = 0x4000;
        public const int WmHotkey = 0x0312;
        public const int HotkeyId = 0x541;
        private const byte VkControl = 0x11;
        private const byte VkC = 0x43;
        private const uint KeyUp = 0x0002;

        [DllImport("user32.dll", SetLastError = true)] public static extern bool RegisterHotKey(IntPtr hwnd, int id, uint modifiers, uint key);
        [DllImport("user32.dll", SetLastError = true)] public static extern bool UnregisterHotKey(IntPtr hwnd, int id);
        [DllImport("user32.dll")] public static extern uint GetClipboardSequenceNumber();
        [DllImport("user32.dll")] private static extern short GetAsyncKeyState(int virtualKey);
        [DllImport("user32.dll")] public static extern void keybd_event(byte key, byte scan, uint flags, UIntPtr extraInfo);

        public static bool IsKeyDown(int virtualKey)
        {
            return (GetAsyncKeyState(virtualKey) & 0x8000) != 0;
        }

        public static bool IsValidHotkey(uint modifiers, int key)
        {
            uint allowedModifiers = ModControl | ModAlt | ModShift;
            Keys keyCode = (Keys)key;
            bool allowedKey = (keyCode >= Keys.A && keyCode <= Keys.Z) ||
                (keyCode >= Keys.D0 && keyCode <= Keys.D9) ||
                (keyCode >= Keys.F1 && keyCode <= Keys.F12);
            return modifiers != 0 && (modifiers & ~allowedModifiers) == 0 && allowedKey;
        }

        // Returns the pre-copy clipboard sequence so the caller can wait for this copy to arrive.
        public static uint CopySelection()
        {
            uint before = GetClipboardSequenceNumber();
            keybd_event(VkControl, 0, 0, UIntPtr.Zero);
            keybd_event(VkC, 0, 0, UIntPtr.Zero);
            keybd_event(VkC, 0, KeyUp, UIntPtr.Zero);
            keybd_event(VkControl, 0, KeyUp, UIntPtr.Zero);
            return before;
        }

        public static string HotkeyText(uint modifiers, int key)
        {
            List<string> parts = new List<string>();
            if ((modifiers & ModControl) != 0) parts.Add("Ctrl");
            if ((modifiers & ModAlt) != 0) parts.Add("Alt");
            if ((modifiers & ModShift) != 0) parts.Add("Shift");
            Keys keyCode = (Keys)key;
            string keyText = keyCode.ToString();
            if (keyCode >= Keys.D0 && keyCode <= Keys.D9)
                keyText = ((int)keyCode - (int)Keys.D0).ToString();
            parts.Add(keyText);
            return String.Join("+", parts.ToArray());
        }
    }

}
