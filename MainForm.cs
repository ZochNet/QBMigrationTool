
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
using rototrack_data_access;
using rototrack_model;
using System.Linq;
using System.Timers;

namespace QBMigrationTool
{
	/// <summary>
	/// CustomerAddForm shows how to invoke QuickBooks qbXMLRP COM object
	/// It uses .NET to create qbXML request and parse qbXML response
	/// </summary>
	public class MainForm : System.Windows.Forms.Form
	{
        private System.Timers.Timer aTimer;

		private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button Exit;
		private System.Windows.Forms.TextBox tbQBListID;
		private System.Windows.Forms.Label label1;
        private Button btnSyncAllActiveMissingSiteDataOnly;
        private TextBox tbResults;
        private Button btnQueryCustomer;
        private Button btnSyncCustomerSite;
        private Button btnSyncAllActive;
        private Button btnGetQBListID;
        private TextBox tbWorkOrderNumber;
        private Label label2;
        private Button btnRunXml;
        private TextBox tbXML;
        private Label label4;
        private Button showItems;
        private Button button1;
        private Button btnSync;
        private Label labelBuildType;
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
            labelBuildType.Text = RototrackConfig.GetBuildType();
            //aTimer = new System.Timers.Timer(10000);
            aTimer = new System.Timers.Timer(300000);
            aTimer.Elapsed += aTimer_Elapsed;
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
            this.btnSyncAllActiveMissingSiteDataOnly = new System.Windows.Forms.Button();
            this.tbResults = new System.Windows.Forms.TextBox();
            this.btnQueryCustomer = new System.Windows.Forms.Button();
            this.btnSyncCustomerSite = new System.Windows.Forms.Button();
            this.btnSyncAllActive = new System.Windows.Forms.Button();
            this.btnGetQBListID = new System.Windows.Forms.Button();
            this.tbWorkOrderNumber = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnRunXml = new System.Windows.Forms.Button();
            this.tbXML = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.showItems = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.btnSync = new System.Windows.Forms.Button();
            this.labelBuildType = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(15, 164);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(303, 36);
            this.label3.TabIndex = 13;
            this.label3.Text = "Note: You need to have QuickBooks with a company file opened.";
            // 
            // Exit
            // 
            this.Exit.Location = new System.Drawing.Point(669, 16);
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
            this.tbQBListID.Size = new System.Drawing.Size(238, 20);
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
            // btnSyncAllActiveMissingSiteDataOnly
            // 
            this.btnSyncAllActiveMissingSiteDataOnly.Location = new System.Drawing.Point(486, 164);
            this.btnSyncAllActiveMissingSiteDataOnly.Name = "btnSyncAllActiveMissingSiteDataOnly";
            this.btnSyncAllActiveMissingSiteDataOnly.Size = new System.Drawing.Size(199, 23);
            this.btnSyncAllActiveMissingSiteDataOnly.TabIndex = 14;
            this.btnSyncAllActiveMissingSiteDataOnly.Text = "Sync All Active Missing Site Data Only";
            this.btnSyncAllActiveMissingSiteDataOnly.UseVisualStyleBackColor = true;
            this.btnSyncAllActiveMissingSiteDataOnly.Click += new System.EventHandler(this.btnSyncAllActiveMissingSiteDataOnly_Click);
            // 
            // tbResults
            // 
            this.tbResults.Location = new System.Drawing.Point(15, 193);
            this.tbResults.Multiline = true;
            this.tbResults.Name = "tbResults";
            this.tbResults.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbResults.Size = new System.Drawing.Size(774, 589);
            this.tbResults.TabIndex = 15;
            // 
            // btnQueryCustomer
            // 
            this.btnQueryCustomer.Location = new System.Drawing.Point(324, 16);
            this.btnQueryCustomer.Name = "btnQueryCustomer";
            this.btnQueryCustomer.Size = new System.Drawing.Size(156, 23);
            this.btnQueryCustomer.TabIndex = 16;
            this.btnQueryCustomer.Text = "Query Ship To / Site Data";
            this.btnQueryCustomer.UseVisualStyleBackColor = true;
            this.btnQueryCustomer.Click += new System.EventHandler(this.btnQueryCustomerSite_Click);
            // 
            // btnSyncCustomerSite
            // 
            this.btnSyncCustomerSite.Location = new System.Drawing.Point(486, 16);
            this.btnSyncCustomerSite.Name = "btnSyncCustomerSite";
            this.btnSyncCustomerSite.Size = new System.Drawing.Size(144, 23);
            this.btnSyncCustomerSite.TabIndex = 17;
            this.btnSyncCustomerSite.Text = "Sync Ship To/Site Data";
            this.btnSyncCustomerSite.UseVisualStyleBackColor = true;
            this.btnSyncCustomerSite.Click += new System.EventHandler(this.btnSyncCustomerSite_Click);
            // 
            // btnSyncAllActive
            // 
            this.btnSyncAllActive.Location = new System.Drawing.Point(691, 164);
            this.btnSyncAllActive.Name = "btnSyncAllActive";
            this.btnSyncAllActive.Size = new System.Drawing.Size(98, 23);
            this.btnSyncAllActive.TabIndex = 18;
            this.btnSyncAllActive.Text = "Sync All Active";
            this.btnSyncAllActive.UseVisualStyleBackColor = true;
            this.btnSyncAllActive.Click += new System.EventHandler(this.btnSyncAllActive_Click);
            // 
            // btnGetQBListID
            // 
            this.btnGetQBListID.Location = new System.Drawing.Point(324, 47);
            this.btnGetQBListID.Name = "btnGetQBListID";
            this.btnGetQBListID.Size = new System.Drawing.Size(186, 23);
            this.btnGetQBListID.TabIndex = 19;
            this.btnGetQBListID.Text = "Get QBListID And EditSeq";
            this.btnGetQBListID.UseVisualStyleBackColor = true;
            this.btnGetQBListID.Click += new System.EventHandler(this.btnGetQBListID_Click);
            // 
            // tbWorkOrderNumber
            // 
            this.tbWorkOrderNumber.Location = new System.Drawing.Point(80, 47);
            this.tbWorkOrderNumber.Name = "tbWorkOrderNumber";
            this.tbWorkOrderNumber.Size = new System.Drawing.Size(238, 20);
            this.tbWorkOrderNumber.TabIndex = 20;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 13);
            this.label2.TabIndex = 21;
            this.label2.Text = "WO #";
            // 
            // btnRunXml
            // 
            this.btnRunXml.Location = new System.Drawing.Point(324, 77);
            this.btnRunXml.Name = "btnRunXml";
            this.btnRunXml.Size = new System.Drawing.Size(75, 23);
            this.btnRunXml.TabIndex = 22;
            this.btnRunXml.Text = "Run XML";
            this.btnRunXml.UseVisualStyleBackColor = true;
            this.btnRunXml.Click += new System.EventHandler(this.btnRunXml_Click);
            // 
            // tbXML
            // 
            this.tbXML.Location = new System.Drawing.Point(80, 79);
            this.tbXML.Name = "tbXML";
            this.tbXML.Size = new System.Drawing.Size(238, 20);
            this.tbXML.TabIndex = 23;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 79);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(44, 13);
            this.label4.TabIndex = 24;
            this.label4.Text = "QBXML";
            // 
            // showItems
            // 
            this.showItems.Location = new System.Drawing.Point(324, 164);
            this.showItems.Name = "showItems";
            this.showItems.Size = new System.Drawing.Size(75, 23);
            this.showItems.TabIndex = 25;
            this.showItems.Text = "Show Items";
            this.showItems.UseVisualStyleBackColor = true;
            this.showItems.Click += new System.EventHandler(this.ShowItems_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(405, 164);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 26;
            this.button1.Text = "Show UOMs";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.btnShowUOM_Click);
            // 
            // btnSync
            // 
            this.btnSync.Location = new System.Drawing.Point(18, 105);
            this.btnSync.Name = "btnSync";
            this.btnSync.Size = new System.Drawing.Size(102, 23);
            this.btnSync.TabIndex = 28;
            this.btnSync.Text = "Enable Sync";
            this.btnSync.UseVisualStyleBackColor = true;
            this.btnSync.Click += new System.EventHandler(this.btnSync_Click);
            // 
            // labelBuildType
            // 
            this.labelBuildType.AutoSize = true;
            this.labelBuildType.Location = new System.Drawing.Point(669, 47);
            this.labelBuildType.Name = "labelBuildType";
            this.labelBuildType.Size = new System.Drawing.Size(0, 13);
            this.labelBuildType.TabIndex = 29;
            // 
            // MainForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(801, 794);
            this.Controls.Add(this.labelBuildType);
            this.Controls.Add(this.btnSync);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.showItems);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tbXML);
            this.Controls.Add(this.btnRunXml);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbWorkOrderNumber);
            this.Controls.Add(this.btnGetQBListID);
            this.Controls.Add(this.btnSyncAllActive);
            this.Controls.Add(this.btnSyncCustomerSite);
            this.Controls.Add(this.btnQueryCustomer);
            this.Controls.Add(this.tbResults);
            this.Controls.Add(this.btnSyncAllActiveMissingSiteDataOnly);
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

        private void btnGetQBListID_Click(object sender, EventArgs e)
        {
            String woNum = tbWorkOrderNumber.Text.Trim();
            if (woNum.Length == 0)
            {
                MessageBox.Show("Please enter a Work Order Number.", "Input Validation");
                return;
            }
            try
            {
                string response = QBUtils.DoRequest(BuildCustomerQueryByWorkorderNum(woNum));
                tbResults.Text = "";
                string qbid = GetQBIDFromResponse(response);
                if (qbid != "")
                {
                    tbResults.AppendText(qbid);
                }
                else
                {
                    tbResults.AppendText("Not found!");
                }

                tbResults.AppendText(Environment.NewLine);

                string qbes = GetQBEditSequenceFromResponse(response);
                if (qbes != "")
                {
                    tbResults.AppendText(qbes);
                }
            }
            catch (Exception ex)
            {
                tbResults.Text = ex.Message;
            }
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
                string response = QBUtils.DoRequest(BuildCustomerQuery(listID));

                tbResults.Text = "";

                Site site = GetSiteFromResponse(response);
                AppendSiteDetails(tbResults, site);

                tbResults.AppendText(Environment.NewLine);                

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
                string response = QBUtils.DoRequest(BuildCustomerQuery(listID));
                ShipTo shipTo = GetShipToFromResponse(response);
                response = QBUtils.DoRequest(BuildSyncShipToToCustomSiteQuery(listID, shipTo));
                tbResults.Text = "Done";
            }
            catch (Exception ex)
            {
                tbResults.Text = ex.Message;
            }
        }

        private void btnSyncAllActiveMissingSiteDataOnly_Click(object sender, EventArgs e)
        {
            SyncSitesToCustomFields(false);
        }

        private void btnSyncAllActive_Click(object sender, EventArgs e)
        {
            SyncSitesToCustomFields(true);
        }

        private void SyncSitesToCustomFields(bool doAll)
        {
            tbResults.Text = "";

            try
            {
                string response = QBUtils.DoRequest(BuildCustomerQueryAllActive());
                List<Customer> activeCustomers = GetActiveCustomersFromResponse(response);
                int count = activeCustomers.Count;
                int current = 1;
                                
                foreach (Customer customer in activeCustomers)
                {
                    if (doAll || (customer.Site.Name == null) || (customer.Site.Name == ""))
                    {
                        if ((customer.ShipTo.Name != null) && (customer.ShipTo.Name != ""))
                        {
                            tbResults.AppendText("Processing: ");
                            tbResults.AppendText(customer.Name);                            
                            tbResults.AppendText (" (" + current.ToString() + " of " + count.ToString() + ")");
                            tbResults.AppendText(Environment.NewLine);

                            response = QBUtils.DoRequest(BuildSyncShipToToCustomSiteQuery(customer.ListID, customer.ShipTo));                  
                                                        
                            tbResults.Select(tbResults.Text.Length, 0);
                            tbResults.ScrollToCaret();
                        }
                    }                    

                    current++;
                    Application.DoEvents();                                        
                }

                tbResults.AppendText("Done");
                tbResults.Select(tbResults.Text.Length, 0);
                tbResults.ScrollToCaret();
            }
            catch (Exception ex)
            {
                tbResults.AppendText(ex.Message);
                tbResults.Select(tbResults.Text.Length, 0);
                tbResults.ScrollToCaret();
            }
        }

        // <?xml version="1.0"?><?qbxml version="12.0"?><QBXML><QBXMLMsgsRq onError="stopOnError"><CustomerQueryRq requestID="1"><ActiveStatus>All</ActiveStatus><FromModifiedDate>2013-11-09T15:08:14</FromModifiedDate><OwnerID>0</OwnerID></CustomerQueryRq></QBXMLMsgsRq></QBXML>
        private void btnRunXml_Click(object sender, EventArgs e)
        {
            String rawXML = tbXML.Text.Trim();
            if (rawXML.Length == 0)
            {
                MessageBox.Show("Please enter QBXML.", "Input Validation");
                return;
            }
            try
            {
                string response = QBUtils.DoRequestRaw(rawXML);
                tbResults.Text = response;
            }
            catch (Exception ex)
            {
                tbResults.Text = ex.Message;
            }
        }

        private void ShowItems_Click(object sender, EventArgs e)
        {
            try
            {
                string response = QBUtils.DoRequest(BuildAllItemsQuery());
                WalkItemQueryRs(response);
            }
            catch (Exception ex)
            {
                tbResults.Text = ex.Message;
            }
        }

        private void btnShowUOM_Click(object sender, EventArgs e)
        {
            try
            {
                string response = QBUtils.DoRequest(BuildAllUOMsQuery());
                WalkUnitOfMeasureSetQueryRs(response);
            }
            catch (Exception ex)
            {
                tbResults.Text = ex.Message;
            }
        }

        void WalkUnitOfMeasureSetQueryRs(string response)
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

        void WalkUnitOfMeasureSetRet(XmlNode UnitOfMeasureSetRet)
        {
            if (UnitOfMeasureSetRet == null) return;

            //Go through all the elements of UnitOfMeasureSetRet
            //Get value of ListID
            if (UnitOfMeasureSetRet.SelectSingleNode("./ListID") != null)
            {
                string ListID = UnitOfMeasureSetRet.SelectSingleNode("./ListID").InnerText;
                tbResults.AppendText(ListID+",");
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
                tbResults.AppendText(Name);
                tbResults.AppendText(Environment.NewLine);
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
            //Get all field values for BaseUnit aggregate
            XmlNode BaseUnit = UnitOfMeasureSetRet.SelectSingleNode("./BaseUnit");
            if (BaseUnit != null)
            {
                //Get value of Name
                string Name = UnitOfMeasureSetRet.SelectSingleNode("./BaseUnit/Name").InnerText;
                //Get value of Abbreviation
                string Abbreviation = UnitOfMeasureSetRet.SelectSingleNode("./BaseUnit/Abbreviation").InnerText;
            }
            //Done with field values for BaseUnit aggregate

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

        }

        private void AppendSiteDetails(TextBox tb, Site site)
        {
            tb.Text += "Site Details";
            tb.AppendText(Environment.NewLine);

            tb.Text += "Name: ";
            tb.Text += site.Name;
            tb.AppendText(Environment.NewLine);

            tb.Text += "Unit Number: ";
            tb.Text += site.UnitNumber;
            tb.AppendText(Environment.NewLine);

            tb.Text += "County: ";
            tb.Text += site.County;
            tb.AppendText(Environment.NewLine);

            tb.Text += "City/State: ";
            tb.Text += site.CityState;
            tb.AppendText(Environment.NewLine);

            tb.Text += "POAFE Number: ";
            tb.Text += site.POAFENumber;
            tb.AppendText(Environment.NewLine);
        }

        private void AppendShipToDetails(TextBox tb, ShipTo shipTo)
        {
            tb.Text += "Ship To Details";
            tb.AppendText(Environment.NewLine);

            tb.Text += "Name: ";
            tb.Text += shipTo.Name;
            tb.AppendText(Environment.NewLine);

            tb.Text += "Addr1: ";
            tb.Text += shipTo.Addr1;
            tb.AppendText(Environment.NewLine);

            tb.Text += "Addr2: ";
            tb.Text += shipTo.Addr2;
            tb.AppendText(Environment.NewLine);

            tb.Text += "Addr3: ";
            tb.Text += shipTo.Addr3;
            tb.AppendText(Environment.NewLine);

            tb.Text += "City: ";
            tb.Text += shipTo.City;
            tb.AppendText(Environment.NewLine);

            tb.Text += "State: ";
            tb.Text += shipTo.State;
            tb.AppendText(Environment.NewLine);

            tb.Text += "Note: ";
            tb.Text += shipTo.Note;
            tb.AppendText(Environment.NewLine);
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
                                                
                        string Sublevel = CustomerRet.SelectSingleNode("./Sublevel").InnerText;
                        if (Sublevel == "0" || Sublevel == "1")
                        {
                            bool IsJob = false;                            
                            XmlNode ParentRef = CustomerRet.SelectSingleNode("./ParentRef");
                            if (ParentRef != null)
                            {
                                IsJob = true;                                
                            }

                            if (IsJob)
                            {
                                Customer customer = new Customer();
                                customer.Name = CustomerRet.SelectSingleNode("./Name").InnerText;
                                customer.ListID = CustomerRet.SelectSingleNode("./ListID").InnerText;
                                customer.ShipTo = GetShipToFromCustomerRet(CustomerRet);
                                customer.Site = GetSiteFromCustomerRet(CustomerRet);
                                customers.Add(customer);
                            }
                        }
                    }                    
                }
            }

            return customers;
        }

        private string GetQBIDFromResponse(string response)
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


        private string GetQBEditSequenceFromResponse(string response)
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

        private string GetQBIDFromCustomerRet(XmlNode CustomerRet)
        {
            string ListID = CustomerRet.SelectSingleNode("./ListID").InnerText;
            return ListID;
        }
        
        private string GetQBEditSequenceFromCustomerRet(XmlNode CustomerRet)
        {
            string ListID = CustomerRet.SelectSingleNode("./EditSequence").InnerText;
            return ListID;
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

        void WalkItemQueryRs(string response)
        {
            //Parse the response XML string into an XmlDocument
            XmlDocument responseXmlDoc = new XmlDocument();
            responseXmlDoc.LoadXml(response);

            //Get the response for our request
            XmlNodeList ItemQueryRsList = responseXmlDoc.GetElementsByTagName("ItemQueryRs");
            if (ItemQueryRsList.Count == 1) //Should always be true since we only did one request in this sample
            {
                XmlNode responseNode = ItemQueryRsList.Item(0);
                //Check the status code, info, and severity
                XmlAttributeCollection rsAttributes = responseNode.Attributes;
                string statusCode = rsAttributes.GetNamedItem("statusCode").Value;
                string statusSeverity = rsAttributes.GetNamedItem("statusSeverity").Value;
                string statusMessage = rsAttributes.GetNamedItem("statusMessage").Value;

                //status code = 0 all OK, > 0 is warning
                if (Convert.ToInt32(statusCode) >= 0)
                {
                    XmlNodeList ItemServiceRetList = responseNode.SelectNodes("//ItemServiceRet");//XPath Query
                    for (int i = 0; i < ItemServiceRetList.Count; i++)
                    {
                        XmlNode ItemServiceRet = ItemServiceRetList.Item(i);
                        WalkItemServiceRet(ItemServiceRet);
                    }
                }
            }

        }

        void WalkItemServiceRet(XmlNode ItemServiceRet)
        {
            if (ItemServiceRet == null) return;

            //Get value of ListID
            string ListID = ItemServiceRet.SelectSingleNode("./ListID").InnerText;
            tbResults.AppendText(ListID);
            tbResults.AppendText(",");
            //Get value of TimeCreated
            string TimeCreated = ItemServiceRet.SelectSingleNode("./TimeCreated").InnerText;
            //Get value of TimeModified
            string TimeModified = ItemServiceRet.SelectSingleNode("./TimeModified").InnerText;
            //Get value of EditSequence
            string EditSequence = ItemServiceRet.SelectSingleNode("./EditSequence").InnerText;
            //Get value of Name
            string Name = ItemServiceRet.SelectSingleNode("./Name").InnerText;
            //Get value of FullName
            string FullName = ItemServiceRet.SelectSingleNode("./FullName").InnerText;
            tbResults.AppendText(FullName);
            tbResults.AppendText(",");

            XmlNode UnitOfMeasureSetRef = ItemServiceRet.SelectSingleNode("./UnitOfMeasureSetRef");
            if (UnitOfMeasureSetRef != null)
            {
                //Get value of ListID
                if (ItemServiceRet.SelectSingleNode("./UnitOfMeasureSetRef/ListID") != null)
                {
                    string UOMListID = ItemServiceRet.SelectSingleNode("./UnitOfMeasureSetRef/ListID").InnerText;
                    tbResults.AppendText(UOMListID);
                    tbResults.AppendText(",");
                }
                //Get value of FullName
                if (ItemServiceRet.SelectSingleNode("./UnitOfMeasureSetRef/FullName") != null)
                {
                    string UOMFullName = ItemServiceRet.SelectSingleNode("./UnitOfMeasureSetRef/FullName").InnerText;
                    tbResults.AppendText(UOMFullName);
                    tbResults.AppendText(Environment.NewLine);
                }
            }
            else
            {
                tbResults.AppendText(Environment.NewLine);
            }

        }

        public XmlDocument BuildDataExtModRq(XmlDocument doc, XmlElement parent, string customerListID, string DataExtName, string DataExtValue)
        {
            XmlElement DataExtModRq = doc.CreateElement("DataExtModRq");
            parent.AppendChild(DataExtModRq);
            DataExtModRq.SetAttribute("requestID", "1");

            XmlElement DataExtMod = doc.CreateElement("DataExtMod");
            DataExtModRq.AppendChild(DataExtMod);            
            DataExtMod.AppendChild(XmlUtils.MakeSimpleElem(doc, "OwnerID", "0"));
            DataExtMod.AppendChild(XmlUtils.MakeSimpleElem(doc, "DataExtName", DataExtName));
            DataExtMod.AppendChild(XmlUtils.MakeSimpleElem(doc, "ListDataExtType", "Customer"));

            XmlElement ListObjRef = doc.CreateElement("ListObjRef");
            DataExtMod.AppendChild(ListObjRef);
            ListObjRef.AppendChild(XmlUtils.MakeSimpleElem(doc, "ListID", customerListID));
            DataExtMod.AppendChild(XmlUtils.MakeSimpleElem(doc, "DataExtValue", DataExtValue));

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

            custAddRq.AppendChild(XmlUtils.MakeSimpleElem(doc, "ListID", listID));
            custAddRq.AppendChild(XmlUtils.MakeSimpleElem(doc, "OwnerID", "0"));

            return doc;
        }

        private XmlDocument BuildAllUOMsQuery()
        {
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", null, null));
            doc.AppendChild(doc.CreateProcessingInstruction("qbxml", "version=\"12.0\""));
            XmlElement qbXML = doc.CreateElement("QBXML");
            doc.AppendChild(qbXML);
            XmlElement qbXMLMsgsRq = doc.CreateElement("QBXMLMsgsRq");
            qbXML.AppendChild(qbXMLMsgsRq);
            qbXMLMsgsRq.SetAttribute("onError", "stopOnError");

            XmlElement custAddRq = doc.CreateElement("UnitOfMeasureSetQueryRq");
            qbXMLMsgsRq.AppendChild(custAddRq);
            //custAddRq.SetAttribute("requestID", "1");

            custAddRq.AppendChild(XmlUtils.MakeSimpleElem(doc, "ActiveStatus", "ActiveOnly"));
            //custAddRq.AppendChild(MakeSimpleElem(doc, "OwnerID", "0"));

            return doc;
        }
        
        private XmlDocument BuildAllItemsQuery()
        {
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", null, null));
            doc.AppendChild(doc.CreateProcessingInstruction("qbxml", "version=\"12.0\""));
            XmlElement qbXML = doc.CreateElement("QBXML");
            doc.AppendChild(qbXML);
            XmlElement qbXMLMsgsRq = doc.CreateElement("QBXMLMsgsRq");
            qbXML.AppendChild(qbXMLMsgsRq);
            qbXMLMsgsRq.SetAttribute("onError", "stopOnError");

            XmlElement custAddRq = doc.CreateElement("ItemQueryRq");
            qbXMLMsgsRq.AppendChild(custAddRq);
            //custAddRq.SetAttribute("requestID", "1");

            custAddRq.AppendChild(XmlUtils.MakeSimpleElem(doc, "ActiveStatus", "ActiveOnly"));
            //custAddRq.AppendChild(MakeSimpleElem(doc, "OwnerID", "0"));

            return doc;
        }

        private XmlDocument BuildCustomerQueryByWorkorderNum (string woNum)
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

            //custAddRq.AppendChild(MakeSimpleElem(doc, "MaxReturned", "3"));
            custAddRq.AppendChild(XmlUtils.MakeSimpleElem(doc, "ActiveStatus", "ActiveOnly"));
            custAddRq.AppendChild(XmlUtils.MakeSimpleElem(doc, "OwnerID", "0"));
            
            return doc;
        }
        
        void aTimer_Elapsed(object sender, ElapsedEventArgs e)
        {            
            SyncQBData();
        }

        private void btnSync_Click(object sender, EventArgs e)
        {
            if (aTimer.Enabled)
            {
                aTimer.Enabled = false;
                btnSync.Text = "Enable Sync";
            }
            else
            {
                aTimer.Enabled = true;
                btnSync.Text = "Disable Sync";
                SyncQBData();
            }            
        }

        private void ClearStatus()
        {
            this.Invoke((MethodInvoker)delegate
            {
                tbResults.Clear();
            });
        }
        
        private void SetStatus(string value)
        {
            this.Invoke((MethodInvoker)delegate
            {
                tbResults.Text = value;
            });
        }

        private void AppendStatus(string value)
        {
            this.Invoke((MethodInvoker)delegate
            {
                tbResults.AppendText(value);
            });
        }

        private void SyncQBData()
        {
            string response = "";
            string fromModifiedDate = AppConfig.GetLastSyncTime();

            ClearStatus();
            SetStatus("");

            AppendStatus("Query Bills...");
            AppendStatus(Environment.NewLine);
            //response = DoRequest(BillDAL.BuildBillQueryRequest(GetAdjustedDateAsQBString(fromModifiedDate, -300, false), GetAdjustedDateAsQBString(DateTime.Now.ToShortDateString(), 1, false)));
            response = QBUtils.DoRequest(BillDAL.BuildBillQueryRequest(XmlUtils.GetAdjustedDateAsQBString(DateTime.Now.ToShortDateString(), -1, false), XmlUtils.GetAdjustedDateAsQBString(DateTime.Now.ToShortDateString(), 1, false)));
            AppendStatus("Processing Bills...");
            AppendStatus(Environment.NewLine);
            BillDAL.HandleResponse(response);

            AppendStatus("Query Vendors...");
            AppendStatus(Environment.NewLine);
            //response = QBUtils.DoRequest(VendorDAL.BuildVendorQueryRequest(XmlUtils.GetAdjustedDateAsQBString(DateTime.Now.ToShortDateString(), -1040, false), XmlUtils.GetAdjustedDateAsQBString(DateTime.Now.ToShortDateString(), 1, false)));
            response = QBUtils.DoRequest(VendorDAL.BuildVendorQueryRequest(XmlUtils.GetAdjustedDateAsQBString(DateTime.Now.ToShortDateString(), -1, false), XmlUtils.GetAdjustedDateAsQBString(DateTime.Now.ToShortDateString(), 1, false)));
            AppendStatus("Processing Vendors...");
            AppendStatus(Environment.NewLine);
            VendorDAL.HandleResponse(response);

            AppendStatus("Query Invoices...");
            AppendStatus(Environment.NewLine);
            //response = QBUtils.DoRequest(InvoiceDAL.BuildInvoiceQueryRequest(XmlUtils.GetAdjustedDateAsQBString(DateTime.Now.ToShortDateString(), -1040, false), XmlUtils.GetAdjustedDateAsQBString(DateTime.Now.ToShortDateString(), 1, false)));
            response = QBUtils.DoRequest(InvoiceDAL.BuildInvoiceQueryRequest(XmlUtils.GetAdjustedDateAsQBString(DateTime.Now.ToShortDateString(), -1, false), XmlUtils.GetAdjustedDateAsQBString(DateTime.Now.ToShortDateString(), 1, false)));
            AppendStatus("Processing Invoices...");
            AppendStatus(Environment.NewLine);
            InvoiceDAL.HandleResponse(response);

            AppendStatus("Query Items...");
            AppendStatus(Environment.NewLine);
            //response = QBUtils.DoRequest(ItemDAL.BuildItemQueryRequest(XmlUtils.GetAdjustedDateAsQBString(DateTime.Now.ToShortDateString(), -1040, false), XmlUtils.GetAdjustedDateAsQBString(DateTime.Now.ToShortDateString(), 1, false)));
            response = QBUtils.DoRequest(ItemDAL.BuildItemQueryRequest(XmlUtils.GetAdjustedDateAsQBString(DateTime.Now.ToShortDateString(), -1, false), XmlUtils.GetAdjustedDateAsQBString(DateTime.Now.ToShortDateString(), 1, false)));
            AppendStatus("Processing Items...");
            AppendStatus(Environment.NewLine);
            ItemDAL.HandleResponse(response);

            AppendStatus("Done");
            AppendStatus(Environment.NewLine);
        }
	}
}
