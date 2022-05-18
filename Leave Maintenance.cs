using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Viscinty_LeaveApp
{
    public partial class Leave_Maintenance : Form
    {
        users LUSR;
        Dictionary<String, users> DBAccts;
        Dictionary<String, List<leave>> Leaves_List;
        Dictionary<leave.LEAVE_STATUS, int> _status;
         public Leave_Maintenance()
        {
            InitializeComponent();
        }

        #region Methods
         public void form_setup(users Luser)
        {
            this.LUSR = Luser;
            this.DBAccts = users.get_User("");

            comboBox2.Items.Clear();
            comboBox1.Items.Clear();
            _status = new Dictionary<leave.LEAVE_STATUS, int>();
            _status.Add(leave.LEAVE_STATUS.PEND, (int)leave.LEAVE_STATUS.PEND);
            _status.Add(leave.LEAVE_STATUS.APRV1, (int)leave.LEAVE_STATUS.APRV1);
            _status.Add(leave.LEAVE_STATUS.APRV2, (int)leave.LEAVE_STATUS.APRV2);
            _status.Add(leave.LEAVE_STATUS.APRV3, (int)leave.LEAVE_STATUS.APRV3);
            _status.Add(leave.LEAVE_STATUS.REJC, (int)leave.LEAVE_STATUS.REJC);

            foreach (leave.LEAVE_STATUS lstate in _status.Keys)
            {
                comboBox2.Items.Add(leave.leaveStatusDesc(lstate));
            }

            foreach (users acctAv in this.DBAccts.Values)
            {
                String acctName = acctAv.UserID + ": " + acctAv.Lastname + ", " + acctAv.Firstname;
                comboBox1.Items.Add(acctName);
            }

            disp_leaveList();
        }
        private void disp_leaveList()
        {
            listBox1.Items.Clear();
            this.DBAccts = users.get_User("");
            Leaves_List = new Dictionary<string, List<leave>>();
            if (this.DBAccts != null)
            {
                foreach (String acctID in DBAccts.Keys)
                {
                    users acct = this.DBAccts[acctID];
                    List<leave> acct_leaves = acct.Get_leaves("", this.LUSR.UserID);
                    if (Leaves_List.ContainsKey(acct.UserID))
                    {
                        Leaves_List[acct.UserID].AddRange(acct_leaves);
                    }
                    else
                    {
                        Leaves_List.Add(acct.UserID, acct_leaves);
                    }

                    for (int li = 0; li < acct_leaves.Count; li++)
                    {
                        leave acctLeave = acct_leaves[li];
                        String LeaveName = acctID +"?" + acctLeave.LeaveID + ":" + acct.Lastname + ", " + acct.Firstname + " (" + acctLeave.GetName() + ")";
                        listBox1.Items.Add(LeaveName);
                    }
                }
            }
        }

        private void disp_leaveDetails(String userID, String leaveID)
        {
            clearForm();
            if (Leaves_List.ContainsKey(userID))
            {
                //int l_index = -1;
                List<leave> _leavesU = Leaves_List[userID].Where(x => x.LeaveID == leaveID).ToList();
                if (_leavesU.Count > 0)
                {
                    leave l_instance = _leavesU[0];
                    label10.Text = this.DBAccts[userID].Lastname + " " + this.DBAccts[userID].Firstname;
                    monthCalendar1.SetDate(l_instance.FromDate);
                    numericUpDown1.Value = l_instance.DaysApplied;
                    label7.Text = l_instance.ToDate.ToString("yyyy-MM-dd");
                    if (_status.ContainsKey(l_instance.leaveStatus))
                    {
                        int ix_key = _status[l_instance.leaveStatus];
                        if (ix_key < comboBox2.Items.Count)
                        {
                            comboBox2.SelectedIndex = ix_key;
                        }
                    }

                    String sent_to = l_instance.Sent_To;
                    if (this.DBAccts.ContainsKey(sent_to))
                    {
                        int ix_acct = this.DBAccts.Keys.ToList().IndexOf(sent_to);
                        if (ix_acct < comboBox1.Items.Count)
                        {
                            comboBox1.SelectedIndex = ix_acct;
                        }
                    }
                }
            }
        }

        private Tuple<String, String> Name_Index_Extract(String _code)
        {
            String _name = "";
            String _name2 = "";
            String[] codes = _code.Trim().Split(':');
            if (codes.Length >= 1)
            {
                String[] codes_2ter = codes[0].Split('?');
                if (codes_2ter.Length >= 2)
                {
                    _name = codes_2ter[0];
                    _name2 = codes_2ter[1];
                }
                else _name = codes[0];
            }
            return (new Tuple<string, string>(_name, _name2));
        }

        private void clearForm()
        {
            label10.Text = "";
            monthCalendar1.SetDate(DateTime.Now);
            numericUpDown1.Value = 0;
            label7.Text = "";
            comboBox1.SelectedIndex = -1;
            comboBox2.SelectedIndex = -1;
            label13.Text = "";
            label13.Visible = false;
        }

        private bool apply_leave()
        {
            bool process = validateLeave();

            if (this.LUSR != null && process)
            {
                DateTime startDate = monthCalendar1.SelectionStart;
                int daysTaken = Convert.ToInt32(numericUpDown1.Value);
                int comBoix = comboBox1.SelectedIndex;
                String sentToUserID = this.LUSR.UserID;
                if (comBoix >= 0)
                {
                    String sentToCode = comboBox1.SelectedItem.ToString();
                    Tuple<String, String> decode = Name_Index_Extract(sentToCode);
                    sentToUserID = decode.Item1;
                }
                leave NewLeave = new leave(startDate, daysTaken, sentToUserID);
                this.LUSR.Add2LeaveList(NewLeave);
                process = this.LUSR.DBupdate();
            }
            return process;
        }

        private void approve_leave(String UserID, String LeaveID)
        {
            if (this.DBAccts.ContainsKey(UserID))
            {
                users modelU = this.DBAccts[UserID];
                modelU.effectLeaveState(LeaveID, leave.LEAVE_STATUS.APRV1, this.LUSR.UserID);
                modelU.DBupdate();
            }
        }

        private void reject_leave(String UserID, String LeaveID)
        {
            if (this.DBAccts.ContainsKey(UserID))
            {
                users modelU = this.DBAccts[UserID];
                modelU.effectLeaveState(LeaveID, leave.LEAVE_STATUS.REJC, this.LUSR.UserID);
                modelU.DBupdate();
            }
        }

        private void send_leave(String UserID, String LeaveID)
        {
            if (comboBox1.SelectedIndex >= 0)
            {
                Tuple<String, String> _roughSend = Name_Index_Extract(comboBox1.SelectedItem.ToString());
                String sender = _roughSend.Item1;
                if (this.DBAccts.ContainsKey(UserID))
                {
                    users modelU = this.DBAccts[UserID];
                    modelU.sendLeave(LeaveID, sender);
                    modelU.DBupdate();
                }
            }
        }

        private bool validateLeave()
        {
            bool valid = true;
            int daysAppld = Convert.ToInt32(numericUpDown1.Value);
            String error_msg = "";

            if (valid)
            {
                valid = (daysAppld > 0);
                if (!valid) error_msg = "Days applying for must be greater than 0";
            }

            if (valid)
            {
                valid = ((this.LUSR.DaysBalance - daysAppld) >= 0);
                if (!valid) error_msg = "Days applying for is beyond available balance [" + this.LUSR.DaysBalance + "]";
            }

            if (!valid)
            {
                label13.Text = error_msg;
                label13.Visible = true;
            }

            return valid;
        }
        #endregion

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int indexU = ((ListBox)sender).SelectedIndex;
            if (indexU >= 0)
            {
                clearForm();
                String _rough = listBox1.SelectedItem.ToString();
                Tuple<String, String> _rough_ext = Name_Index_Extract(_rough);
                disp_leaveDetails(_rough_ext.Item1, _rough_ext.Item2);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            clearForm();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                String _rough = listBox1.SelectedItem.ToString();
                Tuple<String, String> _rough_ext = Name_Index_Extract(_rough);
                approve_leave(_rough_ext.Item1, _rough_ext.Item2);
                clearForm();
                disp_leaveList();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                String _rough = listBox1.SelectedItem.ToString();
                Tuple<String, String> _rough_ext = Name_Index_Extract(_rough);
                reject_leave(_rough_ext.Item1, _rough_ext.Item2);
                disp_leaveList();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            bool state = apply_leave();
            if (state)
            {
                clearForm();
                disp_leaveList();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                String _rough = listBox1.SelectedItem.ToString();
                Tuple<String, String> _rough_ext = Name_Index_Extract(_rough);
                send_leave(_rough_ext.Item1, _rough_ext.Item2);
                clearForm();
                disp_leaveList();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
