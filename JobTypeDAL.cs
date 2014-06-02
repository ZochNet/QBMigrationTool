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
    class JobTypeDAL
    {
        public static XmlDocument BuildQueryRequest(string activeStatus, string fromModifiedDate, string toModifiedDate)
        {
            XmlDocument doc = XmlUtils.MakeRequestDocument();
            XmlElement parent = XmlUtils.MakeRequestParentElement(doc);
            XmlElement queryElement = doc.CreateElement("JobTypeQueryRq");
            parent.AppendChild(queryElement);

            queryElement.AppendChild(XmlUtils.MakeSimpleElem(doc, "ActiveStatus", activeStatus));
            queryElement.AppendChild(XmlUtils.MakeSimpleElem(doc, "FromModifiedDate", fromModifiedDate));
            queryElement.AppendChild(XmlUtils.MakeSimpleElem(doc, "ToModifiedDate", toModifiedDate));            

            return doc;
        }

        public static void HandleResponse(string response)
        {
            WalkJobTypeQueryRs(response);
        }

        private static void WalkJobTypeQueryRs(string response)
        {
            //Parse the response XML string into an XmlDocument
            XmlDocument responseXmlDoc = new XmlDocument();
            responseXmlDoc.LoadXml(response);

            //Get the response for our request
            XmlNodeList JobTypeQueryRsList = responseXmlDoc.GetElementsByTagName("JobTypeQueryRs");
            if (JobTypeQueryRsList.Count == 1) //Should always be true since we only did one request in this sample
            {
                XmlNode responseNode = JobTypeQueryRsList.Item(0);
                //Check the status code, info, and severity
                XmlAttributeCollection rsAttributes = responseNode.Attributes;
                string statusCode = rsAttributes.GetNamedItem("statusCode").Value;
                string statusSeverity = rsAttributes.GetNamedItem("statusSeverity").Value;
                string statusMessage = rsAttributes.GetNamedItem("statusMessage").Value;

                //status code = 0 all OK, > 0 is warning
                if (Convert.ToInt32(statusCode) >= 0)
                {
                    XmlNodeList JobTypeRetList = responseNode.SelectNodes("//JobTypeRet");//XPath Query
                    for (int i = 0; i < JobTypeRetList.Count; i++)
                    {
                        XmlNode JobTypeRet = JobTypeRetList.Item(i);
                        WalkJobTypeRet(JobTypeRet);
                    }
                }
            }
        }

        private static void WalkJobTypeRet(XmlNode JobTypeRet)
        {
            if (JobTypeRet == null) return;

            JobType jt = null;
            RotoTrackDb db = new RotoTrackDb();

            string ListID = JobTypeRet.SelectSingleNode("./ListID").InnerText;
            if (db.JobTypes.Any(f => f.QBListId == ListID))
            {
                jt = db.JobTypes.First(f => f.QBListId == ListID);
            }
            else
            {
                jt = new JobType();
                db.JobTypes.Add(jt);
            }
            jt.QBListId = ListID;

            string Name = JobTypeRet.SelectSingleNode("./Name").InnerText;
            jt.Name = Name;

            string IsActive = "false";
            if (JobTypeRet.SelectSingleNode("./IsActive") != null)
            {
                IsActive = JobTypeRet.SelectSingleNode("./IsActive").InnerText;
            }
            jt.IsActive = (IsActive == "true") ? true : false;

            if (jt != null)
            {
                db.SaveChanges();
            }
        }
    }
}
