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
    public class EstimateDAL
    {
        public static void RemoveDeleted()
        {
            XmlDocument doc = BuildDeletedRequest();
            string response = QBUtils.DoRequest(doc);
            HandleDeletedResponse(response);
        }

        public static XmlDocument BuildDeletedRequest()
        {
            XmlDocument doc = XmlUtils.MakeRequestDocument();
            XmlElement parent = XmlUtils.MakeRequestParentElement(doc);
            XmlElement TxnDeletedQueryRq = doc.CreateElement("TxnDeletedQueryRq");
            parent.AppendChild(TxnDeletedQueryRq);
            TxnDeletedQueryRq.AppendChild(XmlUtils.MakeSimpleElem(doc, "TxnDelType", "Estimate"));

            return doc;
        }

        public static XmlDocument BuildQueryRequest(string fromModifiedDate, string toModifiedDate)
        {
            XmlDocument doc = XmlUtils.MakeRequestDocument();
            XmlElement parent = XmlUtils.MakeRequestParentElement(doc);
            XmlElement queryElement = doc.CreateElement("EstimateQueryRq");
            parent.AppendChild(queryElement);

            XmlElement dateRangeFilter = doc.CreateElement("ModifiedDateRangeFilter");
            queryElement.AppendChild(dateRangeFilter);
            dateRangeFilter.AppendChild(XmlUtils.MakeSimpleElem(doc, "FromModifiedDate", fromModifiedDate));
            dateRangeFilter.AppendChild(XmlUtils.MakeSimpleElem(doc, "ToModifiedDate", toModifiedDate));                        
            queryElement.AppendChild(XmlUtils.MakeSimpleElem(doc, "IncludeLineItems", "1"));
            //queryElement.AppendChild(XmlUtils.MakeSimpleElem(doc, "IncludeLinkedTxns", "1"));

            return doc;
        }

        public static void HandleDeletedResponse(string response)
        {
            WalkTxnDeletedQueryRs(response);
        }

        public static void HandleResponse(string response)
        {
            WalkEstimateQueryRs(response);
        }

        public static void WalkTxnDeletedQueryRs(string response)
        {
            //Parse the response XML string into an XmlDocument
            XmlDocument responseXmlDoc = new XmlDocument();
            responseXmlDoc.LoadXml(response);

            //Get the response for our request
            XmlNodeList TxnDeletedQueryRsList = responseXmlDoc.GetElementsByTagName("TxnDeletedQueryRs");
            if (TxnDeletedQueryRsList.Count == 1) //Should always be true since we only did one request in this sample
            {
                XmlNode responseNode = TxnDeletedQueryRsList.Item(0);
                //Check the status code, info, and severity
                XmlAttributeCollection rsAttributes = responseNode.Attributes;
                string statusCode = rsAttributes.GetNamedItem("statusCode").Value;
                string statusSeverity = rsAttributes.GetNamedItem("statusSeverity").Value;
                string statusMessage = rsAttributes.GetNamedItem("statusMessage").Value;

                //status code = 0 all OK, > 0 is warning
                if (Convert.ToInt32(statusCode) >= 0)
                {
                    XmlNodeList TxnDeletedRetList = responseNode.SelectNodes("//TxnDeletedRet");//XPath Query
                    for (int i = 0; i < TxnDeletedRetList.Count; i++)
                    {
                        XmlNode TxnDeletedRet = TxnDeletedRetList.Item(i);
                        WalkTxnDeletedRet(TxnDeletedRet);
                    }
                }
            }
        }

        private static void WalkTxnDeletedRet(XmlNode TxnDeletedRet)
        {
            if (TxnDeletedRet == null) return;

            //Go through all the elements of TxnDeletedRet
            //Get value of TxnDelType
            string TxnDelType = TxnDeletedRet.SelectSingleNode("./TxnDelType").InnerText;
            //Get value of TxnID
            string TxnID = TxnDeletedRet.SelectSingleNode("./TxnID").InnerText;
            //Get value of TimeCreated
            string TimeCreated = TxnDeletedRet.SelectSingleNode("./TimeCreated").InnerText;
            //Get value of TimeDeleted
            string TimeDeleted = TxnDeletedRet.SelectSingleNode("./TimeDeleted").InnerText;
            //Get value of RefNumber
            if (TxnDeletedRet.SelectSingleNode("./RefNumber") != null)
            {
                string RefNumber = TxnDeletedRet.SelectSingleNode("./RefNumber").InnerText;
            }

            RotoTrackDb db = new RotoTrackDb();
            List<EstimateLine> elList = db.EstimateLines.Where(f => f.EstimateTxnId == TxnID).ToList();
            foreach (EstimateLine el in elList.ToList())
            {
                db.EstimateLines.Remove(el);
            }
            db.SaveChanges();
        }

        private static void WalkEstimateQueryRs(string response)
        {
            //Parse the response XML string into an XmlDocument
            XmlDocument responseXmlDoc = new XmlDocument();
            responseXmlDoc.LoadXml(response);

            //Get the response for our request
            XmlNodeList EstimateQueryRsList = responseXmlDoc.GetElementsByTagName("EstimateQueryRs");
            if (EstimateQueryRsList.Count == 1) //Should always be true since we only did one request in this sample
            {
                XmlNode responseNode = EstimateQueryRsList.Item(0);
                //Check the status code, info, and severity
                XmlAttributeCollection rsAttributes = responseNode.Attributes;
                string statusCode = rsAttributes.GetNamedItem("statusCode").Value;
                string statusSeverity = rsAttributes.GetNamedItem("statusSeverity").Value;
                string statusMessage = rsAttributes.GetNamedItem("statusMessage").Value;

                //status code = 0 all OK, > 0 is warning
                if (Convert.ToInt32(statusCode) >= 0)
                {
                    XmlNodeList EstimateRetList = responseNode.SelectNodes("//EstimateRet");//XPath Query
                    for (int i = 0; i < EstimateRetList.Count; i++)
                    {
                        XmlNode EstimateRet = EstimateRetList.Item(i);
                        WalkEstimateRetForQuery(EstimateRet);
                    }
                }
            }
        }

        private static void WalkEstimateRetForQuery(XmlNode EstimateRet)
        {
            if (EstimateRet == null) return;

            RotoTrackDb db = new RotoTrackDb();

            //Go through all the elements of EstimateRet
            //Get value of TxnID
            string TxnID = EstimateRet.SelectSingleNode("./TxnID").InnerText;

            // New or modified objects will return all current line items, so remove any existing ones first and then recreate them.
            RemoveExistingLineItems(db, TxnID);

            //Get value of TimeCreated
            string TimeCreated = EstimateRet.SelectSingleNode("./TimeCreated").InnerText;
            //Get value of TimeModified
            string TimeModified = EstimateRet.SelectSingleNode("./TimeModified").InnerText;
            //Get value of EditSequence
            string EditSequence = EstimateRet.SelectSingleNode("./EditSequence").InnerText;
            //Get value of TxnNumber
            if (EstimateRet.SelectSingleNode("./TxnNumber") != null)
            {
                string TxnNumber = EstimateRet.SelectSingleNode("./TxnNumber").InnerText;
            }
            //Get all field values for CustomerRef aggregate
            //Get value of ListID
            string CustomerListID = "";
            if (EstimateRet.SelectSingleNode("./CustomerRef/ListID") != null)
            {
                CustomerListID = EstimateRet.SelectSingleNode("./CustomerRef/ListID").InnerText;
            }
            // Get out and do nothing if CustomerListID is empty
            if (CustomerListID == "")
            {
                return;
            }

            //Get value of FullName
            if (EstimateRet.SelectSingleNode("./CustomerRef/FullName") != null)
            {
                string FullName = EstimateRet.SelectSingleNode("./CustomerRef/FullName").InnerText;
            }
            //Done with field values for CustomerRef aggregate

            //Get all field values for ClassRef aggregate
            XmlNode ClassRef = EstimateRet.SelectSingleNode("./ClassRef");
            if (ClassRef != null)
            {
                //Get value of ListID
                if (EstimateRet.SelectSingleNode("./ClassRef/ListID") != null)
                {
                    string ListID = EstimateRet.SelectSingleNode("./ClassRef/ListID").InnerText;
                }
                //Get value of FullName
                if (EstimateRet.SelectSingleNode("./ClassRef/FullName") != null)
                {
                    string FullName = EstimateRet.SelectSingleNode("./ClassRef/FullName").InnerText;
                }
            }
            //Done with field values for ClassRef aggregate

            //Get all field values for TemplateRef aggregate
            XmlNode TemplateRef = EstimateRet.SelectSingleNode("./TemplateRef");
            if (TemplateRef != null)
            {
                //Get value of ListID
                if (EstimateRet.SelectSingleNode("./TemplateRef/ListID") != null)
                {
                    string ListID = EstimateRet.SelectSingleNode("./TemplateRef/ListID").InnerText;
                }
                //Get value of FullName
                if (EstimateRet.SelectSingleNode("./TemplateRef/FullName") != null)
                {
                    string FullName = EstimateRet.SelectSingleNode("./TemplateRef/FullName").InnerText;
                }
            }
            //Done with field values for TemplateRef aggregate

            //Get value of TxnDate
            string TxnDate = EstimateRet.SelectSingleNode("./TxnDate").InnerText;
            //Get value of RefNumber
            if (EstimateRet.SelectSingleNode("./RefNumber") != null)
            {
                string RefNumber = EstimateRet.SelectSingleNode("./RefNumber").InnerText;
            }
            //Get all field values for BillAddress aggregate
            XmlNode BillAddress = EstimateRet.SelectSingleNode("./BillAddress");
            if (BillAddress != null)
            {
                //Get value of Addr1
                if (EstimateRet.SelectSingleNode("./BillAddress/Addr1") != null)
                {
                    string Addr1 = EstimateRet.SelectSingleNode("./BillAddress/Addr1").InnerText;
                }
                //Get value of Addr2
                if (EstimateRet.SelectSingleNode("./BillAddress/Addr2") != null)
                {
                    string Addr2 = EstimateRet.SelectSingleNode("./BillAddress/Addr2").InnerText;
                }
                //Get value of Addr3
                if (EstimateRet.SelectSingleNode("./BillAddress/Addr3") != null)
                {
                    string Addr3 = EstimateRet.SelectSingleNode("./BillAddress/Addr3").InnerText;
                }
                //Get value of Addr4
                if (EstimateRet.SelectSingleNode("./BillAddress/Addr4") != null)
                {
                    string Addr4 = EstimateRet.SelectSingleNode("./BillAddress/Addr4").InnerText;
                }
                //Get value of Addr5
                if (EstimateRet.SelectSingleNode("./BillAddress/Addr5") != null)
                {
                    string Addr5 = EstimateRet.SelectSingleNode("./BillAddress/Addr5").InnerText;
                }
                //Get value of City
                if (EstimateRet.SelectSingleNode("./BillAddress/City") != null)
                {
                    string City = EstimateRet.SelectSingleNode("./BillAddress/City").InnerText;
                }
                //Get value of State
                if (EstimateRet.SelectSingleNode("./BillAddress/State") != null)
                {
                    string State = EstimateRet.SelectSingleNode("./BillAddress/State").InnerText;
                }
                //Get value of PostalCode
                if (EstimateRet.SelectSingleNode("./BillAddress/PostalCode") != null)
                {
                    string PostalCode = EstimateRet.SelectSingleNode("./BillAddress/PostalCode").InnerText;
                }
                //Get value of Country
                if (EstimateRet.SelectSingleNode("./BillAddress/Country") != null)
                {
                    string Country = EstimateRet.SelectSingleNode("./BillAddress/Country").InnerText;
                }
                //Get value of Note
                if (EstimateRet.SelectSingleNode("./BillAddress/Note") != null)
                {
                    string Note = EstimateRet.SelectSingleNode("./BillAddress/Note").InnerText;
                }
            }
            //Done with field values for BillAddress aggregate

            //Get all field values for BillAddressBlock aggregate
            XmlNode BillAddressBlock = EstimateRet.SelectSingleNode("./BillAddressBlock");
            if (BillAddressBlock != null)
            {
                //Get value of Addr1
                if (EstimateRet.SelectSingleNode("./BillAddressBlock/Addr1") != null)
                {
                    string Addr1 = EstimateRet.SelectSingleNode("./BillAddressBlock/Addr1").InnerText;
                }
                //Get value of Addr2
                if (EstimateRet.SelectSingleNode("./BillAddressBlock/Addr2") != null)
                {
                    string Addr2 = EstimateRet.SelectSingleNode("./BillAddressBlock/Addr2").InnerText;
                }
                //Get value of Addr3
                if (EstimateRet.SelectSingleNode("./BillAddressBlock/Addr3") != null)
                {
                    string Addr3 = EstimateRet.SelectSingleNode("./BillAddressBlock/Addr3").InnerText;
                }
                //Get value of Addr4
                if (EstimateRet.SelectSingleNode("./BillAddressBlock/Addr4") != null)
                {
                    string Addr4 = EstimateRet.SelectSingleNode("./BillAddressBlock/Addr4").InnerText;
                }
                //Get value of Addr5
                if (EstimateRet.SelectSingleNode("./BillAddressBlock/Addr5") != null)
                {
                    string Addr5 = EstimateRet.SelectSingleNode("./BillAddressBlock/Addr5").InnerText;
                }
            }
            //Done with field values for BillAddressBlock aggregate

            //Get all field values for ShipAddress aggregate
            XmlNode ShipAddress = EstimateRet.SelectSingleNode("./ShipAddress");
            if (ShipAddress != null)
            {
                //Get value of Addr1
                if (EstimateRet.SelectSingleNode("./ShipAddress/Addr1") != null)
                {
                    string Addr1 = EstimateRet.SelectSingleNode("./ShipAddress/Addr1").InnerText;
                }
                //Get value of Addr2
                if (EstimateRet.SelectSingleNode("./ShipAddress/Addr2") != null)
                {
                    string Addr2 = EstimateRet.SelectSingleNode("./ShipAddress/Addr2").InnerText;
                }
                //Get value of Addr3
                if (EstimateRet.SelectSingleNode("./ShipAddress/Addr3") != null)
                {
                    string Addr3 = EstimateRet.SelectSingleNode("./ShipAddress/Addr3").InnerText;
                }
                //Get value of Addr4
                if (EstimateRet.SelectSingleNode("./ShipAddress/Addr4") != null)
                {
                    string Addr4 = EstimateRet.SelectSingleNode("./ShipAddress/Addr4").InnerText;
                }
                //Get value of Addr5
                if (EstimateRet.SelectSingleNode("./ShipAddress/Addr5") != null)
                {
                    string Addr5 = EstimateRet.SelectSingleNode("./ShipAddress/Addr5").InnerText;
                }
                //Get value of City
                if (EstimateRet.SelectSingleNode("./ShipAddress/City") != null)
                {
                    string City = EstimateRet.SelectSingleNode("./ShipAddress/City").InnerText;
                }
                //Get value of State
                if (EstimateRet.SelectSingleNode("./ShipAddress/State") != null)
                {
                    string State = EstimateRet.SelectSingleNode("./ShipAddress/State").InnerText;
                }
                //Get value of PostalCode
                if (EstimateRet.SelectSingleNode("./ShipAddress/PostalCode") != null)
                {
                    string PostalCode = EstimateRet.SelectSingleNode("./ShipAddress/PostalCode").InnerText;
                }
                //Get value of Country
                if (EstimateRet.SelectSingleNode("./ShipAddress/Country") != null)
                {
                    string Country = EstimateRet.SelectSingleNode("./ShipAddress/Country").InnerText;
                }
                //Get value of Note
                if (EstimateRet.SelectSingleNode("./ShipAddress/Note") != null)
                {
                    string Note = EstimateRet.SelectSingleNode("./ShipAddress/Note").InnerText;
                }
            }
            //Done with field values for ShipAddress aggregate

            //Get all field values for ShipAddressBlock aggregate
            XmlNode ShipAddressBlock = EstimateRet.SelectSingleNode("./ShipAddressBlock");
            if (ShipAddressBlock != null)
            {
                //Get value of Addr1
                if (EstimateRet.SelectSingleNode("./ShipAddressBlock/Addr1") != null)
                {
                    string Addr1 = EstimateRet.SelectSingleNode("./ShipAddressBlock/Addr1").InnerText;
                }
                //Get value of Addr2
                if (EstimateRet.SelectSingleNode("./ShipAddressBlock/Addr2") != null)
                {
                    string Addr2 = EstimateRet.SelectSingleNode("./ShipAddressBlock/Addr2").InnerText;
                }
                //Get value of Addr3
                if (EstimateRet.SelectSingleNode("./ShipAddressBlock/Addr3") != null)
                {
                    string Addr3 = EstimateRet.SelectSingleNode("./ShipAddressBlock/Addr3").InnerText;
                }
                //Get value of Addr4
                if (EstimateRet.SelectSingleNode("./ShipAddressBlock/Addr4") != null)
                {
                    string Addr4 = EstimateRet.SelectSingleNode("./ShipAddressBlock/Addr4").InnerText;
                }
                //Get value of Addr5
                if (EstimateRet.SelectSingleNode("./ShipAddressBlock/Addr5") != null)
                {
                    string Addr5 = EstimateRet.SelectSingleNode("./ShipAddressBlock/Addr5").InnerText;
                }
            }
            //Done with field values for ShipAddressBlock aggregate

            //Get value of PONumber
            if (EstimateRet.SelectSingleNode("./PONumber") != null)
            {
                string PONumber = EstimateRet.SelectSingleNode("./PONumber").InnerText;
            }
            //Get all field values for TermsRef aggregate
            XmlNode TermsRef = EstimateRet.SelectSingleNode("./TermsRef");
            if (TermsRef != null)
            {
                //Get value of ListID
                if (EstimateRet.SelectSingleNode("./TermsRef/ListID") != null)
                {
                    string ListID = EstimateRet.SelectSingleNode("./TermsRef/ListID").InnerText;
                }
                //Get value of FullName
                if (EstimateRet.SelectSingleNode("./TermsRef/FullName") != null)
                {
                    string FullName = EstimateRet.SelectSingleNode("./TermsRef/FullName").InnerText;
                }
            }
            //Done with field values for TermsRef aggregate

            //Get value of DueDate
            if (EstimateRet.SelectSingleNode("./DueDate") != null)
            {
                string DueDate = EstimateRet.SelectSingleNode("./DueDate").InnerText;
            }
            //Get all field values for SalesRepRef aggregate
            XmlNode SalesRepRef = EstimateRet.SelectSingleNode("./SalesRepRef");
            if (SalesRepRef != null)
            {
                //Get value of ListID
                if (EstimateRet.SelectSingleNode("./SalesRepRef/ListID") != null)
                {
                    string ListID = EstimateRet.SelectSingleNode("./SalesRepRef/ListID").InnerText;
                }
                //Get value of FullName
                if (EstimateRet.SelectSingleNode("./SalesRepRef/FullName") != null)
                {
                    string FullName = EstimateRet.SelectSingleNode("./SalesRepRef/FullName").InnerText;
                }
            }
            //Done with field values for SalesRepRef aggregate

            //Get value of FOB
            if (EstimateRet.SelectSingleNode("./FOB") != null)
            {
                string FOB = EstimateRet.SelectSingleNode("./FOB").InnerText;
            }
            //Get value of ShipDate
            if (EstimateRet.SelectSingleNode("./ShipDate") != null)
            {
                string ShipDate = EstimateRet.SelectSingleNode("./ShipDate").InnerText;
            }
            //Get all field values for ShipMethodRef aggregate
            XmlNode ShipMethodRef = EstimateRet.SelectSingleNode("./ShipMethodRef");
            if (ShipMethodRef != null)
            {
                //Get value of ListID
                if (EstimateRet.SelectSingleNode("./ShipMethodRef/ListID") != null)
                {
                    string ListID = EstimateRet.SelectSingleNode("./ShipMethodRef/ListID").InnerText;
                }
                //Get value of FullName
                if (EstimateRet.SelectSingleNode("./ShipMethodRef/FullName") != null)
                {
                    string FullName = EstimateRet.SelectSingleNode("./ShipMethodRef/FullName").InnerText;
                }
            }
            //Done with field values for ShipMethodRef aggregate

            //Get value of Subtotal
            string Subtotal = "";
            if (EstimateRet.SelectSingleNode("./Subtotal") != null)
            {
                Subtotal = EstimateRet.SelectSingleNode("./Subtotal").InnerText;
            }
            //Get all field values for ItemSalesTaxRef aggregate
            XmlNode ItemSalesTaxRef = EstimateRet.SelectSingleNode("./ItemSalesTaxRef");
            if (ItemSalesTaxRef != null)
            {
                //Get value of ListID
                if (EstimateRet.SelectSingleNode("./ItemSalesTaxRef/ListID") != null)
                {
                    string ListID = EstimateRet.SelectSingleNode("./ItemSalesTaxRef/ListID").InnerText;
                }
                //Get value of FullName
                if (EstimateRet.SelectSingleNode("./ItemSalesTaxRef/FullName") != null)
                {
                    string FullName = EstimateRet.SelectSingleNode("./ItemSalesTaxRef/FullName").InnerText;
                }
            }
            //Done with field values for ItemSalesTaxRef aggregate

            //Get value of SalesTaxPercentage
            if (EstimateRet.SelectSingleNode("./SalesTaxPercentage") != null)
            {
                string SalesTaxPercentage = EstimateRet.SelectSingleNode("./SalesTaxPercentage").InnerText;
            }
            //Get value of SalesTaxTotal
            string TotalTax = "";
            if (EstimateRet.SelectSingleNode("./SalesTaxTotal") != null)
            {
                TotalTax = EstimateRet.SelectSingleNode("./SalesTaxTotal").InnerText;
            }
            //Get value of TotalAmount
            string TotalAmount = "";
            if (EstimateRet.SelectSingleNode("./TotalAmount") != null)
            {
                TotalAmount = EstimateRet.SelectSingleNode("./TotalAmount").InnerText;
            }
            //Get all field values for CurrencyRef aggregate
            XmlNode CurrencyRef = EstimateRet.SelectSingleNode("./CurrencyRef");
            if (CurrencyRef != null)
            {
                //Get value of ListID
                if (EstimateRet.SelectSingleNode("./CurrencyRef/ListID") != null)
                {
                    string ListID = EstimateRet.SelectSingleNode("./CurrencyRef/ListID").InnerText;
                }
                //Get value of FullName
                if (EstimateRet.SelectSingleNode("./CurrencyRef/FullName") != null)
                {
                    string FullName = EstimateRet.SelectSingleNode("./CurrencyRef/FullName").InnerText;
                }
            }
            //Done with field values for CurrencyRef aggregate

            //Get value of ExchangeRate
            if (EstimateRet.SelectSingleNode("./ExchangeRate") != null)
            {
                string ExchangeRate = EstimateRet.SelectSingleNode("./ExchangeRate").InnerText;
            }
            //Get value of TotalAmountInHomeCurrency
            if (EstimateRet.SelectSingleNode("./TotalAmountInHomeCurrency") != null)
            {
                string TotalAmountInHomeCurrency = EstimateRet.SelectSingleNode("./TotalAmountInHomeCurrency").InnerText;
            }
            //Get value of IsManuallyClosed
            string IsManuallyClosed = "";
            if (EstimateRet.SelectSingleNode("./IsManuallyClosed") != null)
            {
                IsManuallyClosed = EstimateRet.SelectSingleNode("./IsManuallyClosed").InnerText;
            }
            //Get value of IsFullyInvoiced
            string IsFullyInvoiced = "";
            if (EstimateRet.SelectSingleNode("./IsFullyInvoiced") != null)
            {
                IsFullyInvoiced = EstimateRet.SelectSingleNode("./IsFullyInvoiced").InnerText;
            }
            //Get value of Memo
            if (EstimateRet.SelectSingleNode("./Memo") != null)
            {
                string Memo = EstimateRet.SelectSingleNode("./Memo").InnerText;
            }
            //Get all field values for CustomerMsgRef aggregate
            XmlNode CustomerMsgRef = EstimateRet.SelectSingleNode("./CustomerMsgRef");
            if (CustomerMsgRef != null)
            {
                //Get value of ListID
                if (EstimateRet.SelectSingleNode("./CustomerMsgRef/ListID") != null)
                {
                    string ListID = EstimateRet.SelectSingleNode("./CustomerMsgRef/ListID").InnerText;
                }
                //Get value of FullName
                if (EstimateRet.SelectSingleNode("./CustomerMsgRef/FullName") != null)
                {
                    string FullName = EstimateRet.SelectSingleNode("./CustomerMsgRef/FullName").InnerText;
                }
            }
            //Done with field values for CustomerMsgRef aggregate

            //Get value of IsToBePrinted
            if (EstimateRet.SelectSingleNode("./IsToBePrinted") != null)
            {
                string IsToBePrinted = EstimateRet.SelectSingleNode("./IsToBePrinted").InnerText;
            }
            //Get value of IsToBeEmailed
            if (EstimateRet.SelectSingleNode("./IsToBeEmailed") != null)
            {
                string IsToBeEmailed = EstimateRet.SelectSingleNode("./IsToBeEmailed").InnerText;
            }
            //Get all field values for CustomerSalesTaxCodeRef aggregate
            XmlNode CustomerSalesTaxCodeRef = EstimateRet.SelectSingleNode("./CustomerSalesTaxCodeRef");
            if (CustomerSalesTaxCodeRef != null)
            {
                //Get value of ListID
                if (EstimateRet.SelectSingleNode("./CustomerSalesTaxCodeRef/ListID") != null)
                {
                    string ListID = EstimateRet.SelectSingleNode("./CustomerSalesTaxCodeRef/ListID").InnerText;
                }
                //Get value of FullName
                if (EstimateRet.SelectSingleNode("./CustomerSalesTaxCodeRef/FullName") != null)
                {
                    string FullName = EstimateRet.SelectSingleNode("./CustomerSalesTaxCodeRef/FullName").InnerText;
                }
            }
            //Done with field values for CustomerSalesTaxCodeRef aggregate

            //Get value of Other
            if (EstimateRet.SelectSingleNode("./Other") != null)
            {
                string Other = EstimateRet.SelectSingleNode("./Other").InnerText;
            }
            //Get value of ExternalGUID
            if (EstimateRet.SelectSingleNode("./ExternalGUID") != null)
            {
                string ExternalGUID = EstimateRet.SelectSingleNode("./ExternalGUID").InnerText;
            }
            //Walk list of LinkedTxn aggregates
            XmlNodeList LinkedTxnList = EstimateRet.SelectNodes("./LinkedTxn");
            if (LinkedTxnList != null)
            {
                for (int i = 0; i < LinkedTxnList.Count; i++)
                {
                    XmlNode LinkedTxn = LinkedTxnList.Item(i);
                    //Get value of TxnID
                    string TxnID2 = LinkedTxn.SelectSingleNode("./TxnID").InnerText;
                    //Get value of TxnType
                    string TxnType = LinkedTxn.SelectSingleNode("./TxnType").InnerText;
                    //Get value of TxnDate
                    string TxnDate2 = LinkedTxn.SelectSingleNode("./TxnDate").InnerText;
                    //Get value of RefNumber
                    if (LinkedTxn.SelectSingleNode("./RefNumber") != null)
                    {
                        string RefNumber = LinkedTxn.SelectSingleNode("./RefNumber").InnerText;
                    }
                    //Get value of LinkType
                    if (LinkedTxn.SelectSingleNode("./LinkType") != null)
                    {
                        string LinkType = LinkedTxn.SelectSingleNode("./LinkType").InnerText;
                    }
                    //Get value of Amount
                    string Amount = LinkedTxn.SelectSingleNode("./Amount").InnerText;
                }
            }

            XmlNodeList OREstimateLineRetListChildren = EstimateRet.SelectNodes("./*");
            for (int i = 0; i < OREstimateLineRetListChildren.Count; i++)
            {
                XmlNode Child = OREstimateLineRetListChildren.Item(i);
                if (Child.Name == "EstimateLineRet")
                {
                    //Get value of TxnLineID
                    string TxnLineID = Child.SelectSingleNode("./TxnLineID").InnerText;

                    // Find existing or create new EstimateLine entry
                    EstimateLine el = FindOrCreateEstimateLine(db, TxnLineID, TxnID, TimeCreated, TimeModified, EditSequence, TxnDate, TotalAmount, TotalTax, Subtotal);

                    //Get all field values for ItemRef aggregate
                    XmlNode ItemRef = Child.SelectSingleNode("./ItemRef");
                    if (ItemRef != null)
                    {
                        //Get value of ListID
                        if (Child.SelectSingleNode("./ItemRef/ListID") != null)
                        {
                            string ListID = Child.SelectSingleNode("./ItemRef/ListID").InnerText;
                            el.ItemListID = ListID;
                        }
                        //Get value of FullName
                        if (Child.SelectSingleNode("./ItemRef/FullName") != null)
                        {
                            string FullName = Child.SelectSingleNode("./ItemRef/FullName").InnerText;
                            el.ItemName = FullName;
                        }
                    }
                    //Done with field values for ItemRef aggregate

                    //Get value of Desc
                    if (Child.SelectSingleNode("./Desc") != null)
                    {
                        string Desc = Child.SelectSingleNode("./Desc").InnerText;
                        el.Description = Desc;
                    }
                    //Get value of Quantity
                    if (Child.SelectSingleNode("./Quantity") != null)
                    {
                        string Quantity = Child.SelectSingleNode("./Quantity").InnerText;
                        decimal quantity;
                        if (Decimal.TryParse(Quantity, out quantity))
                        {
                            el.Quantity = quantity;
                        }
                    }
                    //Get value of UnitOfMeasure
                    if (Child.SelectSingleNode("./UnitOfMeasure") != null)
                    {
                        string UnitOfMeasure = Child.SelectSingleNode("./UnitOfMeasure").InnerText;
                    }
                    //Get all field values for OverrideUOMSetRef aggregate
                    XmlNode OverrideUOMSetRef = Child.SelectSingleNode("./OverrideUOMSetRef");
                    if (OverrideUOMSetRef != null)
                    {
                        //Get value of ListID
                        if (Child.SelectSingleNode("./OverrideUOMSetRef/ListID") != null)
                        {
                            string ListID = Child.SelectSingleNode("./OverrideUOMSetRef/ListID").InnerText;
                        }
                        //Get value of FullName
                        if (Child.SelectSingleNode("./OverrideUOMSetRef/FullName") != null)
                        {
                            string FullName = Child.SelectSingleNode("./OverrideUOMSetRef/FullName").InnerText;
                        }
                    }
                    //Done with field values for OverrideUOMSetRef aggregate
                    if (Child.SelectSingleNode("./Rate") != null)
                    {
                        string Rate = Child.SelectSingleNode("./Rate").InnerText;
                        decimal rate;
                        if (decimal.TryParse(Rate, out rate))
                        {
                            el.Rate = rate;
                        }
                    }
                    if (Child.SelectSingleNode("./RatePercent") != null)
                    {
                        string RatePercent = Child.SelectSingleNode("./RatePercent").InnerText;
                        decimal ratePercent;
                        if (decimal.TryParse(RatePercent, out ratePercent))
                        {
                            el.RatePercent = ratePercent;
                        }
                    }
                    if (Child.SelectSingleNode("./MarkupRate") != null)
                    {
                        string MarkupRate = Child.SelectSingleNode("./MarkupRate").InnerText;
                        decimal markuprate;
                        if (decimal.TryParse(MarkupRate, out markuprate))
                        {
                            el.MarkupRate = markuprate;
                        }
                    }
                    if (Child.SelectSingleNode("./MarkupRatePercent") != null)
                    {
                        string MarkupRatePercent = Child.SelectSingleNode("./MarkupRatePercent").InnerText;
                        decimal markupratePercent;
                        if (decimal.TryParse(MarkupRatePercent, out markupratePercent))
                        {
                            el.MarkupRatePercent = markupratePercent;
                        }
                    }

                    //Get all field values for ClassRef aggregate
                    XmlNode ClassRef2 = Child.SelectSingleNode("./ClassRef");
                    if (ClassRef2 != null)
                    {
                        //Get value of ListID
                        if (Child.SelectSingleNode("./ClassRef/ListID") != null)
                        {
                            string ListID = Child.SelectSingleNode("./ClassRef/ListID").InnerText;
                            el.AreaListID = ListID;
                        }
                        //Get value of FullName
                        if (Child.SelectSingleNode("./ClassRef/FullName") != null)
                        {
                            string FullName = Child.SelectSingleNode("./ClassRef/FullName").InnerText;
                        }
                    }
                    //Done with field values for ClassRef aggregate

                    //Get value of Amount
                    if (Child.SelectSingleNode("./Amount") != null)
                    {
                        string Amount = Child.SelectSingleNode("./Amount").InnerText;
                        decimal amount;
                        if (decimal.TryParse(Amount, out amount))
                        {
                            el.Amount = amount;
                        }
                    }
                    //Get all field values for InventorySiteRef aggregate
                    XmlNode InventorySiteRef = Child.SelectSingleNode("./InventorySiteRef");
                    if (InventorySiteRef != null)
                    {
                        //Get value of ListID
                        if (Child.SelectSingleNode("./InventorySiteRef/ListID") != null)
                        {
                            string ListID = Child.SelectSingleNode("./InventorySiteRef/ListID").InnerText;
                        }
                        //Get value of FullName
                        if (Child.SelectSingleNode("./InventorySiteRef/FullName") != null)
                        {
                            string FullName = Child.SelectSingleNode("./InventorySiteRef/FullName").InnerText;
                        }
                    }
                    //Done with field values for InventorySiteRef aggregate

                    //Get all field values for InventorySiteLocationRef aggregate
                    XmlNode InventorySiteLocationRef = Child.SelectSingleNode("./InventorySiteLocationRef");
                    if (InventorySiteLocationRef != null)
                    {
                        //Get value of ListID
                        if (Child.SelectSingleNode("./InventorySiteLocationRef/ListID") != null)
                        {
                            string ListID = Child.SelectSingleNode("./InventorySiteLocationRef/ListID").InnerText;
                        }
                        //Get value of FullName
                        if (Child.SelectSingleNode("./InventorySiteLocationRef/FullName") != null)
                        {
                            string FullName = Child.SelectSingleNode("./InventorySiteLocationRef/FullName").InnerText;
                        }
                    }
                    //Done with field values for InventorySiteLocationRef aggregate

                    //Get all field values for SalesTaxCodeRef aggregate
                    XmlNode SalesTaxCodeRef = Child.SelectSingleNode("./SalesTaxCodeRef");
                    if (SalesTaxCodeRef != null)
                    {
                        //Get value of ListID
                        if (Child.SelectSingleNode("./SalesTaxCodeRef/ListID") != null)
                        {
                            string ListID = Child.SelectSingleNode("./SalesTaxCodeRef/ListID").InnerText;
                        }
                        //Get value of FullName
                        if (Child.SelectSingleNode("./SalesTaxCodeRef/FullName") != null)
                        {
                            string FullName = Child.SelectSingleNode("./SalesTaxCodeRef/FullName").InnerText;
                        }
                    }
                    //Done with field values for SalesTaxCodeRef aggregate

                    //Get value of Invoiced
                    if (Child.SelectSingleNode("./Invoiced") != null)
                    {
                        string Invoiced = Child.SelectSingleNode("./Invoiced").InnerText;
                    }
                    //Get value of IsManuallyClosed
                    if (Child.SelectSingleNode("./IsManuallyClosed") != null)
                    {
                        string IsManuallyClosed2 = Child.SelectSingleNode("./IsManuallyClosed").InnerText;
                    }
                    //Get value of Other1
                    if (Child.SelectSingleNode("./Other1") != null)
                    {
                        string Other1 = Child.SelectSingleNode("./Other1").InnerText;
                    }
                    //Get value of Other2
                    if (Child.SelectSingleNode("./Other2") != null)
                    {
                        string Other2 = Child.SelectSingleNode("./Other2").InnerText;
                    }
                    //Walk list of DataExtRet aggregates
                    XmlNodeList DataExtRetList = Child.SelectNodes("./DataExtRet");
                    if (DataExtRetList != null)
                    {
                        for (int j = 0; j < DataExtRetList.Count; j++)
                        {
                            XmlNode DataExtRet = DataExtRetList.Item(j);
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

                    el.WorkOrderListID = CustomerListID;
                    el.BillableStatus = "Billable";
                    db.SaveChanges();
                }

                if (Child.Name == "EstimateLineGroupRet")
                {
                    //Get value of TxnLineID
                    string TxnLineID = Child.SelectSingleNode("./TxnLineID").InnerText;
                    //Get all field values for ItemGroupRef aggregate
                    //Get value of ListID
                    if (Child.SelectSingleNode("./ItemGroupRef/ListID") != null)
                    {
                        string ListID = Child.SelectSingleNode("./ItemGroupRef/ListID").InnerText;
                    }
                    //Get value of FullName
                    if (Child.SelectSingleNode("./ItemGroupRef/FullName") != null)
                    {
                        string FullName = Child.SelectSingleNode("./ItemGroupRef/FullName").InnerText;
                    }
                    //Done with field values for ItemGroupRef aggregate

                    //Get value of Desc
                    if (Child.SelectSingleNode("./Desc") != null)
                    {
                        string Desc = Child.SelectSingleNode("./Desc").InnerText;
                    }
                    //Get value of Quantity
                    if (Child.SelectSingleNode("./Quantity") != null)
                    {
                        string Quantity = Child.SelectSingleNode("./Quantity").InnerText;
                    }
                    //Get value of UnitOfMeasure
                    if (Child.SelectSingleNode("./UnitOfMeasure") != null)
                    {
                        string UnitOfMeasure = Child.SelectSingleNode("./UnitOfMeasure").InnerText;
                    }
                    //Get all field values for OverrideUOMSetRef aggregate
                    XmlNode OverrideUOMSetRef = Child.SelectSingleNode("./OverrideUOMSetRef");
                    if (OverrideUOMSetRef != null)
                    {
                        //Get value of ListID
                        if (Child.SelectSingleNode("./OverrideUOMSetRef/ListID") != null)
                        {
                            string ListID = Child.SelectSingleNode("./OverrideUOMSetRef/ListID").InnerText;
                        }
                        //Get value of FullName
                        if (Child.SelectSingleNode("./OverrideUOMSetRef/FullName") != null)
                        {
                            string FullName = Child.SelectSingleNode("./OverrideUOMSetRef/FullName").InnerText;
                        }
                    }
                    //Done with field values for OverrideUOMSetRef aggregate

                    //Get value of IsPrintItemsInGroup
                    string IsPrintItemsInGroup = Child.SelectSingleNode("./IsPrintItemsInGroup").InnerText;
                    //Get value of TotalAmount
                    string TotalAmount2 = Child.SelectSingleNode("./TotalAmount").InnerText;
                    //Walk list of EstimateLineRet aggregates
                    XmlNodeList EstimateLineRetList = Child.SelectNodes("./EstimateLineRet");
                    if (EstimateLineRetList != null)
                    {
                        for (int k = 0; k < EstimateLineRetList.Count; k++)
                        {
                            XmlNode EstimateLineRet = EstimateLineRetList.Item(k);
                            //Get value of TxnLineID
                            string TxnLineID2 = EstimateLineRet.SelectSingleNode("./TxnLineID").InnerText;

                            // Find existing or create new EstimateLine entry
                            EstimateLine el = FindOrCreateEstimateLine(db, TxnLineID2, TxnID, TimeCreated, TimeModified, EditSequence, TxnDate, TotalAmount, TotalTax, Subtotal);

                            //Get all field values for ItemRef aggregate
                            XmlNode ItemRef = EstimateLineRet.SelectSingleNode("./ItemRef");
                            if (ItemRef != null)
                            {
                                //Get value of ListID
                                if (EstimateLineRet.SelectSingleNode("./ItemRef/ListID") != null)
                                {
                                    string ListID = EstimateLineRet.SelectSingleNode("./ItemRef/ListID").InnerText;
                                    el.ItemListID = ListID;
                                }
                                //Get value of FullName
                                if (EstimateLineRet.SelectSingleNode("./ItemRef/FullName") != null)
                                {
                                    string FullName = EstimateLineRet.SelectSingleNode("./ItemRef/FullName").InnerText;
                                    el.ItemName = FullName;
                                }
                            }
                            //Done with field values for ItemRef aggregate

                            //Get value of Desc
                            if (EstimateLineRet.SelectSingleNode("./Desc") != null)
                            {
                                string Desc = EstimateLineRet.SelectSingleNode("./Desc").InnerText;
                                el.Description = Desc;
                            }
                            //Get value of Quantity
                            if (EstimateLineRet.SelectSingleNode("./Quantity") != null)
                            {
                                string Quantity = EstimateLineRet.SelectSingleNode("./Quantity").InnerText;
                                decimal quantity;
                                if (Decimal.TryParse(Quantity, out quantity))
                                {
                                    el.Quantity = quantity;
                                }
                            }
                            //Get value of UnitOfMeasure
                            if (EstimateLineRet.SelectSingleNode("./UnitOfMeasure") != null)
                            {
                                string UnitOfMeasure = EstimateLineRet.SelectSingleNode("./UnitOfMeasure").InnerText;
                            }
                            //Get all field values for OverrideUOMSetRef aggregate
                            XmlNode OverrideUOMSetRef2 = EstimateLineRet.SelectSingleNode("./OverrideUOMSetRef");
                            if (OverrideUOMSetRef2 != null)
                            {
                                //Get value of ListID
                                if (EstimateLineRet.SelectSingleNode("./OverrideUOMSetRef/ListID") != null)
                                {
                                    string ListID = EstimateLineRet.SelectSingleNode("./OverrideUOMSetRef/ListID").InnerText;
                                }
                                //Get value of FullName
                                if (EstimateLineRet.SelectSingleNode("./OverrideUOMSetRef/FullName") != null)
                                {
                                    string FullName = EstimateLineRet.SelectSingleNode("./OverrideUOMSetRef/FullName").InnerText;
                                }
                            }
                            //Done with field values for OverrideUOMSetRef aggregate

                            //Done with field values for OverrideUOMSetRef aggregate
                            if (EstimateLineRet.SelectSingleNode("./Rate") != null)
                            {
                                string Rate = EstimateLineRet.SelectSingleNode("./Rate").InnerText;
                                decimal rate;
                                if (decimal.TryParse(Rate, out rate))
                                {
                                    el.Rate = rate;
                                }
                            }
                            if (EstimateLineRet.SelectSingleNode("./RatePercent") != null)
                            {
                                string RatePercent = EstimateLineRet.SelectSingleNode("./RatePercent").InnerText;
                                decimal ratePercent;
                                if (decimal.TryParse(RatePercent, out ratePercent))
                                {
                                    el.RatePercent = ratePercent;
                                }
                            }
                            if (EstimateLineRet.SelectSingleNode("./MarkupRate") != null)
                            {
                                string MarkupRate = EstimateLineRet.SelectSingleNode("./MarkupRate").InnerText;
                                decimal markuprate;
                                if (decimal.TryParse(MarkupRate, out markuprate))
                                {
                                    el.MarkupRate = markuprate;
                                }
                            }
                            if (EstimateLineRet.SelectSingleNode("./MarkupRatePercent") != null)
                            {
                                string MarkupRatePercent = EstimateLineRet.SelectSingleNode("./MarkupRatePercent").InnerText;
                                decimal markupratePercent;
                                if (decimal.TryParse(MarkupRatePercent, out markupratePercent))
                                {
                                    el.MarkupRatePercent = markupratePercent;
                                }
                            }

                            //Get all field values for ClassRef aggregate
                            XmlNode ClassRef2 = EstimateLineRet.SelectSingleNode("./ClassRef");
                            if (ClassRef != null)
                            {
                                //Get value of ListID
                                if (EstimateLineRet.SelectSingleNode("./ClassRef/ListID") != null)
                                {
                                    string ListID = EstimateLineRet.SelectSingleNode("./ClassRef/ListID").InnerText;
                                    el.AreaListID = ListID;
                                }
                                //Get value of FullName
                                if (EstimateLineRet.SelectSingleNode("./ClassRef/FullName") != null)
                                {
                                    string FullName = EstimateLineRet.SelectSingleNode("./ClassRef/FullName").InnerText;
                                }
                            }
                            //Done with field values for ClassRef aggregate

                            //Get value of Amount
                            if (EstimateLineRet.SelectSingleNode("./Amount") != null)
                            {
                                string Amount = EstimateLineRet.SelectSingleNode("./Amount").InnerText;
                                decimal amount;
                                if (decimal.TryParse(Amount, out amount))
                                {
                                    el.Amount = amount;
                                }
                            }
                            //Get all field values for InventorySiteRef aggregate
                            XmlNode InventorySiteRef = EstimateLineRet.SelectSingleNode("./InventorySiteRef");
                            if (InventorySiteRef != null)
                            {
                                //Get value of ListID
                                if (EstimateLineRet.SelectSingleNode("./InventorySiteRef/ListID") != null)
                                {
                                    string ListID = EstimateLineRet.SelectSingleNode("./InventorySiteRef/ListID").InnerText;
                                }
                                //Get value of FullName
                                if (EstimateLineRet.SelectSingleNode("./InventorySiteRef/FullName") != null)
                                {
                                    string FullName = EstimateLineRet.SelectSingleNode("./InventorySiteRef/FullName").InnerText;
                                }
                            }
                            //Done with field values for InventorySiteRef aggregate

                            //Get all field values for InventorySiteLocationRef aggregate
                            XmlNode InventorySiteLocationRef = EstimateLineRet.SelectSingleNode("./InventorySiteLocationRef");
                            if (InventorySiteLocationRef != null)
                            {
                                //Get value of ListID
                                if (EstimateLineRet.SelectSingleNode("./InventorySiteLocationRef/ListID") != null)
                                {
                                    string ListID = EstimateLineRet.SelectSingleNode("./InventorySiteLocationRef/ListID").InnerText;
                                }
                                //Get value of FullName
                                if (EstimateLineRet.SelectSingleNode("./InventorySiteLocationRef/FullName") != null)
                                {
                                    string FullName = EstimateLineRet.SelectSingleNode("./InventorySiteLocationRef/FullName").InnerText;
                                }
                            }
                            //Done with field values for InventorySiteLocationRef aggregate

                            //Get all field values for SalesTaxCodeRef aggregate
                            XmlNode SalesTaxCodeRef = EstimateLineRet.SelectSingleNode("./SalesTaxCodeRef");
                            if (SalesTaxCodeRef != null)
                            {
                                //Get value of ListID
                                if (EstimateLineRet.SelectSingleNode("./SalesTaxCodeRef/ListID") != null)
                                {
                                    string ListID = EstimateLineRet.SelectSingleNode("./SalesTaxCodeRef/ListID").InnerText;
                                }
                                //Get value of FullName
                                if (EstimateLineRet.SelectSingleNode("./SalesTaxCodeRef/FullName") != null)
                                {
                                    string FullName = EstimateLineRet.SelectSingleNode("./SalesTaxCodeRef/FullName").InnerText;
                                }
                            }
                            //Done with field values for SalesTaxCodeRef aggregate

                            //Get value of Invoiced
                            if (EstimateLineRet.SelectSingleNode("./Invoiced") != null)
                            {
                                string Invoiced = EstimateLineRet.SelectSingleNode("./Invoiced").InnerText;
                            }
                            //Get value of IsManuallyClosed
                            if (EstimateLineRet.SelectSingleNode("./IsManuallyClosed") != null)
                            {
                                string IsManuallyClosed2 = EstimateLineRet.SelectSingleNode("./IsManuallyClosed").InnerText;
                            }
                            //Get value of Other1
                            if (EstimateLineRet.SelectSingleNode("./Other1") != null)
                            {
                                string Other1 = EstimateLineRet.SelectSingleNode("./Other1").InnerText;
                            }
                            //Get value of Other2
                            if (EstimateLineRet.SelectSingleNode("./Other2") != null)
                            {
                                string Other2 = EstimateLineRet.SelectSingleNode("./Other2").InnerText;
                            }
                            //Walk list of DataExtRet aggregates
                            XmlNodeList DataExtRetList = EstimateLineRet.SelectNodes("./DataExtRet");
                            if (DataExtRetList != null)
                            {
                                for (int l = 0; l < DataExtRetList.Count; l++)
                                {
                                    XmlNode DataExtRet = DataExtRetList.Item(l);
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

                            el.WorkOrderListID = CustomerListID;
                            el.BillableStatus = "Billable";
                            db.SaveChanges();
                        }
                    }

                    //Walk list of DataExtRet aggregates
                    XmlNodeList DataExtRetList2 = Child.SelectNodes("./DataExtRet");
                    if (DataExtRetList2 != null)
                    {
                        for (int m = 0; m < DataExtRetList2.Count; m++)
                        {
                            XmlNode DataExtRet = DataExtRetList2.Item(m);
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
            }
        }

        private static void RemoveExistingLineItems(RotoTrackDb db, string TxnID)
        {
            List<EstimateLine> elList = db.EstimateLines.Where(f => f.EstimateTxnId == TxnID).ToList();
            foreach (EstimateLine el in elList.ToList())
            {
                db.EstimateLines.Remove(el);
            }
            db.SaveChanges();
        }

        private static EstimateLine FindOrCreateEstimateLine(RotoTrackDb db, string TxnLineID, string TxnID, string TimeCreated, string TimeModified, string EditSequence, string TxnDate, string AmountDue, string TotalTax, string Subtotal)
        {
            EstimateLine el = null;
            if (db.EstimateLines.Any(f => f.TxnLineId == TxnLineID))
            {
                el = db.EstimateLines.First(f => f.TxnLineId == TxnLineID);
            }
            else
            {
                el = new EstimateLine();
                db.EstimateLines.Add(el);
            }

            el.TxnLineId = TxnLineID;
            el.EstimateTxnId = TxnID;
            DateTime createdDate;
            if (DateTime.TryParse(TimeCreated, out createdDate)) el.EstimateCreated = createdDate;
            DateTime modifiedDate;
            if (DateTime.TryParse(TimeModified, out modifiedDate)) el.EstimateModified = modifiedDate;
            el.EstimateEditSequence = EditSequence;
            DateTime txnDate;
            if (DateTime.TryParse(TxnDate, out txnDate)) el.EstimateTxnDate = txnDate;
            decimal amountDue;
            if (Decimal.TryParse(AmountDue, out amountDue)) el.EstimateTotalAmount = amountDue;
            decimal totalTax;
            if (Decimal.TryParse(TotalTax, out totalTax)) el.EstimateTotalTax = totalTax;
            decimal subtotal;
            if (Decimal.TryParse(Subtotal, out subtotal)) el.EstimateSubTotal = subtotal;

            return el;
        }

    }
}
