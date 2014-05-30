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
    public class VendorDAL
    {
        public static XmlDocument BuildQueryRequest(string fromModifiedDate, string toModifiedDate, string activeStatus)
        {
            XmlDocument doc = XmlUtils.MakeRequestDocument();
            XmlElement parent = XmlUtils.MakeRequestParentElement(doc);
            XmlElement queryElement = doc.CreateElement("VendorQueryRq");
            parent.AppendChild(queryElement);

            queryElement.AppendChild(XmlUtils.MakeSimpleElem(doc, "FromModifiedDate", fromModifiedDate));
            queryElement.AppendChild(XmlUtils.MakeSimpleElem(doc, "ToModifiedDate", toModifiedDate));
            queryElement.AppendChild(XmlUtils.MakeSimpleElem(doc, "ActiveStatus", activeStatus));

            return doc;
        }

        public static void HandleResponse(string response)
        {
            WalkVendorQueryRs(response);
        }

        private static void WalkVendorQueryRs(string response)
        {
            //Parse the response XML string into an XmlDocument
            XmlDocument responseXmlDoc = new XmlDocument();
            responseXmlDoc.LoadXml(response);

            //Get the response for our request
            XmlNodeList VendorQueryRsList = responseXmlDoc.GetElementsByTagName("VendorQueryRs");
            if (VendorQueryRsList.Count == 1) //Should always be true since we only did one request in this sample
            {
                XmlNode responseNode = VendorQueryRsList.Item(0);
                //Check the status code, info, and severity
                XmlAttributeCollection rsAttributes = responseNode.Attributes;
                string statusCode = rsAttributes.GetNamedItem("statusCode").Value;
                string statusSeverity = rsAttributes.GetNamedItem("statusSeverity").Value;
                string statusMessage = rsAttributes.GetNamedItem("statusMessage").Value;

                //status code = 0 all OK, > 0 is warning
                if (Convert.ToInt32(statusCode) >= 0)
                {
                    XmlNodeList VendorRetList = responseNode.SelectNodes("//VendorRet");//XPath Query
                    for (int i = 0; i < VendorRetList.Count; i++)
                    {
                        XmlNode VendorRet = VendorRetList.Item(i);
                        WalkVendorRet(VendorRet);
                    }
                }
            }
        }

        private static void WalkVendorRet(XmlNode VendorRet)
        {
            if (VendorRet == null) return;

            RotoTrackDb db = new RotoTrackDb();

            //Go through all the elements of VendorRet
            //Get value of ListID
            string ListID = VendorRet.SelectSingleNode("./ListID").InnerText;
            //Get value of TimeCreated
            string TimeCreated = VendorRet.SelectSingleNode("./TimeCreated").InnerText;
            //Get value of TimeModified
            string TimeModified = VendorRet.SelectSingleNode("./TimeModified").InnerText;
            //Get value of EditSequence
            string EditSequence = VendorRet.SelectSingleNode("./EditSequence").InnerText;
            //Get value of Name
            string Name = VendorRet.SelectSingleNode("./Name").InnerText;
            //Get value of IsActive
            string IsActive = "";
            if (VendorRet.SelectSingleNode("./IsActive") != null)
            {
                IsActive = VendorRet.SelectSingleNode("./IsActive").InnerText;
            }

            Vendor vendor = FindOrCreateVendor(db, ListID, EditSequence, TimeCreated, TimeModified, Name, IsActive);
            db.SaveChanges();

            /*
           //Get all field values for ClassRef aggregate 
           XmlNode ClassRef = VendorRet.SelectSingleNode("./ClassRef");
           if (ClassRef != null)
           {
               //Get value of ListID
               if (VendorRet.SelectSingleNode("./ClassRef/ListID") != null)
               {
                   string AreaListID = VendorRet.SelectSingleNode("./ClassRef/ListID").InnerText;
               }
               //Get value of FullName
               if (VendorRet.SelectSingleNode("./ClassRef/FullName") != null)
               {
                   string FullName = VendorRet.SelectSingleNode("./ClassRef/FullName").InnerText;
               }

           }
           //Done with field values for ClassRef aggregate

           //Get value of CompanyName
           if (VendorRet.SelectSingleNode("./CompanyName") != null)
           {
               string CompanyName = VendorRet.SelectSingleNode("./CompanyName").InnerText;

           }
           //Get value of Salutation
           if (VendorRet.SelectSingleNode("./Salutation") != null)
           {
               string Salutation = VendorRet.SelectSingleNode("./Salutation").InnerText;

           }
           //Get value of FirstName
           if (VendorRet.SelectSingleNode("./FirstName") != null)
           {
               string FirstName = VendorRet.SelectSingleNode("./FirstName").InnerText;

           }
           //Get value of MiddleName
           if (VendorRet.SelectSingleNode("./MiddleName") != null)
           {
               string MiddleName = VendorRet.SelectSingleNode("./MiddleName").InnerText;

           }
           //Get value of LastName
           if (VendorRet.SelectSingleNode("./LastName") != null)
           {
               string LastName = VendorRet.SelectSingleNode("./LastName").InnerText;

           }
           //Get value of JobTitle
           if (VendorRet.SelectSingleNode("./JobTitle") != null)
           {
               string JobTitle = VendorRet.SelectSingleNode("./JobTitle").InnerText;

           }
           //Get all field values for VendorAddress aggregate 
           XmlNode VendorAddress = VendorRet.SelectSingleNode("./VendorAddress");
           if (VendorAddress != null)
           {
               //Get value of Addr1
               if (VendorRet.SelectSingleNode("./VendorAddress/Addr1") != null)
               {
                   string Addr1 = VendorRet.SelectSingleNode("./VendorAddress/Addr1").InnerText;

               }
               //Get value of Addr2
               if (VendorRet.SelectSingleNode("./VendorAddress/Addr2") != null)
               {
                   string Addr2 = VendorRet.SelectSingleNode("./VendorAddress/Addr2").InnerText;

               }
               //Get value of Addr3
               if (VendorRet.SelectSingleNode("./VendorAddress/Addr3") != null)
               {
                   string Addr3 = VendorRet.SelectSingleNode("./VendorAddress/Addr3").InnerText;

               }
               //Get value of Addr4
               if (VendorRet.SelectSingleNode("./VendorAddress/Addr4") != null)
               {
                   string Addr4 = VendorRet.SelectSingleNode("./VendorAddress/Addr4").InnerText;

               }
               //Get value of Addr5
               if (VendorRet.SelectSingleNode("./VendorAddress/Addr5") != null)
               {
                   string Addr5 = VendorRet.SelectSingleNode("./VendorAddress/Addr5").InnerText;

               }
               //Get value of City
               if (VendorRet.SelectSingleNode("./VendorAddress/City") != null)
               {
                   string City = VendorRet.SelectSingleNode("./VendorAddress/City").InnerText;

               }
               //Get value of State
               if (VendorRet.SelectSingleNode("./VendorAddress/State") != null)
               {
                   string State = VendorRet.SelectSingleNode("./VendorAddress/State").InnerText;

               }
               //Get value of PostalCode
               if (VendorRet.SelectSingleNode("./VendorAddress/PostalCode") != null)
               {
                   string PostalCode = VendorRet.SelectSingleNode("./VendorAddress/PostalCode").InnerText;

               }
               //Get value of Country
               if (VendorRet.SelectSingleNode("./VendorAddress/Country") != null)
               {
                   string Country = VendorRet.SelectSingleNode("./VendorAddress/Country").InnerText;

               }
               //Get value of Note
               if (VendorRet.SelectSingleNode("./VendorAddress/Note") != null)
               {
                   string Note = VendorRet.SelectSingleNode("./VendorAddress/Note").InnerText;

               }

           }
           //Done with field values for VendorAddress aggregate

           //Get all field values for VendorAddressBlock aggregate 
           XmlNode VendorAddressBlock = VendorRet.SelectSingleNode("./VendorAddressBlock");
           if (VendorAddressBlock != null)
           {
               //Get value of Addr1
               if (VendorRet.SelectSingleNode("./VendorAddressBlock/Addr1") != null)
               {
                   string Addr1 = VendorRet.SelectSingleNode("./VendorAddressBlock/Addr1").InnerText;

               }
               //Get value of Addr2
               if (VendorRet.SelectSingleNode("./VendorAddressBlock/Addr2") != null)
               {
                   string Addr2 = VendorRet.SelectSingleNode("./VendorAddressBlock/Addr2").InnerText;

               }
               //Get value of Addr3
               if (VendorRet.SelectSingleNode("./VendorAddressBlock/Addr3") != null)
               {
                   string Addr3 = VendorRet.SelectSingleNode("./VendorAddressBlock/Addr3").InnerText;

               }
               //Get value of Addr4
               if (VendorRet.SelectSingleNode("./VendorAddressBlock/Addr4") != null)
               {
                   string Addr4 = VendorRet.SelectSingleNode("./VendorAddressBlock/Addr4").InnerText;

               }
               //Get value of Addr5
               if (VendorRet.SelectSingleNode("./VendorAddressBlock/Addr5") != null)
               {
                   string Addr5 = VendorRet.SelectSingleNode("./VendorAddressBlock/Addr5").InnerText;

               }

           }
           //Done with field values for VendorAddressBlock aggregate

           //Get all field values for ShipAddress aggregate 
           XmlNode ShipAddress = VendorRet.SelectSingleNode("./ShipAddress");
           if (ShipAddress != null)
           {
               //Get value of Addr1
               if (VendorRet.SelectSingleNode("./ShipAddress/Addr1") != null)
               {
                   string Addr1 = VendorRet.SelectSingleNode("./ShipAddress/Addr1").InnerText;

               }
               //Get value of Addr2
               if (VendorRet.SelectSingleNode("./ShipAddress/Addr2") != null)
               {
                   string Addr2 = VendorRet.SelectSingleNode("./ShipAddress/Addr2").InnerText;

               }
               //Get value of Addr3
               if (VendorRet.SelectSingleNode("./ShipAddress/Addr3") != null)
               {
                   string Addr3 = VendorRet.SelectSingleNode("./ShipAddress/Addr3").InnerText;

               }
               //Get value of Addr4
               if (VendorRet.SelectSingleNode("./ShipAddress/Addr4") != null)
               {
                   string Addr4 = VendorRet.SelectSingleNode("./ShipAddress/Addr4").InnerText;

               }
               //Get value of Addr5
               if (VendorRet.SelectSingleNode("./ShipAddress/Addr5") != null)
               {
                   string Addr5 = VendorRet.SelectSingleNode("./ShipAddress/Addr5").InnerText;

               }
               //Get value of City
               if (VendorRet.SelectSingleNode("./ShipAddress/City") != null)
               {
                   string City = VendorRet.SelectSingleNode("./ShipAddress/City").InnerText;

               }
               //Get value of State
               if (VendorRet.SelectSingleNode("./ShipAddress/State") != null)
               {
                   string State = VendorRet.SelectSingleNode("./ShipAddress/State").InnerText;

               }
               //Get value of PostalCode
               if (VendorRet.SelectSingleNode("./ShipAddress/PostalCode") != null)
               {
                   string PostalCode = VendorRet.SelectSingleNode("./ShipAddress/PostalCode").InnerText;

               }
               //Get value of Country
               if (VendorRet.SelectSingleNode("./ShipAddress/Country") != null)
               {
                   string Country = VendorRet.SelectSingleNode("./ShipAddress/Country").InnerText;

               }
               //Get value of Note
               if (VendorRet.SelectSingleNode("./ShipAddress/Note") != null)
               {
                   string Note = VendorRet.SelectSingleNode("./ShipAddress/Note").InnerText;

               }

           }
           //Done with field values for ShipAddress aggregate

           //Get value of Phone
           if (VendorRet.SelectSingleNode("./Phone") != null)
           {
               string Phone = VendorRet.SelectSingleNode("./Phone").InnerText;

           }
           //Get value of AltPhone
           if (VendorRet.SelectSingleNode("./AltPhone") != null)
           {
               string AltPhone = VendorRet.SelectSingleNode("./AltPhone").InnerText;

           }
           //Get value of Fax
           if (VendorRet.SelectSingleNode("./Fax") != null)
           {
               string Fax = VendorRet.SelectSingleNode("./Fax").InnerText;

           }
           //Get value of Email
           if (VendorRet.SelectSingleNode("./Email") != null)
           {
               string Email = VendorRet.SelectSingleNode("./Email").InnerText;

           }
           //Get value of Cc
           if (VendorRet.SelectSingleNode("./Cc") != null)
           {
               string Cc = VendorRet.SelectSingleNode("./Cc").InnerText;

           }
           //Get value of Contact
           if (VendorRet.SelectSingleNode("./Contact") != null)
           {
               string Contact = VendorRet.SelectSingleNode("./Contact").InnerText;

           }
           //Get value of AltContact
           if (VendorRet.SelectSingleNode("./AltContact") != null)
           {
               string AltContact = VendorRet.SelectSingleNode("./AltContact").InnerText;

           }
           //Get all field values for AdditionalContactRef aggregate 
           XmlNode AdditionalContactRef = VendorRet.SelectSingleNode("./AdditionalContactRef");
           if (AdditionalContactRef != null)
           {
               //Get value of ContactName
               string ContactName = VendorRet.SelectSingleNode("./AdditionalContactRef/ContactName").InnerText;
               //Get value of ContactValue
               string ContactValue = VendorRet.SelectSingleNode("./AdditionalContactRef/ContactValue").InnerText;

           }
           //Done with field values for AdditionalContactRef aggregate

           //Walk list of ContactsRet aggregates
           XmlNodeList ContactsRetList = VendorRet.SelectNodes("./ContactsRet");
           if (ContactsRetList != null)
           {
               for (int i = 0; i < ContactsRetList.Count; i++)
               {
                   XmlNode ContactsRet = ContactsRetList.Item(i);
                   //Get value of ListID
                   string cListID = ContactsRet.SelectSingleNode("./ListID").InnerText;
                   //Get value of TimeCreated
                   string cTimeCreated = ContactsRet.SelectSingleNode("./TimeCreated").InnerText;
                   //Get value of TimeModified
                   string cTimeModified = ContactsRet.SelectSingleNode("./TimeModified").InnerText;
                   //Get value of EditSequence
                   string cEditSequence = ContactsRet.SelectSingleNode("./EditSequence").InnerText;
                   //Get value of Contact
                   if (ContactsRet.SelectSingleNode("./Contact") != null)
                   {
                       string Contact = ContactsRet.SelectSingleNode("./Contact").InnerText;

                   }
                   //Get value of Salutation
                   if (ContactsRet.SelectSingleNode("./Salutation") != null)
                   {
                       string Salutation = ContactsRet.SelectSingleNode("./Salutation").InnerText;

                   }
                   //Get value of FirstName
                   string FirstName = ContactsRet.SelectSingleNode("./FirstName").InnerText;
                   //Get value of MiddleName
                   if (ContactsRet.SelectSingleNode("./MiddleName") != null)
                   {
                       string MiddleName = ContactsRet.SelectSingleNode("./MiddleName").InnerText;

                   }
                   //Get value of LastName
                   if (ContactsRet.SelectSingleNode("./LastName") != null)
                   {
                       string LastName = ContactsRet.SelectSingleNode("./LastName").InnerText;

                   }
                   //Get value of JobTitle
                   if (ContactsRet.SelectSingleNode("./JobTitle") != null)
                   {
                       string JobTitle = ContactsRet.SelectSingleNode("./JobTitle").InnerText;

                   }
                   //Get all field values for AdditionalContactRef aggregate 
                   XmlNode cAdditionalContactRef = ContactsRet.SelectSingleNode("./AdditionalContactRef");
                   if (AdditionalContactRef != null)
                   {
                       //Get value of ContactName
                       string ContactName = ContactsRet.SelectSingleNode("./AdditionalContactRef/ContactName").InnerText;
                       //Get value of ContactValue
                       string ContactValue = ContactsRet.SelectSingleNode("./AdditionalContactRef/ContactValue").InnerText;

                   }
                   //Done with field values for AdditionalContactRef aggregate


               }

           }

           //Get value of NameOnCheck
           if (VendorRet.SelectSingleNode("./NameOnCheck") != null)
           {
               string NameOnCheck = VendorRet.SelectSingleNode("./NameOnCheck").InnerText;

           }
           //Get value of AccountNumber
           if (VendorRet.SelectSingleNode("./AccountNumber") != null)
           {
               string AccountNumber = VendorRet.SelectSingleNode("./AccountNumber").InnerText;

           }
           //Get value of Notes
           if (VendorRet.SelectSingleNode("./Notes") != null)
           {
               string Notes = VendorRet.SelectSingleNode("./Notes").InnerText;

           }
           //Walk list of AdditionalNotesRet aggregates
           XmlNodeList AdditionalNotesRetList = VendorRet.SelectNodes("./AdditionalNotesRet");
           if (AdditionalNotesRetList != null)
           {
               for (int i = 0; i < AdditionalNotesRetList.Count; i++)
               {
                   XmlNode AdditionalNotesRet = AdditionalNotesRetList.Item(i);
                   //Get value of NoteID
                   string NoteID = AdditionalNotesRet.SelectSingleNode("./NoteID").InnerText;
                   //Get value of Date
                   string Date = AdditionalNotesRet.SelectSingleNode("./Date").InnerText;
                   //Get value of Note
                   string Note = AdditionalNotesRet.SelectSingleNode("./Note").InnerText;

               }

           }

           //Get all field values for VendorTypeRef aggregate 
           XmlNode VendorTypeRef = VendorRet.SelectSingleNode("./VendorTypeRef");
           if (VendorTypeRef != null)
           {
               //Get value of ListID
               if (VendorRet.SelectSingleNode("./VendorTypeRef/ListID") != null)
               {
                   string VendorTypeListID = VendorRet.SelectSingleNode("./VendorTypeRef/ListID").InnerText;

               }
               //Get value of FullName
               if (VendorRet.SelectSingleNode("./VendorTypeRef/FullName") != null)
               {
                   string FullName = VendorRet.SelectSingleNode("./VendorTypeRef/FullName").InnerText;

               }

           }
           //Done with field values for VendorTypeRef aggregate

           //Get all field values for TermsRef aggregate 
           XmlNode TermsRef = VendorRet.SelectSingleNode("./TermsRef");
           if (TermsRef != null)
           {
               //Get value of ListID
               if (VendorRet.SelectSingleNode("./TermsRef/ListID") != null)
               {
                   string TermsListID = VendorRet.SelectSingleNode("./TermsRef/ListID").InnerText;

               }
               //Get value of FullName
               if (VendorRet.SelectSingleNode("./TermsRef/FullName") != null)
               {
                   string FullName = VendorRet.SelectSingleNode("./TermsRef/FullName").InnerText;

               }

           }
           //Done with field values for TermsRef aggregate

           //Get value of CreditLimit
           if (VendorRet.SelectSingleNode("./CreditLimit") != null)
           {
               string CreditLimit = VendorRet.SelectSingleNode("./CreditLimit").InnerText;

           }
           //Get value of VendorTaxIdent
           if (VendorRet.SelectSingleNode("./VendorTaxIdent") != null)
           {
               string VendorTaxIdent = VendorRet.SelectSingleNode("./VendorTaxIdent").InnerText;

           }
           //Get value of IsVendorEligibleFor1099
           if (VendorRet.SelectSingleNode("./IsVendorEligibleFor1099") != null)
           {
               string IsVendorEligibleFor1099 = VendorRet.SelectSingleNode("./IsVendorEligibleFor1099").InnerText;

           }
           //Get value of Balance
           if (VendorRet.SelectSingleNode("./Balance") != null)
           {
               string Balance = VendorRet.SelectSingleNode("./Balance").InnerText;

           }
           //Get all field values for BillingRateRef aggregate 
           XmlNode BillingRateRef = VendorRet.SelectSingleNode("./BillingRateRef");
           if (BillingRateRef != null)
           {
               //Get value of ListID
               if (VendorRet.SelectSingleNode("./BillingRateRef/ListID") != null)
               {
                   string BillingRateListID = VendorRet.SelectSingleNode("./BillingRateRef/ListID").InnerText;

               }
               //Get value of FullName
               if (VendorRet.SelectSingleNode("./BillingRateRef/FullName") != null)
               {
                   string FullName = VendorRet.SelectSingleNode("./BillingRateRef/FullName").InnerText;

               }

           }
           //Done with field values for BillingRateRef aggregate

           //Get value of ExternalGUID
           if (VendorRet.SelectSingleNode("./ExternalGUID") != null)
           {
               string ExternalGUID = VendorRet.SelectSingleNode("./ExternalGUID").InnerText;

           }
           //Get all field values for PrefillAccountRef aggregate 
           XmlNode PrefillAccountRef = VendorRet.SelectSingleNode("./PrefillAccountRef");
           if (PrefillAccountRef != null)
           {
               //Get value of ListID
               if (VendorRet.SelectSingleNode("./PrefillAccountRef/ListID") != null)
               {
                   string PrefillAccountListID = VendorRet.SelectSingleNode("./PrefillAccountRef/ListID").InnerText;

               }
               //Get value of FullName
               if (VendorRet.SelectSingleNode("./PrefillAccountRef/FullName") != null)
               {
                   string FullName = VendorRet.SelectSingleNode("./PrefillAccountRef/FullName").InnerText;

               }

           }
           //Done with field values for PrefillAccountRef aggregate

           //Get all field values for CurrencyRef aggregate 
           XmlNode CurrencyRef = VendorRet.SelectSingleNode("./CurrencyRef");
           if (CurrencyRef != null)
           {
               //Get value of ListID
               if (VendorRet.SelectSingleNode("./CurrencyRef/ListID") != null)
               {
                   string CurrencyListID = VendorRet.SelectSingleNode("./CurrencyRef/ListID").InnerText;
               }
               //Get value of FullName
               if (VendorRet.SelectSingleNode("./CurrencyRef/FullName") != null)
               {
                   string FullName = VendorRet.SelectSingleNode("./CurrencyRef/FullName").InnerText;
               }
           }
           //Done with field values for CurrencyRef aggregate

           //Walk list of DataExtRet aggregates
           XmlNodeList DataExtRetList = VendorRet.SelectNodes("./DataExtRet");
           if (DataExtRetList != null)
           {
               for (int i = 0; i < DataExtRetList.Count; i++)
               {
                   XmlNode DataExtRet = DataExtRetList.Item(i);
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
           */
        }

        private static Vendor FindOrCreateVendor(RotoTrackDb db, string QBListId, string QBEditSequence, string TimeCreated, string TimeModified, string Name, string IsActive)
        {
            Vendor o = null;
            if (db.Vendors.Any(f => f.QBListId == QBListId))
            {
                o = db.Vendors.First(f => f.QBListId == QBListId);
            }
            else
            {
                o = new Vendor();
                db.Vendors.Add(o);
            }

            o.QBListId = QBListId;
            o.QBEditSequence = QBEditSequence;
            DateTime createdDate;
            if (DateTime.TryParse(TimeCreated, out createdDate)) o.TimeCreated = createdDate;
            DateTime modifiedDate;
            if (DateTime.TryParse(TimeModified, out modifiedDate)) o.TimeModified = modifiedDate;
            o.Name = Name;
            o.IsActive = (IsActive == "true") ? true : false;

            return o;
        }

    }
}
