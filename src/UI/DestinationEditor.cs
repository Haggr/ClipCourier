using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ClipCourier.Models;

namespace ClipCourier.UI
{
    internal sealed class DestinationEditor : Form
    {
        private readonly TextBox nameBox;
        private readonly TextBox pathBox;
        public Destination Result { get; private set; }

        public DestinationEditor(Destination existing)
        {
            Text = existing == null ? "Add destination" : "Edit destination";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(520, 190);
            Label nameLabel = new Label { Text = "Name shown in the picker", AutoSize = true, Location = new Point(16, 16) };
            Controls.Add(nameLabel);
            nameBox = new TextBox { Location = new Point(16, 40), Size = new Size(488, 25), Text = existing == null ? "" : existing.Name };
            Controls.Add(nameBox);
            Label pathLabel = new Label { Text = "Destination file", AutoSize = true, Location = new Point(16, 78) };
            Controls.Add(pathLabel);
            pathBox = new TextBox { Location = new Point(16, 102), Size = new Size(390, 25), Text = existing == null ? "" : existing.Path };
            Controls.Add(pathBox);
            Button browse = new Button { Text = "Browse…", Location = new Point(416, 100), Size = new Size(88, 28) };
            browse.Click += Browse;
            Controls.Add(browse);
            Button save = new Button { Text = "Save", Location = new Point(328, 148), Size = new Size(82, 28) };
            save.Click += Save;
            Controls.Add(save);
            Button cancel = new Button { Text = "Cancel", Location = new Point(422, 148), Size = new Size(82, 28), DialogResult = DialogResult.Cancel };
            Controls.Add(cancel);
            AcceptButton = save;
            CancelButton = cancel;
        }

        private void Browse(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Title = "Choose a text file";
                dialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                dialog.DefaultExt = "txt";
                dialog.AddExtension = true;
                dialog.OverwritePrompt = false;
                if (!String.IsNullOrWhiteSpace(pathBox.Text))
                {
                    try
                    {
                        dialog.InitialDirectory = Path.GetDirectoryName(pathBox.Text);
                        dialog.FileName = Path.GetFileName(pathBox.Text);
                    }
                    catch { }
                }
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    pathBox.Text = dialog.FileName;
                    if (String.IsNullOrWhiteSpace(nameBox.Text)) nameBox.Text = Path.GetFileNameWithoutExtension(dialog.FileName);
                }
            }
        }

        private void Save(object sender, EventArgs e)
        {
            string name = nameBox.Text.Trim();
            string path = Environment.ExpandEnvironmentVariables(pathBox.Text.Trim());
            if (String.IsNullOrWhiteSpace(name) || String.IsNullOrWhiteSpace(path))
            {
                MessageBox.Show(this, "Enter a display name and destination file.", "ClipCourier", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!HasUsableFilePath(path))
            {
                MessageBox.Show(this, "Choose a full file path.", "ClipCourier", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Result = new Destination(name, path);
            DialogResult = DialogResult.OK;
            Close();
        }

        private static bool HasUsableFilePath(string path)
        {
            try
            {
                return Path.IsPathRooted(path) && !String.IsNullOrWhiteSpace(Path.GetDirectoryName(path));
            }
            catch (ArgumentException) { return false; }
            catch (NotSupportedException) { return false; }
            catch (PathTooLongException) { return false; }
        }
    }

}
