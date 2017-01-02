using System;
using System.IO;
using Microsoft.Office.Interop.Outlook;
using OutlookApp = Microsoft.Office.Interop.Outlook.Application;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;


namespace CSTS_PerfEmails
{
    class Program
    {
        static Dictionary<String, CSTS> csDict = GenerateCSTSDict();
        static DateTime timeGenerated = DateTime.Now;
        static string datePath = String.Format("{0}-{1}-{2}", timeGenerated.Month, timeGenerated.Day, timeGenerated.Year);
        static string basePath = String.Format(@"\\epic.com\files\docs\SYSTEMS\PC TS\Performance Excellence\Performance Excellence Federation\GeneratedEmails");

        static string weekPath = String.Format(@"{0}\{1}", basePath, datePath);
        static string weekPathCRTEs = String.Format(@"{0}\{1}", weekPath, "CRTEs");
        

        

        static void Main(string[] args)
        {
            //Console.Title = "CSTS - Response Time SLG utility";
            //Console.BufferWidth = 5000;
            //Console.WindowWidth = 150;

            if (!Directory.Exists(weekPath))
            {
                Directory.CreateDirectory(weekPath);
                Directory.CreateDirectory(weekPathCRTEs);
            }
            TestEmail();
            //TestEmailSingle();
            Console.ReadKey();
        }

        #region TEST METHODS

        static void TestDateTime()
        {
            Console.WriteLine(DateTime.Now);
            List<DateTime> dateList = new List<DateTime>();
            DateTime time1;
            DateTime time2;
            DateTime.TryParse("10/22/2015 1:53:00 PM", out time1);
            DateTime.TryParse("10/23/2015 7:35:00 PM", out time2);

            dateList.Add(time2);
            dateList.Add(time1);
            dateList.Sort();
            Console.WriteLine(new DateTime(1,1,1));

            foreach (DateTime date in dateList)
            {
                Console.WriteLine(date);
            }
            //Console.WriteLine((DateTime.Now - time).Days);
        }

        static void TestCSTSDict()
        {
            Dictionary<String, CSTS> csDict = GenerateCSTSDict();
            foreach (CSTS cs in csDict.Values)
            {
                Console.WriteLine("({0})-({1})-({2})-({3})", cs.TlgID, cs.Name, cs.TLID, cs.sAMAccountName);
            }
        }

        static void TestEmail()
        {
            SqlConnection trackPrego = new SqlConnection("Server=prego.epic.com; Database=Track; Trusted_Connection=True;");
            foreach (CSTS csts in csDict.Values)
            {
                FindRTEs(csts, trackPrego);
                FindCRTEs(csts, trackPrego);
                csts.RTEs.Sort(delegate(rteSLG slg1, rteSLG slg2) { return slg1.LastPostDate.CompareTo(slg2.LastPostDate); });
                csts.CRTEs.Sort(delegate(crteSLG slg1, crteSLG slg2) { return slg1.NextStepDate.CompareTo(slg2.NextStepDate); });
                SendEmail(csts);
            }
            trackPrego.Close();
            //FindPerfSLGs(edward, new SqlConnection("Server=prego.epic.com; Database=Track; Trusted_Connection=True;"));
            //SendEmail(edward);
        }

        static void TestEmailSingle()
        {
            Dictionary<String, CSTS> csDict = GenerateCSTSDict();
            SqlConnection trackPrego = new SqlConnection("Server=prego.epic.com; Database=Track; Trusted_Connection=True;");
            CSTS bashley = csDict["18199"];
            FindRTEs(bashley, trackPrego);
            FindCRTEs(bashley, trackPrego);
            trackPrego.Close();
            bashley.RTEs.Sort(delegate(rteSLG slg1, rteSLG slg2) { return slg1.LastPostDate.CompareTo(slg2.LastPostDate); });
            bashley.CRTEs.Sort(delegate(crteSLG slg1, crteSLG slg2) { return slg1.NextStepDate.CompareTo(slg2.NextStepDate); });
            SendEmail(bashley);
        }

        #endregion

        #region QUERY METHODS

        /* You'll notice that some of these methods create a connection within the method, and others simply take an open
         * connection as a parameter. I wasn't sure which technique is considered best practice, and I'm sure both are 
         * the optimal strategy for different purposes/goals.
         */
            
        /// <summary>
        /// Generates a dictionary of <CSTS.sAMAccountName, CSTS> entries
        /// </summary>
        /// <returns>Dictionary<String,CSTS></returns>
        static Dictionary<String, CSTS> GenerateCSTSDict()
        {
            Dictionary<String, CSTS> cstsDict = new Dictionary<String, CSTS>();
            using (SqlConnection connection = new SqlConnection("Server=prego.epic.com; Database=Track; Trusted_Connection=True;"))
            {
                using (StreamReader file = new StreamReader("returnCSTS.sql"))
                {
                    SqlCommand cmd = new SqlCommand(@file.ReadToEnd(), connection);
                    connection.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            CSTS tempCSTS = new CSTS(reader[0].ToString(), reader[1].ToString(), reader[2].ToString(), reader[3].ToString());
                            cstsDict.Add(tempCSTS.TlgID, tempCSTS);
                        }
                    }
                }
            }

            return cstsDict;

        }

        /// <summary>
        /// Requires a connection to be present first.
        /// </summary>
        /// <param name="csts"></param>
        static void FindRTEs(CSTS csts, SqlConnection conn)
        {
            using (StreamReader file = new StreamReader("returnPerfSLGs.sql"))
            {
                SqlCommand cmd = new SqlCommand(@file.ReadToEnd(), conn);
                cmd.Parameters.AddWithValue("tlgID", csts.TlgID);

                if (conn.State == System.Data.ConnectionState.Closed)
                {
                    conn.Open();
                }
                
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rteSLG newEntry = new rteSLG();
                        newEntry.CustomerName = reader["Customer"].ToString();
                        newEntry.SequenceNumber = reader["SLG"].ToString();
                        newEntry.Title = reader["Title"].ToString();
                        newEntry.LogStatus = SLG.Status[reader["Status"].ToString()];
                        newEntry.DateOfLastPost = reader["LastUpdate"].ToString();
                        newEntry.Priority = reader["PriorityCode"].ToString();
                        newEntry.PerfTSID = reader["PerfID"].ToString();
                        newEntry.PerfTSName = reader["PerfTSName"].ToString();
                        newEntry.BatonHolderID = csts.TlgID;

                        csts.SLGs.Add(newEntry);
                        csts.RTEs.Add(newEntry);

                        Console.WriteLine("SLG {0} added", newEntry.SequenceNumber);                       
                    }
                }
            }
            Console.WriteLine("Writing for {0} finished.", csts.sAMAccountName);
            //conn.Close(); Better to close or keep connection going? Not sure which technique is better.
        }

        /// <summary>
        /// Requires a connection to be present first
        /// </summary>
        /// <param name="csts"></param>
        /// <param name="conn"></param>
        static void FindCRTEs(CSTS csts, SqlConnection conn)
        {
            using (StreamReader file = new StreamReader("returnCRTEs.sql"))
            {
                SqlCommand cmd = new SqlCommand(@file.ReadToEnd(), conn);
                cmd.Parameters.AddWithValue("tlgID", csts.TlgID);

                if (conn.State == System.Data.ConnectionState.Closed)
                {
                    conn.Open();
                }
                
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    string previousSLGNum = "";
                    crteSLG newEntry = new crteSLG();
                    while (reader.Read())
                    {
                        if (!previousSLGNum.Equals(reader[0].ToString())) //executes if this is a new SLG/contact
                        {
                            //if (newEntry != null && newEntry.CRTEStatus != "60") //Don't add CRTEs that are marked as Issue Resolved
                            //{
                            //    csts.SLGs.Add(newEntry);
                            //    csts.CRTEs.Add(newEntry);

                            //    if (newEntry.CRTEStatus == "30" || newEntry.CRTEStatus == "20")
                            //    {
                            //        csts.OpenCRTEs++;
                            //    }
                            //}

                            newEntry = new crteSLG();
                            csts.SLGs.Add(newEntry);
                            csts.CRTEs.Add(newEntry);
                            

                            newEntry.SequenceNumber = reader[0].ToString();
                            newEntry.CustomerName = reader[1].ToString();
                            newEntry.Title = reader[2].ToString();

                            previousSLGNum = newEntry.SequenceNumber;
                        }

                        newEntry.ExtendedPropValue.Add(reader[3].ToString(), reader[4].ToString()); //Kinda confusing, but sometimes Value is human readable enough
                        newEntry.ExtendedPropText.Add(reader[3].ToString(), reader[5].ToString()); //And sometimes Text is way way more human readable
                    }

                    if (newEntry.CRTEStatus == "30" || newEntry.CRTEStatus == "20")
                    {
                        csts.OpenCRTEs++;
                    }
                }
            }
            Console.WriteLine("Writing for {0} finished.", csts.sAMAccountName);
            //conn.Close();
        }


        #endregion

        #region EMAIL UTIL

        static void SendEmail(CSTS csts)
        {
            if (shouldEmail(csts))
            {
                OutlookApp myApp = new OutlookApp();
                MailItem mailItem = myApp.CreateItem(OlItemType.olMailItem);

                string pathName = String.Format(@"{0}\{1}.msg", weekPath, csts.sAMAccountName);
                StringBuilder HTMLBuilder = new StringBuilder();
                
                string crteHeader = @"<h2 style=""color:red"">Open CRTEs - <i style=""color:gray"">Ensure next steps are completed on time or escalate. Badger Next Step Owners as necessary.</i></h2>";
                string rteHeader = @"<h3 style=""color:orange"">Open RTT Concerns - <i style=""color:gray"">Investigate the source of RTT changes and loop in Perf TS or others as needed</i></h3>";
                string notEscalatedHeader = @"<h3 style=""color:black"">FYI -- De-escalated issues with follow-ups - <i style=""color:gray"">Ensure continued progress</i></h3>";

                mailItem.Subject = String.Format("Open Performance SLGs for {0}", csts.Name);
                mailItem.Recipients.Add(csts.EmailAddress);

                try
                {
                    mailItem.CC = csDict[csts.TLID].EmailAddress;
                }
                catch (KeyNotFoundException ex)
                {
                    mailItem.CC = null;
                }
                
                mailItem.Importance = OlImportance.olImportanceNormal;

                //HTMLBuilder.AppendFormat("<html><head><style>{0}</style></head><body>{1}", loadStyles(), generateTopTemplate());

                HTMLBuilder.AppendFormat("<html><head><style>{0}</style></head><body>", loadStyles());
                if (csts.OpenCRTEs > 0)
                {
                    HTMLBuilder.AppendFormat("{0}{1}", crteHeader, generateCRTETable(csts, true));

                    if (csts.RTEs.Count > 0)
                    {
                        HTMLBuilder.AppendFormat("<br><br>{0}{1}", rteHeader, generateRTETable(csts));
                    }

                    if (csts.CRTEs.Count - csts.OpenCRTEs > 0)
                    {
                        HTMLBuilder.AppendFormat("<br><br>{0}{1}", notEscalatedHeader, generateCRTETable(csts, false));
                    }
                }
                else if (csts.RTEs.Count > 0)
                {
                    HTMLBuilder.AppendFormat("No open CRTEs");
                    HTMLBuilder.AppendFormat("{0}{1}", rteHeader, generateRTETable(csts));

                    if (csts.CRTEs.Count - csts.OpenCRTEs > 0)
                    {
                        HTMLBuilder.AppendFormat("<br><br>{0}{1}", notEscalatedHeader, generateCRTETable(csts, false));
                    }
                }
                else if (csts.CRTEs.Count - csts.OpenCRTEs > 0)
                {
                    HTMLBuilder.AppendFormat("{0}{1}", notEscalatedHeader, generateCRTETable(csts, false));
                }

                HTMLBuilder.AppendFormat(@"<br><br>{0}</body></html>", generateBottomTemplate());

                mailItem.HTMLBody = HTMLBuilder.ToString();

                if (csts.CRTEs.Count > 0)
                {
                    //Console.WriteLine(String.Format(@"{0}\{1}.msg", weekPath + @"\CRTEs", csts.sAMAccountName));
                    //Console.ReadKey();
                    mailItem.SaveAs(String.Format(@"{0}\{1}.msg", weekPath + @"\CRTEs", csts.sAMAccountName));
                }
                else
                {
                    mailItem.SaveAs(pathName);
                     
                }
                
                //mailItem.Send(); 
            }
        }

        static Boolean shouldEmail(CSTS csts)
        {
            return csts.SLGs.Count > 0;
        }

        #endregion

        #region HTML/CSS - these are all kinda crappy - should revisit and rewrite these eventually
        
        static string generateTopTemplate()
        {
            StringWriter sw = new StringWriter();

            using (HtmlTextWriter htmltw = new HtmlTextWriter(sw)) //Wow I am being dumb-lazy here, should re-write this eventually.
            {
                htmltw.RenderBeginTag(HtmlTextWriterTag.P); //Begin <p>
                htmltw.Write("Hello,");
                htmltw.RenderBeginTag(HtmlTextWriterTag.Br);
                htmltw.Write("This is your first instance of a recurring e-mail to keep you apprised of open performance issues assigned to you.");

                htmltw.RenderBeginTag(HtmlTextWriterTag.Br);
                htmltw.RenderBeginTag(HtmlTextWriterTag.Br);

                htmltw.Write("The purpose is to provide you with a consolidated list of open CRTEs, ");
                htmltw.Write("RTT concerns and de-escalated issues so you can see all of them in one place, track what's coming due soon and prioritize your ");
                htmltw.Write("time accordingly. For more detail on this e-mail report, please see the meeting notes from the ");

                htmltw.AddAttribute(HtmlTextWriterAttribute.Href, @"onenote://F:/SYSTEMS/PC%20TS/%5eHyperspace%20&%20Desktop%20Group/Hyperspace%20and%20Desktop%20Group/Weekly%20Meetings.one#12/1/2015&section-id={F6D1B37D-2819-482A-92B2-BB29267CC6E2}&page-id={BAB5016E-B4AB-4F90-A7AB-0B88EBA8DCD2}&end");
                htmltw.RenderBeginTag(HtmlTextWriterTag.A);
                htmltw.Write("H&D Meeting on 12/1");
                htmltw.RenderEndTag(); //end <a>

                htmltw.Write(" or the ");

                htmltw.AddAttribute(HtmlTextWriterAttribute.Href, @"onenote:///F:\SYSTEMS\PC%20TS\%5eWeb%20&%20Service%20Servers%20Group\Client%20Systems%20-%20Web%20&%20Service%20Servers%20group\Weekly%20Meetings.one#12/8/2015&section-id={981D7CA8-8725-413F-A6E8-D017A3816C83}&page-id={BB76AAF3-0F00-40C8-B58C-E12767BED0C6}&end");
                htmltw.RenderBeginTag(HtmlTextWriterTag.A);
                htmltw.Write("W&SS Meeting on 12/8");
                htmltw.RenderEndTag(); //end <a>

                htmltw.RenderBeginTag(HtmlTextWriterTag.Br);
                htmltw.RenderBeginTag(HtmlTextWriterTag.Br);

                htmltw.Write("If you have any questions or feedback on how this report can be as useful as possible, please e-mail the ");

                htmltw.AddAttribute(HtmlTextWriterAttribute.Href, @"mailto:Client%20Systems%20-%20Performance%20Excellence?subject=Weekly%20CRTE%20Email%20feedback");
                htmltw.RenderBeginTag(HtmlTextWriterTag.A);
                htmltw.Write("Client Systems - Performance Excellence");
                htmltw.RenderEndTag(); //end <a>

                htmltw.Write(" group.");

                htmltw.RenderBeginTag(HtmlTextWriterTag.Br);
                htmltw.RenderBeginTag(HtmlTextWriterTag.Br);

                htmltw.RenderEndTag(); //end <p>     
            }

            return sw.ToString();
        }

        static string generateBottomTemplate()
        {
            StringWriter sw = new StringWriter();

            using (HtmlTextWriter htmltw = new HtmlTextWriter(sw))
            {
                htmltw.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "16pt");
                htmltw.RenderBeginTag(HtmlTextWriterTag.P); //begin <p>
                htmltw.RenderBeginTag(HtmlTextWriterTag.U); //underline
                htmltw.RenderBeginTag(HtmlTextWriterTag.B); //bold

                htmltw.Write("Resources:");

                htmltw.RenderEndTag(); //end <b>
                htmltw.RenderEndTag(); //end <u>
                htmltw.RenderEndTag(); //end <p>

                htmltw.AddAttribute(HtmlTextWriterAttribute.Href, @"http://wiki.epic.com/main/Client_Systems/Performance_for_CSTS");
                htmltw.RenderBeginTag(HtmlTextWriterTag.A);
                htmltw.Write("Client Systems - Performance Excellence Wiki");
                htmltw.RenderEndTag(); //end <a>
                htmltw.RenderBeginTag(HtmlTextWriterTag.Br);

                htmltw.AddAttribute(HtmlTextWriterAttribute.Href, @"http://wiki.epic.com/main/Performance");
                htmltw.RenderBeginTag(HtmlTextWriterTag.A);
                htmltw.Write("Performance Wiki");
                htmltw.RenderEndTag(); //end <a>
                htmltw.RenderBeginTag(HtmlTextWriterTag.Br);

                htmltw.AddAttribute(HtmlTextWriterAttribute.Href, @"http://wiki.epic.com/main/Performance/Customer_Response_Time_Escalation_(CRTE)");
                htmltw.RenderBeginTag(HtmlTextWriterTag.A);
                htmltw.Write("CRTE Process Wiki");
                htmltw.RenderEndTag(); //end <a>
                htmltw.RenderBeginTag(HtmlTextWriterTag.Br);
            }

            return sw.ToString();
        }


        static string generateRTETable(CSTS csts)
        {
            StringWriter sw = new StringWriter();

            using (HtmlTextWriter htmltw = new HtmlTextWriter(sw))
            {
                htmltw.RenderBeginTag(HtmlTextWriterTag.Table);

                htmltw.RenderBeginTag(HtmlTextWriterTag.Tr);

                generateTableColumn(htmltw, "SLG", header: true);
                generateTableColumn(htmltw, "Customer", header: true);
                generateTableColumn(htmltw, "Description", header: true);
                //generateTableColumn(htmltw, "Log Status", header: true);
                generateTableColumn(htmltw, "Priority", header: true);
                generateTableColumn(htmltw, "Date of Last Public Post", header: true);
                generateTableColumn(htmltw, "Performance TS", header: true);

                htmltw.RenderEndTag(); //Ends <Row>

                int numRow = 0;
                foreach (rteSLG slg in csts.RTEs)
                {
                    if (++numRow % 2 == 0)
                    {
                        htmltw.AddAttribute(HtmlTextWriterAttribute.Class, "alt");
                    }
                    htmltw.RenderBeginTag(HtmlTextWriterTag.Tr); //Begins <Row>

                    generateTableColumn(htmltw, String.Format(@"<a href=""{0}"">{1}</a><br><a href=""{2}"">EMC2</a>", slg.SherlockLink, slg.SequenceNumber, slg.EMC2Link));
                    generateTableColumn(htmltw, slg.CustomerName);
                    generateTableColumn(htmltw, slg.Title);
                    //generateTableColumn(htmltw, slg.LogStatus);
                    generateTableColumn(htmltw, SLG.PriorityCodes[slg.Priority]);
                    generateTableColumn(htmltw, slg.DateOfLastPost);
                    generateTableColumn(htmltw, String.Format(@"<a href=""guru/Staff/EmployeeProfile.aspx?id={0}"">{1}</a>", slg.PerfTSID, slg.PerfTSName));

                    htmltw.RenderEndTag(); //Ends <Row> 
                }

                htmltw.RenderEndTag(); //Ends <Table>
            }
            return sw.ToString();
        }

        static string generateCRTETable(CSTS csts, bool getEscalated)
        {
            StringWriter sw = new StringWriter();
            int numSLGs = 0;
           
            using (HtmlTextWriter htmltw = new HtmlTextWriter(sw))
            {
                htmltw.RenderBeginTag(HtmlTextWriterTag.Table);

                htmltw.RenderBeginTag(HtmlTextWriterTag.Tr); //Begins <Row>

                generateTableColumn(htmltw, "SLG", header: true);
                generateTableColumn(htmltw, "Customer", header: true);
                generateTableColumn(htmltw, "Description", header: true);
                generateTableColumn(htmltw, "Next Step", header: true);
                generateTableColumn(htmltw, "Next Step Owner", header: true);
                generateTableColumn(htmltw, "Next Step Date", header: true);
                generateTableColumn(htmltw, "Performance Federation Owner", header: true);
                generateTableColumn(htmltw, "Performance TS", header: true);
                generateTableColumn(htmltw, "Status", header: true);
                //generateTableColumn(htmltw, "Creation date", header: true);
                //generateTableColumn(htmltw, "Date of Last Post", header: true);

                htmltw.RenderEndTag(); //Ends <Row>

                int numRow = 0;
                foreach (crteSLG slg in csts.CRTEs)
                {
                    if (getEscalated)
                    {
                        if (slg.CRTEStatus == "30" || slg.CRTEStatus == "20")
                        {
                            if (++numRow % 2 == 0)
                            {
                                htmltw.AddAttribute(HtmlTextWriterAttribute.Class, "alt");
                            }
                            htmltw.RenderBeginTag(HtmlTextWriterTag.Tr); //Begins <Row>

                            generateTableColumn(htmltw, String.Format(@"<a href=""{0}"">{1}</a><br><a href=""{2}"">EMC2</a>", slg.SherlockLink, slg.SequenceNumber, slg.EMC2Link));
                            generateTableColumn(htmltw, slg.CustomerName);
                            generateTableColumn(htmltw, slg.Title);
                            generateTableColumn(htmltw, slg.NextStep);
                            generateTableColumn(htmltw, String.Format(@"<a href=""guru/Staff/EmployeeProfile.aspx?id={0}"">{1}</a>", slg.NextStepOwnerID, slg.NextStepOwner));
                            //htmltw.AddAttribute(HtmlTextWriterAttribute.Class, "highlighted");
                            generateTableColumn(htmltw, DateTime.Now > slg.NextStepDate ? String.Format(@"<span class=""highlighted"">{0}</span>",slg.NextStepDate.ToShortDateString()) : slg.NextStepDate.ToShortDateString() );
                            generateTableColumn(htmltw, String.Format(@"<a href=""guru/Staff/EmployeeProfile.aspx?id={0}"">{1}</a>", slg.PerfFedOwnerID, slg.PerfFedOwner));
                            generateTableColumn(htmltw, String.Format(@"<a href=""guru/Staff/EmployeeProfile.aspx?id={0}"">{1}</a>", slg.PerfTSID, slg.PerfTSName));
                            generateTableColumn(htmltw, slg.CRTEStatus);
                            //generateTableColumn(htmltw, slg.DateCreated);
                            //generateTableColumn(htmltw, slg.DateOfLastPost);

                            htmltw.RenderEndTag(); //Ends <Row> 

                            numSLGs++;
                        }  
                    }
                    else
                    {
                        if (slg.CRTEStatus != "30" && slg.CRTEStatus != "20")// && slg.CRTEStatus != "60" && slg.CRTEStatus != "50") //60 == Issue Resolved - don't add it. 50 == Res meeting complete
                        {
                            if (++numRow % 2 == 0)
                            {
                                htmltw.AddAttribute(HtmlTextWriterAttribute.Class, "alt");
                            }
                            htmltw.RenderBeginTag(HtmlTextWriterTag.Tr); //Begins <Row>

                            generateTableColumn(htmltw, String.Format(@"<a href=""{0}"">{1}</a><br><a href=""{2}"">EMC2</a>", slg.SherlockLink, slg.SequenceNumber, slg.EMC2Link));
                            generateTableColumn(htmltw, slg.CustomerName);
                            generateTableColumn(htmltw, slg.Title);
                            generateTableColumn(htmltw, slg.NextStep);
                            generateTableColumn(htmltw, String.Format(@"<a href=""guru/Staff/EmployeeProfile.aspx?id={0}"">{1}</a>", slg.NextStepOwnerID, slg.NextStepOwner));
                            generateTableColumn(htmltw, DateTime.Now > slg.NextStepDate ? String.Format(@"<span class=""highlighted"">{0}</span>", slg.NextStepDate.ToShortDateString()) : slg.NextStepDate.ToShortDateString());
                            generateTableColumn(htmltw, String.Format(@"<a href=""guru/Staff/EmployeeProfile.aspx?id={0}"">{1}</a>", slg.PerfFedOwnerID, slg.PerfFedOwner));
                            generateTableColumn(htmltw, String.Format(@"<a href=""guru/Staff/EmployeeProfile.aspx?id={0}"">{1}</a>", slg.PerfTSID, slg.PerfTSName));
                            generateTableColumn(htmltw, slg.CRTEStatus);
                            //generateTableColumn(htmltw, slg.DateCreated);
                            //generateTableColumn(htmltw, slg.DateOfLastPost);

                            htmltw.RenderEndTag(); //Ends <Row> 

                            numSLGs++;
                        }
                    }
                }

                htmltw.RenderEndTag(); //Ends <Table>
            }

            return (numSLGs > 0 ? sw.ToString() : null);
        }

        /// <summary>
        /// Helper function for slgTable
        /// </summary>
        /// <param name="htmlWriter">Already created in slgTable method</param>
        /// <param name="innerHTML">SLG property to write</param>
        static void generateTableColumn(HtmlTextWriter htmlWriter, string innerHTML, bool header = false)
        {
            if (header)
            {
                htmlWriter.RenderBeginTag(HtmlTextWriterTag.Th);
            }
            else
            {
                htmlWriter.RenderBeginTag(HtmlTextWriterTag.Td);
            }
            if (!String.IsNullOrEmpty(innerHTML))
            {
                htmlWriter.Write(innerHTML);
            }
            else
            {
                htmlWriter.Write("No public posts yet");
            }
            htmlWriter.RenderEndTag();
        }

        static string loadStyles()
        {
            string styles = null;
            using (StreamReader file = new StreamReader("tables.CSS"))
            {
                styles = file.ReadToEnd();
            }

            return styles;
        }

        #endregion

    }
}
