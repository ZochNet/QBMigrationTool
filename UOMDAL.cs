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
    public class UOMDAL
    {
        public static XmlDocument BuildQueryRequest(string activeStatus, string fromModifiedDate, string toModifiedDate)
        {
            XmlDocument doc = XmlUtils.MakeRequestDocument();
            XmlElement parent = XmlUtils.MakeRequestParentElement(doc);
            XmlElement queryElement = doc.CreateElement("UnitOfMeasureSetQueryRq");
            parent.AppendChild(queryElement);

            queryElement.AppendChild(XmlUtils.MakeSimpleElem(doc, "ActiveStatus", activeStatus));
            queryElement.AppendChild(XmlUtils.MakeSimpleElem(doc, "FromModifiedDate", fromModifiedDate));
            queryElement.AppendChild(XmlUtils.MakeSimpleElem(doc, "ToModifiedDate", toModifiedDate));            

            return doc;
        }

        public static void HandleResponse(string response)
        {
            WalkUnitOfMeasureSetQueryRs(response);
        }

        private static void WalkUnitOfMeasureSetQueryRs(string response)
        {
            //Parse the response XML string into an XmlDocument
            XmlDocument responseXmlDoc = new XmlDocument();
            responseXmlDoc.LoadXml(response);

            //Get the response for our request
            XmlNodeList UnitOfMeasureSetQueryRsList = responseXmlDoc.GetElementsByTagName("UnitOfMeasureSetQueryRs");
            if (UnitOfMeasureSetQueryRsList.Count == 1) //Should always be true since we only did one request in this sample
            {
                XmlNode responseNode = UnitOfMeasureSetQueryRsList.Item(0);
                //Check the status code, info, and severity
                XmlAttributeCollection rsAttributes = responseNode.Attributes;
                string statusCode = rsAttributes.GetNamedItem("statusCode").Value;
                string statusSeverity = rsAttributes.GetNamedItem("statusSeverity").Value;
                string statusMessage = rsAttributes.GetNamedItem("statusMessage").Value;

                //status code = 0 all OK, > 0 is warning
                if (Convert.ToInt32(statusCode) >= 0)
                {
                    XmlNodeList UnitOfMeasureSetRetList = responseNode.SelectNodes("//UnitOfMeasureSetRet");//XPath Query
                    for (int i = 0; i < UnitOfMeasureSetRetList.Count; i++)
                    {
                        XmlNode UnitOfMeasureSetRet = UnitOfMeasureSetRetList.Item(i);
                        WalkUnitOfMeasureSetRet(UnitOfMeasureSetRet);
                    }
                }
            }
        }

        private static void WalkUnitOfMeasureSetRet(XmlNode UnitOfMeasureSetRet)
        {
            if (UnitOfMeasureSetRet == null) return;

            UnitOfMeasure uom = null;
            RotoTrackDb db = new RotoTrackDb();

            string ListID = UnitOfMeasureSetRet.SelectSingleNode("./ListID").InnerText;
            if (db.UnitOfMeasures.Any(f => f.QBListId == ListID))
            {
                uom = db.UnitOfMeasures.First(f => f.QBListId == ListID);
            }
            else
            {
                uom = new UnitOfMeasure();
                db.UnitOfMeasures.Add(uom);
            }
            uom.QBListId = ListID;

            string IsActive = "false";
            if (UnitOfMeasureSetRet.SelectSingleNode("./IsActive") != null)
            {
                IsActive = UnitOfMeasureSetRet.SelectSingleNode("./IsActive").InnerText;
            }
            uom.IsActive = (IsActive == "true") ? true : false;

            /*
            //Go through all the elements of UnitOfMeasureSetRet
            //Get value of ListID
            if (UnitOfMeasureSetRet.SelectSingleNode("./ListID") != null)
            {
                string ListID = UnitOfMeasureSetRet.SelectSingleNode("./ListID").InnerText;
            }
            //Get value of TimeCreated
            if (UnitOfMeasureSetRet.SelectSingleNode("./TimeCreated") != null)
            {
                string TimeCreated = UnitOfMeasureSetRet.SelectSingleNode("./TimeCreated").InnerText;
            }
            //Get value of TimeModified
            if (UnitOfMeasureSetRet.SelectSingleNode("./TimeModified") != null)
            {
                string TimeModified = UnitOfMeasureSetRet.SelectSingleNode("./TimeModified").InnerText;
            }
            //Get value of EditSequence
            if (UnitOfMeasureSetRet.SelectSingleNode("./EditSequence") != null)
            {
                string EditSequence = UnitOfMeasureSetRet.SelectSingleNode("./EditSequence").InnerText;
            }
            //Get value of Name
            if (UnitOfMeasureSetRet.SelectSingleNode("./Name") != null)
            {
                string Name = UnitOfMeasureSetRet.SelectSingleNode("./Name").InnerText;
            }
            //Get value of IsActive
            if (UnitOfMeasureSetRet.SelectSingleNode("./IsActive") != null)
            {
                string IsActive = UnitOfMeasureSetRet.SelectSingleNode("./IsActive").InnerText;
            }
            //Get value of UnitOfMeasureType
            if (UnitOfMeasureSetRet.SelectSingleNode("./UnitOfMeasureType") != null)
            {
                string UnitOfMeasureType = UnitOfMeasureSetRet.SelectSingleNode("./UnitOfMeasureType").InnerText;
            }
            */
            //Get all field values for BaseUnit aggregate 
            XmlNode BaseUnit = UnitOfMeasureSetRet.SelectSingleNode("./BaseUnit");
            if (BaseUnit != null)
            {
                //Get value of Name
                string Name = UnitOfMeasureSetRet.SelectSingleNode("./BaseUnit/Name").InnerText;
                //Get value of Abbreviation
                string Abbreviation = UnitOfMeasureSetRet.SelectSingleNode("./BaseUnit/Abbreviation").InnerText;
                uom.Name = Name + " (" + Abbreviation + ")";
            }
            //Done with field values for BaseUnit aggregate

            /*
            //Walk list of RelatedUnit aggregates
            XmlNodeList RelatedUnitList = UnitOfMeasureSetRet.SelectNodes("./RelatedUnit");
            if (RelatedUnitList != null)
            {
                for (int i = 0; i < RelatedUnitList.Count; i++)
                {
                    XmlNode RelatedUnit = RelatedUnitList.Item(i);
                    //Get value of Name
                    string Name = RelatedUnit.SelectSingleNode("./Name").InnerText;
                    //Get value of Abbreviation
                    string Abbreviation = RelatedUnit.SelectSingleNode("./Abbreviation").InnerText;
                    //Get value of ConversionRatio
                    string ConversionRatio = RelatedUnit.SelectSingleNode("./ConversionRatio").InnerText;
                }
            }

            //Walk list of DefaultUnit aggregates
            XmlNodeList DefaultUnitList = UnitOfMeasureSetRet.SelectNodes("./DefaultUnit");
            if (DefaultUnitList != null)
            {
                for (int i = 0; i < DefaultUnitList.Count; i++)
                {
                    XmlNode DefaultUnit = DefaultUnitList.Item(i);
                    //Get value of UnitUsedFor
                    string UnitUsedFor = DefaultUnit.SelectSingleNode("./UnitUsedFor").InnerText;
                    //Get value of Unit
                    string Unit = DefaultUnit.SelectSingleNode("./Unit").InnerText;
                }
            }
            */

            if (uom != null)
            {
                db.SaveChanges();
            }
        }

        /*
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
        */

    }
}
