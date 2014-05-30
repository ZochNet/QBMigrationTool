using rototrack_data_access;
using rototrack_model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

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

        public static void HandleResponse(string response)
        {
            WalkVehicleMileageQueryRs(response);
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
    }
}
