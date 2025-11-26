using Helper;
using LogViewManager;
using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using UniversalBoardTestApp;

namespace KeyPadLEDBarTest
{
    public partial class CustomMessageBox : Form
    {
        private Label lblMessage;
        private FlowLayoutPanel buttonPanel;
        private System.Windows.Forms.Timer hardwarePollTimer;

        public string SelectedButton { get; private set; }
        public int CapturedKeyValue { get; private set; } = -1;

        public CustomMessageBox(string title, string message, string[] buttons)
        {
            InitializeComponent();
            this.Text = title;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(700, 350);
            this.KeyPreview = true;
            this.KeyDown += CustomMessageBox_KeyDown;
            this.FormClosing += CustomMessageBox_FormClosing;

            lblMessage = new Label
            {
                Text = message,
                AutoSize = false,
                Location = new Point(15, 15),
                Size = new Size(600, 250)
            };

            buttonPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Bottom,
                Height = 45
            };

            foreach (var txt in buttons)
            {
                var btn = new Button
                {
                    Text = txt,
                    Width = 80,
                    Height = 30,
                    Margin = new Padding(10)
                };

                btn.Click += (s, e) =>
                {
                    SelectedButton = txt;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                };

                buttonPanel.Controls.Add(btn);
            }

            this.Controls.Add(lblMessage);
            this.Controls.Add(buttonPanel);

            // Hardware polling strictly via timer
            hardwarePollTimer = new System.Windows.Forms.Timer
            {
                Interval = 20
            };
            hardwarePollTimer.Tick += HardwarePollTimer_Tick;
            hardwarePollTimer.Start();
        }

        private void CustomMessageBox_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (hardwarePollTimer != null)
            {
                hardwarePollTimer.Stop();
                hardwarePollTimer.Dispose();
                hardwarePollTimer = null;
            }
        }

        private void HardwarePollTimer_Tick(object sender, EventArgs e)
        {
            int val;
            if (ReadHardwareKey(out val) && val > 0)
            {
                CapturedKeyValue = val;
                Console.WriteLine("Hardware Key Captured: " + val);
                Logger.LogMessage(Level.Info, $"Response from Device is {val}");

                // Optional: reflect capture in UI instead of showing a modal message box              
                lblMessage.Text = lblMessage.Text + "\n Detected Key: " + val;
                // Stop polling and close the dialog automatically once a key is captured
                hardwarePollTimer.Stop();
                SelectedButton = "Yes"; // if capture itself is considered confirmation
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
        private void CustomMessageBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Do NOT read hardware here. Only map keyboard to buttons.
            string target = null;

            if (e.KeyCode == Keys.Y) target = "Yes";
            else if (e.KeyCode == Keys.N) target = "No";
            else if (e.KeyCode == Keys.Enter)
            {
                if (buttonPanel.Controls.Count > 0)
                    target = ((Button)buttonPanel.Controls[0]).Text;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                SelectedButton = "Cancel";
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return;
            }

            if (target == null) return;

            foreach (Control c in buttonPanel.Controls)
            {
                if (c is Button b && b.Text.Equals(target, StringComparison.OrdinalIgnoreCase))
                {
                    b.PerformClick();
                    break;
                }
            }
        }
        private bool ReadHardwareKey(out int val)
        {
            val = -1;
            string resp;

            HardwareParameters.GetParameter(Handler.RESPONSE_KEYPROPERTY, out resp, true);
            if (string.IsNullOrEmpty(resp))
                return false;

            string[] lines = resp.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string ln = lines[i];
                string[] token = ln.Split(Handler.DELIMITER);

                if (token.Length >= 2 && token[0] == Handler.KEYS_RESP_CMD)
                {
                    val = Convert.ToInt32(token[1]);
                    return true;
                }
            }
            return false;
        }

        public static string Show(string title, string message, params string[] buttons)
        {
            using (CustomMessageBox box = new CustomMessageBox(title, message, buttons))
            {
                box.ShowDialog();
                return box.SelectedButton;
            }
        }
    }
}