using Interop.QBXMLRP2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using zochnet_utils;

namespace QBMigrationTool
{
    public class QBUtils
    {
        public static string DoRequest(XmlDocument doc)
        {
            RequestProcessor2 rp = null;
            string ticket = null;
            string response = null;
            bool errorOccurred = false;
            int maxTries = 5;
            int tries = 0;

            do
            {
                try
                {
                    tries++;
                    rp = new RequestProcessor2();
                    rp.OpenConnection("QBMT1", "QBMigrationTool");
                    ticket = rp.BeginSession("", QBFileMode.qbFileOpenDoNotCare);
                    response = rp.ProcessRequest(ticket, doc.OuterXml);

                    if (errorOccurred)
                    {
                        Logging.RototrackErrorLog("QBMigrationTool: DoRequest Retry succeeded.");
                        errorOccurred = false;
                    }
                }
                catch (System.Runtime.InteropServices.COMException)
                {
                    Logging.RototrackErrorLog("QBMigrationTool:  Error in DoRequest.  Retrying...");
                    errorOccurred = true;
                }
                finally
                {
                    if (ticket != null)
                    {
                        rp.EndSession(ticket);
                    }
                    if (rp != null)
                    {
                        rp.CloseConnection();
                    }
                }

            } while (errorOccurred && tries <= maxTries);

            return response;
        }

        public static string DoRequestRaw(string rawXML)
        {
            RequestProcessor2 rp = null;
            string ticket = null;
            string response = null;
            try
            {
                rp = new RequestProcessor2();
                rp.OpenConnection("QBMT1", "QBMigrationTool");
                ticket = rp.BeginSession("", QBFileMode.qbFileOpenDoNotCare);
                response = rp.ProcessRequest(ticket, rawXML);
                return response;
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                MessageBox.Show("COM Error Description = " + ex.Message, "COM error");
                return ex.Message;
            }
            finally
            {
                if (ticket != null)
                {
                    rp.EndSession(ticket);
                }
                if (rp != null)
                {
                    rp.CloseConnection();
                }
            }
        }

        public static string GetGuidFromNotes(string notes)
        {
            string retVal = "";
            Regex guid = new Regex(@"guid=(?<guid>[a-zA-Z0-9-]+)$", RegexOptions.Compiled);
            Match guidMatch = guid.Match(notes);
            if (guidMatch.Success)
            {
                retVal = guidMatch.Result("${guid}");
            }
            return retVal;
        }

    }
}
