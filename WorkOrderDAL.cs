using rototrack_data_access;
using rototrack_model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace QBMigrationTool
{
    public class WorkOrderDAL
    {
        public static XmlDocument BuildAddRq(WorkOrder wo)
        {
            return BuildAddOrModRq(wo);
        }

        public static XmlDocument BuildModRq(WorkOrder wo)
        {
            return BuildAddOrModRq(wo);
        }

        public static XmlDocument BuildQueryRequestForUpdate(string listID, string ownerID)
        {
            XmlDocument doc = XmlUtils.MakeRequestDocument();
            XmlElement parent = XmlUtils.MakeRequestParentElement(doc);
            XmlElement queryElement = doc.CreateElement("CustomerQueryRq");
            parent.AppendChild(queryElement);

            queryElement.AppendChild(XmlUtils.MakeSimpleElem(doc, "ListID", listID));
            queryElement.AppendChild(XmlUtils.MakeSimpleElem(doc, "OwnerID", ownerID));

            return doc;
        }

        private static XmlDocument BuildAddOrModRq(WorkOrder wo)
        {
            RotoTrackDb db = new RotoTrackDb();
            XmlDocument doc = XmlUtils.MakeRequestDocument();
            XmlElement parent = XmlUtils.MakeRequestParentElement(doc);

            BillingInstruction bi = db.BillingInstructions.Find(wo.BillingInstructionsId);
            Area area = db.Areas.Find(wo.AreaId);
            Customer customer = db.Customers.Find(wo.CustomerId);
            WorkOrderType wot = db.WorkOrderTypes.Find(wo.WorkOrderTypeId);
            Contact contact = db.Contacts.Find(wo.ContactId);
            Site site = db.Sites.Find(wo.SiteId);
            JobType jt = db.JobTypes.Find(wo.JobTypeId);

            XmlElement CustomerAddOrModRq = null;
            XmlElement CustomerAddOrMod = null;

            if (wo.QBListId == null)
            {
                CustomerAddOrModRq = doc.CreateElement("CustomerAddRq");
                parent.AppendChild(CustomerAddOrModRq);
                CustomerAddOrMod = doc.CreateElement("CustomerAdd");
                CustomerAddOrModRq.AppendChild(CustomerAddOrMod);
            }
            else
            {
                CustomerAddOrModRq = doc.CreateElement("CustomerModRq");
                parent.AppendChild(CustomerAddOrModRq);
                CustomerAddOrMod = doc.CreateElement("CustomerMod");
                CustomerAddOrModRq.AppendChild(CustomerAddOrMod);

                CustomerAddOrMod.AppendChild(XmlUtils.MakeSimpleElem(doc, "ListID", wo.QBListId));
                CustomerAddOrMod.AppendChild(XmlUtils.MakeSimpleElem(doc, "EditSequence", wo.QBEditSequence));
            }

            CustomerAddOrMod.AppendChild(XmlUtils.MakeSimpleElem(doc, "Name", wo.WorkOrderNumber));

            string isActive = (wo.statusValue != (int)WorkOrderStatus.Inactive) ? "1" : "0";            
            CustomerAddOrMod.AppendChild(XmlUtils.MakeSimpleElem(doc, "IsActive", isActive));

            XmlElement ParentRef = doc.CreateElement("ParentRef");
            CustomerAddOrMod.AppendChild(ParentRef);
            ParentRef.AppendChild(XmlUtils.MakeSimpleElem(doc, "ListID", customer.QBListId));

            CustomerAddOrMod.AppendChild(XmlUtils.MakeSimpleElem(doc, "CompanyName", customer.CompanyName));

            if (contact != null)
            {
                string[] namePieces = contact.Name.Split(' ');
                string firstName;
                string lastName;
                string middleName;
                if (namePieces.Count() == 2)
                {
                    firstName = namePieces[0];
                    lastName = namePieces[1];

                    if (firstName.Length > 15) firstName = firstName.Substring(0, 15);
                    if (lastName.Length > 18) lastName = lastName.Substring(0, 18);
                    contact.Name = firstName + " " + lastName;

                    CustomerAddOrMod.AppendChild(XmlUtils.MakeSimpleElem(doc, "FirstName", firstName));
                    CustomerAddOrMod.AppendChild(XmlUtils.MakeSimpleElem(doc, "LastName", lastName));
                }
                else if (namePieces.Count() == 3)
                {
                    firstName = namePieces[0];
                    middleName = namePieces[1];
                    lastName = namePieces[2];

                    if (firstName.Length > 15) firstName = firstName.Substring(0, 15);
                    if (middleName.Length > 5) middleName = middleName.Substring(0, 5);
                    if (lastName.Length > 18) lastName = lastName.Substring(0, 18);
                    contact.Name = firstName + " " + middleName + " " + lastName;

                    CustomerAddOrMod.AppendChild(XmlUtils.MakeSimpleElem(doc, "FirstName", firstName));
                    CustomerAddOrMod.AppendChild(XmlUtils.MakeSimpleElem(doc, "MiddleName", middleName));
                    CustomerAddOrMod.AppendChild(XmlUtils.MakeSimpleElem(doc, "LastName", lastName));
                }
                else if (namePieces.Count() == 1)
                {
                    firstName = namePieces[0];

                    if (firstName.Length > 15) firstName = firstName.Substring(0, 15);
                    contact.Name = firstName;

                    CustomerAddOrMod.AppendChild(XmlUtils.MakeSimpleElem(doc, "FirstName", firstName));
                }
            }

            XmlElement BillAddress = doc.CreateElement("BillAddress");
            CustomerAddOrMod.AppendChild(BillAddress);

            BillAddress.AppendChild(XmlUtils.MakeSimpleElem(doc, "Addr1", customer.Address.Address1));
            BillAddress.AppendChild(XmlUtils.MakeSimpleElem(doc, "Addr2", customer.Address.Address2));
            BillAddress.AppendChild(XmlUtils.MakeSimpleElem(doc, "Addr3", customer.Address.Address3));
            BillAddress.AppendChild(XmlUtils.MakeSimpleElem(doc, "Addr4", customer.Address.Address4));
            BillAddress.AppendChild(XmlUtils.MakeSimpleElem(doc, "Addr5", customer.Address.Address5));
            BillAddress.AppendChild(XmlUtils.MakeSimpleElem(doc, "City", customer.Address.City));
            BillAddress.AppendChild(XmlUtils.MakeSimpleElem(doc, "State", customer.Address.State));
            BillAddress.AppendChild(XmlUtils.MakeSimpleElem(doc, "PostalCode", customer.Address.Zip));

            if (contact != null)
            {
                if (contact.Name.Length > 40) contact.Name = contact.Name.Substring(0, 40);
                CustomerAddOrMod.AppendChild(XmlUtils.MakeSimpleElem(doc, "Contact", contact.Name));

                if (contact.Phone != null && contact.Phone != "")
                {
                    XmlElement AdditionalContactRef1 = doc.CreateElement("AdditionalContactRef");
                    CustomerAddOrMod.AppendChild(AdditionalContactRef1);
                    AdditionalContactRef1.AppendChild(XmlUtils.MakeSimpleElem(doc, "ContactName", "Main Phone"));
                    AdditionalContactRef1.AppendChild(XmlUtils.MakeSimpleElem(doc, "ContactValue", contact.Phone));
                }

                if (contact.Email != null && contact.Email != "")
                {
                    XmlElement AdditionalContactRef2 = doc.CreateElement("AdditionalContactRef");
                    CustomerAddOrMod.AppendChild(AdditionalContactRef2);
                    AdditionalContactRef2.AppendChild(XmlUtils.MakeSimpleElem(doc, "ContactName", "Main Email"));
                    AdditionalContactRef2.AppendChild(XmlUtils.MakeSimpleElem(doc, "ContactValue", contact.Email));
                }
            }

            XmlElement CustomerTypeRef = doc.CreateElement("CustomerTypeRef");
            CustomerAddOrMod.AppendChild(CustomerTypeRef);
            CustomerTypeRef.AppendChild(XmlUtils.MakeSimpleElem(doc, "ListID", bi.QBListId));

            switch (wo.JobStatus)
            {
                case JobStatus.Awarded:
                    CustomerAddOrMod.AppendChild(XmlUtils.MakeSimpleElem(doc, "JobStatus", "Awarded"));
                    break;

                case JobStatus.Complete:
                    CustomerAddOrMod.AppendChild(XmlUtils.MakeSimpleElem(doc, "JobStatus", "Closed"));
                    break;

                case JobStatus.InProgress:
                    CustomerAddOrMod.AppendChild(XmlUtils.MakeSimpleElem(doc, "JobStatus", "InProgress"));
                    break;

                case JobStatus.NotApplicable:
                    CustomerAddOrMod.AppendChild(XmlUtils.MakeSimpleElem(doc, "JobStatus", "None"));
                    break;

                case JobStatus.NotAwarded:
                    CustomerAddOrMod.AppendChild(XmlUtils.MakeSimpleElem(doc, "JobStatus", "NotAwarded"));
                    break;

                case JobStatus.Pending:
                    CustomerAddOrMod.AppendChild(XmlUtils.MakeSimpleElem(doc, "JobStatus", "Pending"));
                    break;

                default:
                    break;
            }

            CustomerAddOrMod.AppendChild(XmlUtils.MakeSimpleElem(doc, "JobStartDate", wo.EstStartDate.ToString("yyyy-MM-dd")));
            CustomerAddOrMod.AppendChild(XmlUtils.MakeSimpleElem(doc, "JobProjectedEndDate", wo.EstEndDate.ToString("yyyy-MM-dd")));
            if (wo.ActualEndDate != DateTime.Parse("1900-01-01 00:00:00.000"))
            {
                CustomerAddOrMod.AppendChild(XmlUtils.MakeSimpleElem(doc, "JobEndDate", wo.ActualEndDate.ToString("yyyy-MM-dd")));
            }

            if (wo.JobDescription != null)
            {
                CustomerAddOrMod.AppendChild(XmlUtils.MakeSimpleElem(doc, "JobDesc", wo.JobDescription));
            }
            if (jt != null)
            {
                XmlElement JobTypeRef = doc.CreateElement("JobTypeRef");
                CustomerAddOrMod.AppendChild(JobTypeRef);
                JobTypeRef.AppendChild(XmlUtils.MakeSimpleElem(doc, "ListID", jt.QBListId));
            }
            if (wo.BillingNotes != null)
            {
                CustomerAddOrMod.AppendChild(XmlUtils.MakeSimpleElem(doc, "Notes", wo.BillingNotes));
            }

            return doc;
        }

        public static void HandleAddResponse(string response)
        {
            WalkCustomerAddRs(response);
        }

        public static bool HandleModResponse(string response)
        {
            return WalkCustomerModRs(response);
        }

        public static void HandleResponseForUpdate(string response)
        {
            WalkCustomerQueryRs(response);
        }
        
        private static void WalkCustomerAddRs(string response)
        {
            //Parse the response XML string into an XmlDocument
            XmlDocument responseXmlDoc = new XmlDocument();
            responseXmlDoc.LoadXml(response);

            //Get the response for our request            
            XmlNodeList CustomerAddRsList = responseXmlDoc.GetElementsByTagName("CustomerAddRs");
            if (CustomerAddRsList.Count == 1) //Should always be true since we only did one request in this sample
            {
                XmlNode responseNode = CustomerAddRsList.Item(0);
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
                    XmlNodeList CustomerRetList = responseNode.SelectNodes("//CustomerRet");//XPath Query
                    for (int i = 0; i < CustomerRetList.Count; i++)
                    {
                        XmlNode CustomerRet = CustomerRetList.Item(i);
                        WalkCustomerRetForAdd(CustomerRet);
                    }
                }
            }
        }

        private static void WalkCustomerRetForAdd(XmlNode CustomerRet)
        {
            if (CustomerRet == null) return;

            RotoTrackDb db = new RotoTrackDb();

            string ListID = CustomerRet.SelectSingleNode("./ListID").InnerText;
            string EditSequence = CustomerRet.SelectSingleNode("./EditSequence").InnerText;
            string Name = CustomerRet.SelectSingleNode("./Name").InnerText;

            if (db.WorkOrders.Any(f => f.WorkOrderNumber == Name))
            {
                WorkOrder wo = db.WorkOrders.First(f => f.WorkOrderNumber == Name);
                wo.QBListId = ListID;
                wo.QBEditSequence = EditSequence;
                db.Entry(wo).State = EntityState.Modified;
                if (wo != null)
                {
                    db.SaveChanges();
                }                
            }
        }

        private static bool WalkCustomerModRs(string response)
        {
            //Parse the response XML string into an XmlDocument
            XmlDocument responseXmlDoc = new XmlDocument();
            responseXmlDoc.LoadXml(response);

            //Get the response for our request
            XmlNodeList CustomerModRsList = responseXmlDoc.GetElementsByTagName("CustomerModRs");
            if (CustomerModRsList.Count == 1) //Should always be true since we only did one request in this sample
            {
                XmlNode responseNode = CustomerModRsList.Item(0);
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
                    XmlNodeList CustomerRetList = responseNode.SelectNodes("//CustomerRet");//XPath Query
                    for (int i = 0; i < CustomerRetList.Count; i++)
                    {
                        XmlNode CustomerRet = CustomerRetList.Item(i);
                        WalkCustomerRetForMod(CustomerRet);
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

        private static void WalkCustomerRetForMod(XmlNode CustomerRet)
        {
            if (CustomerRet == null) return;

            RotoTrackDb db = new RotoTrackDb();

            string ListID = CustomerRet.SelectSingleNode("./ListID").InnerText;
            WorkOrder wo = null;

            if (db.WorkOrders.Any(f => f.QBListId == ListID))
            {
                wo = db.WorkOrders.First(f => f.QBListId == ListID);
                wo.NeedToUpdateQB = false;
                db.SaveChanges();
            }
        }

        private static void WalkCustomerQueryRs(string response)
        {
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

                // Check status and log any errors
                QBUtils.CheckStatus(statusCode, statusSeverity, statusMessage);

                //status code = 0 all OK, > 0 is warning
                if (Convert.ToInt32(statusCode) >= 0)
                {
                    XmlNodeList CustomerRetList = responseNode.SelectNodes("//CustomerRet");//XPath Query
                    for (int i = 0; i < CustomerRetList.Count; i++)
                    {
                        XmlNode CustomerRet = CustomerRetList.Item(i);
                        WalkCustomerRetForUpdate(CustomerRet);
                    }
                }
            }
        }

        private static void WalkCustomerRetForUpdate(XmlNode CustomerRet)
        {
            if (CustomerRet == null) return;

            RotoTrackDb db = new RotoTrackDb();

            string ListID = CustomerRet.SelectSingleNode("./ListID").InnerText;
            string EditSequence = CustomerRet.SelectSingleNode("./EditSequence").InnerText;

            if (db.WorkOrders.Any(f => f.QBListId == ListID))
            {
                WorkOrder wo = db.WorkOrders.First(f => f.QBListId == ListID);
                wo.QBEditSequence = EditSequence;
                db.Entry(wo).State = EntityState.Modified;
                if (wo != null)
                {
                    db.SaveChanges();
                }
            }
        }

    }
}
