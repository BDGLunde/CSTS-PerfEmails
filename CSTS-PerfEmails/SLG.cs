using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSTS_PerfEmails
{
    
    internal class SLG
    {
        internal static Dictionary<string, string> Status = new Dictionary<string, string>()
        {
            { "10", "New" },
            { "30", "In Progress" },
            { "35", "In Design" },
            { "40", "In Dev" },
            { "45", "RA Scheduled" },
            { "50", "Delivered" },
            { "55", "Testing" },
            { "100", "On Hold" },
            { "110", "Needs Information" },
            { "120", "Waiting to Close" },
            { "300", "Closed" }
        };

        internal static Dictionary<string, string> PriorityCodes = new Dictionary<string, string>()
        {
            { "1", "Critical" },
            { "2", "High" },
            { "3", "Medium" },
            { "4", "Low" }
        };

        internal static Dictionary<int, string> Type = new Dictionary<int, string>()
        {
            { 10, "General Help" },
            { 20, "Enhancement Suggestion" },
            { 25, "Fix Tracking" },
            { 30, "Change Order" },
            { 35, "System Build" },
            { 40, "Documentation Issue" },
            { 50, "Response Time Issue" },
            { 55, "Flag" },
            { 65, "Question" },
            { 70, "Problem Investigation" },
            { 80, "Training Issue" },
            { 85, "Project Tracking" },
            { 90, "Release Utilities Issue" },
            { 95, "Upgrade Tracking" },
            { 99, "Other/Miscellaneous" },
            { 102, "Application/Unit Testing" },
            { 103, "Integrated Testing" },
            { 135, "Reporting Problem" },
            { 160, "Crash - Hyperspace" },
            { 165, "Crash - Other" },
            { 166, "Unplanned Downtime Follow-up" },
            { 170, "Care Concern Bulletin" }
        };

        internal Dictionary<string, string> ExtendedPropValue = new Dictionary<string, string>();
        internal Dictionary<string, string> ExtendedPropText = new Dictionary<string, string>();

        #region CONSTRUCTORS

        public SLG(string sequence, string title, string custName)
        {
            this.CustomerName = custName;
            this.SequenceNumber = sequence;
            this.Title = title;
        }

        public SLG()
        { }

        #endregion

        #region COMMON

        /// <summary>
        /// This is what we typically refer to as an "SLG Number" as TS.
        /// </summary>
        public string SequenceNumber { get; set; }

        public string Title { get; set; }
        public string Priority { get; set; }
        public string CustomerName { get; set; }

        public string BatonHolder { get; set; }
        public string BatonHolderID { get; set; }

        public string EMC2Link
        {
            get
            {   
                string link = String.Format("emc2://TRACK/SLG/{0}?Action=Edit", this.SequenceNumber);
                return link;
            }
        }
        public string SherlockLink
        {
            get
            {
                string link = String.Format("http://emc2summary/GetSummaryReport.ashx/TRACK/SLG/{0}", this.SequenceNumber);
                return link;
            }
        }

        public string LogType { get; set; }
        public string LogStatus { get; set; }

        public string DateCreated { get; set; }
        public string DateOfLastPost { get; set; }

        public DateTime LastPostDate
        {
            get
            {
                string dateString = DateOfLastPost;
                DateTime lastPost;
                if (dateString == "No public posts yet")
                {
                    lastPost = new DateTime(1, 1, 1);
                }
                else
                {
                    DateTime.TryParse(dateString, out lastPost);
                }

                return lastPost;
            }
        }

        #endregion


    }
}
