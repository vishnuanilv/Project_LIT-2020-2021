//-----------------------------------------------------------------------
// <copyright file="BLPatientCard.cs" company="Kameda Infologics PVT Ltd">
//     Copyright (c) Kameda Infologics Pvt Ltd. All rights reserved.
// </copyright>
// <author>Biju S J</author>
//<Date>22-Jan-2010<Date>
//-----------------------------------------------------------------------

namespace Infologics.Medilogics.PrintingLibrary.PatientCard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Infologics.Medilogics.PrintingLibrary.Main;
    using Infologics.Medilogics.Enumerators.General;
    using System.Windows.Forms;
    using System.Drawing.Printing;
    using System.Drawing;
    using System.Data;
    using System.Configuration;
    using System.IO;
    using Infologics.Medilogics.Resources.MessageBoxLib;
    using ZXing.QrCode;
    using ZXing;
    using ZXing.Common;
    using System.Drawing.Imaging;

    public class PatientCard : IPrinting, IDisposable
    {
        string strBarcodeFont, strBarcodeType;
        private static Font BarcodeFont = null;
        private static Font PrintFont = null;
        private static int[] code128Lookup = null;
        private static Font PrintFont1 = null;
        private string cardPrinterName = string.Empty;
        PrintDocument objPrintDoc;
        DataRow drPrintData;
        ServiceType SelectedServiceType;
        PrintType SelectedPrintType;
        DataSet dsTemp = null;
        string strPath = System.AppDomain.CurrentDomain.BaseDirectory + System.DateTime.Now.ToString("ddmmyyhhmmss");
        public PatientCard()
        {
            try
            {
                strBarcodeFont = ConfigurationSettings.AppSettings["BarcodeFont"].ToString();
                strBarcodeType = ConfigurationSettings.AppSettings["BarcodeType"].ToString();
                BarcodeFont = new Font(strBarcodeFont, 18);
                if (strBarcodeType == "Code128b")
                {
                    code128Lookup = new int[103];
                    InitializeLookup();
                }
            }
            catch (Exception)
            {
                throw;
            }

        }
        private void InitializeLookup()
        {
            try
            {
                int cnt;
                if (strBarcodeType == "Code128b")
                {
                    code128Lookup[0] = 128;
                    for (cnt = 1; cnt <= 94; cnt++)
                    {
                        code128Lookup[cnt] = cnt + 32;
                    }
                    for (cnt = 95; cnt <= 102; cnt++)
                    {
                        code128Lookup[cnt] = cnt + 50;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public bool Print(DataSet dsData, ServiceType serviceType, string PrinterName)
        {
            bool printStatus = false;
            PrintType prntype = PrintType.BarCode; //default
            SelectedServiceType = serviceType;
            SelectedPrintType = prntype;
            dsTemp = dsData;
            printStatus = PrintPatientCard(dsData, serviceType,prntype, PrinterName);
            return printStatus;
        }

        #region IPrinting Members

        public bool Print(DataSet dsData, ServiceType serviceType, string PrinterName, PrintType printType)
        {

            bool printStatus = false;
            SelectedServiceType = serviceType;
            dsTemp = dsData;
            SelectedPrintType = printType;
            //if (printType == PrintType.PatientCard)
            //{
            //    printStatus = PrintPatientCard(dsData, serviceType, PrinterName);
            //}
            if (printType == PrintType.Prescription)
            {
                printStatus = PrintPatientCard(dsData, serviceType, printType, PrinterName);
            }
            //else if (printType == Infologics.Medilogics.Enumerators.General.PrintType.StoresShelfLabel)
            //{
            //    printStatus = PrintStoresShelf(dsData, printType, PrinterName);

            //}
            else if (printType == PrintType.PatientBand)
            {
                printStatus = PrintPatientWristBand(dsData, printType, PrinterName);
            }

            //else if (printType == PrintType.StoresLabel)
            //{
            //    printStatus = PrintStores(dsData, printType, PrinterName);
            //}
            //else if (printType == PrintType.AssetLabel)
            //{
            //    printStatus = PrintAsset(dsData, printType, PrinterName);
            //}
            //else if (printType == PrintType.PatientLabel)
            //{
            //    printStatus = PrintPatientLabel(dsData, printType, PrinterName);
            //}
            return printStatus;
            return printStatus;
        }

        #endregion
        private bool PrintPatientCard(DataSet dsData, ServiceType serviceType, PrintType prntype, string CardPrinterName)
        {
            bool printStatus = false;
            DataTable dtPrintData = new DataTable();
            try
            {
                //Setting card printing data.
                dtPrintData = SelectDataToPrint(dsData);
                PrintDialog pd = new PrintDialog();
                if (dtPrintData != null && dtPrintData.Rows.Count > 0)
                {
                    objPrintDoc = new PrintDocument();
                    if (CardPrinterName.Length > 0)
                    {
                        objPrintDoc.PrinterSettings.PrinterName = CardPrinterName;
                    }
                    else
                    {

                        pd.PrinterSettings = new PrinterSettings();
                        pd.ShowDialog();
                        objPrintDoc.PrinterSettings.PrinterName = pd.PrinterSettings.PrinterName;
                    }
                    if (serviceType == ServiceType.Registration || serviceType == ServiceType.ReRegistration || serviceType == ServiceType.Consultation||serviceType==ServiceType.Bloodbank)
                    {
                        foreach (DataRow dr in dtPrintData.Rows)
                        {
                            drPrintData = dr;
                            //Creating print data from dtPrintData in AlignPatientCard event.
                            objPrintDoc.PrintPage += new PrintPageEventHandler(AlignPatientCard);

                            if (dr.Table.Columns.Contains("BARCODE_COUNT") && dr["BARCODE_COUNT"] != DBNull.Value)
                            {
                                for (int i = 1; i <= Convert.ToDecimal(dr["BARCODE_COUNT"]); i++)
                                {
                                    objPrintDoc.Print();
                                }
                            }
                            else
                            {
                                objPrintDoc.Print();
                            }
                            Dispose();
                        }
                        printStatus = true;
                    }
                }
            }
            catch (InvalidPrinterException)
            {
                MessageBox.Show("Card Printer not Found", "Y A S A S I I", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception)
            {
                throw;
            }
            return printStatus;
        }
        private void AlignPatientCard(Object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;

            try
            {

                if (SelectedServiceType == ServiceType.Bloodbank && SelectedPrintType == PrintType.Bloodbank) //Blood Bank label Print
                {
                    Font PrintFont2 = new Font("Times New Roman", 32, FontStyle.Bold);
                    Font PrintFont3 = new Font("Times New Roman", 10, FontStyle.Bold);
                    PrintFont = new Font("Times New Roman", 10, FontStyle.Bold);
                    PrintFont1 = new Font("Times New Roman", 10, FontStyle.Regular);
                    Brush br = new SolidBrush(Color.Black);
                    if (Convert.ToString(drPrintData["BBLABEL_TYPE"]) == "0")
                    {
                        BarcodeFont = new Font(strBarcodeFont, 15);
                        PrintFont = new Font("Times New Roman", 10, FontStyle.Bold);
                        PrintFont1 = new Font("Times New Roman", 10, FontStyle.Regular);

                        //Collection Date
                        g.DrawString("Collection Date : ", PrintFont1, br, 15, 45);//40
                        g.DrawString(Convert.ToString(drPrintData["COLLECTION_DATE"]).ToUpper(), PrintFont, br, 110, 45);//40

                        //Blood Group                        
                        g.DrawString(Convert.ToString(drPrintData["BLOOD_GROUP_PREFIX"]).ToUpper(), PrintFont2, br, 250, 35);
                        g.DrawString(Convert.ToString(drPrintData["BLOOD_GROUP_SUFIX"]), PrintFont1, br, 240, 85);

                        //Donor Barcode                       
                        g.DrawString("*** Donor ***", PrintFont1, br, 80, 95);
                        if (strBarcodeType.Equals("Code3of9"))
                        {
                            // the Asterisk (*) is used to delimit the barcode, and is required as the start and stop charachters by the 3of9 Symbology.
                            g.DrawString("*" + drPrintData["DONOR_ID"].ToString().Trim() + "*", BarcodeFont, br, 60, 110);//6
                        }
                        else if (strBarcodeType.Equals("Code128b"))
                        {
                            // the Symbol (š) and (œ} is used to delimit the barcode, and is required as the start and stop charachters by the Code128b Symbology.
                            g.DrawString("š" + drPrintData["DONOR_ID"].ToString().Trim() + "œ", BarcodeFont, br, 60, 110);//6
                        }
                        g.DrawString(Convert.ToString(drPrintData["DONOR_ID"]).ToUpper(), PrintFont3, br, 80, 135);
                        g.DrawString(Convert.ToString(drPrintData["DONOR_NAME"]).ToUpper(), PrintFont1, br, 80, 150);

                        g.DrawString("ISBT 128 ID", PrintFont3, br, 30, 170);
                        if (strBarcodeType.Equals("Code3of9"))
                        {
                            // the Asterisk (*) is used to delimit the barcode, and is required as the start and stop charachters by the 3of9 Symbology.
                            g.DrawString("*" + drPrintData["BLOOD_UNIT_NO"].ToString().Trim() + drPrintData["PRODUCT_BARCODE"].ToString().Trim() + "*", BarcodeFont, br, 30, 185);//6
                        }
                        else if (strBarcodeType.Equals("Code128b"))
                        {
                            // the Symbol (š) and (œ} is used to delimit the barcode, and is required as the start and stop charachters by the Code128b Symbology.
                            g.DrawString("š" + drPrintData["BLOOD_UNIT_NO"].ToString().Trim() + drPrintData["PRODUCT_BARCODE"].ToString().Trim() + "œ", BarcodeFont, br, 30, 185);//6
                        }
                        g.DrawString(Convert.ToString(drPrintData["BLOOD_UNIT_NO"]).ToUpper(), PrintFont3, br, 90, 210);

                        Rectangle rect1 = new Rectangle(15, 35, 350, 212);
                        g.DrawRectangle(Pens.Black, Rectangle.Round(rect1)); // big rectangle
                    }
                    else if (Convert.ToString(drPrintData["BBLABEL_TYPE"]) == "1")
                    {
                        PrintFont = new Font("Times New Roman", 10, FontStyle.Bold);
                        PrintFont1 = new Font("Times New Roman", 10, FontStyle.Regular);
                        BarcodeFont = new Font(strBarcodeFont, 15);
                        //Blood Unit no
                        if (strBarcodeType.Equals("Code3of9"))
                        {
                            // the Asterisk (*) is used to delimit the barcode, and is required as the start and stop charachters by the 3of9 Symbology.
                            g.DrawString("*" + drPrintData["BLOOD_UNIT_NO"].ToString().Trim() + drPrintData["PRODUCT_BARCODE"].ToString().Trim() + "*", BarcodeFont, br, 30, 45);//6
                        }
                        else if (strBarcodeType.Equals("Code128b"))
                        {
                            // the Symbol (š) and (œ} is used to delimit the barcode, and is required as the start and stop charachters by the Code128b Symbology.
                            g.DrawString("š" + drPrintData["BLOOD_UNIT_NO"].ToString().Trim() + drPrintData["PRODUCT_BARCODE"].ToString().Trim() + "œ", BarcodeFont, br, 30, 45);//6
                        }
                        g.DrawString(Convert.ToString(drPrintData["BLOOD_UNIT_NO"]).ToUpper(), PrintFont3, br, 30, 70);

                        //Blood Group                        
                        g.DrawString(Convert.ToString(drPrintData["BLOOD_GROUP_PREFIX"]).ToUpper(), PrintFont2, br, 290, 35);
                        g.DrawString(Convert.ToString(drPrintData["BLOOD_GROUP_SUFIX"]).ToUpper(), PrintFont1, br, 270, 75);
                        
                        g.DrawString(Convert.ToString(drPrintData["HOSPITAL"]), PrintFont1, br, 15, 90);

                        g.DrawString("Expiration", PrintFont, br, 15, 160);
                        g.DrawString("Date", PrintFont, br, 16, 175);
                        if (strBarcodeType.Equals("Code3of9"))
                        {
                            // the Asterisk (*) is used to delimit the barcode, and is required as the start and stop charachters by the 3of9 Symbology.
                            g.DrawString("*" + drPrintData["EXPIRY_DATE"].ToString().Trim() + "*", BarcodeFont, br, 90, 160);//6
                        }
                        else if (strBarcodeType.Equals("Code128b"))
                        {
                            // the Symbol (š) and (œ} is used to delimit the barcode, and is required as the start and stop charachters by the Code128b Symbology.
                            g.DrawString("š" + drPrintData["EXPIRY_DATE"].ToString().Trim() + "œ", BarcodeFont, br, 90, 160);//6
                        }
                        g.DrawString(Convert.ToString(drPrintData["EXPIRY_DATE"]).ToUpper(), PrintFont3, br, 90, 185);

                        g.DrawString(Convert.ToString(drPrintData["DONOR_CATEGORY_NAME"]) + " " + " Source Donor", PrintFont, br, 15, 205);
                        //Isbt Product Code
                        if (strBarcodeType.Equals("Code3of9"))
                        {
                            // the Asterisk (*) is used to delimit the barcode, and is required as the start and stop charachters by the 3of9 Symbology.
                            g.DrawString("*" + drPrintData["PRODUCT_BARCODE"].ToString().Trim() + "*", BarcodeFont, br, 30, 225);//6
                        }
                        else if (strBarcodeType.Equals("Code128b"))
                        {
                            // the Symbol (š) and (œ} is used to delimit the barcode, and is required as the start and stop charachters by the Code128b Symbology.
                            g.DrawString("š" + drPrintData["PRODUCT_BARCODE"].ToString().Trim() + "œ", BarcodeFont, br, 30, 225);//6
                        }
                        g.DrawString(Convert.ToString(drPrintData["PRODUCT_BARCODE"]).ToUpper(), PrintFont3, br, 30, 250);
                        g.DrawString(Convert.ToString(drPrintData["PRODUCT_DESCRIPTION"]).ToUpper(), PrintFont1, br, 15, 270);

                        Rectangle rect1 = new Rectangle(15, 35, 350, 272);
                        g.DrawRectangle(Pens.Black, Rectangle.Round(rect1)); // big rectangle
                    }
                    else if (Convert.ToString(drPrintData["BBLABEL_TYPE"]) == "2")
                    {
                        PrintFont = new Font("Times New Roman", 10, FontStyle.Bold);
                        PrintFont1 = new Font("Times New Roman", 10, FontStyle.Regular);
                        PrintFont2 = new Font("Times New Roman", 15, FontStyle.Bold);
                        PrintFont3 = new Font("Times New Roman", 12, FontStyle.Bold);
                        if (drPrintData["ORG_NAME"] != DBNull.Value)
                        {
                            g.DrawString(Convert.ToString(drPrintData["ORG_NAME"]).ToUpper(), PrintFont2, br, 20, 35); // Hospital name
                        }
                        else
                        {
                            g.DrawString("DANAT AL EMARAT HOSPITAL", PrintFont2, br, 20, 35); // Hospital name
                        }
                        g.DrawString("Blood Bank System", PrintFont2, br, 20, 55);
                        g.DrawString("COMPATIBLE WITH", PrintFont3, br, 70, 85);
                        g.DrawString("Name:", PrintFont, br, 15, 115);
                        g.DrawString(Convert.ToString(drPrintData["PATIENT_NAME"]), PrintFont1, br, 65, 115);
                        g.DrawString("MRN:", PrintFont, br, 15, 135);
                        g.DrawString(Convert.ToString(drPrintData["MRNO"]).ToUpper(), PrintFont1, br, 65, 135);
                        g.DrawString("ABO/RH:", PrintFont, br, 15, 155);
                        g.DrawString(Convert.ToString(drPrintData["PATIENT_BLOOD"]), PrintFont1, br, 80, 155);
                        g.DrawString("Room:", PrintFont, br, 15, 175);
                        g.DrawString(Convert.ToString(drPrintData["PATIENT_ROOM_NAME"]), PrintFont1, br, 80, 175);
                        g.DrawString("Unit Id:", PrintFont, br, 15, 205);
                        g.DrawString(Convert.ToString(drPrintData["BB_COMPONENT_UNIT_NO"]), PrintFont1, br, 70, 205);
                        g.DrawString("ABO/RH2:", PrintFont, br, 15, 225);
                        g.DrawString(Convert.ToString(drPrintData["RECEIVED_BLOOD"]), PrintFont1, br, 90, 225);
                        g.DrawString("Unit:", PrintFont, br, 15, 245);
                        g.DrawString(Convert.ToString(drPrintData["PRODUCT_BARCODE"]), PrintFont1, br, 50, 245);
                        g.DrawString("EXP:", PrintFont, br, 140, 245);
                        g.DrawString(Convert.ToString(drPrintData["EXPIRY_DATE"]), PrintFont1, br, 180, 245);
                        g.DrawString("Actual Vol:", PrintFont, br, 15, 265);
                        g.DrawString(Convert.ToString(drPrintData["QUANTITY_AND_UNIT"]), PrintFont1, br, 85, 265);
                        g.DrawString("Type:", PrintFont, br, 140, 265);
                        g.DrawString(Convert.ToString(drPrintData["COMPONENT_NAME"]), PrintFont1, br, 180, 265);
                        g.DrawString("Date:", PrintFont, br, 15, 285);
                        g.DrawString(Convert.ToString(drPrintData["CROSS_MATCH_DATE"]), PrintFont1, br, 60, 285);
                        g.DrawString("Crossmatch:", PrintFont, br, 15, 305);
                        g.DrawString(Convert.ToString(drPrintData["CROSSMATCH_BY"]), PrintFont1, br, 100, 305);
                        Rectangle rect1 = new Rectangle(15, 35, 350, 308);
                        g.DrawRectangle(Pens.Black, Rectangle.Round(rect1)); // big rectangle
                        Rectangle rect2 = new Rectangle(15, 105, 350, 90);
                        g.DrawRectangle(Pens.Black, Rectangle.Round(rect2));

                    }
                }
                else
                {
                    string patName = string.Empty;
                    string address1 = string.Empty;
                    string address2 = string.Empty;
                    Brush br = new SolidBrush(Color.Black);
                    PrintFont = new Font("Arial", 10);

                    patName = drPrintData["FIRST_NAME"].ToString() + " " + drPrintData["MIDDLE_NAME"].ToString() + " " + drPrintData["LAST_NAME"].ToString();
                    patName = patName.ToUpper();
                    if (drPrintData["TITLE"].ToString().Trim() != string.Empty)
                    {
                        patName = drPrintData["TITLE"].ToString() + " " + patName;
                    }
                    if (patName.Length > 26)
                    {
                        patName = patName.Substring(0, 26);
                    }

                    address1 = drPrintData["ADDRESS1"].ToString();
                    if (address1.Length > 24)
                    {
                        address1 = address1.Substring(0, 24);
                    }

                    address2 = drPrintData["ADDRESS2"].ToString();
                    if (address2.Length > 24)
                    {
                        address2 = address2.Substring(0, 24);
                    }

                    if (BarcodeFont.Name.Equals("3 of 9 Barcode"))
                    {
                        // the Asterisk (*) is used to delimit the barcode, and is required as the start and stop charachters by the 3of9 Symbology.
                        g.DrawString("*" + drPrintData["MRNO"].ToString() + "*", BarcodeFont, br, 110, 40);
                    }
                    else if (BarcodeFont.Name.Equals("Code128bWin"))
                    {
                        // the Symbol (š) and (œ} is used to delimit the barcode, and is required as the start and stop charachters by the Code128b Symbology.
                        g.DrawString("š" + drPrintData["MRNO"].ToString() + Convert.ToString((char)CheckSum(drPrintData["MRNO"].ToString())) + "œ", BarcodeFont, br, 110, 40);
                    }

                    PrintFont = new Font("Arial", 12, FontStyle.Bold);
                    g.DrawString(patName, PrintFont, br, 30, 75);

                    PrintFont = new Font("Arial", 10, FontStyle.Bold);
                    g.DrawString("MRNO", PrintFont, br, 30, 95);
                    g.DrawString("DOB", PrintFont, br, 30, 112);
                    g.DrawString("Address", PrintFont, br, 30, 130);
                    g.DrawString("Valid Upto", PrintFont, br, 30, 165);
                    g.DrawString("Sex", PrintFont, br, 200, 95);
                    g.DrawString("Date", PrintFont, br, 200, 112);

                    g.DrawString(":", PrintFont, br, 100, 95);
                    g.DrawString(":", PrintFont, br, 100, 112);
                    g.DrawString(":", PrintFont, br, 100, 130);
                    g.DrawString(":", PrintFont, br, 100, 165);
                    g.DrawString(":", PrintFont, br, 240, 95);
                    g.DrawString(":", PrintFont, br, 240, 112);

                    ////////
                    g.DrawString(drPrintData["MRNO"].ToString(), PrintFont, br, 110, 95);
                    g.DrawString(Convert.ToDateTime(drPrintData["DOB"]).ToString("dd-MM-yyyy"), PrintFont, br, 110, 112);
                    g.DrawString(address1, PrintFont, br, 110, 130);
                    g.DrawString(address2, PrintFont, br, 110, 145);

                    if (Convert.ToBoolean(drPrintData["ISLIFELONG"]) == true)
                    {
                        g.DrawString("LIFE LONG", PrintFont, br, 110, 165);
                    }
                    else
                    {
                        g.DrawString(Convert.ToDateTime(drPrintData["VALID_UPTO"]).ToString("dd-MMM-yyyy"), PrintFont, br, 110, 165);
                    }
                    g.DrawString(drPrintData["SEX"].ToString(), PrintFont, br, 250, 95);
                    g.DrawString(Convert.ToDateTime(drPrintData["REG_DATE"]).ToString("dd-MMM-yyyy"), PrintFont, br, 250, 112);

                    //patName = drPrintData["FIRST_NAME"].ToString() + " " + drPrintData["MIDDLE_NAME"].ToString() + " " + drPrintData["LAST_NAME"].ToString();
                    //patName = patName.ToUpper();
                    //if (patName.Length > 26)
                    //{
                    //    patName = patName.Substring(0, 26);
                    //}

                    ////address1 = drPrintData["ADDRESS1"].ToString();
                    ////if (address1.Length > 24)
                    ////{
                    ////    address1 = address1.Substring(0, 24);
                    ////}

                    ////address2 = drPrintData["ADDRESS2"].ToString();
                    ////if (address2.Length > 24)
                    ////{
                    ////    address2 = address2.Substring(0, 24);
                    ////}




                    //PrintFont = new Font("Arial", 10, FontStyle.Bold);
                    //g.DrawString("Name", PrintFont, br, 40, 75);

                    //PrintFont = new Font("Arial", 10, FontStyle.Bold);
                    //g.DrawString("MRNO", PrintFont, br, 40, 95);
                    //g.DrawString("Gender", PrintFont, br, 40, 112);
                    //g.DrawString("DOB", PrintFont, br, 170, 112);
                    //g.DrawString("DOR", PrintFont, br, 40, 129);

                    //g.DrawString(":", PrintFont, br, 90, 75);
                    //g.DrawString(":", PrintFont, br, 90, 95);
                    //g.DrawString(":", PrintFont, br, 90, 112);
                    //g.DrawString(":", PrintFont, br, 202, 112);
                    //g.DrawString(":", PrintFont, br, 90, 129);


                    //////////
                    //g.DrawString(patName, PrintFont, br, 100, 75);
                    //g.DrawString(drPrintData["MRNO"].ToString(), PrintFont, br, 100, 95);
                    //g.DrawString(drPrintData["SEX"].ToString(), PrintFont, br, 100, 112);
                    //g.DrawString(Convert.ToDateTime(drPrintData["DOB"]).ToString("dd-MMM-yyyy"), PrintFont, br, 212, 112);
                    //g.DrawString(Convert.ToDateTime(drPrintData["REG_DATE"]).ToString("dd-MMM-yyyy"), PrintFont, br, 100, 129);


                    //if (BarcodeFont.Name.Equals("3 of 9 Barcode"))
                    //{
                    //    // the Asterisk (*) is used to delimit the barcode, and is required as the start and stop charachters by the 3of9 Symbology.
                    //    g.DrawString("*" + drPrintData["MRNO"].ToString() + "*", BarcodeFont, br, 80, 155);
                    //}
                    //else if (BarcodeFont.Name.Equals("Code128bWin"))
                    //{
                    //    // the Symbol (š) and (œ} is used to delimit the barcode, and is required as the start and stop charachters by the Code128b Symbology.
                    //    g.DrawString("š" + drPrintData["MRNO"].ToString() + Convert.ToString((char)CheckSum(drPrintData["MRNO"].ToString())) + "œ", BarcodeFont, br, 80, 155);
                    //}
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private DataTable SelectDataToPrint(DataSet dsData)
        {
            DataTable dtData = new DataTable("PrintData");
            if (SelectedServiceType == ServiceType.Bloodbank && SelectedPrintType == PrintType.Bloodbank) //Blood Bank Label Print
            {
                if (dsData.Tables.Contains("PRINT_DATA") && Convert.ToString(dsData.Tables["PRINT_DATA"].Rows[0]["BBLABEL_TYPE"]) == "0")
                {
                    dtData.Columns.Add("BLOOD_UNIT_NO", typeof(String));
                    dtData.Columns.Add("PRODUCT_BARCODE", typeof(String));
                    dtData.Columns.Add("COLLECTION_DATE", typeof(String));
                    dtData.Columns.Add("DONOR_ID", typeof(String));
                    dtData.Columns.Add("DONOR_NAME", typeof(String));
                    dtData.Columns.Add("RECEIVED_BLOOD", typeof(String));
                    dtData.Columns.Add("BLOOD_GROUP_PREFIX", typeof(String));
                    dtData.Columns.Add("BLOOD_GROUP_SUFIX", typeof(String));
                    dtData.Columns.Add("BBLABEL_TYPE", typeof(String));
                    dtData.Columns.Add("ORG_NAME", typeof(String));
                    dtData.Columns.Add("BARCODE_COUNT", typeof(String));
                    DataRow drData = null;
                    foreach (DataRow dr in dsData.Tables["PRINT_DATA"].Rows)
                    {
                        drData = dtData.NewRow();
                        drData["BLOOD_UNIT_NO"] = dr["BLOOD_UNIT_NO"].ToString();
                        drData["PRODUCT_BARCODE"] = dr["PRODUCT_BARCODE"].ToString();
                        drData["COLLECTION_DATE"] = Convert.ToDateTime(dr["COLLECTION_DATE"]).ToString("dd-MMM-yyyy HH:mm");
                        drData["DONOR_ID"] = dr["DONOR_ID"].ToString();
                        drData["DONOR_NAME"] = dr["DONOR_NAME"].ToString();
                        drData["RECEIVED_BLOOD"] = dr["RECEIVED_BLOOD"].ToString();
                        drData["BLOOD_GROUP_PREFIX"] = dr["BLOOD_GROUP_PREFIX"].ToString();
                        drData["BLOOD_GROUP_SUFIX"] = dr["BLOOD_GROUP_SUFIX"].ToString();
                        drData["BBLABEL_TYPE"] = dr["BBLABEL_TYPE"].ToString();
                        drData["ORG_NAME"] = dr["ORG_NAME"].ToString();
                        drData["BARCODE_COUNT"] = dr["BARCODE_COUNT"].ToString();
                        dtData.Rows.Add(drData);
                    }
                }
                else if (dsData.Tables.Contains("PRINT_DATA") && Convert.ToString(dsData.Tables["PRINT_DATA"].Rows[0]["BBLABEL_TYPE"]) == "1")
                {
                    dtData.Columns.Add("BLOOD_UNIT_NO", typeof(String));
                    dtData.Columns.Add("PRODUCT_BARCODE", typeof(String));
                    dtData.Columns.Add("EXPIRY_DATE", typeof(String));
                    dtData.Columns.Add("DONOR_CATEGORY_NAME", typeof(String));
                    dtData.Columns.Add("HOSPITAL", typeof(String));
                    dtData.Columns.Add("RECEIVED_BLOOD", typeof(String));
                    dtData.Columns.Add("PRODUCT_DESCRIPTION", typeof(String));
                    dtData.Columns.Add("BLOOD_GROUP_PREFIX", typeof(String));
                    dtData.Columns.Add("BLOOD_GROUP_SUFIX", typeof(String));
                    dtData.Columns.Add("BBLABEL_TYPE", typeof(String));
                    dtData.Columns.Add("ORG_NAME", typeof(String));
                    dtData.Columns.Add("BARCODE_COUNT", typeof(String));
                    DataRow drData = null;
                    foreach (DataRow dr in dsData.Tables["PRINT_DATA"].Rows)
                    {
                        drData = dtData.NewRow();
                        drData["BLOOD_UNIT_NO"] = dr["BLOOD_UNIT_NO"].ToString();
                        drData["PRODUCT_BARCODE"] = dr["PRODUCT_BARCODE"].ToString();
                        drData["EXPIRY_DATE"] = Convert.ToDateTime(dr["EXPIRY_DATE"]).ToString("dd-MMM-yyyy");
                        drData["DONOR_CATEGORY_NAME"] = dr["DONOR_CATEGORY_NAME"].ToString();
                        drData["HOSPITAL"] = dr["HOSPITAL"].ToString();
                        drData["RECEIVED_BLOOD"] = dr["RECEIVED_BLOOD"].ToString();
                        drData["PRODUCT_DESCRIPTION"] = dr["PRODUCT_DESCRIPTION"].ToString();
                        drData["BLOOD_GROUP_PREFIX"] = dr["BLOOD_GROUP_PREFIX"].ToString();
                        drData["BLOOD_GROUP_SUFIX"] = dr["BLOOD_GROUP_SUFIX"].ToString();
                        drData["BBLABEL_TYPE"] = dr["BBLABEL_TYPE"].ToString();
                        drData["ORG_NAME"] = dr["ORG_NAME"].ToString();
                        drData["BARCODE_COUNT"] = dr["BARCODE_COUNT"].ToString();
                        dtData.Rows.Add(drData);
                    }
                }
                else if (dsData.Tables.Contains("PRINT_DATA") && Convert.ToString(dsData.Tables["PRINT_DATA"].Rows[0]["BBLABEL_TYPE"]) == "2")
                {
                    dtData.Columns.Add("PATIENT_NAME", typeof(String));
                    dtData.Columns.Add("MRNO", typeof(String));
                    dtData.Columns.Add("PATIENT_BLOOD", typeof(String));
                    dtData.Columns.Add("PATIENT_ROOM_NAME", typeof(String));
                    dtData.Columns.Add("BB_COMPONENT_UNIT_NO", typeof(String));
                    dtData.Columns.Add("RECEIVED_BLOOD", typeof(String));
                    dtData.Columns.Add("PRODUCT_BARCODE", typeof(String));
                    dtData.Columns.Add("EXPIRY_DATE", typeof(String));
                    dtData.Columns.Add("QUANTITY_AND_UNIT", typeof(String));
                    dtData.Columns.Add("DONOR_CATEGORY_NAME", typeof(String));
                    dtData.Columns.Add("CROSS_MATCH_DATE", typeof(String));
                    dtData.Columns.Add("CROSSMATCH_BY", typeof(String));
                    dtData.Columns.Add("COMPONENT_NAME", typeof(String));
                    dtData.Columns.Add("BBLABEL_TYPE", typeof(String));
                    dtData.Columns.Add("ORG_NAME", typeof(String));
                    dtData.Columns.Add("BARCODE_COUNT", typeof(String));
                    DataRow drData = null;
                    foreach (DataRow dr in dsData.Tables["PRINT_DATA"].Rows)
                    {
                        drData = dtData.NewRow();
                        drData["PATIENT_NAME"] = dr["PATIENT_NAME"].ToString();
                        drData["MRNO"] = dr["MRNO"].ToString();
                        drData["PATIENT_BLOOD"] = dr["PATIENT_BLOOD"].ToString();
                        drData["PATIENT_ROOM_NAME"] = dr["PATIENT_ROOM_NAME"].ToString();
                        drData["BB_COMPONENT_UNIT_NO"] = dr["BB_COMPONENT_UNIT_NO"].ToString();
                        drData["RECEIVED_BLOOD"] = dr["RECEIVED_BLOOD"].ToString();
                        drData["PRODUCT_BARCODE"] = dr["PRODUCT_BARCODE"].ToString();
                        drData["EXPIRY_DATE"] = Convert.ToDateTime(dr["EXPIRY_DATE"]).ToString("dd-MMM-yyyy");
                        drData["QUANTITY_AND_UNIT"] = dr["QUANTITY_AND_UNIT"].ToString();
                        drData["DONOR_CATEGORY_NAME"] = dr["DONOR_CATEGORY_NAME"].ToString();
                        drData["CROSS_MATCH_DATE"] = dr["CROSS_MATCH_DATE"].ToString();
                        drData["CROSSMATCH_BY"] = dr["CROSSMATCH_BY"].ToString();
                        drData["COMPONENT_NAME"] = dr["COMPONENT_NAME"].ToString();
                        drData["BBLABEL_TYPE"] = dr["BBLABEL_TYPE"].ToString();
                        drData["ORG_NAME"] = dr["ORG_NAME"].ToString();
                        drData["BARCODE_COUNT"] = dr["BARCODE_COUNT"].ToString();
                        dtData.Rows.Add(drData);
                    }
                }
            }
            else
            {
                DataTable dtTemp = new DataTable();
                DataRow[] drArray;
                int i = 0;
                if (dsData.Tables["PAT_PATIENT_NAME"] != null
                    && dsData.Tables["REG_PATIENT_REGISTRATION"] != null
                    && dsData.Tables["PAT_PATIENT_NAME"].Rows.Count > 0 && dsData.Tables["REG_PATIENT_REGISTRATION"].Rows.Count > 0)
                {
                    //Create table to print     
                    dtData.Columns.Add("TITLE", System.Type.GetType("System.String"));
                    dtData.Columns.Add("FIRST_NAME", System.Type.GetType("System.String"));
                    dtData.Columns.Add("MIDDLE_NAME", System.Type.GetType("System.String"));
                    dtData.Columns.Add("LAST_NAME", System.Type.GetType("System.String"));
                    dtData.Columns.Add("MRNO", System.Type.GetType("System.String"));
                    dtData.Columns.Add("ADDRESS1", System.Type.GetType("System.String"));
                    dtData.Columns.Add("ADDRESS2", System.Type.GetType("System.String"));
                    dtData.Columns.Add("DOB", System.Type.GetType("System.String"));
                    dtData.Columns.Add("SEX", System.Type.GetType("System.String"));
                    dtData.Columns.Add("ISLIFELONG", System.Type.GetType("System.Int32"));
                    dtData.Columns.Add("VALID_UPTO", System.Type.GetType("System.String"));
                    dtData.Columns.Add("REG_DATE", System.Type.GetType("System.String"));
                    ////
                    //Add necessary data from dataset to datatable to print
                    DataRow drData = null;
                    foreach (DataRow dr in dsData.Tables["PAT_PATIENT_NAME"].Rows)
                    {
                        drData = dtData.NewRow();
                        drData["TITLE"] = dr["TITLE"].ToString();
                        drData["FIRST_NAME"] = dr["FIRST_NAME"].ToString();
                        drData["MIDDLE_NAME"] = dr["MIDDLE_NAME"].ToString();
                        drData["LAST_NAME"] = dr["LAST_NAME"].ToString();
                        drData["MRNO"] = dr["MRNO"].ToString();
                        drData["DOB"] = dr["DOB"].ToString();
                        drData["SEX"] = dr["GENDER"].ToString();

                        if (dsData.Tables["GEN_PROFILE_ADDRESS"] != null && dsData.Tables["GEN_PROFILE_ADDRESS"].Rows.Count > 0)
                        {
                            drArray = dsData.Tables["GEN_PROFILE_ADDRESS"].Select("PROFILE_ID='" + dr["MRNO"].ToString() + "'");
                            dtTemp = dsData.Tables["GEN_PROFILE_ADDRESS"].Clone();
                            drArray.CopyToDataTable(dtTemp, LoadOption.OverwriteChanges);
                            //dtData.Rows[i]["ADDRESS1"] = dsData.Tables["PATIENT_CARD"].Rows[i]["ADDRESS1"].ToString();
                            drData["ADDRESS1"] = dtTemp.Rows[0]["ADDRESS1"].ToString(); //House No.
                            if (dtTemp.Rows[0]["ADDRESS2"].ToString().Trim() != String.Empty &&
                                 (dtTemp.Rows[0]["ADDRESS3"].ToString().Trim() != String.Empty))
                            {
                                drData["ADDRESS2"] = dtTemp.Rows[0]["ADDRESS2"].ToString() + ", " + dtTemp.Rows[0]["ADDRESS3"].ToString();//street and place 
                            }
                            else if (dtTemp.Rows[0]["ADDRESS2"].ToString().Trim() != string.Empty)
                            {
                                drData["ADDRESS2"] = dtTemp.Rows[0]["ADDRESS2"].ToString();
                            }
                            else if (dtTemp.Rows[0]["ADDRESS3"].ToString().Trim() != string.Empty)
                            {
                                drData["ADDRESS2"] = dtTemp.Rows[0]["ADDRESS3"].ToString();
                            }
                            else
                            {
                                drData["ADDRESS2"] = String.Empty;
                            }
                        }

                        drArray = dsData.Tables["REG_PATIENT_REGISTRATION"].Select("MRNO='" + dr["MRNO"].ToString() + "'");
                        dtTemp = dsData.Tables["REG_PATIENT_REGISTRATION"].Clone();
                        drArray.CopyToDataTable(dtTemp, LoadOption.OverwriteChanges);
                        drData["ISLIFELONG"] = Convert.ToInt32(dtTemp.Rows[0]["ISLIFELONG"].ToString());
                        drData["VALID_UPTO"] = dtTemp.Rows[0]["VALID_UPTO"].ToString();

                        drArray = dsData.Tables["PAT_MAST_PATIENT"].Select("MRNO='" + dr["MRNO"].ToString() + "'");
                        dtTemp = dsData.Tables["PAT_MAST_PATIENT"].Clone();
                        drArray.CopyToDataTable(dtTemp, LoadOption.OverwriteChanges);
                        drData["REG_DATE"] = dtTemp.Rows[0]["REGISTERED_SINCE"].ToString();
                        dtData.Rows.Add(drData);
                    }
                    ////
                }
            }
            return dtData;
        }

        private int CheckSum(string PatId)
        {
            int chkSum = 0;
            int patDigit = 0, patVal = 0, patModVal = 0;
            int cnt;

            try
            {
                if (strBarcodeType == "Code128b")
                {
                    char[] ch = PatId.ToCharArray();
                    cnt = 1;
                    foreach (char chr in ch)
                    {
                        switch (chr)
                        {
                            case '0':
                                patDigit = 16;
                                break;
                            case '1':
                                patDigit = 17;
                                break;
                            case '2':
                                patDigit = 18;
                                break;
                            case '3':
                                patDigit = 19;
                                break;
                            case '4':
                                patDigit = 20;
                                break;
                            case '5':
                                patDigit = 21;
                                break;
                            case '6':
                                patDigit = 22;
                                break;
                            case '7':
                                patDigit = 23;
                                break;
                            case '8':
                                patDigit = 24;
                                break;
                            case '9':
                                patDigit = 25;
                                break;
                            default:
                                break;
                        }
                        patVal += (patDigit * cnt);
                        cnt++;
                    }
                    patVal = patVal + 104;
                    patModVal = patVal % 103;
                    chkSum = code128Lookup[patModVal];
                }
                return chkSum;
            }
            catch (Exception)
            {

                throw;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            objPrintDoc.PrintPage -= new PrintPageEventHandler(AlignPatientCard);
        }

        #endregion
        private bool PrintStoresShelf(DataSet dsData, PrintType PrintType, string PrinterName)
        {
            bool printStatus = false;
            DataTable dtPrintData = new DataTable();
            try
            {
                //Setting card printing data.
                // dtPrintData = SelectDataToPrint(dsData);
                PrintDialog pd = new PrintDialog();
                if (dsData.Tables["STORE_SHELF"].Rows.Count > 0)
                {
                    objPrintDoc = new PrintDocument();
                    if (PrinterName.Length > 0)
                    {
                        objPrintDoc.PrinterSettings.PrinterName = PrinterName;
                    }
                    else
                    {
                        pd.PrinterSettings = new PrinterSettings();
                        pd.ShowDialog();
                        objPrintDoc.PrinterSettings.PrinterName = pd.PrinterSettings.PrinterName;
                    }

                    foreach (DataRow dr in dsData.Tables["STORE_SHELF"].Rows)
                    {
                        drPrintData = dr;
                        //Creating print data from dtPrintData in AlignPatientCard event.
                        objPrintDoc.PrintPage += new PrintPageEventHandler(AlignPatientStoreShelf);
                        objPrintDoc.Print();
                        Dispose();
                    }
                    printStatus = true;
                }
            }
            catch (InvalidPrinterException)
            {
                giMessageBox.Show(Infologics.Medilogics.CommonClient.Controls.StaticData.CommonData.MESSAGEHEADER, "Wrist Band Printer not Found", MessageBoxButtonType.OK, MessageBoxImages.Warning);
            }
            catch (Exception)
            {
                throw;
            }
            return printStatus;
        }
        private void AlignPatientStoreShelf(Object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            try
            {


                Brush br = new SolidBrush(Color.Black);
                PrintFont = new Font("Times New Roman", 8);
                string photoPath = string.Empty;
                Image logo1 = null;
                SolidBrush drawBrush = new SolidBrush(Color.Black);
                Font drawFont = new System.Drawing.Font("Times New Roman", 8, FontStyle.Bold);
                Font drawHeading = new System.Drawing.Font("Times New Roman", 8, FontStyle.Bold);
                StringFormat drawFormat = new StringFormat();
                drawFormat.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                g.DrawString("ERP Code of Item: " + drPrintData["ERP_CODE"].ToString(), drawFont, br, 120, 60);// left qty  260
                g.DrawString("Name of Item   : " + drPrintData["ITEM_NAME"].ToString(), drawFont, br, 120, 80);//pharmacy code



            }
            catch (Exception)
            {

                throw;
            }
        }
        private bool PrintPatientWristBand(DataSet dsData, PrintType PrintType, string PrinterName)
        {
            bool printStatus = false;
            DataTable dtPrintData = new DataTable();
            try
            {
                //Setting card printing data.
                // dtPrintData = SelectDataToPrint(dsData);
                PrintDialog pd = new PrintDialog();
                if (dsData.Tables["PH_PATIENT"].Rows.Count > 0)
                {
                    objPrintDoc = new PrintDocument();
                    if (PrinterName.Length > 0)
                    {
                        objPrintDoc.PrinterSettings.PrinterName = PrinterName;
                    }
                    else
                    {
                        pd.PrinterSettings = new PrinterSettings();
                        pd.ShowDialog();
                        objPrintDoc.PrinterSettings.PrinterName = pd.PrinterSettings.PrinterName;
                    }

                    foreach (DataRow dr in dsData.Tables["PH_PATIENT"].Rows)
                    {
                        drPrintData = dr;
                        //Creating print data from dtPrintData in AlignPatientCard event.
                        objPrintDoc.PrintPage += new PrintPageEventHandler(AlignPatientWristBand);
                        objPrintDoc.Print();
                        Dispose();
                    }
                    printStatus = true;
                }
            }
            catch (InvalidPrinterException)
            {
                giMessageBox.Show(Infologics.Medilogics.CommonClient.Controls.StaticData.CommonData.MESSAGEHEADER, "Wrist Band Printer not Found", MessageBoxButtonType.OK, MessageBoxImages.Warning);
            }
            catch (Exception)
            {
                throw;
            }
            return printStatus;
        }
        private bool PrintStores(DataSet dsData, PrintType PrintType, string PrinterName)
        {
            bool printStatus = false;
            DataTable dtPrintData = new DataTable();
            try
            {
                //Setting card printing data.
                // dtPrintData = SelectDataToPrint(dsData);
                PrintDialog pd = new PrintDialog();
                if (dsData.Tables["PH_PAT_DTLS_ORDER"].Rows.Count > 0)
                {
                    objPrintDoc = new PrintDocument();
                    if (PrinterName.Length > 0)
                    {
                        objPrintDoc.PrinterSettings.PrinterName = PrinterName;
                    }
                    else
                    {
                        pd.PrinterSettings = new PrinterSettings();
                        pd.ShowDialog();
                        objPrintDoc.PrinterSettings.PrinterName = pd.PrinterSettings.PrinterName;
                    }

                    foreach (DataRow dr in dsData.Tables["PH_PAT_DTLS_ORDER"].Rows)
                    {
                        drPrintData = dr;
                        //Creating print data from dtPrintData in AlignPatientCard event.
                        objPrintDoc.PrintPage += new PrintPageEventHandler(AlignPatientStore);
                        objPrintDoc.Print();
                        Dispose();
                    }
                    printStatus = true;
                }
            }
            catch (InvalidPrinterException)
            {
                giMessageBox.Show(Infologics.Medilogics.CommonClient.Controls.StaticData.CommonData.MESSAGEHEADER, "Printer not Found", MessageBoxButtonType.OK, MessageBoxImages.Warning);
            }
            catch (Exception)
            {
                throw;
            }
            return printStatus;
        }
        private void AlignPatientStore(Object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            try
            {

                //string StrAllergy = string.Empty;
                //Brush br = new SolidBrush(Color.Black);
                //PrintFont = new Font("Times New Roman", 8);
                //string photoPath = string.Empty;
                //Image logo1 = null;
                //SolidBrush drawBrush = new SolidBrush(Color.Black);
                //Font drawFont = new System.Drawing.Font("Times New Roman", 8, FontStyle.Bold);
                //Font drawHeading = new System.Drawing.Font("Times New Roman", 9, FontStyle.Bold);
                //StringFormat drawFormat = new StringFormat();
                //drawFormat.FormatFlags = StringFormatFlags.DirectionRightToLeft;

                ////g.DrawString("Danat Al Emarat Hospital Women’s & Children", drawHeading, br, 130, 15); // Hospital name
                //// g.DrawString(drPrintData["Address1"].ToString(), drawHeading, br, 150, 25);//
                //// g.DrawString(drPrintData["Address2"].ToString(), drawHeading, br, 180, 35);//

                //g.DrawString("Name : ", PrintFont, br, 06, 10);//Mrno
                //g.DrawString("MRN : ", PrintFont, br, 202, 10);//clinic code 250
                //g.DrawString("Weight : ", PrintFont, drawBrush, 06, 25);
                //g.DrawString("Allergy : ", PrintFont, drawBrush, 85, 25);

                //g.DrawString(drPrintData["PATIENT_NAME"].ToString(), drawFont, br, 45, 10);//Mrno
                //g.DrawString(drPrintData["MRNO"].ToString(), drawFont, br, 232, 10);//clinic code 250
                //if (dsTemp.Tables.Contains("EMR_PAT_WARNING") & dsTemp.Tables["EMR_PAT_WARNING"].Rows.Count > 0)
                //{
                //    bool IsMultiAllergy = false;
                //    foreach (DataRow dr in dsTemp.Tables["EMR_PAT_WARNING"].Rows)
                //    {
                //        if (IsMultiAllergy)
                //        {
                //            StrAllergy = StrAllergy + " " + "-" + " ";
                //        }
                //        StrAllergy = StrAllergy + Convert.ToString(dr["ALLERGY"]);
                //        IsMultiAllergy = true;

                //    }
                //    g.DrawString(StrAllergy, drawFont, drawBrush, 130, 25);
                //}
                //else
                //{
                //    g.DrawString(drPrintData["ALLERGY"].ToString(), drawFont, drawBrush, 130, 25);
                //}
                //g.DrawString(drPrintData["Weight"].ToString(), drawFont, drawBrush, 52, 25);
                //g.DrawString("Concentration:", PrintFont, drawBrush, 06, 40);
                //g.DrawString(drPrintData["DRUG_NAME"].ToString(), drawFont, drawBrush, 80, 40);

                ////g.DrawString("Patient Name: " + drPrintData["PATIENT_NAME"].ToString(), PrintFont, br, 15, 50);//Mrno
                ////g.DrawString("MRN: " + drPrintData["MRNO"].ToString(), PrintFont, br, 150, 50);//clinic code 250
                ////g.DrawString("Allergy: " + drPrintData["ALLERGY"].ToString(), PrintFont, drawBrush, 320, 50, drawFormat);
                ////g.DrawString("Weight: " + drPrintData["Weight"].ToString(), PrintFont, drawBrush, 420, 50, drawFormat);

                //g.DrawString("Generics : ", PrintFont, br, 06, 60);//Mrno
                //g.DrawString("Dose : ", PrintFont, br, 180, 60);//clinic code 250


                //g.DrawString(drPrintData["GENERIC"].ToString(), drawFont, br, 52, 60);
                //g.DrawString(drPrintData["DOSE"].ToString(), drawFont, drawBrush, 210, 60);
                ////g.DrawString("Volume : ", PrintFont, drawBrush, 360, 26);
                ////g.DrawString(drPrintData["QUANTITY"].ToString(), drawFont, drawBrush, 410, 26);

                //// g.DrawString("Diluent : " + drPrintData["DILUENT"].ToString(), PrintFont, br, 15, 100);
                ////g.DrawString("Volume : " + drPrintData["QUANTITY"].ToString(), PrintFont, drawBrush, 280, 100);

                //g.DrawString("Diluent : ", PrintFont, br, 06, 75);
                //g.DrawString(drPrintData["FLUID"].ToString(), drawFont, br, 52, 75);

                //g.DrawString("Route : ", PrintFont, br, 06, 95);
                //g.DrawString("Frequency : ", PrintFont, drawBrush, 130, 95);

                //g.DrawString(drPrintData["ROUTE"].ToString(), drawFont, br, 52, 95);
                //g.DrawString(drPrintData["FREQUENCY"].ToString(), drawFont, drawBrush, 190, 95);

                //g.DrawString("Volume : ", PrintFont, drawBrush, 06, 110);
                //g.DrawString(drPrintData["QUANTITY"].ToString(), drawFont, drawBrush, 52, 110);

                //g.DrawString("Rate : ", PrintFont, drawBrush, 130, 110);
                //g.DrawString("Infuse over : ", PrintFont, br, 05, 125);//pharmacy code

                //g.DrawString(drPrintData["DURATION"].ToString(), drawFont, br, 65, 125);//pharmacy code
                //g.DrawString(drPrintData["FLOW_RATE"].ToString(), drawFont, drawBrush, 190, 110);

                //g.DrawString("Storage and Handling : ", PrintFont, br, 06, 150);
                //g.DrawString(drPrintData["STORAGE"].ToString(), drawFont, br, 125, 150);

                //g.DrawString("Due date : ", PrintFont, br, 130, 170);
                //g.DrawString(drPrintData["DUE_DATE"].ToString(), drawFont, br, 180, 170);
                //g.DrawString("Expiry : ", PrintFont, br, 06, 170);
                //g.DrawString("Prepared by : ", PrintFont, br, 05, 210);

                //g.DrawString("Prepared Date : ", PrintFont, br, 05, 190);
                //g.DrawString("Checked  PH : " + drPrintData["CHECKEDBY_USERNAME"].ToString(), PrintFont, br, 05, 230);
                //g.DrawString("RN Ckd : ", PrintFont, br, 05, 250);
                ////g.DrawString("Expiry : " + drPrintData["EXPIRY"].ToString(), PrintFont, br, 150, 140);
                ////g.DrawString("Prepared By : ", PrintFont, br, 15, 160);// left qty  260
                ////g.DrawString("Checked PH : ", PrintFont, br, 150, 160);
                ////g.DrawString("RN Checked : ", PrintFont, br, 150, 160);


                //g.DrawString("Please return to Pharmacy when d/c or expired ", drawHeading, br, 20, 280);
                ////BarcodeLib.Barcode.QRCode barcode = new BarcodeLib.Barcode.QRCode();
                ////barcode.Data = Convert.ToString(drPrintData["BARCODE"]);
                ////barcode.ModuleSize = 1;
                ////barcode.LeftMargin = 0;
                ////barcode.RightMargin = 0;
                ////barcode.TopMargin = 0;
                ////barcode.BottomMargin = 0;
                ////barcode.Encoding = BarcodeLib.Barcode.QRCodeEncoding.Auto;
                ////barcode.Version = BarcodeLib.Barcode.QRCodeVersion.V1;
                ////barcode.ECL = BarcodeLib.Barcode.QRCodeErrorCorrectionLevel.L;
                ////barcode.ImageFormat = System.Drawing.Imaging.ImageFormat.Png;
                ////string Path = System.AppDomain.CurrentDomain.BaseDirectory + "Qrcode1.png";
                ////barcode.drawBarcode(Path);
                ////Image logo2 = Image.FromFile(Path);// drPrintData["LOGO_PATH"].ToString()
                ////Rectangle destinationn = new Rectangle(210, 200, 50, 50);
                ////g.DrawImage(logo2, destinationn, 0, 0, logo2.Width, logo2.Height, GraphicsUnit.Pixel);
                //var qrcode = new QRCodeWriter();
                //var qrValue = drPrintData["MRNO"].ToString();

                //var barcodeWriter = new BarcodeWriter
                //{
                //    Format = BarcodeFormat.QR_CODE,
                //    Options = new EncodingOptions
                //    {
                //        Height = 300,
                //        Width = 300,
                //        Margin = 1
                //    }
                //};

                //using (var bitmap = barcodeWriter.Write(qrValue))
                //    bitmap.Save(strPath, ImageFormat.Png);

                //Image logo2 = Image.FromFile(strPath);// drPrintData["LOGO_PATH"].ToString()
                //Rectangle destinationn = new Rectangle(210, 200, 50, 50);
                //g.DrawImage(logo2, destinationn, 0, 0, logo2.Width, logo2.Height, GraphicsUnit.Pixel);


                //// Rectangle rect2 = new Rectangle(05, 05, 283, 280);
                //// g.DrawRectangle(Pens.Black, Rectangle.Round(rect2));
                //logo2.Dispose();
                //if (File.Exists(strPath))
                //{
                //    File.Delete(strPath);
                //}
            }
            catch (Exception)
            {


            }
        }
        private bool PrintAsset(DataSet dsData, PrintType PrintType, string PrinterName)
        {
            bool printStatus = false;
            DataTable dtPrintData = new DataTable();
            try
            {
                //Setting card printing data.
                // dtPrintData = SelectDataToPrint(dsData);
                PrintDialog pd = new PrintDialog();
                if (dsData.Tables["ASSET"].Rows.Count > 0)
                {
                    objPrintDoc = new PrintDocument();
                    if (PrinterName.Length > 0)
                    {
                        objPrintDoc.PrinterSettings.PrinterName = PrinterName;
                    }
                    else
                    {
                        pd.PrinterSettings = new PrinterSettings();
                        pd.ShowDialog();
                        objPrintDoc.PrinterSettings.PrinterName = pd.PrinterSettings.PrinterName;
                    }

                    foreach (DataRow dr in dsData.Tables["ASSET"].Rows)
                    {
                        drPrintData = dr;
                        //Creating print data from dtPrintData in AlignPatientCard event.
                        objPrintDoc.PrintPage += new PrintPageEventHandler(AlignAsset);
                        objPrintDoc.Print();
                        Dispose();
                    }
                    printStatus = true;
                }
            }
            catch (InvalidPrinterException)
            {
                giMessageBox.Show(Infologics.Medilogics.CommonClient.Controls.StaticData.CommonData.MESSAGEHEADER, "Printer not Found", MessageBoxButtonType.OK, MessageBoxImages.Warning);
            }
            catch (Exception)
            {
                throw;
            }
            return printStatus;
        }
        private void AlignAsset(Object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            try
            {


                Brush br = new SolidBrush(Color.Black);
                PrintFont = new Font("Times New Roman", 8);
                string photoPath = string.Empty;
                Image logo1 = null;
                SolidBrush drawBrush = new SolidBrush(Color.Black);
                Font drawFont = new System.Drawing.Font("Times New Roman", 8, FontStyle.Bold);
                Font drawHeading = new System.Drawing.Font("Times New Roman", 8, FontStyle.Bold);
                StringFormat drawFormat = new StringFormat();
                drawFormat.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                g.DrawString("ERP Code of Asset : " + drPrintData["ERP_CODE"].ToString(), drawFont, br, 120, 60);// left qty  260
                g.DrawString("Name of Asset    : " + drPrintData["ASSET_NAME"].ToString(), drawFont, br, 120, 80);//pharmacy code


            }
            catch (Exception)
            {

                throw;
            }
        }
        private bool PrintPatientLabel(DataSet dsData, PrintType PrintType, string PrinterName)
        {
            bool printStatus = false;
            DataTable dtPrintData = new DataTable();
            try
            {
                //Setting card printing data.
                // dtPrintData = SelectDataToPrint(dsData);
                PrintDialog pd = new PrintDialog();
                if (dsData.Tables["PATIENT_SLIP_DATA"].Rows.Count > 0)
                {
                    objPrintDoc = new PrintDocument();
                    if (PrinterName.Length > 0)
                    {
                        objPrintDoc.PrinterSettings.PrinterName = PrinterName;
                    }
                    else
                    {
                        pd.PrinterSettings = new PrinterSettings();
                        pd.ShowDialog();
                        objPrintDoc.PrinterSettings.PrinterName = pd.PrinterSettings.PrinterName;
                    }

                    foreach (DataRow dr in dsData.Tables["PATIENT_SLIP_DATA"].Rows)
                    {
                        drPrintData = dr;
                        //Creating print data from dtPrintData in AlignPatientCard event.
                        objPrintDoc.PrintPage += new PrintPageEventHandler(AlignPatientLabel);
                        objPrintDoc.Print();
                        Dispose();
                    }
                    printStatus = true;
                }
            }
            catch (InvalidPrinterException)
            {
                giMessageBox.Show(Infologics.Medilogics.CommonClient.Controls.StaticData.CommonData.MESSAGEHEADER, "Patient label Printer not Found", MessageBoxButtonType.OK, MessageBoxImages.Warning);
            }
            catch (Exception)
            {
                throw;
            }
            return printStatus;
        }
        private void AlignPatientWristBand(Object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            try
            {
                string Path = System.AppDomain.CurrentDomain.BaseDirectory + System.DateTime.Now.ToString("ddmmyyhhmmss");
                string Gender = Convert.ToString(drPrintData["GENDER"]).ToUpper();
                if (Gender == "MALE")
                {
                    Gender = "M";
                }
                else if (Gender == "FEMALE")
                {
                    Gender = "F";
                }
                if (Convert.ToString(drPrintData["ISINFANT"]) == "1")
                {
                    string Strmrno = "MRN : " + Convert.ToString(drPrintData["MRNO"]);
                    string StrName = Convert.ToString(drPrintData["PATIENT_NAME"]).ToUpper();
                    string Strdob = "DOB : " + Convert.ToString(drPrintData["DOB"]);
                    //NEED TO WRITE LOGIC TO SHOW AGE IN DAY FOR INFANT 
                    string Strage = "AGE : " + Convert.ToString(drPrintData["AGE"]) + "(D)" + "   " + "GENDER :" + Gender;
                    System.Drawing.Font drawFont = new System.Drawing.Font("Times New Roman", 8, FontStyle.Bold);
                    System.Drawing.Font drawName = new System.Drawing.Font("Times New Roman", 10, FontStyle.Bold);
                    System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
                    float x = 150.0f;
                    float y = 170.0f;
                    System.Drawing.StringFormat drawFormat = new System.Drawing.StringFormat(StringFormatFlags.DirectionVertical);
                    if (StrName.Length > 30)
                    {
                        g.DrawString(StrName, drawName, drawBrush, 80, 40, drawFormat);
                    }
                    else
                    {
                        g.DrawString(StrName, drawName, drawBrush, 80, 130, drawFormat);
                    }
                    g.DrawString(Strmrno, drawFont, drawBrush, 60, y, drawFormat);
                    g.DrawString(Strdob, drawFont, drawBrush, 40, y, drawFormat);
                    g.DrawString(Strage, drawFont, drawBrush, 20, y, drawFormat);
                    var qrcode = new QRCodeWriter();
                    var qrValue = drPrintData["MRNO"].ToString();
                    var barcodeWriter = new BarcodeWriter
                    {
                        Format = BarcodeFormat.QR_CODE,
                        Options = new EncodingOptions
                        {
                            Height = 300,
                            Width = 300,
                            Margin = 1
                        }
                    };

                    using (var bitmap = barcodeWriter.Write(qrValue))
                        bitmap.Save(strPath, ImageFormat.Png);
                    Image logo1 = Image.FromFile(strPath);
                    Rectangle destinationn = new Rectangle(25, 40, 50, 50);
                    g.DrawImage(logo1, destinationn, 0, 0, logo1.Width, logo1.Height, GraphicsUnit.Pixel);
                    Rectangle destinationn1 = new Rectangle(25, 350, 50, 50);
                    g.DrawImage(logo1, destinationn1, 0, 0, logo1.Width, logo1.Height, GraphicsUnit.Pixel);
                    logo1.Dispose();
                    if (File.Exists(strPath))
                    {
                        File.Delete(strPath);
                    }
                }
                else if (Convert.ToString(drPrintData["ISCHILD"]) == "1")
                {

                    string Strmrno = "MRN : " + Convert.ToString(drPrintData["MRNO"]);
                    string StrName = Convert.ToString(drPrintData["PATIENT_NAME"]).ToUpper();
                    string Strdob = "DOB : " + Convert.ToString(drPrintData["DOB"]);
                    //NEED TO WRITE LOGIC TO SHOW AGE IN NO YEAR FOR CHILD
                    string Strage = "AGE : " + Convert.ToString(drPrintData["AGE"]) + "(Y)" + "   " + "GENDER :" + Gender;
                    System.Drawing.Font drawFont = new System.Drawing.Font("Times New Roman", 10, FontStyle.Bold);
                    System.Drawing.Font drawName = new System.Drawing.Font("Times New Roman", 12, FontStyle.Bold);
                    System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
                    float x = 150.0f;
                    float y = 180.0f;
                    System.Drawing.StringFormat drawFormat = new System.Drawing.StringFormat(StringFormatFlags.DirectionVertical);
                    if (StrName.Length < 30)
                    {
                        g.DrawString(StrName, drawName, drawBrush, 80, 140, drawFormat);
                    }
                    else
                    {
                        g.DrawString(StrName, drawName, drawBrush, 80, 55, drawFormat);
                    }
                    g.DrawString(Strmrno, drawFont, drawBrush, 60, y, drawFormat);
                    g.DrawString(Strdob, drawFont, drawBrush, 40, y, drawFormat);
                    g.DrawString(Strage, drawFont, drawBrush, 20, y, drawFormat);
                    #region Create QR Code Image
                    var qrcode = new QRCodeWriter();
                    var qrValue = drPrintData["MRNO"].ToString();

                    var barcodeWriter = new BarcodeWriter
                    {
                        Format = BarcodeFormat.QR_CODE,
                        Options = new EncodingOptions
                        {
                            Height = 300,
                            Width = 300,
                            Margin = 1
                        }
                    };
                    using (var bitmap = barcodeWriter.Write(qrValue))
                        bitmap.Save(strPath, ImageFormat.Png);
                    Image logo1 = Image.FromFile(strPath);
                    Rectangle destinationn = new Rectangle(25, 60, 50, 50);
                    g.DrawImage(logo1, destinationn, 0, 0, logo1.Width, logo1.Height, GraphicsUnit.Pixel);
                    Rectangle destinationn1 = new Rectangle(25, 420, 50, 50);
                    g.DrawImage(logo1, destinationn1, 0, 0, logo1.Width, logo1.Height, GraphicsUnit.Pixel);

                    logo1.Dispose();
                    //deleting qr image after printing 
                    if (File.Exists(strPath))
                    {
                        File.Delete(strPath);
                    }
                    #endregion
                }

                else if (Convert.ToString(drPrintData["ISADULT"]) == "1")
                {

                    string Strmrno = "MRN : " + Convert.ToString(drPrintData["MRNO"]);
                    string StrName = Convert.ToString(drPrintData["PATIENT_NAME"]).ToUpper();
                    string Strdob = "DOB : " + Convert.ToString(drPrintData["DOB"]);
                    //NEED TO WRITE LOGIC TO SHOW AGE IN NO OF YEARS
                    string Strage = "AGE : " + Convert.ToString(drPrintData["AGE"]) + "(Y)" + "   " + "GENDER :" + Gender;
                    System.Drawing.Font drawFont = new System.Drawing.Font("Times New Roman", 10, FontStyle.Bold);
                    System.Drawing.Font drawName = new System.Drawing.Font("Times New Roman", 12, FontStyle.Bold);
                    System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
                    float x = 150.0f;
                    float y = 480.0f;
                    System.Drawing.StringFormat drawFormat = new System.Drawing.StringFormat(StringFormatFlags.DirectionVertical);
                    g.DrawString(StrName, drawName, drawBrush, 77, y, drawFormat);
                    g.DrawString(Strmrno, drawFont, drawBrush, 60, y, drawFormat);
                    g.DrawString(Strdob, drawFont, drawBrush, 40, y, drawFormat);
                    g.DrawString(Strage, drawFont, drawBrush, 20, y, drawFormat);

                    #region Create QR Code Image
                    var qrcode = new QRCodeWriter();
                    var qrValue = drPrintData["MRNO"].ToString();

                    var barcodeWriter = new BarcodeWriter
                    {
                        Format = BarcodeFormat.QR_CODE,
                        Options = new EncodingOptions
                        {
                            Height = 300,
                            Width = 300,
                            Margin = 1
                        }
                    };

                    using (var bitmap = barcodeWriter.Write(qrValue))
                        bitmap.Save(strPath, ImageFormat.Png);
                    Image logo1 = Image.FromFile(strPath);

                    Rectangle destinationn = new Rectangle(30, 410, 50, 50);
                    g.DrawImage(logo1, destinationn, 0, 0, logo1.Width, logo1.Height, GraphicsUnit.Pixel);
                    Rectangle destinationn1 = new Rectangle(30, 800, 50, 50);
                    g.DrawImage(logo1, destinationn1, 0, 0, logo1.Width, logo1.Height, GraphicsUnit.Pixel);
                    logo1.Dispose();
                    //deleting image after printing
                    if (File.Exists(strPath))
                    {
                        File.Delete(strPath);
                    }
                    #endregion
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void AlignPatientLabel(Object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            try
            {
                //NO NEED TO CHECK BARCODE COUNT NEED TO PRINT  ON 3*8 A4 PAPER
                string Gender = Convert.ToString(drPrintData["GENDER"]).ToUpper();
                if (Gender == "MALE")
                {
                    Gender = "M";
                }
                else if (Gender == "FEMALE")
                {
                    Gender = "F";
                }
                Brush br = new SolidBrush(Color.Black);
                string MrnoLabel = string.Empty;
                MrnoLabel = "MRNO :";
                string mrno = Convert.ToString(drPrintData["MRNO"]);
                string EncounterNo = Convert.ToString(drPrintData["ENCOUNTER_NO"]);
                // string Insurance = Convert.ToString(drPrintData["INSURANCE_NAME"]) +"                  "+"Encounter No"+"  "+ EncounterNo;
                string Name = Convert.ToString(drPrintData["PATIENT_NAME"]);
                string doB = "DOB :" + Convert.ToString(drPrintData["DOB"]);
                //NEED TO CALCULATE AGE FROM DATE OF BIRTH AND SHOW BELOW 
                //BASED ON THE AGE IF ITS LESS THEN YEAR AGE WILL BE PREFIXED WITH D,
                //IF ITS MORE THEN YEAR IT WILL BE PREFIXED WITH Y
                string AGE = "AGE : 5" + "(Y)" + "   " + "GENDER :" + Gender;
                string InsuranceName = "INS:";// + Insurance;
                System.Drawing.Font drawFont = new System.Drawing.Font("Times New Roman", 8, FontStyle.Bold);
                System.Drawing.Font drawName = new System.Drawing.Font("Times New Roman", 8, FontStyle.Bold);
                System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
                float y = 100.0f;
                // StringFormat drawFormat = new StringFormat();
                // drawFormat.FormatFlags = StringFormatFlags.;
                g.DrawString(Name, drawFont, drawBrush, 10, 40);
                g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 10, 55);
                g.DrawString(doB, drawFont, drawBrush, 120, 55);
                g.DrawString(AGE, drawFont, drawBrush, 10, 70);
                g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 10, 90);//6
                g.DrawString(InsuranceName, drawFont, drawBrush, 10, 125);
                //g.DrawString("š" + drPrintData["MRNO"].ToString().Trim() + Convert.ToString((char)CheckSum(drPrintData["MRNO"].ToString().Trim())) + "œ", BarcodeFont, br, 15, 12);//6
                //g.DrawString("*" + drPrintData["MRNO"].ToString().Trim() + "*", BarcodeFont, br, 15, 1);//6

                g.DrawString(Name, drawFont, drawBrush, 270, 40);
                g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 270, 55);
                g.DrawString(doB, drawFont, drawBrush, 380, 55);
                g.DrawString(AGE, drawFont, drawBrush, 270, 70);
                g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 270, 90);//6
                g.DrawString(InsuranceName, drawFont, drawBrush, 270, 125);

                g.DrawString(Name, drawFont, drawBrush, 550, 40);
                g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 550, 55);
                g.DrawString(doB, drawFont, drawBrush, 660, 55);
                g.DrawString(AGE, drawFont, drawBrush, 550, 70);
                g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 550, 90);//6
                g.DrawString(InsuranceName, drawFont, drawBrush, 550, 130);
                //first row ends here
                g.DrawString(Name, drawFont, drawBrush, 10, 170);
                g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 10, 185);
                g.DrawString(doB, drawFont, drawBrush, 120, 185);
                g.DrawString(AGE, drawFont, drawBrush, 10, 200);
                g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 10, 220);
                g.DrawString(InsuranceName, drawFont, drawBrush, 10, 260);

                g.DrawString(Name, drawFont, drawBrush, 270, 170);
                g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 270, 185);
                g.DrawString(doB, drawFont, drawBrush, 380, 185);
                g.DrawString(AGE, drawFont, drawBrush, 270, 200);
                g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 270, 220);
                g.DrawString(InsuranceName, drawFont, drawBrush, 270, 260);

                g.DrawString(Name, drawFont, drawBrush, 550, 170);
                g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 550, 185);
                g.DrawString(doB, drawFont, drawBrush, 660, 185);
                g.DrawString(AGE, drawFont, drawBrush, 550, 200);
                g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 550, 220);
                g.DrawString(InsuranceName, drawFont, drawBrush, 550, 260);
                //2nd row end
                g.DrawString(Name, drawFont, drawBrush, 10, 310);
                g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 10, 325);
                g.DrawString(doB, drawFont, drawBrush, 120, 325);
                g.DrawString(AGE, drawFont, drawBrush, 10, 340);
                g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 10, 360);
                g.DrawString(InsuranceName, drawFont, drawBrush, 10, 400);

                g.DrawString(Name, drawFont, drawBrush, 270, 310);
                g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 270, 325);
                g.DrawString(doB, drawFont, drawBrush, 380, 325);
                g.DrawString(AGE, drawFont, drawBrush, 270, 340);
                g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 270, 360);
                g.DrawString(InsuranceName, drawFont, drawBrush, 270, 400);

                g.DrawString(Name, drawFont, drawBrush, 550, 310);
                g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 550, 325);
                g.DrawString(doB, drawFont, drawBrush, 660, 325);
                g.DrawString(AGE, drawFont, drawBrush, 550, 340);
                g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 550, 360);
                g.DrawString(InsuranceName, drawFont, drawBrush, 550, 400);
                //third row end here
                g.DrawString(Name, drawFont, drawBrush, 10, 450);
                g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 10, 465);
                g.DrawString(doB, drawFont, drawBrush, 120, 465);
                g.DrawString(AGE, drawFont, drawBrush, 10, 480);
                g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 10, 500);
                g.DrawString(InsuranceName, drawFont, drawBrush, 10, 540);

                g.DrawString(Name, drawFont, drawBrush, 270, 450);
                g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 270, 465);
                g.DrawString(doB, drawFont, drawBrush, 380, 465);
                g.DrawString(AGE, drawFont, drawBrush, 270, 480);
                g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 270, 500);
                g.DrawString(InsuranceName, drawFont, drawBrush, 270, 540);

                g.DrawString(Name, drawFont, drawBrush, 550, 450);
                g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 550, 465);
                g.DrawString(doB, drawFont, drawBrush, 660, 465);
                g.DrawString(AGE, drawFont, drawBrush, 550, 480);
                g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 550, 500);
                g.DrawString(InsuranceName, drawFont, drawBrush, 550, 540);
                //fourth ends here
                g.DrawString(Name, drawFont, drawBrush, 10, 580);
                g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 10, 595);
                g.DrawString(doB, drawFont, drawBrush, 120, 595);
                g.DrawString(AGE, drawFont, drawBrush, 10, 610);
                g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 10, 630);
                g.DrawString(InsuranceName, drawFont, drawBrush, 10, 670);

                g.DrawString(Name, drawFont, drawBrush, 270, 580);
                g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 270, 595);
                g.DrawString(doB, drawFont, drawBrush, 380, 595);
                g.DrawString(AGE, drawFont, drawBrush, 270, 610);
                g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 270, 630);
                g.DrawString(InsuranceName, drawFont, drawBrush, 270, 670);

                g.DrawString(Name, drawFont, drawBrush, 550, 580);
                g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 550, 595);
                g.DrawString(doB, drawFont, drawBrush, 660, 595);
                g.DrawString(AGE, drawFont, drawBrush, 550, 610);
                g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 550, 630);
                g.DrawString(InsuranceName, drawFont, drawBrush, 550, 670);
                //fifth row ends here

                g.DrawString(Name, drawFont, drawBrush, 10, 710);
                g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 10, 725);
                g.DrawString(doB, drawFont, drawBrush, 120, 725);
                g.DrawString(AGE, drawFont, drawBrush, 10, 740);
                g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 10, 760);
                g.DrawString(InsuranceName, drawFont, drawBrush, 10, 800);

                g.DrawString(Name, drawFont, drawBrush, 270, 710);
                g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 270, 725);
                g.DrawString(doB, drawFont, drawBrush, 380, 725);
                g.DrawString(AGE, drawFont, drawBrush, 270, 740);
                g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 270, 760);
                g.DrawString(InsuranceName, drawFont, drawBrush, 270, 800);

                g.DrawString(Name, drawFont, drawBrush, 550, 710);
                g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 550, 725);
                g.DrawString(doB, drawFont, drawBrush, 660, 725);
                g.DrawString(AGE, drawFont, drawBrush, 550, 740);
                g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 550, 760);
                g.DrawString(InsuranceName, drawFont, drawBrush, 550, 800);
                //sixth row ends here

                g.DrawString(Name, drawFont, drawBrush, 10, 840);
                g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 10, 855);
                g.DrawString(doB, drawFont, drawBrush, 120, 855);
                g.DrawString(AGE, drawFont, drawBrush, 10, 870);
                g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 10, 890);
                g.DrawString(InsuranceName, drawFont, drawBrush, 10, 930);

                g.DrawString(Name, drawFont, drawBrush, 270, 840);
                g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 270, 855);
                g.DrawString(doB, drawFont, drawBrush, 380, 855);
                g.DrawString(AGE, drawFont, drawBrush, 270, 870);
                g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 270, 890);
                g.DrawString(InsuranceName, drawFont, drawBrush, 270, 930);

                g.DrawString(Name, drawFont, drawBrush, 550, 840);
                g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 550, 855);
                g.DrawString(doB, drawFont, drawBrush, 660, 855);
                g.DrawString(AGE, drawFont, drawBrush, 550, 870);
                g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 550, 890);
                g.DrawString(InsuranceName, drawFont, drawBrush, 550, 930);
                //seventh row ends here
                g.DrawString(Name, drawFont, drawBrush, 10, 970);
                g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 10, 985);
                g.DrawString(doB, drawFont, drawBrush, 120, 985);
                g.DrawString(AGE, drawFont, drawBrush, 10, 1000);
                g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 10, 1020);
                g.DrawString(InsuranceName, drawFont, drawBrush, 10, 1060);

                g.DrawString(Name, drawFont, drawBrush, 270, 970);
                g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 270, 985);
                g.DrawString(doB, drawFont, drawBrush, 380, 985);
                g.DrawString(AGE, drawFont, drawBrush, 270, 1000);
                g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 270, 1020);
                g.DrawString(InsuranceName, drawFont, drawBrush, 270, 1060);

                g.DrawString(Name, drawFont, drawBrush, 550, 970);
                g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 550, 985);
                g.DrawString(doB, drawFont, drawBrush, 660, 985);
                g.DrawString(AGE, drawFont, drawBrush, 550, 1000);
                g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 550, 1020);
                g.DrawString(InsuranceName, drawFont, drawBrush, 550, 1060);


            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}




////using System;
////using System.Collections.Generic;
////using System.Linq;
////using System.Text;
////using Infologics.Medilogics.PrintingLibrary.Main;
////using Infologics.Medilogics.Enumerators.General;
////using System.Windows.Forms;
////using System.Drawing.Printing;
////using System.Drawing;
////using System.Data;
////using System.Configuration;

////namespace Infologics.Medilogics.PrintingLibrary.PatientCard
////{
////    public class BLPatientCard : IPrinting,IDisposable
////    {
////        string strBarcodeFont, strBarcodeType;
////        private static Font BarcodeFont = null;
////        private static Font PrintFont = null;
////        private static int[] code128Lookup = null;
////        private string cardPrinterName = string.Empty;
////        PrintDocument objPrintDoc;        
////        DataRow drPrintData; 
////        public BLPatientCard()
////        {
////            try
////            {
////                strBarcodeFont = ConfigurationSettings.AppSettings["BarcodeFont"].ToString();
////                strBarcodeType = ConfigurationSettings.AppSettings["BarcodeType"].ToString();
////                BarcodeFont = new Font(strBarcodeFont, 18);
////                if (strBarcodeType == "Code128b")
////                {
////                    code128Lookup = new int[103];
////                    InitializeLookup();
////                }
////            }
////            catch (Exception)
////            {
////                throw;
////            }

////        }
////        private void InitializeLookup()
////        {
////            try
////            {
////                int cnt;
////                if (strBarcodeType == "Code128b")
////                {
////                    code128Lookup[0] = 128;
////                    for (cnt = 1; cnt <= 94; cnt++)
////                    {
////                        code128Lookup[cnt] = cnt + 32;
////                    }
////                    for (cnt = 95; cnt <= 102; cnt++)
////                    {
////                        code128Lookup[cnt] = cnt + 50;
////                    }
////                }
////            }
////            catch (Exception)
////            {
////                throw;
////            }
////        }
////        public bool Print(DataSet dsData, ServiceType serviceType, string PrinterName)
////        {
////            bool printStatus = false;
////            printStatus = PrintPatientCard(dsData, serviceType, PrinterName);
////            return printStatus;
////        }
////        private bool PrintPatientCard(DataSet dsData, ServiceType serviceType, string CardPrinterName)
////        {
////            bool printStatus = false;
////            DataTable dtPrintData = new DataTable();
////            try
////            {
////                //Setting card printing data.
////                dtPrintData = SelectDataToPrint(dsData);
////                if (dtPrintData != null && dtPrintData.Rows.Count > 0)
////                {
////                    objPrintDoc = new PrintDocument();
////                    if (CardPrinterName.Length > 0)
////                    {
////                        objPrintDoc.PrinterSettings.PrinterName = CardPrinterName;
////                    }
////                    else
////                    {
////                        PrintDialog pd = new PrintDialog();
////                        pd.PrinterSettings = new PrinterSettings();
////                        pd.ShowDialog();
////                        objPrintDoc.PrinterSettings.PrinterName = pd.PrinterSettings.PrinterName;
////                    }
////                    if (serviceType == ServiceType.Registration || serviceType == ServiceType.ReRegistration || serviceType == ServiceType.Consultation)
////                    {
////                        foreach (DataRow dr in dtPrintData.Rows)
////                        {
////                            drPrintData = dr;
////                            //Creating print data from dtPrintData in AlignPatientCard event.
////                            objPrintDoc.PrintPage += new PrintPageEventHandler(AlignPatientCard);
////                            objPrintDoc.Print();
////                            Dispose();
////                        }
////                        printStatus = true;
////                    }
////                }
////            }
////            catch (InvalidPrinterException)
////            {
////                MessageBox.Show("Card Printer not Found", "Y A S A S I I", MessageBoxButtons.OK, MessageBoxIcon.Warning);
////            }
////            catch (Exception)
////            {
////                throw;
////            }
////            return printStatus;
////        }
////        private void AlignPatientCard(Object sender, PrintPageEventArgs e)
////        {
////            Graphics g = e.Graphics;
////            string patName = string.Empty;
////            string address1 = string.Empty;
////            string address2 = string.Empty;
////            Brush br = new SolidBrush(Color.Black);
////            PrintFont = new Font("Arial", 10);

////            try
////            {
////                patName = drPrintData["FIRST_NAME"].ToString() + " " + drPrintData["MIDDLE_NAME"].ToString() + " " + drPrintData["LAST_NAME"].ToString();
////                patName = patName.ToUpper();
////                if (patName.Length > 26)
////                {
////                    patName = patName.Substring(0, 26);
////                }

////                address1 = drPrintData["ADDRESS1"].ToString();
////                if (address1.Length > 24)
////                {
////                    address1 = address1.Substring(0, 24);
////                }

////                address2 = drPrintData["ADDRESS2"].ToString();
////                if (address2.Length > 24)
////                {
////                    address2 = address2.Substring(0, 24);
////                }

////                if (BarcodeFont.Name.Equals("3 of 9 Barcode"))
////                {
////                    // the Asterisk (*) is used to delimit the barcode, and is required as the start and stop charachters by the 3of9 Symbology.
////                    g.DrawString("*" + drPrintData["MRNO"].ToString() + "*", BarcodeFont, br, 110, 40);
////                }
////                else if (BarcodeFont.Name.Equals("Code128bWin"))
////                {
////                    // the Symbol (š) and (œ} is used to delimit the barcode, and is required as the start and stop charachters by the Code128b Symbology.
////                    g.DrawString("š" + drPrintData["MRNO"].ToString() + Convert.ToString((char)CheckSum(drPrintData["MRNO"].ToString())) + "œ", BarcodeFont, br, 110, 40);
////                }

////                PrintFont = new Font("Arial", 12, FontStyle.Bold);
////                g.DrawString(patName, PrintFont, br, 30, 75);

////                PrintFont = new Font("Arial", 10, FontStyle.Bold);
////                g.DrawString("MRNO", PrintFont, br, 30, 95);
////                g.DrawString("DOB", PrintFont, br, 30, 112);
////                g.DrawString("Address", PrintFont, br, 30, 130);
////                g.DrawString("Valid Upto", PrintFont, br, 30, 165);
////                g.DrawString("Sex", PrintFont, br, 200, 95);
////                g.DrawString("Date", PrintFont, br, 200, 112);

////                g.DrawString(":", PrintFont, br, 100, 95);
////                g.DrawString(":", PrintFont, br, 100, 112);
////                g.DrawString(":", PrintFont, br, 100, 130);
////                g.DrawString(":", PrintFont, br, 100, 165);
////                g.DrawString(":", PrintFont, br, 240, 95);
////                g.DrawString(":", PrintFont, br, 240, 112);

////                ////////
////                g.DrawString(drPrintData["MRNO"].ToString(), PrintFont, br, 110, 95);
////                g.DrawString(Convert.ToDateTime(drPrintData["DOB"]).ToString("dd-MM-yyyy"), PrintFont, br, 110, 112);
////                g.DrawString(address1, PrintFont, br, 110, 130);
////                g.DrawString(address2, PrintFont, br, 110, 145);

////                if (Convert.ToBoolean(drPrintData["ISLIFELONG"]) == true)
////                {
////                    g.DrawString("LIFE LONG", PrintFont, br, 110, 165);
////                }
////                else
////                {
////                    g.DrawString(Convert.ToDateTime(drPrintData["VALID_UPTO"]).ToString("dd-MMM-yyyy"), PrintFont, br, 110, 165);
////                }
////                g.DrawString(drPrintData["SEX"].ToString(), PrintFont, br, 250, 95);
////                g.DrawString(Convert.ToDateTime(drPrintData["REG_DATE"]).ToString("dd-MMM-yyyy"), PrintFont, br, 250, 112);
////            }
////            catch (Exception)
////            {
////                throw;
////            }
////        }
////        private DataTable SelectDataToPrint(DataSet dsData)
////        {
////            DataTable dtData = new DataTable("PrintData");
////            DataTable dtTemp = new DataTable();
////            DataRow[] drArray;
////            int i = 0;
////            if (dsData.Tables["GEN_PROFILE_ADDRESS"] != null && dsData.Tables["PAT_PATIENT_NAME"] != null
////                && dsData.Tables["REG_PATIENT_REGISTRATION"] != null && dsData.Tables["GEN_PROFILE_ADDRESS"].Rows.Count > 0 
////                && dsData.Tables["PAT_PATIENT_NAME"].Rows.Count > 0 && dsData.Tables["REG_PATIENT_REGISTRATION"].Rows.Count > 0)
////            {
////                //Create table to print                
////                dtData.Columns.Add("FIRST_NAME", System.Type.GetType("System.String"));
////                dtData.Columns.Add("MIDDLE_NAME", System.Type.GetType("System.String"));
////                dtData.Columns.Add("LAST_NAME", System.Type.GetType("System.String"));
////                dtData.Columns.Add("MRNO", System.Type.GetType("System.String"));
////                dtData.Columns.Add("ADDRESS1", System.Type.GetType("System.String"));
////                dtData.Columns.Add("ADDRESS2", System.Type.GetType("System.String"));
////                dtData.Columns.Add("DOB", System.Type.GetType("System.String"));
////                dtData.Columns.Add("SEX", System.Type.GetType("System.String"));
////                dtData.Columns.Add("ISLIFELONG", System.Type.GetType("System.Int32"));
////                dtData.Columns.Add("VALID_UPTO", System.Type.GetType("System.String"));
////                dtData.Columns.Add("REG_DATE", System.Type.GetType("System.String"));
////                ////
////                //Add necessary data from dataset to datatable to print
////                foreach(DataRow dr in dsData.Tables["PAT_PATIENT_NAME"].Rows)
////                {
////                    dtData.Rows.Add();
////                    dtData.Rows[i]["FIRST_NAME"] = dr["FIRST_NAME"].ToString();
////                    dtData.Rows[i]["MIDDLE_NAME"] = dr["MIDDLE_NAME"].ToString();
////                    dtData.Rows[i]["LAST_NAME"] = dr["LAST_NAME"].ToString();
////                    dtData.Rows[i]["MRNO"] = dr["MRNO"].ToString();
////                    dtData.Rows[i]["DOB"] = dr["DOB"].ToString();
////                    dtData.Rows[i]["SEX"] = dr["GENDER"].ToString();

////                    drArray = dsData.Tables["GEN_PROFILE_ADDRESS"].Select("PROFILE_ID='" + dr["MRNO"].ToString() + "'");
////                    dtTemp = dsData.Tables["GEN_PROFILE_ADDRESS"].Clone();
////                    drArray.CopyToDataTable(dtTemp, LoadOption.OverwriteChanges);
////                    //dtData.Rows[i]["ADDRESS1"] = dsData.Tables["PATIENT_CARD"].Rows[i]["ADDRESS1"].ToString();
////                    dtData.Rows[i]["ADDRESS1"] = dtTemp.Rows[0]["ADDRESS1"].ToString(); //House No.
////                    if (dtTemp.Rows[0]["ADDRESS2"].ToString().Trim() != String.Empty &&
////                         (dtTemp.Rows[0]["ADDRESS3"].ToString().Trim() != String.Empty))
////                    {
////                        dtData.Rows[i]["ADDRESS2"] = dtTemp.Rows[0]["ADDRESS2"].ToString() + ", " + dtTemp.Rows[0]["ADDRESS3"].ToString();//street and place 
////                    }
////                    else if (dtTemp.Rows[0]["ADDRESS2"].ToString().Trim() != string.Empty)
////                    {
////                        dtData.Rows[i]["ADDRESS2"] = dtTemp.Rows[0]["ADDRESS2"].ToString();
////                    }
////                    else if (dtTemp.Rows[0]["ADDRESS3"].ToString().Trim() != string.Empty)
////                    {
////                        dtData.Rows[i]["ADDRESS2"] = dtTemp.Rows[0]["ADDRESS3"].ToString();
////                    }
////                    else
////                    {
////                        dtData.Rows[i]["ADDRESS2"] = String.Empty;
////                    }

////                    drArray = dsData.Tables["REG_PATIENT_REGISTRATION"].Select("MRNO='" + dr["MRNO"].ToString() + "'");
////                    dtTemp = dsData.Tables["REG_PATIENT_REGISTRATION"].Clone();
////                    drArray.CopyToDataTable(dtTemp, LoadOption.OverwriteChanges);
////                    dtData.Rows[i]["ISLIFELONG"] = Convert.ToInt32(dtTemp.Rows[0]["ISLIFELONG"].ToString());
////                    dtData.Rows[i]["VALID_UPTO"] = dtTemp.Rows[0]["VALID_UPTO"].ToString();

////                    drArray = dsData.Tables["PAT_MAST_PATIENT"].Select("MRNO='" + dr["MRNO"].ToString() + "'");
////                    dtTemp = dsData.Tables["PAT_MAST_PATIENT"].Clone();
////                    drArray.CopyToDataTable(dtTemp, LoadOption.OverwriteChanges);
////                    dtData.Rows[i]["REG_DATE"] = dtTemp.Rows[0]["REG_DATE"].ToString();
////                    i++;
////                }
////                ////
////            }
////            return dtData;
////        }

////        private int CheckSum(string PatId)
////        {
////            int chkSum = 0;
////            int patDigit = 0, patVal = 0, patModVal = 0;
////            int cnt;

////            try
////            {
////                if (strBarcodeType == "Code128b")
////                {
////                    char[] ch = PatId.ToCharArray();
////                    cnt = 1;
////                    foreach (char chr in ch)
////                    {
////                        switch (chr)
////                        {
////                            case '0':
////                                patDigit = 16;
////                                break;
////                            case '1':
////                                patDigit = 17;
////                                break;
////                            case '2':
////                                patDigit = 18;
////                                break;
////                            case '3':
////                                patDigit = 19;
////                                break;
////                            case '4':
////                                patDigit = 20;
////                                break;
////                            case '5':
////                                patDigit = 21;
////                                break;
////                            case '6':
////                                patDigit = 22;
////                                break;
////                            case '7':
////                                patDigit = 23;
////                                break;
////                            case '8':
////                                patDigit = 24;
////                                break;
////                            case '9':
////                                patDigit = 25;
////                                break;
////                            default:
////                                break;
////                        }
////                        patVal += (patDigit * cnt);
////                        cnt++;
////                    }
////                    patVal = patVal + 104;
////                    patModVal = patVal % 103;
////                    chkSum = code128Lookup[patModVal];
////                }
////                return chkSum;
////            }
////            catch (Exception)
////            {

////                throw;
////            }
////        }

////        #region IDisposable Members

////        public void Dispose()
////        {
////            objPrintDoc.PrintPage -= new PrintPageEventHandler(AlignPatientCard);
////        }

////        #endregion

////        #region IPrinting Members

////        public bool Print(DataSet dsData, ServiceType serviceType, string PrinterName, PrintType printType)
////        {
////            throw new NotImplementedException();
////        }

////        #endregion
////    }
////}
