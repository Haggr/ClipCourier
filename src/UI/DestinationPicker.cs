using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ClipCourier.Models;

namespace ClipCourier.UI
{
    internal sealed class DestinationPicker : Form
    {
        private readonly List<Destination> destinations;
        public List<Destination> SelectedDestinations { get; private set; }
        private readonly ListBox list;

        public DestinationPicker(List<Destination> items, string selectedText)
        {
            destinations = new List<Destination>(items);
            Text = "Append selection to…";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = true;
            TopMost = true;
            ClientSize = new Size(540, 350);

            Label heading = new Label { Text = "Choose destination files (Ctrl/Shift selects multiple)", AutoSize = true, Location = new Point(16, 14), Font = new Font("Segoe UI", 11, FontStyle.Bold) };
            Controls.Add(heading);
            string preview = selectedText.Replace("\r", " ").Replace("\n", " ").Trim();
            if (preview.Length > 130) preview = preview.Substring(0, 127) + "…";
            Label previewLabel = new Label { Text = "Selection: " + preview, AutoEllipsis = true, Location = new Point(16, 43), Size = new Size(505, 32), ForeColor = SystemColors.GrayText };
            Controls.Add(previewLabel);

            list = new ListBox { Location = new Point(16, 82), Size = new Size(505, 208), Font = new Font("Segoe UI", 10), IntegralHeight = false, SelectionMode = SelectionMode.MultiExtended };
            foreach (Destination destination in destinations) list.Items.Add(destination);
            if (list.Items.Count > 0) list.SelectedIndex = 0;
            list.DoubleClick += delegate { Commit(); };
            Controls.Add(list);

            Button append = new Button { Text = "Append to selected", Location = new Point(308, 304), Size = new Size(119, 30) };
            append.Click += delegate { Commit(); };
            Controls.Add(append);
            Button cancel = new Button { Text = "Cancel", Location = new Point(439, 304), Size = new Size(82, 30), DialogResult = DialogResult.Cancel };
            Controls.Add(cancel);
            AcceptButton = append;
            CancelButton = cancel;
        }

        private void Commit()
        {
            SelectedDestinations = new List<Destination>();
            foreach (object item in list.SelectedItems)
            {
                Destination destination = item as Destination;
                if (destination != null) SelectedDestinations.Add(destination);
            }
            if (SelectedDestinations.Count > 0) DialogResult = DialogResult.OK;
        }
    }

}
