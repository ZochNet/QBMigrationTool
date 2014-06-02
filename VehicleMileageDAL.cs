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
    class VehicleMileageDAL
    {
        public static XmlDocument BuildQueryRequest(string fromModifiedDate, string toModifiedDate)
        {
            XmlDocument doc = XmlUtils.MakeRequestDocument();
            XmlElement parent = XmlUtils.MakeRequestParentElement(doc);
            XmlElement queryElement = doc.CreateElement("VehicleMileageQueryRq");
            parent.AppendChild(queryElement);

            XmlElement dateRangeFilter = doc.CreateElement("TxnDateRangeFilter");
            queryElement.AppendChild(dateRangeFilter);
            dateRangeFilter.AppendChild(XmlUtils.MakeSimpleElem(doc, "FromTxnDate", fromModifiedDate));
            dateRangeFilter.AppendChild(XmlUtils.MakeSimpleElem(doc, "ToTxnDate", toModifiedDate));

            return doc;
        }

        public static XmlDocument BuildAddRq(ServiceEntry se, ServiceDetail sd)
        {
            try
            {
                XmlDocument doc = XmlUtils.MakeRequestDocument();
                XmlElement parent = XmlUtils.MakeRequestParentElement(doc);

                RotoTrackDb db = new RotoTrackDb();
                UserProfile u = db.UserProfiles.Find(sd.TechnicianId);
                Vehicle v = db.Vehicles.Find(sd.VehicleId);
                DSR dsr = db.DSRs.Include("WorkOrder").First(f => f.Id == se.DSRId);
                Customer c = db.Customers.Find(dsr.WorkOrder.CustomerId);
                MileageRate mr = db.MileageRates.Find(sd.MileageRateId);
                Area a = db.Areas.Find(u.AreaId);

                XmlElement Rq = doc.CreateElement("VehicleMileageAddRq");
                parent.AppendChild(Rq);

                XmlElement RqType = doc.CreateElement("VehicleMileageAdd");
                Rq.AppendChild(RqType);

                XmlElement VehicleRef = doc.CreateElement("VehicleRef");
                RqType.AppendChild(VehicleRef);
                VehicleRef.AppendChild(XmlUtils.MakeSimpleElem(doc, "ListID", v.QBListId));

                XmlElement CustomerRef = doc.CreateElement("CustomerRef");
                RqType.AppendChild(CustomerRef);
                CustomerRef.AppendChild(XmlUtils.MakeSimpleElem(doc, "ListID", dsr.WorkOrder.QBListId));

                XmlElement ItemRef = doc.CreateElement("ItemRef");
                RqType.AppendChild(ItemRef);
                ItemRef.AppendChild(XmlUtils.MakeSimpleElem(doc, "ListID", mr.QBListId));

                XmlElement ClassRef = doc.CreateElement("ClassRef");
                RqType.AppendChild(ClassRef);
                ClassRef.AppendChild(XmlUtils.MakeSimpleElem(doc, "ListID", a.QBListId));

                RqType.AppendChild(XmlUtils.MakeSimpleElem(doc, "TripStartDate", se.DateWorked.ToString("yyyy-MM-dd")));
                RqType.AppendChild(XmlUtils.MakeSimpleElem(doc, "TripEndDate", se.DateWorked.ToString("yyyy-MM-dd")));
                RqType.AppendChild(XmlUtils.MakeSimpleElem(doc, "TotalMiles", se.Mileage.ToString()));
                string fullName = "";
                if (!string.IsNullOrEmpty(u.FirstName)) fullName += u.FirstName;
                if (!string.IsNullOrEmpty(u.LastName)) fullName += (" " + u.LastName);
                fullName += " guid=";
                fullName += se.GUID;
                RqType.AppendChild(XmlUtils.MakeSimpleElem(doc, "Notes", fullName));
                RqType.AppendChild(XmlUtils.MakeSimpleElem(doc, "BillableStatus", "Billable"));

                return doc;
            }
            catch (Exception e)
            {
                string evLogTxt = "";
                evLogTxt = "Error building vehicle mileage add request! " + e.Message + "\r\n";                
                Logging.RototrackErrorLog(evLogTxt);
                return null;
            }
        }

        public static void HandleResponse(string response)
        {
            WalkVehicleMileageQueryRs(response);
        }

        public static void HandleAddResponse(string response)
        {
            WalkVehicleMileageAddRs(response);
        }

        private static void WalkVehicleMileageQueryRs(string response)
        {
            //Parse the response XML string into an XmlDocument
            XmlDocument responseXmlDoc = new XmlDocument();
            responseXmlDoc.LoadXml(response);

            //Get the response for our request
            XmlNodeList VehicleMileageQueryRsList = responseXmlDoc.GetElementsByTagName("VehicleMileageQueryRs");
            if (VehicleMileageQueryRsList.Count == 1) //Should always be true since we only did one request in this sample
            {
                XmlNode responseNode = VehicleMileageQueryRsList.Item(0);
                //Check the status code, info, and severity
                XmlAttributeCollection rsAttributes = responseNode.Attributes;
                string statusCode = rsAttributes.GetNamedItem("statusCode").Value;
                string statusSeverity = rsAttributes.GetNamedItem("statusSeverity").Value;
                string statusMessage = rsAttributes.GetNamedItem("statusMessage").Value;

                //status code = 0 all OK, > 0 is warning
                if (Convert.ToInt32(statusCode) >= 0)
                {
                    XmlNodeList VehicleMileageRetList = responseNode.SelectNodes("//VehicleMileageRet");//XPath Query
                    for (int i = 0; i < VehicleMileageRetList.Count; i++)
                    {
                        XmlNode VehicleMileageRet = VehicleMileageRetList.Item(i);
                        WalkVehicleMileageRetForQuery(VehicleMileageRet);
                    }
                }
            }
        }

        private static void WalkVehicleMileageRetForQuery(XmlNode VehicleMileageRet)
        {
            if (VehicleMileageRet == null) return;

            string TxnID = VehicleMileageRet.SelectSingleNode("./TxnID").InnerText;
            string TimeCreated = VehicleMileageRet.SelectSingleNode("./TimeCreated").InnerText;
            string TimeModified = VehicleMileageRet.SelectSingleNode("./TimeCreated").InnerText;
            string TripStartDate = "";
            if (VehicleMileageRet.SelectSingleNode("./TripStartDate") != null)
            {
                TripStartDate = VehicleMileageRet.SelectSingleNode("./TripStartDate").InnerText;
            }
            string EditSequence = VehicleMileageRet.SelectSingleNode("./EditSequence").InnerText;

            string VehicleRefListID = "";
            XmlNode VehicleRef = VehicleMileageRet.SelectSingleNode("./VehicleRef");
            if (VehicleRef != null)
            {
                if (VehicleMileageRet.SelectSingleNode("./VehicleRef/ListID") != null)
                {
                    VehicleRefListID = VehicleMileageRet.SelectSingleNode("./VehicleRef/ListID").InnerText;
                }
            }

            string WorkOrderListID = "";
            XmlNode CustomerRef = VehicleMileageRet.SelectSingleNode("./CustomerRef");
            if (CustomerRef != null)
            {
                if (VehicleMileageRet.SelectSingleNode("./CustomerRef/ListID") != null)
                {
                    WorkOrderListID = VehicleMileageRet.SelectSingleNode("./CustomerRef/ListID").InnerText;
                }
            }

            string ItemRefListID = "";
            XmlNode ItemRef = VehicleMileageRet.SelectSingleNode("./ItemRef");
            if (ItemRef != null)
            {
                if (VehicleMileageRet.SelectSingleNode("./ItemRef/ListID") != null)
                {
                    ItemRefListID = VehicleMileageRet.SelectSingleNode("./ItemRef/ListID").InnerText;
                }
            }

            string AreaListID = "";
            XmlNode ClassRef = VehicleMileageRet.SelectSingleNode("./ClassRef");
            if (ClassRef != null)
            {
                if (VehicleMileageRet.SelectSingleNode("./ClassRef/ListID") != null)
                {
                    AreaListID = VehicleMileageRet.SelectSingleNode("./ClassRef/ListID").InnerText;
                }
            }

            string TotalMiles = "";
            if (VehicleMileageRet.SelectSingleNode("./TotalMiles") != null)
            {
                TotalMiles = VehicleMileageRet.SelectSingleNode("./TotalMiles").InnerText;
            }

            string Notes = "";
            if (VehicleMileageRet.SelectSingleNode("./Notes") != null)
            {
                Notes = VehicleMileageRet.SelectSingleNode("./Notes").InnerText;
            }

            string BillableStatus = "";
            if (VehicleMileageRet.SelectSingleNode("./BillableStatus") != null)
            {
                BillableStatus = VehicleMileageRet.SelectSingleNode("./BillableStatus").InnerText;
            }

            string BillableRate = "";
            if (VehicleMileageRet.SelectSingleNode("./BillableRate") != null)
            {
                BillableRate = VehicleMileageRet.SelectSingleNode("./BillableRate").InnerText;
            }

            string BillableAmount = "";
            if (VehicleMileageRet.SelectSingleNode("./BillableAmount") != null)
            {
                BillableAmount = VehicleMileageRet.SelectSingleNode("./BillableAmount").InnerText;
            }

            RotoTrackDb db = new RotoTrackDb();

            MileageTracking mt = null;
            if (db.MileageTrackings.Any(j => j.QBTxnId == TxnID))
            {
                mt = db.MileageTrackings.First(j => j.QBTxnId == TxnID);
            }
            else
            {
                mt = new MileageTracking();
                db.MileageTrackings.Add(mt);
            }

            mt.QBTxnId = TxnID;
            DateTime created, modified, tripstartdate;
            if (DateTime.TryParse(TimeCreated, out created))
            {
                mt.Created = created;
            }
            if (DateTime.TryParse(TimeModified, out modified))
            {
                mt.Modified = modified;
            }
            mt.TripStartDate = DateTime.MinValue;
            if (DateTime.TryParse(TripStartDate, out tripstartdate))
            {
                mt.TripStartDate = tripstartdate;
            }

            mt.QBEditSequence = EditSequence;
            mt.QBVehicleListID = VehicleRefListID;
            mt.QBWorkOrderListID = WorkOrderListID;
            mt.QBMileageRateListID = ItemRefListID;
            mt.QBAreaListID = AreaListID;

            decimal totalMiles;
            if (Decimal.TryParse(TotalMiles, out totalMiles))
            {
                mt.TotalMiles = totalMiles;
            }

            mt.Notes = Notes;
            mt.BillableStatus = BillableStatus;

            decimal billableRate, billableAmount;
            if (Decimal.TryParse(BillableRate, out billableRate))
            {
                mt.BillableRate = billableRate;
            }
            if (Decimal.TryParse(BillableAmount, out billableAmount))
            {
                mt.BillableAmount = billableAmount;
            }

            db.SaveChanges();
        }

        private static void WalkVehicleMileageAddRs(string response)
        {
            //Parse the response XML string into an XmlDocument
            XmlDocument responseXmlDoc = new XmlDocument();
            responseXmlDoc.LoadXml(response);

            //Get the response for our request
            XmlNodeList VehicleMileageAddRsList = responseXmlDoc.GetElementsByTagName("VehicleMileageAddRs");
            if (VehicleMileageAddRsList.Count == 1) //Should always be true since we only did one request in this sample
            {
                XmlNode responseNode = VehicleMileageAddRsList.Item(0);
                //Check the status code, info, and severity
                XmlAttributeCollection rsAttributes = responseNode.Attributes;
                string statusCode = rsAttributes.GetNamedItem("statusCode").Value;
                string statusSeverity = rsAttributes.GetNamedItem("statusSeverity").Value;
                string statusMessage = rsAttributes.GetNamedItem("statusMessage").Value;

                //status code = 0 all OK, > 0 is warning
                if (Convert.ToInt32(statusCode) >= 0)
                {
                    XmlNodeList VehicleMileageRetList = responseNode.SelectNodes("//VehicleMileageRet");//XPath Query
                    for (int i = 0; i < VehicleMileageRetList.Count; i++)
                    {
                        XmlNode VehicleMileageRet = VehicleMileageRetList.Item(i);
                        WalkVehicleMileageRetForAdd(VehicleMileageRet);
                    }
                }
            }
        }

        private static void WalkVehicleMileageRetForAdd(XmlNode VehicleMileageRet)
        {
            if (VehicleMileageRet == null) return;

            RotoTrackDb db = new RotoTrackDb();

            //Get value of Notes--we should have the ServiceEntry GUID encoded in it as well.
            string seGUID = "";
            if (VehicleMileageRet.SelectSingleNode("./Notes") != null)
            {
                string Notes = VehicleMileageRet.SelectSingleNode("./Notes").InnerText;
                seGUID = QBUtils.GetGuidFromNotes(Notes);
            }

            string TxnID = "";
            if (VehicleMileageRet.SelectSingleNode("./TxnID") != null)
            {
                TxnID = VehicleMileageRet.SelectSingleNode("./TxnID").InnerText;
            }

            if (seGUID != "" && TxnID != "")
            {
                ServiceEntry se = null;
                if (db.ServiceEntries.Any(f => f.GUID == seGUID))
                {
                    se = db.ServiceEntries.First(f => f.GUID == seGUID);
                }
                if (se != null)
                {
                    se.QBListIdForMileage = TxnID;
                    db.Entry(se).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
        }

    }
}
