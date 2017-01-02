using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Epic.ClientSystems.Utilities.HyperspaceMonitorSharedSettings;

namespace CSTS_PerfEmails
{
    internal class CSTS
    {
        public int OpenCRTEs { get; set; }
        public string TlgID { get; set; }
        public string Name { get; set; }
        public string TLID { get; set; }
        public string EmailAddress { get; set; }
        public string sAMAccountName
        {
            get
            { 
                return EmailAddress.Substring(0, EmailAddress.IndexOf('@'));
            }
        }

        public List<SLG> SLGs = new List<SLG>();
        public List<rteSLG> RTEs = new List<rteSLG>();
        public List<crteSLG> CRTEs = new List<crteSLG>();

        public CSTS(string tlgid, string name, string tlID, string emailaddr)
        {
            this.TlgID = tlgid;
            this.Name = name;
            this.TLID = tlID;
            this.EmailAddress = emailaddr;
        }
    }
}
