using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UniversalBoardTestApp;
using Helper;

namespace LEDBarTest
{
    public partial class KeyPadForm : Form
    {
        private List<Button> buttons;

        public KeyPadForm()
        {
            InitializeComponent();
        }

        private void btnMuteAlarm_Click(object sender, EventArgs e)
        {
            if (sender is Button clickedButton)
            {
                int index = buttons.IndexOf(clickedButton);
                if (index == -1) return;

                if(!ValidationButton(index))
                {
                    MessageBox.Show($"{clickedButton.Text} Validation failed.");
                    return;
                }

                //disable current button
                clickedButton.Enabled = false;

                // Enable next Button, or restart if at end
                if (index + 1 < buttons.Count)
                {
                    buttons[index + 1].Enabled = true;
                }
                else if (index + 1 == buttons.Count)
                {
                    index = 0;
                    MessageBox.Show("All Steps Completed! Restarting Sequence...");
                    ResetButtonSequence();
                }
            }

        //    btnMuteAlarm.Enabled = true;
        //    // check for the received Key (1,2,4,8,16,32,48 are expected from different keys).
        //    // check for the Beep sound after each key press.
        //    Button clickedButton = (Button) sender;
        //    string response = "";
        //    switch (clickedButton.Name)
        //    {
        //        case "btnMuteAlarm":
        //            UpdateButtonStates(btnMuteAlarm, false);
        //           // HardwareParameters.SetParameter($"MuteAlarm");
        //            HardwareParameters.GetParameter($"Keys", out response);
        //            // check whether it is receiving or not.
        //            // then convert to int and see it matches with the expected value.
        //            if (MessageBox.Show("Please confirm a Beep Sound is heard & received a Key value", Handler.KEYPAD_CAPTION, MessageBoxButtons.YesNo) == DialogResult.Yes)
        //            {

        //                // Disable MuteAlarm Button and Enable Select Button.
        //                UpdateButtonStates(btnSelect, false);
        //            }
        //            else return;
        //                break;
        //        case "btnSelect":
        //            UpdateButtonStates(btnSelect, false);
        //            HardwareParameters.GetParameter($"Keys", out response);
        //            if (MessageBox.Show("Please confirm a Beep Sound is heard & received a Key value", Handler.KEYPAD_CAPTION, MessageBoxButtons.YesNo) == DialogResult.Yes)
        //            {

        //                // Disable MuteAlarm Button and Enable Select Button.
        //                UpdateButtonStates(btnDock, false);
        //            }
        //            else return;
        //                break;
        //        case "btnDock":
        //            UpdateButtonStates(btnDock, false);
        //            HardwareParameters.GetParameter($"Keys", out response);
        //            if (MessageBox.Show("Please confirm a Beep Sound is heard & received a Key value", Handler.KEYPAD_CAPTION, MessageBoxButtons.YesNo) == DialogResult.Yes)
        //            {

        //                // Disable MuteAlarm Button and Enable Select Button.
        //                UpdateButtonStates(btnPurge, false);
        //            }
        //            else return;
        //            break;
        //        case "btnPurge": break;
        //            UpdateButtonStates(btnPurge, false);
        //            HardwareParameters.GetParameter($"Keys", out response);
        //            if (MessageBox.Show("Please confirm a Beep Sound is heard & received a Key value", Handler.KEYPAD_CAPTION, MessageBoxButtons.YesNo) == DialogResult.Yes)
        //            {

        //                // Disable MuteAlarm Button and Enable Select Button.
        //                UpdateButtonStates(btnFlow, false);
        //            }
        //            else return;
        //        case "btnFlow":
        //            UpdateButtonStates(btnFlow, false);
        //            HardwareParameters.GetParameter($"Keys", out response);
        //            if (MessageBox.Show("Please confirm a Beep Sound is heard & received a Key value", Handler.KEYPAD_CAPTION, MessageBoxButtons.YesNo) == DialogResult.Yes)
        //            {

        //                // Disable MuteAlarm Button and Enable Select Button.
        //                UpdateButtonStates(btnMuteAlarm, false);
        //            }
        //            else return;
        //            break;
        //    }

        }

        private bool ValidationButton(int index)
        {
            // Customizing each button as needed.
            switch (index)
            {
                case 0: // Mute Alarm
                    // validation logic

                    return true;
                case 1: // Select
                    // validation logic
                    return true;
                case 2: // Dock

                    // validation logic
                    return true;
                case 3: // Purge
                    // validation logic
                    return true;
                case 4: // Flow

                    // validation logic
                    return true;
                default: return false;
            }
        }

        private void ResetButtonSequence()
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].Enabled = (i == 0); // Enabiling only first i.e., Mute Alarm Button.
            }
        }
        //private void DisableOtherButtons(Button clickedButton)
        //{
        //    foreach( Control ctrl in this.Controls )
        //    {
        //        if(ctrl is Button button && button != clickedButton)
        //        {
        //            button.Enabled = false;
        //        }
        //    }
                
        //}

        private void KeyPadForm_Load(object sender, EventArgs e)
        {
            // sequence of buttons order is important here.
            buttons = new List<Button> { btnMuteAlarm, btnSelect, btnDock, btnPurge, btnFlow };
            foreach(var btn in buttons)
            {
                btn.Click += btnMuteAlarm_Click;
                btn.Enabled = false; //disable all buttons initially.
            }

            buttons[0].Enabled = true; // Mute Alarm is enabled.
        }
    }
}
