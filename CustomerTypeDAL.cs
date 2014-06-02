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
    class CustomerTypeDAL
    {
        public static XmlDocument BuildQueryRequest(string fromModifiedDate, string toModifiedDate, string activeStatus)
        {
            XmlDocument doc = XmlUtils.MakeRequestDocument();
            XmlElement parent = XmlUtils.MakeRequestParentElement(doc);
            XmlElement queryElement = doc.CreateElement("CustomerTypeQueryRq");
            parent.AppendChild(queryElement);

            queryElement.AppendChild(XmlUtils.MakeSimpleElem(doc, "FromModifiedDate", fromModifiedDate));
            queryElement.AppendChild(XmlUtils.MakeSimpleElem(doc, "ToModifiedDate", toModifiedDate));
            queryElement.AppendChild(XmlUtils.MakeSimpleElem(doc, "ActiveStatus", activeStatus));            

            return doc;
        }

        public static XmlDocument BuildAddRq(BillingInstruction bi)
        {
            XmlDocument doc = XmlUtils.MakeRequestDocument();
            XmlElement parent = XmlUtils.MakeRequestParentElement(doc);

            XmlElement CustomerTypeAddRq = doc.CreateElement("CustomerTypeAddRq");
            parent.AppendChild(CustomerTypeAddRq);

            XmlElement CustomerTypeAdd = doc.CreateElement("CustomerTypeAdd");
            CustomerTypeAddRq.AppendChild(CustomerTypeAdd);
            CustomerTypeAdd.AppendChild(XmlUtils.MakeSimpleElem(doc, "Name", bi.Name));
            CustomerTypeAdd.AppendChild(XmlUtils.MakeSimpleElem(doc, "IsActive", "1"));

            return doc;
        }

        public static void HandleResponse(string response)
        {
            WalkCustomerTypeQueryRs(response);
        }

        public static void HandleAddResponse(string response)
        {
            WalkCustomerTypeAddRs(response);
        }

        private static void WalkCustomerTypeQueryRs(string response)
        {
            //Parse the response XML string into an XmlDocument
            XmlDocument responseXmlDoc = new XmlDocument();
            responseXmlDoc.LoadXml(response);

            //Get the response for our request
            XmlNodeList CustomerTypeQueryRsList = responseXmlDoc.GetElementsByTagName("CustomerTypeQueryRs");
            if (CustomerTypeQueryRsList.Count == 1) //Should always be true since we only did one request in this sample
            {
                XmlNode responseNode = CustomerTypeQueryRsList.Item(0);
                //Check the status code, info, and severity
                XmlAttributeCollection rsAttributes = responseNode.Attributes;
                string statusCode = rsAttributes.GetNamedItem("statusCode").Value;
                string statusSeverity = rsAttributes.GetNamedItem("statusSeverity").Value;
                string statusMessage = rsAttributes.GetNamedItem("statusMessage").Value;

                //status code = 0 all OK, > 0 is warning
                if (Convert.ToInt32(statusCode) >= 0)
                {
                    XmlNodeList CustomerTypeRetList = responseNode.SelectNodes("//CustomerTypeRet");//XPath Query
                    for (int i = 0; i < CustomerTypeRetList.Count; i++)
                    {
                        XmlNode CustomerTypeRet = CustomerTypeRetList.Item(i);
                        WalkCustomerTypeRet(CustomerTypeRet);
                    }
                }
            }
        }

        private static void WalkCustomerTypeRet(XmlNode CustomerTypeRet)
        {
            if (CustomerTypeRet == null) return;

            BillingInstruction bi = null;
            RotoTrackDb db = new RotoTrackDb();

            string ListID = CustomerTypeRet.SelectSingleNode("./ListID").InnerText;
            if (db.BillingInstructions.Any(f => f.QBListId == ListID))
            {
                bi = db.BillingInstructions.First(f => f.QBListId == ListID);
            }
            else
            {
                bi = new BillingInstruction();
                db.BillingInstructions.Add(bi);
            }
            bi.QBListId = ListID;

            string Name = CustomerTypeRet.SelectSingleNode("./Name").InnerText;
            bi.Name = Name;

            string FullName = CustomerTypeRet.SelectSingleNode("./FullName").InnerText;

            string IsActive = "false";
            if (CustomerTypeRet.SelectSingleNode("./IsActive") != null)
            {
                IsActive = CustomerTypeRet.SelectSingleNode("./IsActive").InnerText;
            }
            bi.IsActive = (IsActive == "true") ? true : false;

            if (bi != null)
            {
                db.SaveChanges();
            }
        }

        private static void WalkCustomerTypeAddRs(string response)
        {
            //Parse the response XML string into an XmlDocument
            XmlDocument responseXmlDoc = new XmlDocument();
            responseXmlDoc.LoadXml(response);

            //Get the response for our request
            XmlNodeList CustomerTypeAddRsList = responseXmlDoc.GetElementsByTagName("CustomerTypeAddRs");
            if (CustomerTypeAddRsList.Count == 1) //Should always be true since we only did one request in this sample
            {
                XmlNode responseNode = CustomerTypeAddRsList.Item(0);
                //Check the status code, info, and severity
                XmlAttributeCollection rsAttributes = responseNode.Attributes;
                string statusCode = rsAttributes.GetNamedItem("statusCode").Value;
                string statusSeverity = rsAttributes.GetNamedItem("statusSeverity").Value;
                string statusMessage = rsAttributes.GetNamedItem("statusMessage").Value;

                //status code = 0 all OK, > 0 is warning
                if (Convert.ToInt32(statusCode) >= 0)
                {
                    XmlNodeList CustomerTypeRetList = responseNode.SelectNodes("//CustomerTypeRet");//XPath Query
                    for (int i = 0; i < CustomerTypeRetList.Count; i++)
                    {
                        XmlNode CustomerTypeRet = CustomerTypeRetList.Item(i);
                        WalkCustomerTypeRetForAdd(CustomerTypeRet);
                    }
                }
            }
        }

        private static void WalkCustomerTypeRetForAdd(XmlNode CustomerTypeRet)
        {
            // Update the QBListID for the newly added BillingInstruction
            if (CustomerTypeRet == null) return;

            BillingInstruction bi = null;
            RotoTrackDb db = new RotoTrackDb();

            string ListID = CustomerTypeRet.SelectSingleNode("./ListID").InnerText;
            string Name = CustomerTypeRet.SelectSingleNode("./Name").InnerText;

            if (db.BillingInstructions.Any(f => f.Name == Name))
            {
                bi = db.BillingInstructions.First(f => f.Name == Name);
            }
            bi.QBListId = ListID;

            if (bi != null)
            {
                db.SaveChanges();
            }            
        }
        
    }
}
