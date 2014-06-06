using rototrack_data_access;
using rototrack_model;
using System;
using System.Collections.Generic;
using System.Data;
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

        public static XmlDocument BuildAddRq(ServiceEntry se, ServiceDetail sd, int serviceId, decimal hours)
        {
            try
            {
                XmlDocument doc = XmlUtils.MakeRequestDocument();
                XmlElement parent = XmlUtils.MakeRequestParentElement(doc);

                RotoTrackDb db = new RotoTrackDb();
                UserProfile u = db.UserProfiles.Find(sd.TechnicianId);
                ServiceType st = db.ServiceTypes.Find(serviceId);
                DSR dsr = db.DSRs.Include("WorkOrder").First(f => f.Id == se.DSRId);
                Customer c = db.Customers.Find(dsr.WorkOrder.CustomerId);
                Area a = db.Areas.Find(u.AreaId);

                XmlElement Rq = doc.CreateElement("TimeTrackingAddRq");
                parent.AppendChild(Rq);

                XmlElement RqType = doc.CreateElement("TimeTrackingAdd");
                Rq.AppendChild(RqType);

                RqType.AppendChild(XmlUtils.MakeSimpleElem(doc, "TxnDate", se.DateWorked.ToString("yyyy-MM-dd")));

                XmlElement EntityRef = doc.CreateElement("EntityRef");
                RqType.AppendChild(EntityRef);
                EntityRef.AppendChild(XmlUtils.MakeSimpleElem(doc, "ListID", u.QBListId));

                XmlElement CustomerRef = doc.CreateElement("CustomerRef");
                RqType.AppendChild(CustomerRef);
                CustomerRef.AppendChild(XmlUtils.MakeSimpleElem(doc, "ListID", dsr.WorkOrder.QBListId));

                XmlElement ItemServiceRef = doc.CreateElement("ItemServiceRef");
                RqType.AppendChild(ItemServiceRef);
                ItemServiceRef.AppendChild(XmlUtils.MakeSimpleElem(doc, "ListID", st.QBListId));

                HoursMinutes hrsMins = GetHoursMinutesFromDecimal(hours);
                int hoursInteger = hrsMins.Hours;
                int minutesInteger = hrsMins.Minutes;

                string duration = "PT" + hoursInteger.ToString() + "H" + minutesInteger.ToString() + "M" + "0S";
                RqType.AppendChild(XmlUtils.MakeSimpleElem(doc, "Duration", duration));

                XmlElement ClassRef = doc.CreateElement("ClassRef");
                RqType.AppendChild(ClassRef);
                ClassRef.AppendChild(XmlUtils.MakeSimpleElem(doc, "ListID", a.QBListId));

                string notes = "guid=" + se.GUID;
                RqType.AppendChild(XmlUtils.MakeSimpleElem(doc, "Notes", notes));

                if (st.Name.ToUpper().StartsWith("DIRECT"))
                {
                    RqType.AppendChild(XmlUtils.MakeSimpleElem(doc, "BillableStatus", "Billable"));
                }
                else
                {
                    RqType.AppendChild(XmlUtils.MakeSimpleElem(doc, "BillableStatus", "NotBillable"));
                }

                return doc;
            }
            catch (Exception e)
            {
                string evLogTxt = "";
                evLogTxt = "Error building time tracking add request! " + e.Message + "\r\n";                
                Logging.RototrackErrorLog(evLogTxt);
                return null;
            }
        }

        public static void HandleResponse(string response)
        {
            WalkTimeTrackingQueryRs(response);
        }

        public static void HandleAddResponse(string response)
        {
            WalkTimeTrackingAddRs(response);
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

        private static void WalkTimeTrackingAddRs(string response)
        {
            //Parse the response XML string into an XmlDocument
            XmlDocument responseXmlDoc = new XmlDocument();
            responseXmlDoc.LoadXml(response);

            //Get the response for our request
            XmlNodeList TimeTrackingAddRsList = responseXmlDoc.GetElementsByTagName("TimeTrackingAddRs");
            if (TimeTrackingAddRsList.Count == 1) //Should always be true since we only did one request in this sample
            {
                XmlNode responseNode = TimeTrackingAddRsList.Item(0);
                //Check the status code, info, and severity
                XmlAttributeCollection rsAttributes = responseNode.Attributes;
                string statusCode = rsAttributes.GetNamedItem("statusCode").Value;
                string statusSeverity = rsAttributes.GetNamedItem("statusSeverity").Value;
                string statusMessage = rsAttributes.GetNamedItem("statusMessage").Value;

                // Check status and log any errors
                QBUtils.CheckStatus(statusCode, statusSeverity, statusMessage);

                //status code = 0 all OK, > 0 is warning
                if (Convert.ToInt32(statusCode) >= 0)
                {
                    XmlNodeList TimeTrackingRetList = responseNode.SelectNodes("//TimeTrackingRet");//XPath Query
                    for (int i = 0; i < TimeTrackingRetList.Count; i++)
                    {
                        XmlNode TimeTrackingRet = TimeTrackingRetList.Item(i);
                        WalkTimeTrackingRetForAdd(TimeTrackingRet);
                    }
                }
            }
        }

        private static void WalkTimeTrackingRetForAdd(XmlNode TimeTrackingRet)
        {
            if (TimeTrackingRet == null) return;

            string Duration = TimeTrackingRet.SelectSingleNode("./Duration").InnerText;
            HoursMinutes hrsMins = GetHoursMinutesFromDuration(Duration);
            if (hrsMins == null) return;

            RotoTrackDb db = new RotoTrackDb();

            string TxnID = "";
            string seGUID = "";
            string ItemServiceListID = "";

            if (TimeTrackingRet.SelectSingleNode("./TxnID") != null)
            {
                TxnID = TimeTrackingRet.SelectSingleNode("./TxnID").InnerText;
            }
            if (TimeTrackingRet.SelectSingleNode("./Notes") != null)
            {
                string Notes = TimeTrackingRet.SelectSingleNode("./Notes").InnerText;
                seGUID = QBUtils.GetGuidFromNotes(Notes);
            }
            XmlNode ItemServiceRef = TimeTrackingRet.SelectSingleNode("./ItemServiceRef");
            if (ItemServiceRef != null)
            {
                if (ItemServiceRef.SelectSingleNode("./ListID") != null)
                {
                    ItemServiceListID = ItemServiceRef.SelectSingleNode("./ListID").InnerText;
                }
            }

            if (TxnID != null && seGUID != null && ItemServiceListID != null)
            {
                ServiceEntry se = null;
                if (db.ServiceEntries.Any(f => f.GUID == seGUID))
                {
                    se = db.ServiceEntries.First(f => f.GUID == seGUID);
                }
                if (se != null)
                {
                    ServiceDetail sd = db.ServiceDetails.Find(se.ServiceDetailId);
                    if (sd != null)
                    {
                        ServiceType regST = db.ServiceTypes.Find(sd.ServiceTypeId);
                        ServiceType otST = db.ServiceTypes.Find(sd.OTServiceTypeId);
                        if (regST != null && otST != null)
                        {
                            // We always update the regular hours first, so if reg and OT both have the same service list ID, then we need to also check if we already
                            // wrote out the regular hours or not--else we will put both the reg and ot hours into regular hours since we match on reg hours first.
                            // I was going to also look at the actual hours and minutes, but if those are identical, too, then this reduces to the same problem we have
                            // solved here--we just have to assume reg hours are always written first, which they are.  Also, I don't want to compare on hours/minutes
                            // because that may fail due to rounding errors.
                            if (ItemServiceListID == regST.QBListId && se.QBListIdForRegularHours == null)
                            {
                                se.QBListIdForRegularHours = TxnID;
                                db.Entry(se).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            else if (ItemServiceListID == otST.QBListId && se.QBListIdForOTHours == null)
                            {
                                se.QBListIdForOTHours = TxnID;
                                db.Entry(se).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                    }
                }
            }
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
