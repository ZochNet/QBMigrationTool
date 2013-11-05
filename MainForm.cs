
/*-----------------------------------------------------------
 * CustomerAddForm : implementation file
 *
 * Description:  This sample demonstrates the simple use 
 *               QuickBooks qbXMLRP COM object
 *				 Also it shows how to create and parse qbXML 	
 *				 using .NET XML classes
 *
 * Created On: 8/15/2002
 *
 * Copyright © 2002-2012 Intuit Inc. All rights reserved.
 * Use is subject to the terms specified at:
 *      http://developer.intuit.com/legal/devsite_tos.html
 *
 *----------------------------------------------------------
 */ 


using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Xml;
using Interop.QBXMLRP2;
using System.Collections.Generic;

namespace CustomerAdd
{
	/// <summary>
	/// CustomerAddForm shows how to invoke QuickBooks qbXMLRP COM object
	/// It uses .NET to create qbXML request and parse qbXML response
	/// </summary>
	public class MainForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button Exit;
		private System.Windows.Forms.TextBox tbQBListID;
		private System.Windows.Forms.Label label1;
        private Button SyncAllActiveMissingSiteData;
        private TextBox tbResults;
        private Button btnQueryCustomer;
        private Button btnSyncCustomerSite;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public MainForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.label3 = new System.Windows.Forms.Label();
            this.Exit = new System.Windows.Forms.Button();
            this.tbQBListID = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SyncAllActiveMissingSiteData = new System.Windows.Forms.Button();
            this.tbResults = new System.Windows.Forms.TextBox();
            this.btnQueryCustomer = new System.Windows.Forms.Button();
            this.btnSyncCustomerSite = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(12, 80);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(333, 17);
            this.label3.TabIndex = 13;
            this.label3.Text = "Note: You need to have QuickBooks with a company file opened.";
            // 
            // Exit
            // 
            this.Exit.Location = new System.Drawing.Point(669, 74);
            this.Exit.Name = "Exit";
            this.Exit.Size = new System.Drawing.Size(120, 24);
            this.Exit.TabIndex = 12;
            this.Exit.Text = "Exit";
            this.Exit.Click += new System.EventHandler(this.Exit_Click);
            // 
            // tbQBListID
            // 
            this.tbQBListID.Location = new System.Drawing.Point(80, 16);
            this.tbQBListID.Name = "tbQBListID";
            this.tbQBListID.Size = new System.Drawing.Size(213, 20);
            this.tbQBListID.TabIndex = 8;
            this.tbQBListID.Text = "80000B28-1383332088";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 16);
            this.label1.TabIndex = 7;
            this.label1.Text = "QB ListID";
            // 
            // SyncAllActiveMissingSiteData
            // 
            this.SyncAllActiveMissingSiteData.Location = new System.Drawing.Point(606, 42);
            this.SyncAllActiveMissingSiteData.Name = "SyncAllActiveMissingSiteData";
            this.SyncAllActiveMissingSiteData.Size = new System.Drawing.Size(183, 23);
            this.SyncAllActiveMissingSiteData.TabIndex = 14;
            this.SyncAllActiveMissingSiteData.Text = "Sync All Active Missing Site Data";
            this.SyncAllActiveMissingSiteData.UseVisualStyleBackColor = true;
            this.SyncAllActiveMissingSiteData.Click += new System.EventHandler(this.btnSyncSitesToCustomFields_Click);
            // 
            // tbResults
            // 
            this.tbResults.Location = new System.Drawing.Point(15, 104);
            this.tbResults.Multiline = true;
            this.tbResults.Name = "tbResults";
            this.tbResults.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbResults.Size = new System.Drawing.Size(774, 421);
            this.tbResults.TabIndex = 15;
            // 
            // btnQueryCustomer
            // 
            this.btnQueryCustomer.Location = new System.Drawing.Point(12, 42);
            this.btnQueryCustomer.Name = "btnQueryCustomer";
            this.btnQueryCustomer.Size = new System.Drawing.Size(139, 23);
            this.btnQueryCustomer.TabIndex = 16;
            this.btnQueryCustomer.Text = "Query Ship To / Site Data";
            this.btnQueryCustomer.UseVisualStyleBackColor = true;
            this.btnQueryCustomer.Click += new System.EventHandler(this.btnQueryCustomerSite_Click);
            // 
            // btnSyncCustomerSite
            // 
            this.btnSyncCustomerSite.Location = new System.Drawing.Point(157, 42);
            this.btnSyncCustomerSite.Name = "btnSyncCustomerSite";
            this.btnSyncCustomerSite.Size = new System.Drawing.Size(136, 23);
            this.btnSyncCustomerSite.TabIndex = 17;
            this.btnSyncCustomerSite.Text = "Sync Ship To/Site Data";
            this.btnSyncCustomerSite.UseVisualStyleBackColor = true;
            this.btnSyncCustomerSite.Click += new System.EventHandler(this.btnSyncCustomerSite_Click);
            // 
            // MainForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(801, 537);
            this.Controls.Add(this.btnSyncCustomerSite);
            this.Controls.Add(this.btnQueryCustomer);
            this.Controls.Add(this.tbResults);
            this.Controls.Add(this.SyncAllActiveMissingSiteData);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.Exit);
            this.Controls.Add(this.tbQBListID);
            this.Controls.Add(this.label1);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Manage Custom Site Fields for Customers";
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion
		static void Main() 
		{
			Application.Run(new MainForm());
		}

		private void Exit_Click(object sender, System.EventArgs e)
		{
			this.Close();

		}

        private XmlElement MakeSimpleElem(XmlDocument doc, string tagName, string tagVal)
        {
            XmlElement elem = doc.CreateElement(tagName);
            elem.InnerText = tagVal;
            return elem;
        }

        private void btnQueryCustomerSite_Click(object sender, EventArgs e)
        {
            String listID = tbQBListID.Text.Trim();
            if (listID.Length == 0)
            {
                MessageBox.Show("Please enter a QB ListID value for a customer.", "Input Validation");
                return;
            }
            try
            {
                string response = DoRequest(BuildCustomerQuery(listID));

                tbResults.Text = "";

                Site site = GetSiteFromResponse(response);
                AppendSiteDetails(tbResults, site);

                AppendNewLine(tbResults);

                ShipTo shipTo = GetShipToFromResponse(response);
                AppendShipToDetails(tbResults, shipTo);
            }
            catch (Exception ex)
            {
                tbResults.Text = ex.Message;
            }
        }

        private void btnSyncCustomerSite_Click(object sender, EventArgs e)
        {
            String listID = tbQBListID.Text.Trim();
            if (listID.Length == 0)
            {
                MessageBox.Show("Please enter a QB ListID value for a customer.", "Input Validation");
                return;
            }
            try
            {
                string response = DoRequest(BuildCustomerQuery(listID));
                ShipTo shipTo = GetShipToFromResponse(response);
                response = DoRequest(BuildSyncShipToToCustomSiteQuery(listID, shipTo));
                tbResults.Text = "Done";
            }
            catch (Exception ex)
            {
                tbResults.Text = ex.Message;
            }
            
        }

        private void btnSyncSitesToCustomFields_Click(object sender, EventArgs e)
        {
            try
            {
                string response = DoRequest(BuildCustomerQueryAllActive());
                List<Customer> activeCustomers = GetActiveCustomersFromResponse(response);

                foreach (Customer customer in activeCustomers)
                {
                    response = DoRequest(BuildSyncShipToToCustomSiteQuery(customer.ListID, customer.ShipTo));
                }

                tbResults.Text = "Done";
            }
            catch (Exception ex)
            {
                tbResults.Text = ex.Message;
            }
        }

        private void AppendSiteDetails(TextBox tb, Site site)
        {
            tb.Text += "Site Details";
            AppendNewLine(tb);

            tb.Text += "Name: ";
            tb.Text += site.Name;
            tb.Text += "\r\n";

            tb.Text += "Unit Number: ";
            tb.Text += site.UnitNumber;
            tb.Text += "\r\n";

            tb.Text += "County: ";
            tb.Text += site.County;
            tb.Text += "\r\n";

            tb.Text += "City/State: ";
            tb.Text += site.CityState;
            tb.Text += "\r\n";

            tb.Text += "POAFE Number: ";
            tb.Text += site.POAFENumber;
            tb.Text += "\r\n";
        }

        private void AppendShipToDetails(TextBox tb, ShipTo shipTo)
        {
            tb.Text += "Ship To Details";
            AppendNewLine(tb);

            tb.Text += "Name: ";
            tb.Text += shipTo.Name;
            tb.Text += "\r\n";

            tb.Text += "Addr1: ";
            tb.Text += shipTo.Addr1;
            tb.Text += "\r\n";

            tb.Text += "Addr2: ";
            tb.Text += shipTo.Addr2;
            tb.Text += "\r\n";

            tb.Text += "Addr3: ";
            tb.Text += shipTo.Addr3;
            tb.Text += "\r\n";

            tb.Text += "City: ";
            tb.Text += shipTo.City;
            tb.Text += "\r\n";

            tb.Text += "State: ";
            tb.Text += shipTo.State;
            tb.Text += "\r\n";

            tb.Text += "Note: ";
            tb.Text += shipTo.Note;
            tb.Text += "\r\n";
        }

        private void AppendNewLine(TextBox tb)
        {
            tb.Text += "\r\n";
        }

        private Site GetSiteFromCustomerRet(XmlNode CustomerRet)
        {
            Site site = new Site();

            //Walk list of DataExtRet aggregates
            XmlNodeList DataExtRetList = CustomerRet.SelectNodes("./DataExtRet");
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

                    switch (DataExtName)
                    {
                        case "Site Name":
                            site.Name = DataExtValue;
                            break;
                        case "Site Unit Number":
                            site.UnitNumber = DataExtValue;
                            break;

                        case "Site County":
                            site.County = DataExtValue;
                            break;

                        case "Site City/State":
                            site.CityState = DataExtValue;
                            break;

                        case "Site POAFE":
                            site.POAFENumber = DataExtValue;
                            break;
                    }
                }
            }

            return site;
        }

        private List<Customer> GetActiveCustomersFromResponse(string response)
        {
            List<Customer> customers = new List<Customer>();

            //Parse the response XML string into an XmlDocument
            XmlDocument responseXmlDoc = new XmlDocument();
            responseXmlDoc.LoadXml(response);

            //Get the response for our request
            XmlNodeList CustomerQueryRsList = responseXmlDoc.GetElementsByTagName("CustomerQueryRs");
            if (CustomerQueryRsList.Count == 1) //Should always be true since we only did one request in this sample
            {
                Customer customer = new Customer();

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
                    for (int i = 0; i < CustomerRetList.Count; i++)
                    {
                        XmlNode CustomerRet = CustomerRetList.Item(i);
                        
                    }                    
                }
            }

            return customers;
        }

        private Site GetSiteFromResponse(string response)
        {
            Site site = new Site();

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
                        site = GetSiteFromCustomerRet(CustomerRet);
                    }
                }
            }

            return site;
        }

        private ShipTo GetShipToFromCustomerRet(XmlNode CustomerRet)
        {
            ShipTo shipTo = new ShipTo();

            XmlNodeList ShipToList = CustomerRet.SelectNodes("./ShipToAddress");
            if (ShipToList != null)
            {
                for (int i = 0; i < ShipToList.Count; i++)
                {
                    XmlNode ShipToRet = ShipToList.Item(i);

                    if (ShipToRet.SelectSingleNode("./DefaultShipTo") != null)
                    {
                        string DefaultShipTo = ShipToRet.SelectSingleNode("./DefaultShipTo").InnerText;

                        if (DefaultShipTo == "true")
                        {
                            shipTo.Name = ShipToRet.SelectSingleNode("./Name").InnerText;

                            if (ShipToRet.SelectSingleNode("./Addr1") != null)
                            {
                                shipTo.Addr1 = ShipToRet.SelectSingleNode("./Addr1").InnerText;
                            }
                            if (ShipToRet.SelectSingleNode("./Addr2") != null)
                            {
                                shipTo.Addr2 = ShipToRet.SelectSingleNode("./Addr2").InnerText;
                            }
                            if (ShipToRet.SelectSingleNode("./Addr3") != null)
                            {
                                shipTo.Addr3 = ShipToRet.SelectSingleNode("./Addr3").InnerText;
                            }
                            if (ShipToRet.SelectSingleNode("./City") != null)
                            {
                                shipTo.City = ShipToRet.SelectSingleNode("./City").InnerText;
                            }
                            if (ShipToRet.SelectSingleNode("./State") != null)
                            {
                                shipTo.State = ShipToRet.SelectSingleNode("./State").InnerText;
                            }
                            if (ShipToRet.SelectSingleNode("./Note") != null)
                            {
                                shipTo.Note = ShipToRet.SelectSingleNode("./Note").InnerText;
                            }
                        }
                    }
                }
            }

            return shipTo;
        }

        private ShipTo GetShipToFromResponse(string response)
        {
            ShipTo shipTo = new ShipTo();

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
                        shipTo = GetShipToFromCustomerRet(CustomerRet);                        
                    }
                }
            }

            return shipTo;
        }

        private XmlDocument BuildDataExtModRq(XmlDocument doc, XmlElement parent, string customerListID, string DataExtName, string DataExtValue)
        {
            XmlElement DataExtModRq = doc.CreateElement("DataExtModRq");
            parent.AppendChild(DataExtModRq);
            DataExtModRq.SetAttribute("requestID", "1");

            XmlElement DataExtMod = doc.CreateElement("DataExtMod");
            DataExtModRq.AppendChild(DataExtMod);
            DataExtMod.AppendChild(MakeSimpleElem(doc, "OwnerID", "0"));
            DataExtMod.AppendChild(MakeSimpleElem(doc, "DataExtName", DataExtName));
            DataExtMod.AppendChild(MakeSimpleElem(doc, "ListDataExtType", "Customer"));

            XmlElement ListObjRef = doc.CreateElement("ListObjRef");
            DataExtMod.AppendChild(ListObjRef);
            ListObjRef.AppendChild(MakeSimpleElem(doc, "ListID", customerListID));
            DataExtMod.AppendChild(MakeSimpleElem(doc, "DataExtValue", DataExtValue));

            return doc;
        }

        private XmlDocument BuildSyncShipToToCustomSiteQuery(string listID, ShipTo shipTo)
        {
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", null, null));
            doc.AppendChild(doc.CreateProcessingInstruction("qbxml", "version=\"12.0\""));
            XmlElement qbXML = doc.CreateElement("QBXML");
            doc.AppendChild(qbXML);
            XmlElement qbXMLMsgsRq = doc.CreateElement("QBXMLMsgsRq");
            qbXML.AppendChild(qbXMLMsgsRq);
            qbXMLMsgsRq.SetAttribute("onError", "stopOnError");

            doc = BuildDataExtModRq(doc, qbXMLMsgsRq, listID, "Site Name", shipTo.Addr1);
            doc = BuildDataExtModRq(doc, qbXMLMsgsRq, listID, "Site Unit Number", shipTo.Addr2);
            doc = BuildDataExtModRq(doc, qbXMLMsgsRq, listID, "Site County", shipTo.Addr3);
            string CityState = "";
            if ((shipTo.City != "") && (shipTo.State != "")) CityState = shipTo.City + ", " + shipTo.State;
            doc = BuildDataExtModRq(doc, qbXMLMsgsRq, listID, "Site City/State", CityState);
            doc = BuildDataExtModRq(doc, qbXMLMsgsRq, listID, "Site POAFE", shipTo.Note);

            return doc;
        }

        private XmlDocument BuildCustomerQuery(string listID)
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

            custAddRq.AppendChild(MakeSimpleElem(doc, "ListID", listID));
            custAddRq.AppendChild(MakeSimpleElem(doc, "OwnerID", "0"));

            return doc;
        }

        private XmlDocument BuildCustomerQueryAllActive()
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

            custAddRq.AppendChild(MakeSimpleElem(doc, "ActiveStatus", "ActiveOnly"));
            custAddRq.AppendChild(MakeSimpleElem(doc, "OwnerID", "0"));

            return doc;
        }

        private string DoRequest(XmlDocument doc)
        {         
            RequestProcessor2 rp = null;
            string ticket = null;
            string response = null;
            try
            {
                rp = new RequestProcessor2();
                rp.OpenConnection("QBMT1", "QBMigrationTool");
                ticket = rp.BeginSession("", QBFileMode.qbFileOpenDoNotCare);
                response = rp.ProcessRequest(ticket, doc.OuterXml);
                return response;
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                MessageBox.Show("COM Error Description = " + ex.Message, "COM error");
                return ex.Message;
            }
            finally
            {
                if (ticket != null)
                {
                    rp.EndSession(ticket);
                }
                if (rp != null)
                {
                    rp.CloseConnection();
                }
            }
        }
	}
}
