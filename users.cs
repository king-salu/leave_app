using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Viscinty_LeaveApp
{
    [Serializable]
    public class users : ISerializable
    {
        #region Properties
        String firstname;
        String lastname;
        String userid;
        String password;
        int daysentitled;
        int dayBalance;
        private List<leave> _leaves = null;

        public String Firstname { get { return firstname; } set { firstname = value; } }
        public String Lastname { get { return lastname; } set { lastname = value; } }
        public String UserID { get { return userid; } }
        public int DaysBalance { get { return dayBalance; } }
        public static String fExt { get { return ".usx"; } }
        #endregion

        #region Constructors
        public users(string fname, string lname, string passwrd, int dayentitle)
        {
            this.firstname = fname;
            this.lastname = lname;
            this.password = passwrd;
            this.daysentitled = dayentitle;
            this.dayBalance = dayentitle;
            this.userid = generateUserID();
            this._leaves = new List<leave>();
        }

        protected users(SerializationInfo info, StreamingContext ctxt)
        {
            this._leaves = (List<leave>)info.GetValue("lapplied", typeof(List<leave>));
            this.firstname = Convert.ToString(info.GetValue("uFirst", typeof(String)));
            this.lastname = Convert.ToString(info.GetValue("uLast", typeof(String)));
            this.password = Convert.ToString(info.GetValue("uStain", typeof(String)));
            this.daysentitled = Convert.ToInt32(info.GetValue("uDays", typeof(int)));
            this.dayBalance = Convert.ToInt32(info.GetValue("uDaysBal", typeof(int)));
            this.userid = Convert.ToString(info.GetValue("uID", typeof(String)));
        }
        #endregion

        #region Methods
        public virtual void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("lapplied", this._leaves);
            info.AddValue("uFirst", this.firstname);
            info.AddValue("uLast", this.lastname);
            info.AddValue("uStain", this.password);
            info.AddValue("uDays", this.daysentitled);
            info.AddValue("uDaysBal", this.dayBalance);
            info.AddValue("uID", this.userid);
        }

        public static String getFolder()
        {
            string setPath = "C:\\Users\\Public\\Documents\\DBSJAS_VISCLEAVE\\accounts";
            if (!System.IO.Directory.Exists(setPath)) { System.IO.Directory.CreateDirectory(setPath); }

            return setPath;
        }

        private String generateUserID()
        {
            string temp_userID = "U";
            int npick = 0;
            while (npick < 2)
            {
                int nameCnt = 0;
                string nameUsed = "";
                switch (npick)
                {
                    case 0:
                        nameUsed = this.lastname;
                        break;
                    case 1:
                        nameUsed = this.Firstname;
                        break;
                }

                nameCnt = nameUsed.Length;
                if (nameCnt > 0)
                {
                    int setLength = (nameCnt <= 3) ? nameCnt : 3;
                    temp_userID += "_" + nameUsed.Substring(0, setLength).ToUpper();
                }
                npick++;
            }

            temp_userID += "-" + DateTime.Now.ToString("tt-yyyy-ss");
            return temp_userID;
        }

        public static Dictionary<String,users> get_User(String user_id = "")
        {
            user_id = user_id.Trim();
            Dictionary<String, users> Users = new Dictionary<String, users>();
            String getPath = getFolder();
            String Pattern = "*" + fExt;
            String[] db_uLists = Directory.GetFiles(getPath, Pattern, SearchOption.AllDirectories);
            if (db_uLists.Length > 0)
            {
                bool found = false;
                foreach (String uPath in db_uLists)
                {
                    try
                    {
                        BinaryFormatter bformat = new BinaryFormatter();
                        Stream strm = File.Open(uPath, FileMode.Open);
                        Object Raw = bformat.Deserialize(strm);
                        strm.Close();
                        users dbacct = (users)Raw;
                        if (user_id.Trim() != "")
                        {
                            if (user_id == dbacct.UserID)
                            {
                                found = true;
                            }
                        }

                        if (found || user_id.Trim() == "")
                        {
                            if (Users.ContainsKey(user_id))
                            {
                                Users[user_id] = dbacct;
                            }
                            else Users.Add(dbacct.UserID, dbacct);
                            if (found) break;
                        }
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }
            }

            return Users;
        }

        public bool validatePassword(String _password)
        {
            Boolean valid = (this.password == _password);
            return valid;
        }

        public bool DBupdate()
        {
            bool status = false;
            try
            {
                String RegPath = users.getFolder();
                RegPath += "\\" + this.UserID + users.fExt;
                Stream strm = File.Open(RegPath, FileMode.Create, FileAccess.Write, FileShare.None);
                BinaryFormatter bformat = new BinaryFormatter();
                bformat.Serialize(strm, this);
                strm.Close();
                status = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return status;
        }

        public List<leave> Get_leaves(String leave_ID = "",String sentto = "", leave.LEAVE_STATUS lstate = leave.LEAVE_STATUS.DEFAULT)
        {
            List<leave> llist = this._leaves;
            if (llist != null)
            {
                if (leave_ID != "")
                {
                    llist = llist.Where(x => x.LeaveID == leave_ID).ToList();
                }

                if (sentto.Trim() != "")
                {
                    llist = llist.Where(x => x.Sent_To == sentto).ToList();
                }

                if (lstate != leave.LEAVE_STATUS.DEFAULT)
                {
                    llist = llist.Where(x => x.leaveStatus == lstate).ToList();
                }
                
            }
            return llist;
        }

        public void Add2LeaveList(leave NewLeave)
        {
            if (NewLeave != null)
            {
                int temp_balance = this.dayBalance - NewLeave.DaysApplied;
                if (temp_balance >= 0)
                {
                    this._leaves.Add(NewLeave);
                    this.dayBalance -= NewLeave.DaysApplied;
                }
            }
        }

        public bool effectLeaveState(String leaveID, leave.LEAVE_STATUS lstate, String actionID)
        {
            bool status = false;
            int lindex = findLeaveIndex(leaveID);
            if (lindex >= 0)
            {
                switch (lstate)
                {
                    case leave.LEAVE_STATUS.APRV1:
                        this._leaves[lindex].Approve(actionID);
                        break;

                    case leave.LEAVE_STATUS.REJC:
                        this._leaves[lindex].Reject(actionID);
                        break;
                }
                status = true;
            }

            return status;
        }

        public bool sendLeave(String leaveID, String sendUID)
        {
            bool status = false;
            int lindex = findLeaveIndex(leaveID);
            if (lindex >= 0)
            {
                this._leaves[lindex].Sent_To = sendUID;
                status = true;
            }

            return status;
        }

        private int findLeaveIndex(String LeaveID)
        {
            int lindex = -1;
            for (int i = 0; i < this._leaves.Count; i++)
            {
                if (this._leaves[i].LeaveID == LeaveID)
                {
                    lindex = i;
                    break;
                }
            }

            return lindex;
        }


        #endregion
    }
}
