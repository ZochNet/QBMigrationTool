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
    public class ClassDAL
    {
        public static XmlDocument BuildQueryRequest(string activeStatus, string fromModifiedDate, string toModifiedDate)
        {
            XmlDocument doc = XmlUtils.MakeRequestDocument();
            XmlElement parent = XmlUtils.MakeRequestParentElement(doc);
            XmlElement queryElement = doc.CreateElement("ClassQueryRq");
            parent.AppendChild(queryElement);

            queryElement.AppendChild(XmlUtils.MakeSimpleElem(doc, "ActiveStatus", activeStatus));
            queryElement.AppendChild(XmlUtils.MakeSimpleElem(doc, "FromModifiedDate", fromModifiedDate));
            queryElement.AppendChild(XmlUtils.MakeSimpleElem(doc, "ToModifiedDate", toModifiedDate));            

            return doc;
        }

        public static void HandleResponse(string response)
        {
            WalkClassQueryRs(response);
        }

        private static void WalkClassQueryRs(string response)
        {
            //Parse the response XML string into an XmlDocument
            XmlDocument responseXmlDoc = new XmlDocument();
            responseXmlDoc.LoadXml(response);

            //Get the response for our request
            XmlNodeList ClassQueryRsList = responseXmlDoc.GetElementsByTagName("ClassQueryRs");
            if (ClassQueryRsList.Count == 1) //Should always be true since we only did one request in this sample
            {
                XmlNode responseNode = ClassQueryRsList.Item(0);
                //Check the status code, info, and severity
                XmlAttributeCollection rsAttributes = responseNode.Attributes;
                string statusCode = rsAttributes.GetNamedItem("statusCode").Value;
                string statusSeverity = rsAttributes.GetNamedItem("statusSeverity").Value;
                string statusMessage = rsAttributes.GetNamedItem("statusMessage").Value;

                //status code = 0 all OK, > 0 is warning
                if (Convert.ToInt32(statusCode) >= 0)
                {
                    XmlNodeList ClassRetList = responseNode.SelectNodes("//ClassRet");//XPath Query
                    for (int i = 0; i < ClassRetList.Count; i++)
                    {
                        XmlNode ClassRet = ClassRetList.Item(i);
                        WalkClassRet(ClassRet);
                    }
                }
            }
        }

        private static void WalkClassRet(XmlNode ClassRet)
        {
            if (ClassRet == null) return;

            Area area = null;
            RotoTrackDb db = new RotoTrackDb();

            string ListID = ClassRet.SelectSingleNode("./ListID").InnerText;
            if (db.Areas.Any(a => a.QBListId == ListID))
            {
                area = db.Areas.First(a => a.QBListId == ListID);
            }
            else
            {
                area = new Area();
                db.Areas.Add(area);
            }
            area.QBListId = ListID;

            string Name = ClassRet.SelectSingleNode("./Name").InnerText;
            area.Name = Name;

            string IsActive = "false";
            if (ClassRet.SelectSingleNode("./IsActive") != null)
            {
                IsActive = ClassRet.SelectSingleNode("./IsActive").InnerText;
            }
            area.IsActive = (IsActive == "true") ? true : false;

            if (area != null)
            {
                db.SaveChanges();
            }
        }
    }
}
