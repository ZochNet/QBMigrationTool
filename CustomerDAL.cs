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
    public class CustomerDAL
    {
        public static XmlDocument BuildCustomerQueryByWorkorderNum(string woNum)
        {
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", null, null));
            doc.AppendChild(doc.CreateProcessingInstruction("qbxml", "version=\"12.0\""));
            XmlElement qbXML = doc.CreateElement("QBXML");
            doc.AppendChild(qbXML);
            XmlElement qbXMLMsgsRq = doc.CreateElement("QBXMLMsgsRq");
            qbXML.AppendChild(qbXMLMsgsRq);
            qbXMLMsgsRq.SetAttribute("onError", "stopOnError");
            XmlElement custAddRq = doc.CreateElement("CustomerQueryRq");
            qbXMLMsgsRq.AppendChild(custAddRq);
            custAddRq.SetAttribute("requestID", "1");

            XmlElement NameFilter = doc.CreateElement("NameFilter");
            custAddRq.AppendChild(NameFilter);
            NameFilter.AppendChild(XmlUtils.MakeSimpleElem(doc, "MatchCriterion", "Contains"));
            NameFilter.AppendChild(XmlUtils.MakeSimpleElem(doc, "Name", woNum));
            custAddRq.AppendChild(XmlUtils.MakeSimpleElem(doc, "OwnerID", "0"));

            return doc;
        } 

        public static string GetQBIDFromResponse(string response)
        {
            string qbid = "";

            //Parse the response XML string into an XmlDocument
            XmlDocument responseXmlDoc = new XmlDocument();
            responseXmlDoc.LoadXml(response);

            //Get the response for our request
            XmlNodeList CustomerQueryRsList = responseXmlDoc.GetElementsByTagName("CustomerQueryRs");
            if (CustomerQueryRsList.Count == 1) //Should always be true since we only did one request in this sample
            {
                XmlNode responseNode = CustomerQueryRsList.Item(0);
                //Check the status code, info, and severity
                XmlAttributeCollection rsAttributes = responseNode.Attributes;
                string statusCode = rsAttributes.GetNamedItem("statusCode").Value;
                string statusSeverity = rsAttributes.GetNamedItem("statusSeverity").Value;
                string statusMessage = rsAttributes.GetNamedItem("statusMessage").Value;

                //status code = 0 all OK, > 0 is warning
                if (Convert.ToInt32(statusCode) >= 0)
                {
                    XmlNodeList CustomerRetList = responseNode.SelectNodes("//CustomerRet");//XPath Query
                    if (CustomerRetList.Count == 1)
                    {
                        XmlNode CustomerRet = CustomerRetList.Item(0);
                        qbid = GetQBIDFromCustomerRet(CustomerRet);
                    }
                }
            }

            return qbid;
        }

        public static string GetQBEditSequenceFromResponse(string response)
        {
            string qbes = "";

            //Parse the response XML string into an XmlDocument
            XmlDocument responseXmlDoc = new XmlDocument();
            responseXmlDoc.LoadXml(response);

            //Get the response for our request
            XmlNodeList CustomerQueryRsList = responseXmlDoc.GetElementsByTagName("CustomerQueryRs");
            if (CustomerQueryRsList.Count == 1) //Should always be true since we only did one request in this sample
            {
                XmlNode responseNode = CustomerQueryRsList.Item(0);
                //Check the status code, info, and severity
                XmlAttributeCollection rsAttributes = responseNode.Attributes;
                string statusCode = rsAttributes.GetNamedItem("statusCode").Value;
                string statusSeverity = rsAttributes.GetNamedItem("statusSeverity").Value;
                string statusMessage = rsAttributes.GetNamedItem("statusMessage").Value;

                //status code = 0 all OK, > 0 is warning
                if (Convert.ToInt32(statusCode) >= 0)
                {
                    XmlNodeList CustomerRetList = responseNode.SelectNodes("//CustomerRet");//XPath Query
                    if (CustomerRetList.Count == 1)
                    {
                        XmlNode CustomerRet = CustomerRetList.Item(0);
                        qbes = GetQBEditSequenceFromCustomerRet(CustomerRet);
                    }
                }
            }

            return qbes;
        }

        private static string GetQBIDFromCustomerRet(XmlNode CustomerRet)
        {
            string ListID = CustomerRet.SelectSingleNode("./ListID").InnerText;
            return ListID;
        }

        private static string GetQBEditSequenceFromCustomerRet(XmlNode CustomerRet)
        {
            string ListID = CustomerRet.SelectSingleNode("./EditSequence").InnerText;
            return ListID;
        }   
    }
}
