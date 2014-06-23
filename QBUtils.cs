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
                    CheckIfBuildTypeAndQuickBooksFileMatch(rp.GetCurrentCompanyFileName(ticket));
                    response = rp.ProcessRequest(ticket, doc.OuterXml);

                    if (errorOccurred)
                    {
                        Logging.RototrackErrorLog("QBMigrationTool: " + RototrackConfig.GetBuildType() + ": " + "DoRequest Retry succeeded.");
                        errorOccurred = false;
                    }
                }
                catch (System.Runtime.InteropServices.COMException e)
                {
                    Logging.RototrackErrorLog("QBMigrationTool: " + RototrackConfig.GetBuildType() + ": " + "Error in DoRequest.  Retrying.  Details: " + e.ToString());
                    errorOccurred = true;
                }
                catch (Exception e)
                {
                    Logging.RototrackErrorLog("QBMigrationTool: " + RototrackConfig.GetBuildType() + ": " + "Error in DoRequest.  Retrying.  Details: " + e.ToString());
                    break;
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
                CheckIfBuildTypeAndQuickBooksFileMatch(rp.GetCurrentCompanyFileName(ticket));
                response = rp.ProcessRequest(ticket, rawXML);
                return response;
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                MessageBox.Show("COM Error Description = " + ex.Message, "COM error");
                return ex.Message;
            }
            catch (Exception e)
            {
                MessageBox.Show("QBMigrationTool: " + RototrackConfig.GetBuildType() + ": " + "Error in DoRequestRaw.  Details: " + e.ToString(), "Exception");
                return e.Message;
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

        private static void CheckIfBuildTypeAndQuickBooksFileMatch(string filename)
        {
            string buildType = RototrackConfig.GetBuildType();
            bool validMatch = false;
            if (filename.Contains("Roto-Versal") && buildType.Contains("ROTO"))
            {
                validMatch = true;
            }
            else if (filename.Contains("Guardiant") && buildType.Contains("GUARD"))
            {
                validMatch = true;
            }

            if (!validMatch)
            {
                throw new Exception("Build type is: " + buildType + " but open Quickbooks file is " + filename + "!");
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

        public static void CheckStatus(string statusCode, string statusSeverity, string statusMessage)
        {
            if (Convert.ToInt32(statusCode) != 0)
            {
                Logging.RototrackErrorLog("QBMigrationTool: " + RototrackConfig.GetBuildType() + ": " + "Error: " + statusMessage);
            }
        }
    }
}
