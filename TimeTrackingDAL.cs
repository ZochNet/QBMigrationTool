using rototrack_data_access;
using rototrack_model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using zochnet_utils;

namespace QBMigrationTool
{
    class TimeTrackingDAL
    {
        public static XmlDocument BuildQueryRequest(string fromModifiedDate, string toModifiedDate)
        {
            XmlDocument doc = XmlUtils.MakeRequestDocument();
            XmlElement parent = XmlUtils.MakeRequestParentElement(doc);
            XmlElement queryElement = doc.CreateElement("TimeTrackingQueryRq");
            parent.AppendChild(queryElement);

            XmlElement dateRangeFilter = doc.CreateElement("ModifiedDateRangeFilter");
            queryElement.AppendChild(dateRangeFilter);
            dateRangeFilter.AppendChild(XmlUtils.MakeSimpleElem(doc, "FromModifiedDate", fromModifiedDate));
            dateRangeFilter.AppendChild(XmlUtils.MakeSimpleElem(doc, "ToModifiedDate", toModifiedDate));
            
            return doc;
        }

        public static void HandleResponse(string response)
        {
            WalkTimeTrackingQueryRs(response);
        }

        private static void WalkTimeTrackingQueryRs(string response)
        {
            //Parse the response XML string into an XmlDocument
            XmlDocument responseXmlDoc = new XmlDocument();
            responseXmlDoc.LoadXml(response);

            //Get the response for our request
            XmlNodeList TimeTrackingQueryRsList = responseXmlDoc.GetElementsByTagName("TimeTrackingQueryRs");
            if (TimeTrackingQueryRsList.Count == 1) //Should always be true since we only did one request in this sample
            {
                XmlNode responseNode = TimeTrackingQueryRsList.Item(0);
                //Check the status code, info, and severity
                XmlAttributeCollection rsAttributes = responseNode.Attributes;
                string statusCode = rsAttributes.GetNamedItem("statusCode").Value;
                string statusSeverity = rsAttributes.GetNamedItem("statusSeverity").Value;
                string statusMessage = rsAttributes.GetNamedItem("statusMessage").Value;

                //status code = 0 all OK, > 0 is warning
                if (Convert.ToInt32(statusCode) >= 0)
                {
                    XmlNodeList TimeTrackingRetList = responseNode.SelectNodes("//TimeTrackingRet");//XPath Query
                    for (int i = 0; i < TimeTrackingRetList.Count; i++)
                    {
                        XmlNode TimeTrackingRet = TimeTrackingRetList.Item(i);
                        WalkTimeTrackingRetForQuery(TimeTrackingRet);
                    }
                }
            }
        }

        private static void WalkTimeTrackingRetForQuery(XmlNode TimeTrackingRet)
        {
            if (TimeTrackingRet == null) return;

            string QBTxnID = TimeTrackingRet.SelectSingleNode("./TxnID").InnerText;
            string QBEditSequence = TimeTrackingRet.SelectSingleNode("./EditSequence").InnerText;
            string TxnDate = TimeTrackingRet.SelectSingleNode("./TxnDate").InnerText;

            string QBEmployeeListID = "";
            if (TimeTrackingRet.SelectSingleNode("./EntityRef/ListID") != null)
            {
                QBEmployeeListID = TimeTrackingRet.SelectSingleNode("./EntityRef/ListID").InnerText;
            }

            string QBWorkOrderListID = "";
            XmlNode CustomerRef = TimeTrackingRet.SelectSingleNode("./CustomerRef");
            if (CustomerRef != null)
            {
                if (TimeTrackingRet.SelectSingleNode("./CustomerRef/ListID") != null)
                {
                    QBWorkOrderListID = TimeTrackingRet.SelectSingleNode("./CustomerRef/ListID").InnerText;
                }
            }

            string QBServiceTypeListID = "";
            XmlNode ItemServiceRef = TimeTrackingRet.SelectSingleNode("./ItemServiceRef");
            if (ItemServiceRef != null)
            {
                if (TimeTrackingRet.SelectSingleNode("./ItemServiceRef/ListID") != null)
                {
                    QBServiceTypeListID = TimeTrackingRet.SelectSingleNode("./ItemServiceRef/ListID").InnerText;
                }
            }

            string Duration = TimeTrackingRet.SelectSingleNode("./Duration").InnerText;

            string QBAreaListID = "";
            XmlNode ClassRef = TimeTrackingRet.SelectSingleNode("./ClassRef");
            if (ClassRef != null)
            {
                if (TimeTrackingRet.SelectSingleNode("./ClassRef/ListID") != null)
                {
                    QBAreaListID = TimeTrackingRet.SelectSingleNode("./ClassRef/ListID").InnerText;
                }
            }

            string Notes = "";
            if (TimeTrackingRet.SelectSingleNode("./Notes") != null)
            {
                Notes = TimeTrackingRet.SelectSingleNode("./Notes").InnerText;
            }

            string BillableStatus = "";
            if (TimeTrackingRet.SelectSingleNode("./BillableStatus") != null)
            {
                BillableStatus = TimeTrackingRet.SelectSingleNode("./BillableStatus").InnerText;
            }

            RotoTrackDb db = new RotoTrackDb();

            TimeTracking tt = null;
            if (db.TimeTrackings.Any(j => j.QBTxnId == QBTxnID))
            {
                tt = db.TimeTrackings.First(j => j.QBTxnId == QBTxnID);
            }
            else
            {
                tt = new TimeTracking();
                db.TimeTrackings.Add(tt);
            }

            tt.QBTxnId = QBTxnID;
            tt.QBEditSequence = QBEditSequence;
            DateTime dateWorked;
            if (DateTime.TryParse(TxnDate, out dateWorked))
            {
                tt.DateWorked = dateWorked;
            }
            tt.QBEmployeeListID = QBEmployeeListID;
            tt.QBWorkOrderListID = QBWorkOrderListID;
            tt.QBServiceTypeListID = QBServiceTypeListID;

            HoursMinutes hrsMins = GetHoursMinutesFromDuration(Duration);
            if (hrsMins != null)
            {
                tt.HoursWorked = hrsMins.Hours;
                tt.MinutesWorked = hrsMins.Minutes;
            }

            tt.QBAreaListID = QBAreaListID;
            tt.Notes = Notes;
            tt.BillableStatus = BillableStatus;

            db.SaveChanges();
        }

        public class HoursMinutes
        {
            public int Hours { get; set; }
            public int Minutes { get; set; }
        }

        private static HoursMinutes GetHoursMinutesFromDecimal(decimal hours)
        {
            int hoursInteger = Convert.ToInt32(decimal.Truncate(hours));
            decimal minutesDecimal = (hours - decimal.Truncate(hours)) * 60M;
            minutesDecimal = decimal.Round(minutesDecimal, 0, MidpointRounding.AwayFromZero);
            int minutesInteger = Convert.ToInt32(decimal.Truncate(minutesDecimal));

            HoursMinutes hrsMins = new HoursMinutes();
            hrsMins.Hours = hoursInteger;
            hrsMins.Minutes = minutesInteger;

            return hrsMins;
        }

        private static HoursMinutes GetHoursMinutesFromDuration(string Duration)
        {
            HoursMinutes hrsMins = null;

            try
            {
                var parts = Duration.Split('T');
                if (parts.Length == 2)
                {
                    if (parts[0] == "P")
                    {
                        var parts2 = parts[1].Split('H');
                        if (parts2.Length == 2)
                        {
                            int hours = Convert.ToInt32(parts2[0]);
                            var parts3 = parts2[1].Split('M');
                            if (parts3.Length == 2)
                            {
                                int minutes = Convert.ToInt32(parts3[0]);
                                hrsMins = new HoursMinutes();
                                hrsMins.Hours = hours;
                                hrsMins.Minutes = minutes;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logging.RototrackErrorLog(e.ToString());
            }

            return hrsMins;
        }

    }
}
