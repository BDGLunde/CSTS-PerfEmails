using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSTS_PerfEmails
{
    class crteSLG : SLG
    {
        public string NextStep
        {
            get
            {
                string result = null;
                ExtendedPropText.TryGetValue("108976", out result);
                return result;
            }
        }

        public string NextStepOwner
        {
            get
            {
                string result = null;
                ExtendedPropText.TryGetValue("123520", out result);
                return result;
            }
        }

        public string NextStepOwnerID
        {
            get
            {
                string result = null;
                ExtendedPropValue.TryGetValue("123520", out result);
                return result;
            }
        }

        public DateTime NextStepDate
        {
            get
            {
                DateTime stepDate;
                string dateString = null;
                ExtendedPropText.TryGetValue("108977", out dateString);

                DateTime.TryParse(dateString, out stepDate);

                return stepDate;
            }
        }

        public string CRTEStatus
        {
            get
            {
                string result = null;
                ExtendedPropValue.TryGetValue("105885", out result);
                return result;
            }
        }

        public string CSTSOwner
        {
            get
            {
                string result = null;
                ExtendedPropText.TryGetValue("10512", out result);
                return result;
            }
        }

        public string PerfFedOwner
        {
            get
            {
                string result = null;
                ExtendedPropText.TryGetValue("115326", out result);
                return result;
            }
        }

        public string PerfFedOwnerID
        {
            get
            {
                string result = null;
                ExtendedPropValue.TryGetValue("115326", out result);
                return result;
            }
        }

        public string PerfTSID
        {
            get
            {
                string result = null;
                ExtendedPropValue.TryGetValue("105886", out result);
                return result;
            }
        }

        public string PerfTSName
        {
            get
            {
                string result = null;
                ExtendedPropText.TryGetValue("105886", out result);
                return result;
            }
        }
    }
}
