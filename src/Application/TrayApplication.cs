using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ClipCourier.Infrastructure;
using ClipCourier.Models;
using ClipCourier.Platform;
using ClipCourier.Services;
using ClipCourier.UI;

namespace ClipCourier.App
{
    internal sealed class TrayApplication : Form
    {
        private AppConfig config;
        private readonly NotifyIcon tray;
        private readonly System.Windows.Forms.Timer releaseTimer;
        private readonly System.Windows.Forms.Timer clipboardTimer;
        private uint clipboardSequence;
        private DateTime captureStarted;
        private DateTime releaseWaitStarted;
        private bool capturing;
        private bool hotkeyRegistered;

        public TrayApplication(AppConfig initialConfig)
        {
            config = initialConfig;
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            WindowState = FormWindowState.Minimized;
            Text = "ClipCourier";
            Size = new Size(1, 1);

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("Choose destination…", null, delegate { BeginCapture(); });
            menu.Items.Add("Settings…", null, delegate { OpenSettings(); });
            menu.Items.Add("Open settings folder", null, delegate { OpenSettingsFolder(); });
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Exit", null, delegate { Close(); });
            tray = new NotifyIcon { Icon = SystemIcons.Application, Text = "ClipCourier", ContextMenuStrip = menu, Visible = true };
            tray.DoubleClick += delegate { OpenSettings(); };

            releaseTimer = new System.Windows.Forms.Timer { Interval = 30 };
            releaseTimer.Tick += WaitForHotkeyRelease;
            clipboardTimer = new System.Windows.Forms.Timer { Interval = 45 };
            clipboardTimer.Tick += PollClipboard;

            Shown += delegate
            {
                Hide();
                ApplyHotkey();
                // Repair the sign-in entry when the app is moved or a previous registry
                // update did not stick. This also makes the saved setting authoritative.
                try { StartupManager.SetEnabled(config.LaunchAtLogin); }
                catch (Exception ex) { AppLog.Write(ex); }
                if (config.Destinations.Count == 0) OpenSettings();
            };
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Native.WmHotkey && m.WParam.ToInt32() == Native.HotkeyId)
            {
                BeginCapture();
                return;
            }
            base.WndProc(ref m);
        }

        private void ApplyHotkey()
        {
            if (hotkeyRegistered)
            {
                Native.UnregisterHotKey(Handle, Native.HotkeyId);
                hotkeyRegistered = false;
            }
            hotkeyRegistered = Native.RegisterHotKey(Handle, Native.HotkeyId,
                config.HotkeyModifiers | Native.ModNoRepeat, (uint)config.HotkeyKey);
            tray.Text = hotkeyRegistered ? "ClipCourier — " + Native.HotkeyText(config.HotkeyModifiers, config.HotkeyKey) : "ClipCourier — shortcut unavailable";
            if (!hotkeyRegistered)
                tray.ShowBalloonTip(5000, "Shortcut unavailable", "Another app is using " + Native.HotkeyText(config.HotkeyModifiers, config.HotkeyKey) + ". Change it in Settings.", ToolTipIcon.Warning);
        }

        private void BeginCapture()
        {
            if (capturing) return;
            capturing = true;
            releaseWaitStarted = DateTime.UtcNow;
            // RegisterHotKey fires on key-down. Wait until the chord is fully released,
            // or the synthetic Ctrl+C can still include the user's Ctrl/Alt modifiers.
            releaseTimer.Start();
        }

        private void WaitForHotkeyRelease(object sender, EventArgs e)
        {
            bool shortcutKeyDown = Native.IsKeyDown(config.HotkeyKey);
            bool modifierDown = Native.IsKeyDown(0x11) || Native.IsKeyDown(0x12) || Native.IsKeyDown(0x10);
            if (shortcutKeyDown || modifierDown)
            {
                if ((DateTime.UtcNow - releaseWaitStarted).TotalMilliseconds <= 2000) return;
                releaseTimer.Stop();
                capturing = false;
                tray.ShowBalloonTip(4000, "Shortcut still held", "Release the shortcut keys, then try again.", ToolTipIcon.Warning);
                return;
            }

            releaseTimer.Stop();
            SendCopyAndWait();
        }

        // Arm clipboard polling only after sending Ctrl+C, using the earlier sequence as the baseline.
        private void SendCopyAndWait()
        {
            clipboardSequence = Native.CopySelection();
            captureStarted = DateTime.UtcNow;
            clipboardTimer.Start();
        }

        private void PollClipboard(object sender, EventArgs e)
        {
            // The sequence distinguishes this copy from old clipboard data.
            if (Native.GetClipboardSequenceNumber() != clipboardSequence)
            {
                try
                {
                    string text = Clipboard.ContainsText(TextDataFormat.UnicodeText) ? Clipboard.GetText(TextDataFormat.UnicodeText) : "";
                    if (!String.IsNullOrWhiteSpace(text))
                    {
                        clipboardTimer.Stop();
                        capturing = false;
                        ShowPicker(text);
                        return;
                    }
                }
                catch (ExternalException)
                {
                    // The next timer tick retries if another process has locked the clipboard.
                }
            }
            if ((DateTime.UtcNow - captureStarted).TotalMilliseconds > 2500)
            {
                clipboardTimer.Stop();
                capturing = false;
                tray.ShowBalloonTip(4000, "Could not copy selection", "Select text in an app that supports Ctrl+C, then try again.", ToolTipIcon.Warning);
            }
        }

        // Do not write anything until the user explicitly chooses a destination in the picker.
        private void ShowPicker(string text)
        {
            if (config.Destinations.Count == 0)
            {
                tray.ShowBalloonTip(4000, "No destinations yet", "Add one or more files in ClipCourier Settings.", ToolTipIcon.Info);
                OpenSettings();
                return;
            }
            using (DestinationPicker picker = new DestinationPicker(config.Destinations, text))
            {
                if (picker.ShowDialog() == DialogResult.OK && picker.SelectedDestinations != null)
                {
                    int saved = 0;
                    string failures = "";
                    foreach (Destination destination in picker.SelectedDestinations)
                    {
                        try
                        {
                            TextFileAppender.Append(destination.Path, text);
                            saved++;
                        }
                        catch (Exception ex)
                        {
                            AppLog.Write(ex);
                            failures += "\r\n• " + destination.Name + ": " + ex.Message;
                        }
                    }
                    if (failures.Length == 0)
                        tray.ShowBalloonTip(2500, "Selection saved", "Appended to " + saved + " file(s).", ToolTipIcon.Info);
                    else
                        MessageBox.Show("Saved to " + saved + " of " + picker.SelectedDestinations.Count + " selected files. Your copied text is still on the clipboard.\r\n" + failures,
                            "ClipCourier", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        // Roll back the shortcut and startup entry if saving settings fails.
        private void OpenSettings()
        {
            AppConfig edited = config.Copy();
            using (SettingsWindow window = new SettingsWindow(edited))
            {
                if (window.ShowDialog() != DialogResult.OK) return;
                edited = window.Result;
            }

            AppConfig previous = config;
            config = edited;
            ApplyHotkey();
            if (!hotkeyRegistered)
            {
                config = previous;
                ApplyHotkey();
                MessageBox.Show("That shortcut is already in use. Choose a different combination.", "ClipCourier",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // Reconcile even when the checkbox is unchanged: users can move the portable
            // executable or the Run value can be removed by Windows or another utility.
            bool startupUpdateAttempted = true;
            try
            {
                StartupManager.SetEnabled(config.LaunchAtLogin);
                ConfigStore.Save(config);
            }
            catch (Exception ex)
            {
                config = previous;
                ApplyHotkey();
                string message = ex.Message;
                if (startupUpdateAttempted)
                {
                    try { StartupManager.SetEnabled(previous.LaunchAtLogin); }
                    catch (Exception rollbackError)
                    {
                        AppLog.Write(rollbackError);
                        message += "\r\n\r\nThe previous sign-in startup setting could not be restored.";
                    }
                }
                AppLog.Write(ex);
                MessageBox.Show("Settings could not be saved.\r\n\r\n" + message, "ClipCourier",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenSettingsFolder()
        {
            try
            {
                Directory.CreateDirectory(ConfigStore.Folder);
                Process.Start("explorer.exe", "\"" + ConfigStore.Folder + "\"");
            }
            catch (Exception ex)
            {
                AppLog.Write(ex);
                MessageBox.Show("Could not open the settings folder.\r\n\r\n" + ex.Message, "ClipCourier",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            releaseTimer.Stop();
            clipboardTimer.Stop();
            if (hotkeyRegistered) Native.UnregisterHotKey(Handle, Native.HotkeyId);
            tray.Visible = false;
            tray.Dispose();
            base.OnFormClosing(e);
        }
    }

}
