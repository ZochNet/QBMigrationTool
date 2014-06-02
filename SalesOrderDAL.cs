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
    public class SalesOrderDAL
    {
        public static XmlDocument BuildQueryRequest(string fromModifiedDate, string toModifiedDate)
        {
            XmlDocument doc = XmlUtils.MakeRequestDocument();
            XmlElement parent = XmlUtils.MakeRequestParentElement(doc);
            XmlElement queryElement = doc.CreateElement("SalesOrderQueryRq");
            parent.AppendChild(queryElement);

            XmlElement dateRangeFilter = doc.CreateElement("ModifiedDateRangeFilter");
            queryElement.AppendChild(dateRangeFilter);
            dateRangeFilter.AppendChild(XmlUtils.MakeSimpleElem(doc, "FromModifiedDate", fromModifiedDate));
            dateRangeFilter.AppendChild(XmlUtils.MakeSimpleElem(doc, "ToModifiedDate", toModifiedDate));                        
            queryElement.AppendChild(XmlUtils.MakeSimpleElem(doc, "IncludeLineItems", "1"));

            return doc;
        }

        public static void HandleResponse(string response)
        {
            WalkSalesOrderQueryRs(response);
        }

        private static void WalkSalesOrderQueryRs(string response)
        {
            //Parse the response XML string into an XmlDocument
            XmlDocument responseXmlDoc = new XmlDocument();
            responseXmlDoc.LoadXml(response);

            //Get the response for our request
            XmlNodeList SalesOrderQueryRsList = responseXmlDoc.GetElementsByTagName("SalesOrderQueryRs");
            if (SalesOrderQueryRsList.Count == 1) //Should always be true since we only did one request in this sample
            {
                XmlNode responseNode = SalesOrderQueryRsList.Item(0);
                //Check the status code, info, and severity
                XmlAttributeCollection rsAttributes = responseNode.Attributes;
                string statusCode = rsAttributes.GetNamedItem("statusCode").Value;
                string statusSeverity = rsAttributes.GetNamedItem("statusSeverity").Value;
                string statusMessage = rsAttributes.GetNamedItem("statusMessage").Value;

                //status code = 0 all OK, > 0 is warning
                if (Convert.ToInt32(statusCode) >= 0)
                {
                    XmlNodeList SalesOrderRetList = responseNode.SelectNodes("//SalesOrderRet");//XPath Query
                    for (int i = 0; i < SalesOrderRetList.Count; i++)
                    {
                        XmlNode SalesOrderRet = SalesOrderRetList.Item(i);
                        WalkSalesOrderRetForQuery(SalesOrderRet);
                    }
                }
            }
        }

        private static void WalkSalesOrderRetForQuery(XmlNode SalesOrderRet)
        {
            if (SalesOrderRet == null) return;

            RotoTrackDb db = new RotoTrackDb();

            //Go through all the elements of SalesOrderRet
            //Get value of TxnID
            string TxnID = SalesOrderRet.SelectSingleNode("./TxnID").InnerText;
            //Get value of TimeCreated
            string TimeCreated = SalesOrderRet.SelectSingleNode("./TimeCreated").InnerText;
            //Get value of TimeModified
            string TimeModified = SalesOrderRet.SelectSingleNode("./TimeModified").InnerText;
            //Get value of EditSequence
            string EditSequence = SalesOrderRet.SelectSingleNode("./EditSequence").InnerText;
            //Get value of TxnNumber
            if (SalesOrderRet.SelectSingleNode("./TxnNumber") != null)
            {
                string TxnNumber = SalesOrderRet.SelectSingleNode("./TxnNumber").InnerText;
            }
            //Get all field values for CustomerRef aggregate
            //Get value of ListID
            string CustomerListID = "";
            if (SalesOrderRet.SelectSingleNode("./CustomerRef/ListID") != null)
            {
                CustomerListID = SalesOrderRet.SelectSingleNode("./CustomerRef/ListID").InnerText;
            }
            // Get out and do nothing if CustomerListID is empty
            if (CustomerListID == "")
            {
                return;
            }

            //Get value of FullName
            if (SalesOrderRet.SelectSingleNode("./CustomerRef/FullName") != null)
            {
                string FullName = SalesOrderRet.SelectSingleNode("./CustomerRef/FullName").InnerText;
            }
            //Done with field values for CustomerRef aggregate

            //Get all field values for ClassRef aggregate
            XmlNode ClassRef = SalesOrderRet.SelectSingleNode("./ClassRef");
            if (ClassRef != null)
            {
                //Get value of ListID
                if (SalesOrderRet.SelectSingleNode("./ClassRef/ListID") != null)
                {
                    string ListID = SalesOrderRet.SelectSingleNode("./ClassRef/ListID").InnerText;
                }
                //Get value of FullName
                if (SalesOrderRet.SelectSingleNode("./ClassRef/FullName") != null)
                {
                    string FullName = SalesOrderRet.SelectSingleNode("./ClassRef/FullName").InnerText;
                }
            }
            //Done with field values for ClassRef aggregate

            //Get all field values for TemplateRef aggregate
            XmlNode TemplateRef = SalesOrderRet.SelectSingleNode("./TemplateRef");
            if (TemplateRef != null)
            {
                //Get value of ListID
                if (SalesOrderRet.SelectSingleNode("./TemplateRef/ListID") != null)
                {
                    string ListID = SalesOrderRet.SelectSingleNode("./TemplateRef/ListID").InnerText;
                }
                //Get value of FullName
                if (SalesOrderRet.SelectSingleNode("./TemplateRef/FullName") != null)
                {
                    string FullName = SalesOrderRet.SelectSingleNode("./TemplateRef/FullName").InnerText;
                }
            }
            //Done with field values for TemplateRef aggregate

            //Get value of TxnDate
            string TxnDate = SalesOrderRet.SelectSingleNode("./TxnDate").InnerText;
            //Get value of RefNumber
            if (SalesOrderRet.SelectSingleNode("./RefNumber") != null)
            {
                string RefNumber = SalesOrderRet.SelectSingleNode("./RefNumber").InnerText;
            }
            //Get all field values for BillAddress aggregate
            XmlNode BillAddress = SalesOrderRet.SelectSingleNode("./BillAddress");
            if (BillAddress != null)
            {
                //Get value of Addr1
                if (SalesOrderRet.SelectSingleNode("./BillAddress/Addr1") != null)
                {
                    string Addr1 = SalesOrderRet.SelectSingleNode("./BillAddress/Addr1").InnerText;
                }
                //Get value of Addr2
                if (SalesOrderRet.SelectSingleNode("./BillAddress/Addr2") != null)
                {
                    string Addr2 = SalesOrderRet.SelectSingleNode("./BillAddress/Addr2").InnerText;
                }
                //Get value of Addr3
                if (SalesOrderRet.SelectSingleNode("./BillAddress/Addr3") != null)
                {
                    string Addr3 = SalesOrderRet.SelectSingleNode("./BillAddress/Addr3").InnerText;
                }
                //Get value of Addr4
                if (SalesOrderRet.SelectSingleNode("./BillAddress/Addr4") != null)
                {
                    string Addr4 = SalesOrderRet.SelectSingleNode("./BillAddress/Addr4").InnerText;
                }
                //Get value of Addr5
                if (SalesOrderRet.SelectSingleNode("./BillAddress/Addr5") != null)
                {
                    string Addr5 = SalesOrderRet.SelectSingleNode("./BillAddress/Addr5").InnerText;
                }
                //Get value of City
                if (SalesOrderRet.SelectSingleNode("./BillAddress/City") != null)
                {
                    string City = SalesOrderRet.SelectSingleNode("./BillAddress/City").InnerText;
                }
                //Get value of State
                if (SalesOrderRet.SelectSingleNode("./BillAddress/State") != null)
                {
                    string State = SalesOrderRet.SelectSingleNode("./BillAddress/State").InnerText;
                }
                //Get value of PostalCode
                if (SalesOrderRet.SelectSingleNode("./BillAddress/PostalCode") != null)
                {
                    string PostalCode = SalesOrderRet.SelectSingleNode("./BillAddress/PostalCode").InnerText;
                }
                //Get value of Country
                if (SalesOrderRet.SelectSingleNode("./BillAddress/Country") != null)
                {
                    string Country = SalesOrderRet.SelectSingleNode("./BillAddress/Country").InnerText;
                }
                //Get value of Note
                if (SalesOrderRet.SelectSingleNode("./BillAddress/Note") != null)
                {
                    string Note = SalesOrderRet.SelectSingleNode("./BillAddress/Note").InnerText;
                }
            }
            //Done with field values for BillAddress aggregate

            //Get all field values for BillAddressBlock aggregate
            XmlNode BillAddressBlock = SalesOrderRet.SelectSingleNode("./BillAddressBlock");
            if (BillAddressBlock != null)
            {
                //Get value of Addr1
                if (SalesOrderRet.SelectSingleNode("./BillAddressBlock/Addr1") != null)
                {
                    string Addr1 = SalesOrderRet.SelectSingleNode("./BillAddressBlock/Addr1").InnerText;
                }
                //Get value of Addr2
                if (SalesOrderRet.SelectSingleNode("./BillAddressBlock/Addr2") != null)
                {
                    string Addr2 = SalesOrderRet.SelectSingleNode("./BillAddressBlock/Addr2").InnerText;
                }
                //Get value of Addr3
                if (SalesOrderRet.SelectSingleNode("./BillAddressBlock/Addr3") != null)
                {
                    string Addr3 = SalesOrderRet.SelectSingleNode("./BillAddressBlock/Addr3").InnerText;
                }
                //Get value of Addr4
                if (SalesOrderRet.SelectSingleNode("./BillAddressBlock/Addr4") != null)
                {
                    string Addr4 = SalesOrderRet.SelectSingleNode("./BillAddressBlock/Addr4").InnerText;
                }
                //Get value of Addr5
                if (SalesOrderRet.SelectSingleNode("./BillAddressBlock/Addr5") != null)
                {
                    string Addr5 = SalesOrderRet.SelectSingleNode("./BillAddressBlock/Addr5").InnerText;
                }
            }
            //Done with field values for BillAddressBlock aggregate

            //Get all field values for ShipAddress aggregate
            XmlNode ShipAddress = SalesOrderRet.SelectSingleNode("./ShipAddress");
            if (ShipAddress != null)
            {
                //Get value of Addr1
                if (SalesOrderRet.SelectSingleNode("./ShipAddress/Addr1") != null)
                {
                    string Addr1 = SalesOrderRet.SelectSingleNode("./ShipAddress/Addr1").InnerText;
                }
                //Get value of Addr2
                if (SalesOrderRet.SelectSingleNode("./ShipAddress/Addr2") != null)
                {
                    string Addr2 = SalesOrderRet.SelectSingleNode("./ShipAddress/Addr2").InnerText;
                }
                //Get value of Addr3
                if (SalesOrderRet.SelectSingleNode("./ShipAddress/Addr3") != null)
                {
                    string Addr3 = SalesOrderRet.SelectSingleNode("./ShipAddress/Addr3").InnerText;
                }
                //Get value of Addr4
                if (SalesOrderRet.SelectSingleNode("./ShipAddress/Addr4") != null)
                {
                    string Addr4 = SalesOrderRet.SelectSingleNode("./ShipAddress/Addr4").InnerText;
                }
                //Get value of Addr5
                if (SalesOrderRet.SelectSingleNode("./ShipAddress/Addr5") != null)
                {
                    string Addr5 = SalesOrderRet.SelectSingleNode("./ShipAddress/Addr5").InnerText;
                }
                //Get value of City
                if (SalesOrderRet.SelectSingleNode("./ShipAddress/City") != null)
                {
                    string City = SalesOrderRet.SelectSingleNode("./ShipAddress/City").InnerText;
                }
                //Get value of State
                if (SalesOrderRet.SelectSingleNode("./ShipAddress/State") != null)
                {
                    string State = SalesOrderRet.SelectSingleNode("./ShipAddress/State").InnerText;
                }
                //Get value of PostalCode
                if (SalesOrderRet.SelectSingleNode("./ShipAddress/PostalCode") != null)
                {
                    string PostalCode = SalesOrderRet.SelectSingleNode("./ShipAddress/PostalCode").InnerText;
                }
                //Get value of Country
                if (SalesOrderRet.SelectSingleNode("./ShipAddress/Country") != null)
                {
                    string Country = SalesOrderRet.SelectSingleNode("./ShipAddress/Country").InnerText;
                }
                //Get value of Note
                if (SalesOrderRet.SelectSingleNode("./ShipAddress/Note") != null)
                {
                    string Note = SalesOrderRet.SelectSingleNode("./ShipAddress/Note").InnerText;
                }
            }
            //Done with field values for ShipAddress aggregate

            //Get all field values for ShipAddressBlock aggregate
            XmlNode ShipAddressBlock = SalesOrderRet.SelectSingleNode("./ShipAddressBlock");
            if (ShipAddressBlock != null)
            {
                //Get value of Addr1
                if (SalesOrderRet.SelectSingleNode("./ShipAddressBlock/Addr1") != null)
                {
                    string Addr1 = SalesOrderRet.SelectSingleNode("./ShipAddressBlock/Addr1").InnerText;
                }
                //Get value of Addr2
                if (SalesOrderRet.SelectSingleNode("./ShipAddressBlock/Addr2") != null)
                {
                    string Addr2 = SalesOrderRet.SelectSingleNode("./ShipAddressBlock/Addr2").InnerText;
                }
                //Get value of Addr3
                if (SalesOrderRet.SelectSingleNode("./ShipAddressBlock/Addr3") != null)
                {
                    string Addr3 = SalesOrderRet.SelectSingleNode("./ShipAddressBlock/Addr3").InnerText;
                }
                //Get value of Addr4
                if (SalesOrderRet.SelectSingleNode("./ShipAddressBlock/Addr4") != null)
                {
                    string Addr4 = SalesOrderRet.SelectSingleNode("./ShipAddressBlock/Addr4").InnerText;
                }
                //Get value of Addr5
                if (SalesOrderRet.SelectSingleNode("./ShipAddressBlock/Addr5") != null)
                {
                    string Addr5 = SalesOrderRet.SelectSingleNode("./ShipAddressBlock/Addr5").InnerText;
                }
            }
            //Done with field values for ShipAddressBlock aggregate

            //Get value of PONumber
            if (SalesOrderRet.SelectSingleNode("./PONumber") != null)
            {
                string PONumber = SalesOrderRet.SelectSingleNode("./PONumber").InnerText;
            }
            //Get all field values for TermsRef aggregate
            XmlNode TermsRef = SalesOrderRet.SelectSingleNode("./TermsRef");
            if (TermsRef != null)
            {
                //Get value of ListID
                if (SalesOrderRet.SelectSingleNode("./TermsRef/ListID") != null)
                {
                    string ListID = SalesOrderRet.SelectSingleNode("./TermsRef/ListID").InnerText;
                }
                //Get value of FullName
                if (SalesOrderRet.SelectSingleNode("./TermsRef/FullName") != null)
                {
                    string FullName = SalesOrderRet.SelectSingleNode("./TermsRef/FullName").InnerText;
                }
            }
            //Done with field values for TermsRef aggregate

            //Get value of DueDate
            if (SalesOrderRet.SelectSingleNode("./DueDate") != null)
            {
                string DueDate = SalesOrderRet.SelectSingleNode("./DueDate").InnerText;
            }
            //Get all field values for SalesRepRef aggregate
            XmlNode SalesRepRef = SalesOrderRet.SelectSingleNode("./SalesRepRef");
            if (SalesRepRef != null)
            {
                //Get value of ListID
                if (SalesOrderRet.SelectSingleNode("./SalesRepRef/ListID") != null)
                {
                    string ListID = SalesOrderRet.SelectSingleNode("./SalesRepRef/ListID").InnerText;
                }
                //Get value of FullName
                if (SalesOrderRet.SelectSingleNode("./SalesRepRef/FullName") != null)
                {
                    string FullName = SalesOrderRet.SelectSingleNode("./SalesRepRef/FullName").InnerText;
                }
            }
            //Done with field values for SalesRepRef aggregate

            //Get value of FOB
            if (SalesOrderRet.SelectSingleNode("./FOB") != null)
            {
                string FOB = SalesOrderRet.SelectSingleNode("./FOB").InnerText;
            }
            //Get value of ShipDate
            if (SalesOrderRet.SelectSingleNode("./ShipDate") != null)
            {
                string ShipDate = SalesOrderRet.SelectSingleNode("./ShipDate").InnerText;
            }
            //Get all field values for ShipMethodRef aggregate
            XmlNode ShipMethodRef = SalesOrderRet.SelectSingleNode("./ShipMethodRef");
            if (ShipMethodRef != null)
            {
                //Get value of ListID
                if (SalesOrderRet.SelectSingleNode("./ShipMethodRef/ListID") != null)
                {
                    string ListID = SalesOrderRet.SelectSingleNode("./ShipMethodRef/ListID").InnerText;
                }
                //Get value of FullName
                if (SalesOrderRet.SelectSingleNode("./ShipMethodRef/FullName") != null)
                {
                    string FullName = SalesOrderRet.SelectSingleNode("./ShipMethodRef/FullName").InnerText;
                }
            }
            //Done with field values for ShipMethodRef aggregate

            //Get value of Subtotal
            if (SalesOrderRet.SelectSingleNode("./Subtotal") != null)
            {
                string Subtotal = SalesOrderRet.SelectSingleNode("./Subtotal").InnerText;
            }
            //Get all field values for ItemSalesTaxRef aggregate
            XmlNode ItemSalesTaxRef = SalesOrderRet.SelectSingleNode("./ItemSalesTaxRef");
            if (ItemSalesTaxRef != null)
            {
                //Get value of ListID
                if (SalesOrderRet.SelectSingleNode("./ItemSalesTaxRef/ListID") != null)
                {
                    string ListID = SalesOrderRet.SelectSingleNode("./ItemSalesTaxRef/ListID").InnerText;
                }
                //Get value of FullName
                if (SalesOrderRet.SelectSingleNode("./ItemSalesTaxRef/FullName") != null)
                {
                    string FullName = SalesOrderRet.SelectSingleNode("./ItemSalesTaxRef/FullName").InnerText;
                }
            }
            //Done with field values for ItemSalesTaxRef aggregate

            //Get value of SalesTaxPercentage
            if (SalesOrderRet.SelectSingleNode("./SalesTaxPercentage") != null)
            {
                string SalesTaxPercentage = SalesOrderRet.SelectSingleNode("./SalesTaxPercentage").InnerText;
            }
            //Get value of SalesTaxTotal
            if (SalesOrderRet.SelectSingleNode("./SalesTaxTotal") != null)
            {
                string SalesTaxTotal = SalesOrderRet.SelectSingleNode("./SalesTaxTotal").InnerText;
            }
            //Get value of TotalAmount
            string TotalAmount = "";
            if (SalesOrderRet.SelectSingleNode("./TotalAmount") != null)
            {
                TotalAmount = SalesOrderRet.SelectSingleNode("./TotalAmount").InnerText;
            }
            //Get all field values for CurrencyRef aggregate
            XmlNode CurrencyRef = SalesOrderRet.SelectSingleNode("./CurrencyRef");
            if (CurrencyRef != null)
            {
                //Get value of ListID
                if (SalesOrderRet.SelectSingleNode("./CurrencyRef/ListID") != null)
                {
                    string ListID = SalesOrderRet.SelectSingleNode("./CurrencyRef/ListID").InnerText;
                }
                //Get value of FullName
                if (SalesOrderRet.SelectSingleNode("./CurrencyRef/FullName") != null)
                {
                    string FullName = SalesOrderRet.SelectSingleNode("./CurrencyRef/FullName").InnerText;
                }
            }
            //Done with field values for CurrencyRef aggregate

            //Get value of ExchangeRate
            if (SalesOrderRet.SelectSingleNode("./ExchangeRate") != null)
            {
                string ExchangeRate = SalesOrderRet.SelectSingleNode("./ExchangeRate").InnerText;
            }
            //Get value of TotalAmountInHomeCurrency
            if (SalesOrderRet.SelectSingleNode("./TotalAmountInHomeCurrency") != null)
            {
                string TotalAmountInHomeCurrency = SalesOrderRet.SelectSingleNode("./TotalAmountInHomeCurrency").InnerText;
            }
            //Get value of IsManuallyClosed
            string IsManuallyClosed = "";
            if (SalesOrderRet.SelectSingleNode("./IsManuallyClosed") != null)
            {
                IsManuallyClosed = SalesOrderRet.SelectSingleNode("./IsManuallyClosed").InnerText;
            }
            //Get value of IsFullyInvoiced
            string IsFullyInvoiced = "";
            if (SalesOrderRet.SelectSingleNode("./IsFullyInvoiced") != null)
            {
                IsFullyInvoiced = SalesOrderRet.SelectSingleNode("./IsFullyInvoiced").InnerText;
            }
            //Get value of Memo
            if (SalesOrderRet.SelectSingleNode("./Memo") != null)
            {
                string Memo = SalesOrderRet.SelectSingleNode("./Memo").InnerText;
            }
            //Get all field values for CustomerMsgRef aggregate
            XmlNode CustomerMsgRef = SalesOrderRet.SelectSingleNode("./CustomerMsgRef");
            if (CustomerMsgRef != null)
            {
                //Get value of ListID
                if (SalesOrderRet.SelectSingleNode("./CustomerMsgRef/ListID") != null)
                {
                    string ListID = SalesOrderRet.SelectSingleNode("./CustomerMsgRef/ListID").InnerText;
                }
                //Get value of FullName
                if (SalesOrderRet.SelectSingleNode("./CustomerMsgRef/FullName") != null)
                {
                    string FullName = SalesOrderRet.SelectSingleNode("./CustomerMsgRef/FullName").InnerText;
                }
            }
            //Done with field values for CustomerMsgRef aggregate

            //Get value of IsToBePrinted
            if (SalesOrderRet.SelectSingleNode("./IsToBePrinted") != null)
            {
                string IsToBePrinted = SalesOrderRet.SelectSingleNode("./IsToBePrinted").InnerText;
            }
            //Get value of IsToBeEmailed
            if (SalesOrderRet.SelectSingleNode("./IsToBeEmailed") != null)
            {
                string IsToBeEmailed = SalesOrderRet.SelectSingleNode("./IsToBeEmailed").InnerText;
            }
            //Get all field values for CustomerSalesTaxCodeRef aggregate
            XmlNode CustomerSalesTaxCodeRef = SalesOrderRet.SelectSingleNode("./CustomerSalesTaxCodeRef");
            if (CustomerSalesTaxCodeRef != null)
            {
                //Get value of ListID
                if (SalesOrderRet.SelectSingleNode("./CustomerSalesTaxCodeRef/ListID") != null)
                {
                    string ListID = SalesOrderRet.SelectSingleNode("./CustomerSalesTaxCodeRef/ListID").InnerText;
                }
                //Get value of FullName
                if (SalesOrderRet.SelectSingleNode("./CustomerSalesTaxCodeRef/FullName") != null)
                {
                    string FullName = SalesOrderRet.SelectSingleNode("./CustomerSalesTaxCodeRef/FullName").InnerText;
                }
            }
            //Done with field values for CustomerSalesTaxCodeRef aggregate

            //Get value of Other
            if (SalesOrderRet.SelectSingleNode("./Other") != null)
            {
                string Other = SalesOrderRet.SelectSingleNode("./Other").InnerText;
            }
            //Get value of ExternalGUID
            if (SalesOrderRet.SelectSingleNode("./ExternalGUID") != null)
            {
                string ExternalGUID = SalesOrderRet.SelectSingleNode("./ExternalGUID").InnerText;
            }
            //Walk list of LinkedTxn aggregates
            XmlNodeList LinkedTxnList = SalesOrderRet.SelectNodes("./LinkedTxn");
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

            XmlNodeList ORSalesOrderLineRetListChildren = SalesOrderRet.SelectNodes("./*");
            for (int i = 0; i < ORSalesOrderLineRetListChildren.Count; i++)
            {
                XmlNode Child = ORSalesOrderLineRetListChildren.Item(i);
                if (Child.Name == "SalesOrderLineRet")
                {
                    //Get value of TxnLineID
                    string TxnLineID = Child.SelectSingleNode("./TxnLineID").InnerText;

                    // Find existing or create new SalesOrderLine entry
                    SalesOrderLine sol = FindOrCreateSalesOrderLine(db, TxnLineID, TxnID, TimeCreated, TimeModified, EditSequence, TxnDate, TotalAmount, IsManuallyClosed, IsFullyInvoiced);

                    //Get all field values for ItemRef aggregate
                    XmlNode ItemRef = Child.SelectSingleNode("./ItemRef");
                    if (ItemRef != null)
                    {
                        //Get value of ListID
                        if (Child.SelectSingleNode("./ItemRef/ListID") != null)
                        {
                            string ListID = Child.SelectSingleNode("./ItemRef/ListID").InnerText;
                            sol.ItemListID = ListID;
                        }
                        //Get value of FullName
                        if (Child.SelectSingleNode("./ItemRef/FullName") != null)
                        {
                            string FullName = Child.SelectSingleNode("./ItemRef/FullName").InnerText;
                            sol.ItemName = FullName;
                        }
                    }
                    //Done with field values for ItemRef aggregate

                    //Get value of Desc
                    if (Child.SelectSingleNode("./Desc") != null)
                    {
                        string Desc = Child.SelectSingleNode("./Desc").InnerText;
                        sol.Description = Desc;
                    }
                    //Get value of Quantity
                    if (Child.SelectSingleNode("./Quantity") != null)
                    {
                        string Quantity = Child.SelectSingleNode("./Quantity").InnerText;
                        int quantity;
                        if (Int32.TryParse(Quantity, out quantity))
                        {
                            sol.Quantity = quantity;
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
                            sol.UnitCost = rate;
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
                            sol.AreaListID = ListID;
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
                            sol.Amount = amount;
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

                    sol.WorkOrderListID = CustomerListID;
                    sol.BillableStatus = "Billable";
                    db.SaveChanges();
                }

                if (Child.Name == "SalesOrderLineGroupRet")
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
                    //Walk list of SalesOrderLineRet aggregates
                    XmlNodeList SalesOrderLineRetList = Child.SelectNodes("./SalesOrderLineRet");
                    if (SalesOrderLineRetList != null)
                    {
                        for (int k = 0; k < SalesOrderLineRetList.Count; k++)
                        {
                            XmlNode SalesOrderLineRet = SalesOrderLineRetList.Item(k);
                            //Get value of TxnLineID
                            string TxnLineID2 = SalesOrderLineRet.SelectSingleNode("./TxnLineID").InnerText;

                            // Find existing or create new SalesOrderLine entry
                            SalesOrderLine sol = FindOrCreateSalesOrderLine(db, TxnLineID2, TxnID, TimeCreated, TimeModified, EditSequence, TxnDate, TotalAmount, IsManuallyClosed, IsFullyInvoiced);

                            //Get all field values for ItemRef aggregate
                            XmlNode ItemRef = SalesOrderLineRet.SelectSingleNode("./ItemRef");
                            if (ItemRef != null)
                            {
                                //Get value of ListID
                                if (SalesOrderLineRet.SelectSingleNode("./ItemRef/ListID") != null)
                                {
                                    string ListID = SalesOrderLineRet.SelectSingleNode("./ItemRef/ListID").InnerText;
                                    sol.ItemListID = ListID;
                                }
                                //Get value of FullName
                                if (SalesOrderLineRet.SelectSingleNode("./ItemRef/FullName") != null)
                                {
                                    string FullName = SalesOrderLineRet.SelectSingleNode("./ItemRef/FullName").InnerText;
                                    sol.ItemName = FullName;
                                }
                            }
                            //Done with field values for ItemRef aggregate

                            //Get value of Desc
                            if (SalesOrderLineRet.SelectSingleNode("./Desc") != null)
                            {
                                string Desc = SalesOrderLineRet.SelectSingleNode("./Desc").InnerText;
                                sol.Description = Desc;
                            }
                            //Get value of Quantity
                            if (SalesOrderLineRet.SelectSingleNode("./Quantity") != null)
                            {
                                string Quantity = SalesOrderLineRet.SelectSingleNode("./Quantity").InnerText;
                                Int32 quantity;
                                if (Int32.TryParse(Quantity, out quantity))
                                {
                                    sol.Quantity = quantity;
                                }
                            }
                            //Get value of UnitOfMeasure
                            if (SalesOrderLineRet.SelectSingleNode("./UnitOfMeasure") != null)
                            {
                                string UnitOfMeasure = SalesOrderLineRet.SelectSingleNode("./UnitOfMeasure").InnerText;
                            }
                            //Get all field values for OverrideUOMSetRef aggregate
                            XmlNode OverrideUOMSetRef2 = SalesOrderLineRet.SelectSingleNode("./OverrideUOMSetRef");
                            if (OverrideUOMSetRef2 != null)
                            {
                                //Get value of ListID
                                if (SalesOrderLineRet.SelectSingleNode("./OverrideUOMSetRef/ListID") != null)
                                {
                                    string ListID = SalesOrderLineRet.SelectSingleNode("./OverrideUOMSetRef/ListID").InnerText;
                                }
                                //Get value of FullName
                                if (SalesOrderLineRet.SelectSingleNode("./OverrideUOMSetRef/FullName") != null)
                                {
                                    string FullName = SalesOrderLineRet.SelectSingleNode("./OverrideUOMSetRef/FullName").InnerText;
                                }
                            }
                            //Done with field values for OverrideUOMSetRef aggregate

                            //Done with field values for OverrideUOMSetRef aggregate
                            if (SalesOrderLineRet.SelectSingleNode("./Rate") != null)
                            {
                                string Rate = SalesOrderLineRet.SelectSingleNode("./Rate").InnerText;
                                decimal rate;
                                if (decimal.TryParse(Rate, out rate))
                                {
                                    sol.UnitCost = rate;
                                }
                            }

                            //Get all field values for ClassRef aggregate
                            XmlNode ClassRef2 = SalesOrderLineRet.SelectSingleNode("./ClassRef");
                            if (ClassRef != null)
                            {
                                //Get value of ListID
                                if (SalesOrderLineRet.SelectSingleNode("./ClassRef/ListID") != null)
                                {
                                    string ListID = SalesOrderLineRet.SelectSingleNode("./ClassRef/ListID").InnerText;
                                    sol.AreaListID = ListID;
                                }
                                //Get value of FullName
                                if (SalesOrderLineRet.SelectSingleNode("./ClassRef/FullName") != null)
                                {
                                    string FullName = SalesOrderLineRet.SelectSingleNode("./ClassRef/FullName").InnerText;
                                }
                            }
                            //Done with field values for ClassRef aggregate

                            //Get value of Amount
                            if (SalesOrderLineRet.SelectSingleNode("./Amount") != null)
                            {
                                string Amount = SalesOrderLineRet.SelectSingleNode("./Amount").InnerText;
                                decimal amount;
                                if (decimal.TryParse(Amount, out amount))
                                {
                                    sol.Amount = amount;
                                }
                            }
                            //Get all field values for InventorySiteRef aggregate
                            XmlNode InventorySiteRef = SalesOrderLineRet.SelectSingleNode("./InventorySiteRef");
                            if (InventorySiteRef != null)
                            {
                                //Get value of ListID
                                if (SalesOrderLineRet.SelectSingleNode("./InventorySiteRef/ListID") != null)
                                {
                                    string ListID = SalesOrderLineRet.SelectSingleNode("./InventorySiteRef/ListID").InnerText;
                                }
                                //Get value of FullName
                                if (SalesOrderLineRet.SelectSingleNode("./InventorySiteRef/FullName") != null)
                                {
                                    string FullName = SalesOrderLineRet.SelectSingleNode("./InventorySiteRef/FullName").InnerText;
                                }
                            }
                            //Done with field values for InventorySiteRef aggregate

                            //Get all field values for InventorySiteLocationRef aggregate
                            XmlNode InventorySiteLocationRef = SalesOrderLineRet.SelectSingleNode("./InventorySiteLocationRef");
                            if (InventorySiteLocationRef != null)
                            {
                                //Get value of ListID
                                if (SalesOrderLineRet.SelectSingleNode("./InventorySiteLocationRef/ListID") != null)
                                {
                                    string ListID = SalesOrderLineRet.SelectSingleNode("./InventorySiteLocationRef/ListID").InnerText;
                                }
                                //Get value of FullName
                                if (SalesOrderLineRet.SelectSingleNode("./InventorySiteLocationRef/FullName") != null)
                                {
                                    string FullName = SalesOrderLineRet.SelectSingleNode("./InventorySiteLocationRef/FullName").InnerText;
                                }
                            }
                            //Done with field values for InventorySiteLocationRef aggregate

                            //Get all field values for SalesTaxCodeRef aggregate
                            XmlNode SalesTaxCodeRef = SalesOrderLineRet.SelectSingleNode("./SalesTaxCodeRef");
                            if (SalesTaxCodeRef != null)
                            {
                                //Get value of ListID
                                if (SalesOrderLineRet.SelectSingleNode("./SalesTaxCodeRef/ListID") != null)
                                {
                                    string ListID = SalesOrderLineRet.SelectSingleNode("./SalesTaxCodeRef/ListID").InnerText;
                                }
                                //Get value of FullName
                                if (SalesOrderLineRet.SelectSingleNode("./SalesTaxCodeRef/FullName") != null)
                                {
                                    string FullName = SalesOrderLineRet.SelectSingleNode("./SalesTaxCodeRef/FullName").InnerText;
                                }
                            }
                            //Done with field values for SalesTaxCodeRef aggregate

                            //Get value of Invoiced
                            if (SalesOrderLineRet.SelectSingleNode("./Invoiced") != null)
                            {
                                string Invoiced = SalesOrderLineRet.SelectSingleNode("./Invoiced").InnerText;
                            }
                            //Get value of IsManuallyClosed
                            if (SalesOrderLineRet.SelectSingleNode("./IsManuallyClosed") != null)
                            {
                                string IsManuallyClosed2 = SalesOrderLineRet.SelectSingleNode("./IsManuallyClosed").InnerText;
                            }
                            //Get value of Other1
                            if (SalesOrderLineRet.SelectSingleNode("./Other1") != null)
                            {
                                string Other1 = SalesOrderLineRet.SelectSingleNode("./Other1").InnerText;
                            }
                            //Get value of Other2
                            if (SalesOrderLineRet.SelectSingleNode("./Other2") != null)
                            {
                                string Other2 = SalesOrderLineRet.SelectSingleNode("./Other2").InnerText;
                            }
                            //Walk list of DataExtRet aggregates
                            XmlNodeList DataExtRetList = SalesOrderLineRet.SelectNodes("./DataExtRet");
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

                            sol.WorkOrderListID = CustomerListID;
                            sol.BillableStatus = "Billable";
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

        private static SalesOrderLine FindOrCreateSalesOrderLine(RotoTrackDb db, string TxnLineID, string TxnID, string TimeCreated, string TimeModified, string EditSequence, string TxnDate, string AmountDue, string IsManuallyClosed, string IsFullyInvoiced)
        {
            SalesOrderLine sol = null;
            if (db.SalesOrderLines.Any(f => f.TxnLineId == TxnLineID))
            {
                sol = db.SalesOrderLines.First(f => f.TxnLineId == TxnLineID);
            }
            else
            {
                sol = new SalesOrderLine();
                db.SalesOrderLines.Add(sol);
            }

            sol.TxnLineId = TxnLineID;
            sol.SalesOrderTxnId = TxnID;
            DateTime createdDate;
            if (DateTime.TryParse(TimeCreated, out createdDate)) sol.SalesOrderCreated = createdDate;
            DateTime modifiedDate;
            if (DateTime.TryParse(TimeModified, out modifiedDate)) sol.SalesOrderModified = modifiedDate;
            sol.SalesOrderEditSequence = EditSequence;
            DateTime txnDate;
            if (DateTime.TryParse(TxnDate, out txnDate)) sol.SalesOrderTxnDate = txnDate;
            decimal amountDue;
            if (Decimal.TryParse(AmountDue, out amountDue)) sol.SalesOrderTotalAmount = amountDue;

            bool isManuallyClosed;
            if (bool.TryParse(IsManuallyClosed, out isManuallyClosed))
            {
                sol.SalesOrderIsManuallyClosed = isManuallyClosed;
            }
            bool isFullyInvoiced;
            if (bool.TryParse(IsFullyInvoiced, out isFullyInvoiced))
            {
                sol.SalesOrderIsFullyInvoiced = isFullyInvoiced;
            }

            return sol;
        }

    }
}
