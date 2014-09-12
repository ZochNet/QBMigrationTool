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
    public class SiteAndAdditionalInfoDAL
    {
        public static XmlDocument BuildUpdateSalesRepRq(WorkOrder wo)
        {
            RotoTrackDb db = new RotoTrackDb();
            XmlDocument doc = XmlUtils.MakeRequestDocument();
            XmlElement parent = XmlUtils.MakeRequestParentElement(doc);

            string listID = wo.QBListId;
            Site site = db.Sites.Find(wo.SiteId);

            string salesRep = "";
            UserProfile up = db.UserProfiles.Find(wo.SalesRepId);
            if (up != null)
            {
                salesRep = up.FirstName + " " + up.LastName;
            }
             
            doc = BuildDataExtModOrDelRq(doc, parent, listID, "Work Order Sales Rep", salesRep);

            return doc;
        }

        public static XmlDocument BuildUpdateRq(WorkOrder wo)
        {
            RotoTrackDb db = new RotoTrackDb();
            XmlDocument doc = XmlUtils.MakeRequestDocument();
            XmlElement parent = XmlUtils.MakeRequestParentElement(doc);
            
            string listID = wo.QBListId;
            Site site = db.Sites.Find(wo.SiteId);

            string salesRep = "";
            UserProfile up = db.UserProfiles.Find(wo.SalesRepId);            
            if (up != null)
            {
                salesRep = up.FirstName + " " + up.LastName;
            }

            doc = BuildDataExtModOrDelRq(doc, parent, listID, "Site Name", site.SiteName);
            doc = BuildDataExtModOrDelRq(doc, parent, listID, "Site Unit Number", site.UnitNumber);
            doc = BuildDataExtModOrDelRq(doc, parent, listID, "Site County", site.County);
            doc = BuildDataExtModOrDelRq(doc, parent, listID, "Site City/State", site.CityState);
            doc = BuildDataExtModOrDelRq(doc, parent, listID, "Site POAFE", site.POAFENumber);
            doc = BuildDataExtModOrDelRq(doc, parent, listID, "Work Order Sales Rep", salesRep);

            switch (wo.CloseStatus)
            {
                case CloseStatus.BillAtCost:
                    doc = BuildDataExtModOrDelRq(doc, parent, listID, "Close Status", "Bill at Cost");
                    break;

                case CloseStatus.BillPerQuote:
                    doc = BuildDataExtModOrDelRq(doc, parent, listID, "Close Status", "Bill per Quote");
                    break;

                case CloseStatus.ClosedAtZeroDollars:
                    doc = BuildDataExtModOrDelRq(doc, parent, listID, "Close Status", "Closed at Zero $");
                    break;

                case CloseStatus.NotApplicable:
                    doc = BuildDataExtModOrDelRq(doc, parent, listID, "Close Status", "");
                    break;

                case CloseStatus.PreClose:
                    doc = BuildDataExtModOrDelRq(doc, parent, listID, "Close Status", "Pre-close");
                    break;

                case CloseStatus.ReadyForPreClose:
                    doc = BuildDataExtModOrDelRq(doc, parent, listID, "Close Status", "Ready for Pre-close");
                    break;

                case CloseStatus.Warranty:
                    doc = BuildDataExtModOrDelRq(doc, parent, listID, "Close Status", "Warranty");
                    break;
                    
                default:
                    doc = BuildDataExtModOrDelRq(doc, parent, listID, "Close Status", "");
                    break;
            }            

            return doc;
        }

        private static XmlDocument BuildDataExtModOrDelRq(XmlDocument doc, XmlElement parent, string customerListID, string DataExtName, string DataExtValue)
        {
            string reqType = "DataExtMod";
            if ((DataExtValue == null) || (DataExtValue == ""))
            {
                reqType = "DataExtDel";
            }

            XmlElement DataExtModRq = doc.CreateElement(reqType + "Rq");
            parent.AppendChild(DataExtModRq);
            DataExtModRq.SetAttribute("requestID", "1");

            XmlElement DataExtMod = doc.CreateElement(reqType);
            DataExtModRq.AppendChild(DataExtMod);
            DataExtMod.AppendChild(XmlUtils.MakeSimpleElem(doc, "OwnerID", "0"));
            DataExtMod.AppendChild(XmlUtils.MakeSimpleElem(doc, "DataExtName", DataExtName));
            DataExtMod.AppendChild(XmlUtils.MakeSimpleElem(doc, "ListDataExtType", "Customer"));

            XmlElement ListObjRef = doc.CreateElement("ListObjRef");
            DataExtMod.AppendChild(ListObjRef);
            ListObjRef.AppendChild(XmlUtils.MakeSimpleElem(doc, "ListID", customerListID));

            if (reqType == "DataExtMod") DataExtMod.AppendChild(XmlUtils.MakeSimpleElem(doc, "DataExtValue", DataExtValue));

            return doc;
        }

        public static bool HandleResponse(string response)
        {
            return WalkDataExtModRs(response);
        }

        private static bool WalkDataExtModRs(string response)
        {
            //Parse the response XML string into an XmlDocument
            XmlDocument responseXmlDoc = new XmlDocument();
            responseXmlDoc.LoadXml(response);

            //Get the response for our request
            XmlNodeList DataExtModRsList = responseXmlDoc.GetElementsByTagName("DataExtModRs");
            if (DataExtModRsList.Count >= 5) //Should always be true since we update at least 5 elements in this request
            {
                XmlNode responseNode = DataExtModRsList.Item(0);
                //Check the status code, info, and severity
                XmlAttributeCollection rsAttributes = responseNode.Attributes;
                string statusCode = rsAttributes.GetNamedItem("statusCode").Value;
                string statusSeverity = rsAttributes.GetNamedItem("statusSeverity").Value;
                string statusMessage = rsAttributes.GetNamedItem("statusMessage").Value;

                // Check status and log any errors
                QBUtils.CheckStatus(statusCode, statusSeverity, statusMessage);

                //status code = 0 all OK, > 0 is warning
                if (Convert.ToInt32(statusCode) == 0)
                {
                    XmlNodeList DataExtRetList = responseNode.SelectNodes("//DataExtRet");//XPath Query
                    for (int i = 0; i < DataExtRetList.Count; i++)
                    {
                        XmlNode DataExtRet = DataExtRetList.Item(i);
                        WalkDataExtRet(DataExtRet);
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private static void WalkDataExtRet(XmlNode DataExtRet)
        {
            if (DataExtRet == null) return;

            //Go through all the elements of DataExtRet
            //Get value of OwnerID
            if (DataExtRet.SelectSingleNode("./OwnerID") != null)
            {
                string OwnerID = DataExtRet.SelectSingleNode("./OwnerID").InnerText;

            }
            //Get value of DataExtName
            string DataExtName = DataExtRet.SelectSingleNode("./DataExtName").InnerText;
            //Get value of DataExtType
            string DataExtType = DataExtRet.SelectSingleNode("./DataExtType").InnerText;
            //Get value of DataExtValue
            string DataExtValue = DataExtRet.SelectSingleNode("./DataExtValue").InnerText;

        }

    }
}
