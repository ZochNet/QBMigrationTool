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
    class VehicleDAL
    {
        public static XmlDocument BuildQueryRequest(string activeStatus, string fromModifiedDate, string toModifiedDate)
        {
            XmlDocument doc = XmlUtils.MakeRequestDocument();
            XmlElement parent = XmlUtils.MakeRequestParentElement(doc);
            XmlElement queryElement = doc.CreateElement("VehicleQueryRq");
            parent.AppendChild(queryElement);

            queryElement.AppendChild(XmlUtils.MakeSimpleElem(doc, "ActiveStatus", activeStatus));     
            queryElement.AppendChild(XmlUtils.MakeSimpleElem(doc, "FromModifiedDate", fromModifiedDate));
            queryElement.AppendChild(XmlUtils.MakeSimpleElem(doc, "ToModifiedDate", toModifiedDate));                  

            return doc;
        }

        public static void HandleResponse(string response)
        {
            WalkVehicleQueryRs(response);
        }

        private static void WalkVehicleQueryRs(string response)
        {
            //Parse the response XML string into an XmlDocument
            XmlDocument responseXmlDoc = new XmlDocument();
            responseXmlDoc.LoadXml(response);

            //Get the response for our request
            XmlNodeList VehicleQueryRsList = responseXmlDoc.GetElementsByTagName("VehicleQueryRs");
            if (VehicleQueryRsList.Count == 1) //Should always be true since we only did one request in this sample
            {
                XmlNode responseNode = VehicleQueryRsList.Item(0);
                //Check the status code, info, and severity
                XmlAttributeCollection rsAttributes = responseNode.Attributes;
                string statusCode = rsAttributes.GetNamedItem("statusCode").Value;
                string statusSeverity = rsAttributes.GetNamedItem("statusSeverity").Value;
                string statusMessage = rsAttributes.GetNamedItem("statusMessage").Value;

                //status code = 0 all OK, > 0 is warning
                if (Convert.ToInt32(statusCode) >= 0)
                {
                    XmlNodeList VehicleRetList = responseNode.SelectNodes("//VehicleRet");//XPath Query
                    for (int i = 0; i < VehicleRetList.Count; i++)
                    {
                        XmlNode VehicleRet = VehicleRetList.Item(i);
                        WalkVehicleRet(VehicleRet);
                    }
                }
            }
        }

        private static void WalkVehicleRet(XmlNode VehicleRet)
        {
            if (VehicleRet == null) return;

            Vehicle v = null;
            RotoTrackDb db = new RotoTrackDb();

            string ListID = VehicleRet.SelectSingleNode("./ListID").InnerText;
            if (db.Vehicles.Any(f => f.QBListId == ListID))
            {
                v = db.Vehicles.First(f => f.QBListId == ListID);
            }
            else
            {
                v = new Vehicle();
                db.Vehicles.Add(v);
            }
            v.QBListId = ListID;

            if (VehicleRet.SelectSingleNode("./Name") != null)
            {
                string Name = VehicleRet.SelectSingleNode("./Name").InnerText;
                v.Name = Name;
            }

            string IsActive = "false";
            if (VehicleRet.SelectSingleNode("./IsActive") != null)
            {
                IsActive = VehicleRet.SelectSingleNode("./IsActive").InnerText;
            }
            v.IsActive = (IsActive == "true") ? true : false;

            if (VehicleRet.SelectSingleNode("./Desc") != null)
            {
                string Desc = VehicleRet.SelectSingleNode("./Desc").InnerText;
                v.Description = Desc;
            }

            if (v != null)
            {
                db.SaveChanges();
            }

        }
    }
}
