using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ClipCourier.Models;
using ClipCourier.Platform;

namespace ClipCourier.UI
{
    internal sealed class SettingsWindow : Form
    {
        private readonly AppConfig config;
        private readonly ListBox list;
        private readonly TextBox hotkey;
        private readonly CheckBox launchAtLogin;
        private uint capturedModifiers;
        private int capturedKey;
        public AppConfig Result { get; private set; }

        public SettingsWindow(AppConfig initial)
        {
            config = initial.Copy();
            capturedModifiers = config.HotkeyModifiers;
            capturedKey = config.HotkeyKey;
            Text = "ClipCourier Settings";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = true;
            ClientSize = new Size(650, 475);

            Label hotkeyLabel = new Label { Text = "Global shortcut (click and press a combination)", AutoSize = true, Location = new Point(18, 18) };
            Controls.Add(hotkeyLabel);
            hotkey = new TextBox { ReadOnly = true, Location = new Point(18, 43), Size = new Size(230, 26), Text = Native.HotkeyText(capturedModifiers, capturedKey) };
            hotkey.KeyDown += CaptureHotkey;
            hotkey.Click += delegate { hotkey.Focus(); };
            Controls.Add(hotkey);
            Label hint = new Label { Text = "Use Ctrl, Alt, or Shift plus a letter, number, or function key.", AutoSize = true, Location = new Point(260, 48), ForeColor = SystemColors.GrayText };
            Controls.Add(hint);

            Label destinationsLabel = new Label { Text = "Destination text files", AutoSize = true, Location = new Point(18, 88) };
            Controls.Add(destinationsLabel);
            list = new ListBox { Location = new Point(18, 113), Size = new Size(614, 240), IntegralHeight = false };
            RefreshList();
            Controls.Add(list);

            Button add = new Button { Text = "Add files…", Location = new Point(18, 365), Size = new Size(90, 30) };
            add.Click += AddDestinations;
            Controls.Add(add);
            Button edit = new Button { Text = "Edit…", Location = new Point(118, 365), Size = new Size(90, 30) };
            edit.Click += delegate { if (list.SelectedIndex >= 0) EditDestination(config.Destinations[list.SelectedIndex]); };
            Controls.Add(edit);
            Button remove = new Button { Text = "Remove", Location = new Point(218, 365), Size = new Size(90, 30) };
            remove.Click += delegate { if (list.SelectedIndex >= 0) { config.Destinations.RemoveAt(list.SelectedIndex); RefreshList(); } };
            Controls.Add(remove);

            launchAtLogin = new CheckBox { Text = "Start ClipCourier when I sign in", Checked = config.LaunchAtLogin, AutoSize = true, Location = new Point(18, 421) };
            Controls.Add(launchAtLogin);
            Button save = new Button { Text = "Save", Location = new Point(456, 417), Size = new Size(82, 32) };
            save.Click += delegate { SaveAndClose(); };
            Controls.Add(save);
            Button cancel = new Button { Text = "Cancel", Location = new Point(550, 417), Size = new Size(82, 32), DialogResult = DialogResult.Cancel };
            Controls.Add(cancel);
            AcceptButton = save;
            CancelButton = cancel;
        }

        // Store modifiers separately because RegisterHotKey expects a key and modifier bitmask.
        private void CaptureHotkey(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
            e.Handled = true;
            Keys key = e.KeyCode;
            if (key == Keys.ControlKey || key == Keys.Menu || key == Keys.ShiftKey || key == Keys.LWin || key == Keys.RWin) return;
            uint modifiers = 0;
            if (e.Control) modifiers |= Native.ModControl;
            if (e.Alt) modifiers |= Native.ModAlt;
            if (e.Shift) modifiers |= Native.ModShift;
            if (!Native.IsValidHotkey(modifiers, (int)key))
            {
                System.Media.SystemSounds.Beep.Play();
                return;
            }
            capturedModifiers = modifiers;
            capturedKey = (int)key;
            hotkey.Text = Native.HotkeyText(capturedModifiers, capturedKey);
        }

        private void RefreshList()
        {
            if (list == null) return;
            list.Items.Clear();
            foreach (Destination destination in config.Destinations) list.Items.Add(destination);
        }

        private void EditDestination(Destination existing)
        {
            using (DestinationEditor editor = new DestinationEditor(existing))
            {
                if (editor.ShowDialog(this) != DialogResult.OK) return;
                if (existing == null) config.Destinations.Add(editor.Result);
                else
                {
                    int index = config.Destinations.IndexOf(existing);
                    config.Destinations[index] = editor.Result;
                }
                RefreshList();
            }
        }

        private void AddDestinations(object sender, System.EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Choose destination files";
                dialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                dialog.Multiselect = true;
                if (dialog.ShowDialog(this) != DialogResult.OK) return;

                foreach (string path in dialog.FileNames)
                {
                    bool alreadyAdded = false;
                    foreach (Destination existing in config.Destinations)
                        if (String.Equals(Path.GetFullPath(existing.Path), Path.GetFullPath(path), StringComparison.OrdinalIgnoreCase))
                        {
                            alreadyAdded = true;
                            break;
                        }
                    if (!alreadyAdded)
                        config.Destinations.Add(new Destination(Path.GetFileNameWithoutExtension(path), path));
                }
                RefreshList();
            }
        }

        private void SaveAndClose()
        {
            if (config.Destinations.Count == 0)
            {
                DialogResult answer = MessageBox.Show(this, "No destination files are configured. Save anyway?",
                    "ClipCourier", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (answer != DialogResult.Yes) return;
            }
            config.HotkeyKey = capturedKey;
            config.HotkeyModifiers = capturedModifiers;
            config.LaunchAtLogin = launchAtLogin.Checked;
            Result = config;
            DialogResult = DialogResult.OK;
            Close();
        }
    }

}
