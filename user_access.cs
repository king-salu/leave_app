using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Viscinty_LeaveApp
{
    public partial class user_access : Form
    {
        public user_access()
        {
            InitializeComponent();
        }


        #region Methods
        private bool registerUser()
        {
            Boolean state = validateRegUser();
            if (state)
            {
                state = false;
                String Firstname = textBox3.Text;
                String Lastname = textBox4.Text;
                int daysEntld = Convert.ToInt32(numericUpDown1.Value);
                String Password = textBox6.Text;

                users RegUser = new users(Firstname, Lastname, Password, daysEntld);
                try
                {
                    String RegPath = users.getFolder();
                    RegPath += "\\" + RegUser.UserID + users.fExt;
                    Stream strm = File.Open(RegPath, FileMode.Create, FileAccess.Write, FileShare.None);
                    BinaryFormatter bformat = new BinaryFormatter();
                    bformat.Serialize(strm, RegUser);
                    strm.Close();
                    textBox5.Text = RegUser.UserID;
                    state = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            
            return state;
        }

        private bool validateRegUser()
        {
            bool valid = true;
            String f_name = textBox3.Text;
            String l_name = textBox4.Text;
            String pass1 = textBox6.Text;
            String pass2 = textBox7.Text;
            String error_msg = "";

            if (valid)
            {
                valid = (f_name.Trim() != "");
                if (!valid) error_msg = "First Name can't be empty!";
            }

            if (valid)
            {
                valid = (l_name.Trim() != "");
                if (!valid) error_msg = "Last Name can't be empty!";
            }

            if (valid)
            {
                valid = ((pass1.Trim() != "") && (pass1.Length>8));
                if (!valid) error_msg = "Password Must have a minimum of 8 characters";
            }

            if (valid)
            {
                valid = (pass1 == pass2);
                if (!valid) error_msg = "Password Mismatch";
            }

            if (!valid)
            {
                label13.Text = error_msg;
                label13.Visible = true;
                label13.ForeColor = Color.Maroon;
            }

            return valid;
        }

        private Tuple<bool,users> LoginUser()
        {
            bool state = false;
            String username = textBox1.Text;
            String password = textBox2.Text;
            users uAcct = null;
            Dictionary<String, users> Accounts = users.get_User(username);
            if (Accounts.Count > 0)
            {
                uAcct = Accounts[username];
                state = uAcct.validatePassword(password);
            }

            return (new Tuple<bool,users>(state,uAcct));
        }

        private void clearForm(String _type)
        {
            switch (_type.ToLower())
            {
                case "register":
                    textBox3.Text = "";
                    textBox4.Text = "";
                    textBox5.Text = "";
                    numericUpDown1.Value = 0;
                    textBox6.Text = "";
                    textBox7.Text = "";
                    label13.Visible=false;
                    break;

                case "register_success":
                    textBox6.Text = "";
                    textBox7.Text = "";
                    label13.Text = "User Account Registered Successfully!";
                    label13.Visible = true;
                    label13.ForeColor = Color.Green;
                    break;

                case "login":
                    textBox1.Text = "";
                    textBox2.Text = "";
                    label5.Visible = false;
                    break;
            }
        }
        #endregion

        private void button2_Click(object sender, EventArgs e)
        {
            clearForm("register");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            bool status = registerUser();
            if (status)
            {
                clearForm("register_success");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Tuple<bool,users> logStates = LoginUser();
            bool status = logStates.Item1;
            if (status)
            {
                clearForm("login");
                this.Hide();
                Leave_Maintenance lmaint = new Leave_Maintenance();
                lmaint.form_setup(logStates.Item2);
                lmaint.FormClosing += lmaint_FormClosing;
                lmaint.Show();
                
            }
            else
            {
                label5.Visible = true;
            }
        }

        private void lmaint_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Show();
        }
    }
}
