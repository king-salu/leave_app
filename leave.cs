using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Viscinty_LeaveApp
{
    [Serializable]
    public class leave
    {
        public enum LEAVE_STATUS
        {
            DEFAULT = -1,
            PEND = 0,
            APRV1 = 1,
            APRV2 = 2,
            APRV3 = 3,
            REJC = 4
        }

        #region Properties
        String leaveID;
        DateTime startdate;
        DateTime enddate;
        int daysapplied;
        String sentto;
        String apprv1by;
        String apprv2by;
        String apprv3by;
        String rejctby;
        LEAVE_STATUS l_status;

        public DateTime FromDate { get { return startdate; } }
        public DateTime ToDate { get { return enddate; } }
        public int DaysApplied { get { return daysapplied; } }
        public String Sent_To { get { return sentto; } set { sentto = value; } }
        public LEAVE_STATUS leaveStatus { get { return l_status; } }
        public String LeaveID { get { return leaveID; } }
        #endregion

        #region Constructors
        public leave(DateTime strtdate, int daysused, string sentTo = "")
        {
            this.startdate = strtdate;
            this.daysapplied = daysused;
            if (sentTo.Trim() != "")
            {
                this.sentto = sentTo;
            }
            this.l_status = LEAVE_STATUS.PEND;
            //generate end date
            DateTime end_date = strtdate;
            for (int d = 1; d <= daysused; d++)
            {
                end_date = end_date.AddDays(1);
                while (end_date.DayOfWeek == DayOfWeek.Saturday || end_date.DayOfWeek == DayOfWeek.Sunday)
                {
                    end_date = end_date.AddDays(1);
                }
            }

            this.enddate = end_date;
            this.leaveID = "LV" + daysused + "-" + DateTime.Now.ToString("ss");
        }
        #endregion

        #region Methods
        public String GetName()
        {
            string gen_name = this.startdate.ToString("yyyy-MM-dd") + "->" + this.enddate.ToString("yyyy-MM-dd");
            return gen_name;
        }
        public void Approve(string approvedby)
        {
            this.rejctby = "";
            switch (this.l_status)
            {
                case LEAVE_STATUS.PEND:
                    this.l_status = LEAVE_STATUS.APRV1;
                    this.apprv1by = approvedby;
                    break;

                case LEAVE_STATUS.APRV1:
                    this.l_status = LEAVE_STATUS.APRV2;
                    this.apprv2by = approvedby;
                    break;

                case LEAVE_STATUS.APRV2:
                    this.l_status = LEAVE_STATUS.APRV3;
                    this.apprv3by = approvedby;
                    break;
            }
        }

        public void Reject(string rejectedby)
        {
            this.l_status = LEAVE_STATUS.REJC;
            this.rejctby = rejectedby;
            this.apprv1by = "";
            this.apprv2by = "";
            this.apprv3by = "";
        }

        public static String leaveStatusDesc(LEAVE_STATUS lstate)
        {
            string lstate_desc = lstate.ToString();
            switch (lstate)
            {
                case LEAVE_STATUS.APRV1:
                    lstate_desc = "1-level Approved";
                    break;

                case LEAVE_STATUS.APRV2:
                    lstate_desc = "2-level Approved";
                    break;

                case LEAVE_STATUS.APRV3:
                    lstate_desc = "Fully Approved";
                    break;

                case LEAVE_STATUS.PEND:
                    lstate_desc = "Pending";
                    break;

                case LEAVE_STATUS.REJC:
                    lstate_desc = "Rejected";
                    break;
            }

            return lstate_desc;
        }
        #endregion
    }
}
