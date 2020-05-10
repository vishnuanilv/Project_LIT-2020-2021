//-----------------------------------------------------------------------
// <copyright file="GraphicsPrinting.cs" company="Kameda Infologics PVT Ltd">
//     Copyright (c) Kameda Infologics Pvt Ltd. All rights reserved.
// </copyright>
// <author>Biju S J</author>
//<Date>22-Jan-2010<Date>
//-----------------------------------------------------------------------

namespace Infologics.Medilogics.PrintingLibrary.GraphicsPrinting
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
    using Infologics.Medilogics.CommonClient.Controls.Message;
    using Infologics.Medilogics.Resources.MessageBoxLib;
    using Infologics.Medilogics.General.Control;
    using System.Text.RegularExpressions;
    using Infologics.Medilogics.CommonClient.Controls.StaticData;
    using System.Drawing.Drawing2D;
    using BarcodeLib.Barcode;
    using ZXing;
    using ZXing.Common;
    using ZXing.QrCode;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.Threading;

    public class GraphicsPrinting : IPrinting, IDisposable
    {
        string strBarcodeFont, strBarcodeType;
        private static Font BarcodeFont = null;
        private static Font PrintFont = null;
        private static int[] code128Lookup = null;
        private static Font PrintFont1 = null;
        private string cardPrinterName = string.Empty;
        string strPath = System.AppDomain.CurrentDomain.BaseDirectory + System.DateTime.Now.ToString("ddmmyyhhmmss");
        PrintDocument objPrintDoc;
        DataRow drPrintData;
        ServiceType SelectedServiceType;
        PrintType SelectedPrintType;
        public GraphicsPrinting()
        {
            try
            {
                strBarcodeFont = ConfigurationManager.AppSettings["BarcodeFont"].ToString();
                strBarcodeType = ConfigurationManager.AppSettings["BarcodeType"].ToString();
                BarcodeFont = new Font(strBarcodeFont, 10);
                strBarcodeType.Trim();
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
            printStatus = PrintGraphics(dsData, serviceType, prntype, PrinterName);
            return printStatus;
        }
        public bool Print(DataSet dsData, ServiceType serviceType, string PrinterName, PrintType printType)
        {
            bool printStatus = false;
            SelectedServiceType = serviceType;
            SelectedPrintType = printType;
            printStatus = PrintGraphics(dsData, serviceType, printType, PrinterName);
            return printStatus;
        }


        private bool PrintGraphics(DataSet dsData, ServiceType serviceType, PrintType prntype, string CardPrinterName)
        {
            string strSelectedIVPrinter = string.Empty, strSelectedNormalPrinter=string.Empty;
            bool printStatus = false;
            DataTable dtPrintData = new DataTable();
            try
            {
                //Setting card printing data.
               
                dtPrintData = SelectDataToPrint(dsData);
                if (dsData != null && dsData.Tables.Count > 0 && dsData.Tables.Contains("PH_PAT_DTLS_ORDER") && dsData.Tables["PH_PAT_DTLS_ORDER"] != null && dsData.Tables["PH_PAT_DTLS_ORDER"].Columns.Contains("ISINFUSION") && dsData.Tables["PH_PAT_DTLS_ORDER"].Rows.Count > 0)
                {
                    int isinfusion = Convert.ToInt32(dsData.Tables["PH_PAT_DTLS_ORDER"].Rows[0]["ISINFUSION"]);
                    if (isinfusion == 1)
                    {
                        dtPrintData.Columns.Add("AGE");
                       
                        dtPrintData.Columns.Add("BRAND_NAME");
                        dtPrintData.Rows[0]["AGE"] = dsData.Tables["PH_PAT_DTLS_ORDER"].Rows[0]["AGE"];
                       
                        dtPrintData.Rows[0]["BRAND_NAME"] = dsData.Tables["PH_PAT_DTLS_ORDER"].Rows[0]["BRAND_NAME"];  //edied sajin for arabic label prinitng
                    }
                }
                PrintDialog pd = new PrintDialog();
                if (dtPrintData != null && dtPrintData.Rows.Count > 0)
                {
                    objPrintDoc = new PrintDocument();
                    if (!(SelectedServiceType == ServiceType.Pharmacy && SelectedPrintType == PrintType.Prescription))
                    {
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
                    }

                    foreach (DataRow dr in dtPrintData.Rows)
                    {
                        if (SelectedServiceType == ServiceType.Pharmacy && SelectedPrintType == PrintType.Prescription)
                        {
                            if (Convert.ToInt32(dr["ISINFUSION"]) == 1)
                            {
                                string printerKey = ConfigurationManager.AppSettings["BarcodePrinterPharmacy2"].ToString();
                                if (strSelectedIVPrinter == string.Empty && ConfigurationManager.AppSettings[printerKey] != null && ConfigurationManager.AppSettings[printerKey].ToString().Trim() != string.Empty)
                                {
                                    objPrintDoc.PrinterSettings.PrinterName = ConfigurationManager.AppSettings[printerKey].ToString();
                                    strSelectedIVPrinter = ConfigurationManager.AppSettings[printerKey].ToString(); 
                                }
                                else
                                {
                                    if (strSelectedIVPrinter != string.Empty)
                                    {
                                        objPrintDoc.PrinterSettings.PrinterName = strSelectedIVPrinter;
                                    }
                                    else
                                    {
                                        pd.PrinterSettings = new PrinterSettings();
                                        pd.ShowDialog();
                                        objPrintDoc.PrinterSettings.PrinterName = pd.PrinterSettings.PrinterName;
                                        strSelectedIVPrinter = pd.PrinterSettings.PrinterName; 
                                    }
                                }
                            }
                            else
                            {
                                if (CardPrinterName != string.Empty)
                                {
                                    objPrintDoc.PrinterSettings.PrinterName = CardPrinterName;
                                }
                                else 
                                {
                                    if (strSelectedNormalPrinter != string.Empty)
                                    {
                                        objPrintDoc.PrinterSettings.PrinterName = strSelectedNormalPrinter;
                                    }
                                    else
                                    {
                                        pd.PrinterSettings = new PrinterSettings();
                                        pd.ShowDialog();
                                        objPrintDoc.PrinterSettings.PrinterName = pd.PrinterSettings.PrinterName;
                                        strSelectedNormalPrinter = pd.PrinterSettings.PrinterName;
                                    }
                                }
                            }
                        }
                        
                        drPrintData = dr;
                        //Creating print data from dtPrintData in AlignPatientCard event.
                        objPrintDoc.PrintPage += new PrintPageEventHandler(AlignGraphicsData);
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
        private void AlignGraphicsData(Object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;

            try
            {

                if (SelectedServiceType == ServiceType.Pharmacy && SelectedPrintType == PrintType.Prescription)
                {
                    if (Convert.ToInt32(drPrintData["ISCONSUMABLE"]) == 1)
                    {
                        //Brush br = new SolidBrush(Color.Black);
                        //PrintFont = new Font("Times New Roman", 8);
                        //string strPath = string.Empty;
                        //Image logo1 = null;
                        //SolidBrush drawBrush = new SolidBrush(Color.Black);
                        //Font drawFont = new System.Drawing.Font("Times New Roman", 7, FontStyle.Bold);
                        //Font drawHeading = new System.Drawing.Font("Times New Roman", 10, FontStyle.Bold);
                        //StringFormat drawFormat = new StringFormat();

                        //drawFormat.FormatFlags = StringFormatFlags.DirectionRightToLeft;

                        //g.DrawString("Danat Al Emarat Hospital Women’s & Children", PrintFont, br, 25, 10); // Hospital name
                        //g.DrawString("CIVA Services- Pharmacy Department", PrintFont, br, 30, 35);//
                        //g.DrawString("Telephone: Extn 2255", PrintFont, br, 35, 60);//
                        //g.DrawString("Drug: ", PrintFont, br, 5, 85); // MRNO. Label
                        //g.DrawString(drPrintData["MEDICINE_NAME"].ToString(), drawFont, drawBrush, 70, 85);
                        //g.DrawString("Lot# ", PrintFont, br, 5, 110); // MRNO. Label
                        //g.DrawString(drPrintData["BATCHNO"].ToString(), drawFont, drawBrush, 50, 110);
                        //g.DrawString("Expiry: ", PrintFont, br, 5, 135); // MRNO. Label
                        //g.DrawString(drPrintData["EXPIRY_DATE"].ToString(), drawFont, drawBrush, 50, 135);
                        //g.DrawString("Prepared/Date: ", PrintFont, br, 5, 160); // MRNO. Label
                        //g.DrawString("Storage/handling: ", PrintFont, br, 5, 185); // MRNO. Label
                        //g.DrawString(drPrintData["COMMENT"].ToString(), drawFont, drawBrush, 70, 185);
                        //g.DrawString("Please return to Pharmacy when d/c or expired", PrintFont, br, 5, 210); // MRNO. Label

                        //Code From Khader
                        Brush br = new SolidBrush(Color.Black);
                        PrintFont = new Font("Times New Roman", 8);
                        string photoPath = string.Empty;
                        Image logo1 = null;
                        SolidBrush drawBrush = new SolidBrush(Color.Black);
                        Font drawFont = new System.Drawing.Font("Times New Roman", 8, FontStyle.Bold);
                        Font drawHeading = new System.Drawing.Font("Times New Roman", 10, FontStyle.Bold);
                        StringFormat drawFormat = new StringFormat();
                        drawFormat.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                        g.DrawString("Danat Al Emarat Hospital Women’s & Children", drawFont, br, 110, 15); // Hospital name
                        g.DrawString("CIVA Services- Pharmacy Department", drawFont, br, 130, 25);//
                        g.DrawString("Telephone: Extn 2255", drawFont, br, 160, 35);//

                        g.DrawString("Drug name : " + drPrintData["MEDICINE_NAME"].ToString(), drawHeading, br, 15, 60);//pharmacy code  120, 60
                        g.DrawString("LOT : " + drPrintData["BATCHNO"].ToString(), PrintFont, br, 15, 100);// left qty  260
                        g.DrawString("Expiry : " + drPrintData["EXPIRY_DATE"].ToString(), PrintFont, br, 150, 100);
                        g.DrawString("Prepared/Date : ", PrintFont, br, 250, 100);//
                     
                        if (Convert.ToInt16(drPrintData["IS_ARABIC_NOTE"] != DBNull.Value ? drPrintData["IS_ARABIC_NOTE"] : 0) == 1)
                        {
                            string Remarks = "ملاحظات:"+" " + drPrintData["COMMENT"].ToString();
                            g.DrawString(Remarks, drawFont, br, 80, 120);

                        }
                        else
                        {
                            g.DrawString("Remarks : ", PrintFont, br, 15, 120);
                            g.DrawString(drPrintData["COMMENT"].ToString(), drawFont, br, 65, 120);
                        }
                        g.DrawString("Please return to Pharmacy when d/c or expired", drawFont, br, 35, 160);//100, 160
                        if (Convert.ToString(drPrintData["BARCODE"]).Trim() != string.Empty)
                        {
                            var qrcode = new QRCodeWriter();
                            var qrValue = drPrintData["BARCODE"].ToString();

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

                            Image logo2 = Image.FromFile(strPath);// drPrintData["LOGO_PATH"].ToString()
                            Rectangle destinationn = new Rectangle(290, 150, 50, 50);//210, 150, 50, 50
                            g.DrawImage(logo2, destinationn, 0, 0, logo2.Width, logo2.Height, GraphicsUnit.Pixel);
                        }
                    }
                    else
                    {
                        if (Convert.ToInt32(drPrintData["ISINFUSION"]) == 1)
                        {
                            //Brush br = new SolidBrush(Color.Black);
                            //PrintFont = new Font("Times New Roman", 8);
                            //string photoPath = string.Empty;
                            //Image logo1 = null;
                            //SolidBrush drawBrush = new SolidBrush(Color.Black);
                            //Font drawFont = new System.Drawing.Font("Times New Roman", 7, FontStyle.Bold);
                            //Font drawHeading = new System.Drawing.Font("Times New Roman", 10, FontStyle.Bold);
                            //StringFormat drawFormat = new StringFormat();

                            //drawFormat.FormatFlags = StringFormatFlags.DirectionRightToLeft;

                            ////if (Convert.ToInt32(drPrintData["ISINFUSION"]) == 0)
                            ////{
                            ////Rectangle destinationn = new Rectangle(175, 10, 30, 30);
                            ////g.DrawImage(logo1, destinationn, 0, 0, logo1.Width, logo1.Height, GraphicsUnit.Pixel);
                            ////logo1.Dispose();
                            //g.DrawString("Danat Al Emarat Hospital Women’s & Children", PrintFont, br, 25, 10); // Hospital name
                            //g.DrawString("CIVA Services- Pharmacy Department", PrintFont, br, 30, 35);//
                            //g.DrawString("Telephone: Extn 2255", PrintFont, br, 35, 60);//
                            //g.DrawString("Patient Name: ", PrintFont, br, 5, 85); // MRNO. Label
                            //g.DrawString(drPrintData["PATIENT_NAME"].ToString(), drawFont, drawBrush, 70, 85);
                            //g.DrawString("MRN # ", PrintFont, br, 5, 110); // MRNO. Label
                            //g.DrawString(drPrintData["MRNO"].ToString(), drawFont, drawBrush, 50, 110);
                            //g.DrawString("Allergy: ", PrintFont, br, 5, 135); // MRNO. Label
                            //g.DrawString(drPrintData["ALLERGY_DRUG_NAME"].ToString(), drawFont, drawBrush, 50, 135);
                            //g.DrawString("Weight: ", PrintFont, br, 5, 160); // MRNO. Label
                            //g.DrawString(drPrintData["WEIGHT"].ToString(), drawFont, drawBrush, 50, 160);

                            //g.DrawString("Concentration: ", PrintFont, br, 5, 185); // MRNO. Label
                            //g.DrawString(drPrintData["MEDICINE_NAME"].ToString(), drawFont, drawBrush, 70, 185);
                            //g.DrawString("Drug Name: ", PrintFont, br, 5, 210); // MRNO. Label
                            //g.DrawString(drPrintData["GENERIC_NAME"].ToString(), drawFont, drawBrush, 50, 210);

                            //g.DrawString("Dose: ", PrintFont, br, 5, 235); // MRNO. Label
                            //g.DrawString(drPrintData["DOSAGE"].ToString(), drawFont, drawBrush, 50, 235);
                            //g.DrawString("Diluent: ", PrintFont, br, 5, 260); // MRNO. Label
                            //g.DrawString(drPrintData["DILUENT"].ToString(), drawFont, drawBrush, 50, 260);

                            //g.DrawString("Volume: ", PrintFont, br, 5, 285); // MRNO. Label
                            //g.DrawString(drPrintData["QUANTITY"].ToString() + " (" + drPrintData["QUANTITY_UNIT"].ToString() + ")", drawFont, drawBrush, 50, 285);

                            //g.DrawString("Route: ", PrintFont, br, 5, 310); // MRNO. Label
                            //g.DrawString(drPrintData["ROUTE"].ToString(), drawFont, drawBrush, 70, 310);
                            //g.DrawString("Frequency: ", PrintFont, br, 5, 335); // MRNO. Label
                            //g.DrawString(drPrintData["FREQUENCY_VALUE"].ToString(), drawFont, drawBrush, 50, 335);
                            //g.DrawString("Infuse over: ", PrintFont, br, 5, 360); // MRNO. Label
                            //g.DrawString(drPrintData["INFUSE_OVER"].ToString(), drawFont, drawBrush, 70, 360);
                            //g.DrawString("Rate: ", PrintFont, br, 5, 385); // MRNO. Label
                            //g.DrawString(drPrintData["RATE"].ToString(), drawFont, drawBrush, 70, 385);

                            //g.DrawString("Storage and Handling: ", PrintFont, br, 5, 410); // MRNO. Label
                            //g.DrawString(drPrintData["COMMENT"].ToString(), drawFont, drawBrush, 70, 410);

                            //g.DrawString("Due date/Time: ", PrintFont, br, 5, 435); // MRNO. Label
                            //g.DrawString(drPrintData["START_DATE"].ToString(), drawFont, drawBrush, 70, 435);
                            //g.DrawString("Expiry: ", PrintFont, br, 5, 460); // MRNO. Label
                            //g.DrawString(drPrintData["EXPIRY_DATE"].ToString(), drawFont, drawBrush, 70, 460);

                            //g.DrawString("Prepared Date: ", PrintFont, br, 5, 485); // MRNO. Label
                            //g.DrawString("Prepared by: ", PrintFont, br, 5, 510); // MRNO. Label
                            //g.DrawString("Checked PH: ", PrintFont, br, 5, 535); // MRNO. Label
                            //g.DrawString("RN Ckd: ", PrintFont, br, 5, 560); // MRNO. Label

                            //g.DrawString("Please return to Pharmacy when d/c or expired", PrintFont, br, 10, 585);//warning

                            string StrAllergy = string.Empty;
                            Brush br = new SolidBrush(Color.Black);
                            PrintFont = new Font("Times New Roman", 8);
                            string photoPath = string.Empty;
                            Image logo1 = null;
                            SolidBrush drawBrush = new SolidBrush(Color.Black);
                            Font drawFont = new System.Drawing.Font("Times New Roman", 8, FontStyle.Bold);
                            Font drawHeading = new System.Drawing.Font("Times New Roman", 9, FontStyle.Bold);
                            StringFormat drawFormat = new StringFormat();
                            drawFormat.FormatFlags = StringFormatFlags.DirectionRightToLeft;

                            g.DrawString("Name : ", PrintFont, br, 06, 10);//Mrno
                           
                            g.DrawString("MRN  ", PrintFont, br, 202, 10);//clinic code 250
                            g.DrawString("Weight : ", PrintFont, drawBrush, 06, 25);
                            //g.DrawString("Allergy : ", PrintFont, drawBrush, 90, 25);

                            g.DrawString(drPrintData["PATIENT_NAME"].ToString(), drawFont, br, 45, 10);//Mrno
                           
                            g.DrawString(drPrintData["MRNO"].ToString(), drawFont, br, 232, 10);//clinic code 250
                            //g.DrawString(drPrintData["ALLERGY_DRUG_NAME"].ToString(), drawFont, drawBrush, 135, 25);
                            g.DrawString(drPrintData["WEIGHT"].ToString(), drawFont, drawBrush, 52, 25);
                            g.DrawString("Diluent:", PrintFont, drawBrush, 06, 40);
                            g.DrawString(drPrintData["MEDICINE_NAME"].ToString(), drawFont, drawBrush, 65, 40);

                            g.DrawString("Generic : ", PrintFont, br, 06, 60);//Mrno
                            //g.DrawString("Dose : ", PrintFont, br, 185, 60);//clinic code 250

                            string strGen = drPrintData["GENERIC_NAME"].ToString();

                            g.DrawString(strGen, drawFont, br, 65, 60);
                            //g.DrawString(drPrintData["DOSAGE"].ToString(), drawFont, drawBrush, 215, 60);
                            string strDoseLabel = drPrintData["DOSE_DISPLAY_NAME"].ToString();

                            //g.DrawString("Dose : ", PrintFont, br, 06, 75);
                            g.DrawString(strDoseLabel + " : ", PrintFont, br, 06, 75);
                            g.DrawString(drPrintData["DOSAGE"].ToString(), drawFont, br, 65, 75);

                            //string strDil = drPrintData["DILUENT"].ToString().Length > 30 ? (drPrintData["DILUENT"].ToString().Substring(0, 30) + "...") : drPrintData["DILUENT"].ToString();

                            g.DrawString("Concentration : ", PrintFont, br, 06, 90);
                            //g.DrawString(strDil, drawFont, br, 52, 90);

                            g.DrawString("Route : ", PrintFont, br, 06, 105);
                            g.DrawString("Frequency/Infusion Type : ", PrintFont, drawBrush, 220, 105);

                            g.DrawString(drPrintData["ROUTE"].ToString(), drawFont, br, 65, 105);
                            g.DrawString(drPrintData["FREQUENCY_VALUE"].ToString() + " " + drPrintData["FORM"].ToString(), drawFont, drawBrush, 360, 105);

                            string strQuantityLabel = drPrintData["QUANTITY_DISPLAY_NAME"].ToString();

                            //g.DrawString("Volume : ", PrintFont, drawBrush, 06, 120);
                            //g.DrawString(strQuantityLabel + " : ", PrintFont, drawBrush, 06, 120);
                            //g.DrawString(drPrintData["QUANTITY"].ToString() , drawFont, drawBrush, 52, 120);

                            g.DrawString("Rate : ", PrintFont, drawBrush, 06, 120);
                            //g.DrawString("FORM : ", PrintFont, drawBrush, 130, 135);
                            g.DrawString("Infuse Over: ", PrintFont, br, 05, 135);//pharmacy code

                            g.DrawString(drPrintData["INFUSE_OVER"].ToString(), drawFont, br, 65, 135);//pharmacy code
                            g.DrawString(drPrintData["RATE"].ToString(), drawFont, drawBrush, 65, 120);
                            //g.DrawString(drPrintData["FORM"].ToString(), drawFont, drawBrush, 190, 135);
                            ///////////////
                            if (Convert.ToInt16(drPrintData["IS_ARABIC_NOTE"] != DBNull.Value?drPrintData["IS_ARABIC_NOTE"]:0) == 1)
                            {
                                string Remarks = "ملاحظات:"+" " + drPrintData["COMMENT"].ToString();
                                g.DrawString(Remarks, drawFont, br, 300, 155);
                               
                            }
                            else
                            {
                                g.DrawString("Remarks : ", PrintFont, br, 06, 155);
                                g.DrawString(drPrintData["COMMENT"].ToString(), drawFont, br, 65, 155);
                            }


                            g.DrawString("Due date : ", PrintFont, br, 06, 310);
                            g.DrawString(drPrintData["START_DATE"].ToString(), drawFont, br, 65, 310);
                            g.DrawString("Expiry : ", PrintFont, br, 350, 330);
                            g.DrawString(drPrintData["EXPIRY_DATE"].ToString(), drawFont, br, 390, 330);
                            g.DrawString("Prepared by : ", PrintFont, br, 05, 330);
                         
                            g.DrawString("Prepared Date : ", PrintFont, br, 05, 350);
                            g.DrawString("Checked  PH : ", PrintFont, br, 05, 370); //+ drPrintData["CHECKEDBY_USERNAME"].ToString()
                            g.DrawString("RN Ckd : ", PrintFont, br, 05, 390);

                            #region AddedforLabelPrinting  

                            g.DrawString("Age: ", PrintFont, br, 05, 190);
                            g.DrawString(drPrintData["AGE"].ToString(), drawFont, br, 65, 190);
                            g.DrawString(" Frequency  : ", PrintFont, drawBrush, 300, 190);
                            g.DrawString(drPrintData["FREQUENCY_VALUE"].ToString(), drawFont, drawBrush, 370, 190);
                            if (Convert.ToInt16(drPrintData["IS_OP"]) == 1)
                            {
                                g.DrawString("", PrintFont, br, 05, 210);
                            }
                            else
                            {
                                g.DrawString("Location: ", PrintFont, br, 05, 210);
                                g.DrawString(drPrintData["LOCATION"].ToString(), drawFont, br, 65, 210);
                            }
                           
                            g.DrawString("Brand: ", PrintFont, br, 05, 230);
                            g.DrawString(drPrintData["BRAND_NAME"].ToString(), drawFont, drawBrush, 65, 230);
                            g.DrawString("Batch No: ", PrintFont, br, 05, 250);
                            g.DrawString(drPrintData["BATCHNO"].ToString(), drawFont, br, 65, 250);
                            g.DrawString("Dose: ", PrintFont, br, 05, 175);
                            g.DrawString(drPrintData["QUANTITY"].ToString(), drawFont, drawBrush, 65, 175);
                            g.DrawString("Volume: " , PrintFont, br, 05, 270);
                            g.DrawString(drPrintData["VOLUMN"].ToString(), drawFont, drawBrush, 65, 270);
                            g.DrawString("Admin Time: ", PrintFont, br, 05, 290);
                            g.DrawString(drPrintData["ADMIN_TIME"].ToString(), drawFont, br, 65, 290);


                            #endregion

                            g.DrawString("Please return to Pharmacy when d/c or expired ", drawHeading, br, 20, 410);
                            //var qrcode = new QRCodeWriter();
                            if (Convert.ToString(drPrintData["BARCODE"]).Trim() != string.Empty)
                            {
                                //var qrValue = drPrintData["BARCODE"].ToString();

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
                                if (strBarcodeType.Equals("Code128bWin"))
                                {
                                    // the Asterisk (*) is used to delimit the barcode, and is required as the start and stop charachters by the 3of9 Symbology.
                                    g.DrawString("*" + drPrintData["MRNO"].ToString().Trim() + "*", BarcodeFont, br, 350, 365);//6
                                }
                                else if (strBarcodeType.Equals("Code128b"))
                                {
                                    // the Symbol (š) and (œ} is used to delimit the barcode, and is required as the start and stop charachters by the Code128b Symbology.
                                    g.DrawString("š" + drPrintData["MRNO"].ToString().Trim() + "œ", BarcodeFont, br, 350, 365);//6
                                }
                            }
                            if (File.Exists(strPath))
                            {
                                File.Delete(strPath);
                            }
                        }
                        else
                        {
                            if (Convert.ToInt32(drPrintData["IS_OP"]) == 1)
                            {
                                //Brush br = new SolidBrush(Color.Black);
                                //PrintFont = new Font("Times New Roman", 8);
                                //string photoPath = string.Empty;
                                //Image logo1 = null;
                                //SolidBrush drawBrush = new SolidBrush(Color.Black);
                                //Font drawFont = new System.Drawing.Font("Times New Roman", 7, FontStyle.Bold);
                                //Font drawHeading = new System.Drawing.Font("Times New Roman", 10, FontStyle.Bold);
                                //StringFormat drawFormat = new StringFormat();

                                //drawFormat.FormatFlags = StringFormatFlags.DirectionRightToLeft;

                                //g.DrawString("Danat Al Emarat Hospital Women’s & Children", PrintFont, br, 25, 10); // Hospital name
                                //g.DrawString("24/7 Outpatient Pharmacy", PrintFont, br, 30, 35);//
                                //g.DrawString("Telephone: Extn 2222", PrintFont, br, 35, 60);//
                                //g.DrawString("Patient Name: ", PrintFont, br, 5, 85); // MRNO. Label
                                //g.DrawString(drPrintData["PATIENT_NAME"].ToString(), drawFont, drawBrush, 70, 85);
                                //g.DrawString("MRN # ", PrintFont, br, 5, 110); // MRNO. Label
                                //g.DrawString(drPrintData["MRNO"].ToString(), drawFont, drawBrush, 50, 110);
                                //g.DrawString("Allergy: ", PrintFont, br, 5, 135); // MRNO. Label
                                //g.DrawString(drPrintData["ALLERGY_DRUG_NAME"].ToString(), drawFont, drawBrush, 50, 135);
                                //g.DrawString("Weight: ", PrintFont, br, 5, 160); // MRNO. Label
                                //g.DrawString(drPrintData["WEIGHT"].ToString(), drawFont, drawBrush, 50, 160);

                                //g.DrawString("Drug Name: ", PrintFont, br, 5, 185); // MRNO. Label
                                //g.DrawString(drPrintData["MEDICINE_NAME"].ToString(), drawFont, drawBrush, 70, 185);
                                //g.DrawString("Dose: ", PrintFont, br, 5, 210); // MRNO. Label
                                //g.DrawString(drPrintData["DOSAGE"].ToString(), drawFont, drawBrush, 50, 210);

                                //g.DrawString("Route: ", PrintFont, br, 5, 235); // MRNO. Label
                                //g.DrawString(drPrintData["ROUTE"].ToString(), drawFont, drawBrush, 50, 235);
                                //g.DrawString("Frequency: ", PrintFont, br, 5, 260); // MRNO. Label
                                //g.DrawString(drPrintData["FREQUENCY_VALUE"].ToString(), drawFont, drawBrush, 50, 260);

                                //g.DrawString("Comment: ", PrintFont, br, 5, 285); // MRNO. Label
                                //g.DrawString(drPrintData["COMMENT"].ToString(), drawFont, drawBrush, 50, 285);

                                //g.DrawString("Batch/Lot# ", PrintFont, br, 5, 310); // MRNO. Label
                                //g.DrawString(drPrintData["BATCHNO"].ToString(), drawFont, drawBrush, 70, 310);
                                //g.DrawString("Expiry: ", PrintFont, br, 5, 335); // MRNO. Label
                                //g.DrawString(drPrintData["EXPIRY_DATE"].ToString(), drawFont, drawBrush, 50, 335);

                                //g.DrawString("Prepared by: ", PrintFont, br, 5, 360); // MRNO. Label
                                //g.DrawString("Checked PH: ", PrintFont, br, 5, 385); // MRNO. Label
                                //g.DrawString("Quantity: ", PrintFont, br, 5, 410); // MRNO. Label
                                //g.DrawString(drPrintData["QUANTITY"].ToString() + " " + drPrintData["QUANTITY_UNIT"].ToString(), drawFont, drawBrush, 50, 410);

                                //g.DrawString("Refill: ", PrintFont, br, 5, 435); // MRNO. Label
                                //g.DrawString(drPrintData["REFILL"].ToString(), drawFont, drawBrush, 50, 435);

                                //g.DrawString("Keep out from children's reach", PrintFont, br, 10, 460);//warning

                                string StrAllergy = string.Empty;
                                Brush br = new SolidBrush(Color.Black);
                                PrintFont = new Font("Times New Roman", 8);
                                string photoPath = string.Empty;
                                Image logo1 = null;
                                SolidBrush drawBrush = new SolidBrush(Color.Black);
                                Font drawFont = new System.Drawing.Font("Times New Roman", 8, FontStyle.Bold);
                                Font drawHeading = new System.Drawing.Font("Times New Roman", 9, FontStyle.Bold);
                                StringFormat drawFormat = new StringFormat();
                                drawFormat.FormatFlags = StringFormatFlags.DirectionRightToLeft;

                                g.DrawString("Danat Al Emarat Hospital-Abu Dhabi", drawHeading, br, 20, 08); // Hospital name
                                g.DrawString("Telephone: +97126149905", PrintFont, br, 50, 22);
                     
                                // g.DrawString(drPrintData["Address1"].ToString(), drawHeading, br, 150, 25);//
                                // g.DrawString(drPrintData["Address2"].ToString(), drawHeading, br, 180, 35);//
                                string strName = Convert.ToString(drPrintData["PATIENT_NAME"]);
                                if (strName.Length > 25)
                                {
                                    strName = strName.Substring(0, 24);
                                    strName = strName + "..";
                                }
                                g.DrawString("Name : ", PrintFont, br, 08, 50);//Mrno
                                g.DrawString("Pres.Name :", PrintFont, br, 08, 65);
                                if (drPrintData["PROVIDER_NAME"] != DBNull.Value)
                                {
                                    string dr = drPrintData["PROVIDER_NAME"].ToString();
                                    if (dr.Length > 22)
                                    {
                                        dr = dr.Substring(0, 20) + "..";
                                    }
                                    string provider="Dr."+dr;
                                    g.DrawString(provider, drawFont, br, 70, 65);
                                }
                                g.DrawString("MRN: ", PrintFont, br, 204, 65);//clinic code 250
                                g.DrawString("Date :", PrintFont, br, 202, 50);
                                g.DrawString(drPrintData["DATE"].ToString(), drawFont, br, 235, 50);
                                //g.DrawString("Weight : ", PrintFont, drawBrush, 08, 55);
                                //g.DrawString("Allergy : ", PrintFont, drawBrush, 92, 55);
                                g.DrawString(strName, drawFont, br, 50, 50);//name
                                g.DrawString(drPrintData["MRNO"].ToString(), drawFont, br, 235, 65);//mrno 
                                bool IsSucess = false;
                                //if (dsTemp.Tables.Contains("EMR_PAT_WARNING") & dsTemp.Tables["EMR_PAT_WARNING"].Rows.Count > 0)
                                //{
                                //    bool IsMultiAllergy = false;
                                //    bool IsAllergyMorethenTwo = false;
                                //    foreach (DataRow dr in dsTemp.Tables["EMR_PAT_WARNING"].Rows)
                                //    {
                                //        IsSucess = true;
                                //        if (!IsAllergyMorethenTwo)
                                //        {
                                //            if (IsMultiAllergy)
                                //            {
                                //                StrAllergy = StrAllergy + " " + "-" + " " + Convert.ToString(dr["ALLERGY"]);
                                //                IsAllergyMorethenTwo = true;
                                //                IsSucess = false;
                                //            }
                                //            else
                                //            {
                                //                StrAllergy = StrAllergy + Convert.ToString(dr["ALLERGY"]);
                                //                IsMultiAllergy = true;
                                //            }
                                //        }

                                //    }
                                //    if (IsAllergyMorethenTwo & IsSucess)
                                //    {
                                //        string str = "++";
                                //        StrAllergy = StrAllergy + " " + str;
                                //    }
                                //if (Convert.ToString(drPrintData["ALLERGY_DRUG_NAME"]).Length < 28)
                                //{
                                //    g.DrawString(drPrintData["ALLERGY_DRUG_NAME"].ToString(), drawFont, drawBrush, 134, 55);
                                //}
                                //else if (Convert.ToString(drPrintData["ALLERGY_DRUG_NAME"]).Length > 28)
                                //{
                                //    g.DrawString(Convert.ToString(drPrintData["ALLERGY_DRUG_NAME"]).Substring(0, 35) + " ++", drawFont, drawBrush, 140, 50);
                                //}
                                //}
                                //  g.DrawString(drPrintData["ALLERGY"].ToString(), drawFont, drawBrush, 130, 27);
                                //g.DrawString(drPrintData["WEIGHT"].ToString(), drawFont, drawBrush, 54, 55);
                                //g.DrawString("Patient Name: " + drPrintData["PATIENT_NAME"].ToString(), PrintFont, br, 15, 50);//Mrno
                                //g.DrawString("MRN: " + drPrintData["MRNO"].ToString(), PrintFont, br, 150, 50);//clinic code 250
                                //g.DrawString("Allergy: " + drPrintData["ALLERGY"].ToString(), PrintFont, drawBrush, 320, 50, drawFormat);
                                //g.DrawString("Weight: " + drPrintData["Weight"].ToString(), PrintFont, drawBrush, 420, 50, drawFormat);
                                g.DrawString("Drug Name : ", PrintFont, br, 08, 85);//Mrno

                                // string strDoseLabel = drPrintData["DOSE_DISPLAY_NAME"].ToString();

                                // g.DrawString(strDoseLabel + " : ", PrintFont, br, 06, 60);//clinic code 250
                                if (Convert.ToString(drPrintData["MEDICINE_NAME"]).Length < 45)
                                {
                                    g.DrawString(drPrintData["MEDICINE_NAME"].ToString(), drawFont, br, 67, 85);
                                }
                                else
                                {
                                    g.DrawString(drPrintData["MEDICINE_NAME"].ToString().Substring(0, 44) + "..", drawFont, br, 67, 85);
                                }
                                //  g.DrawString(drPrintData["DOSAGE"].ToString(), drawFont, drawBrush, 48, 60);



                                //g.DrawString("Route : " + drPrintData["ROUTE"].ToString(), PrintFont, br, 08, 85);//pharmacy code
                                //g.DrawString("Frequency : ", PrintFont, drawBrush, 152, 85);
                               //g.DrawString(drPrintData["FREQUENCY_VALUE"].ToString(), drawFont, drawBrush, 202, 88);
                                if (drPrintData["IS_ARABIC_NOTE"] == DBNull.Value)
                                {
                                    drPrintData["IS_ARABIC_NOTE"] = 0;     
                                }
                                if (Convert.ToInt16(drPrintData["IS_ARABIC_NOTE"] != DBNull.Value ? drPrintData["IS_ARABIC_NOTE"] : 0) == 1)
                                {
                                    string Remarks = "ملاحظات  :"+"" + drPrintData["COMMENT"].ToString();
                                    g.DrawString(Remarks, drawFont, br, 150, 105);
                                }
                                else
                                {
                                    if (drPrintData["COMMENT"].ToString().Length < 50)
                                    {
                                        g.DrawString("Direction : " + drPrintData["COMMENT"].ToString(), drawFont, br, 08, 105);
                                    }
                                    else
                                    {
                                        string str1 = drPrintData["COMMENT"].ToString();

                                        string str2 = str1.Substring(0,40);
                                        int i = str1.Length;
                                        if (i > 50)
                                        {
                                            string str3 = str1.Substring(48, str1.Length - 48);
                                            g.DrawString(str3 + "..", drawFont, br, 08, 105);
                                        }
                                        else
                                        {
                                            string str3 = str1.Substring(48, str1.Length - 48);
                                            g.DrawString(str3, drawFont, br, 08, 105);
                                        }
                                        //string str3 = str1;
                                        //string str = drPrintData["COMMENT"].ToString().Substring(0, 40);
                                        //string str1 = drPrintData["COMMENT"].ToString().Substring(40);
                                        //g.DrawString("Comment : " + strComment[0] + "-", drawFont, br, 06, 95);

                                        g.DrawString("Comment : " + str2, drawFont, br, 08, 105);
                                        // }
                                        // if (strComment.Length > 1)
                                        // {

                                        // }
                                    }
                                }
                                string strQuantityLabel = "Quantity Dispensed";

                                g.DrawString(strQuantityLabel + " : " + drPrintData["DELIVERY_QUANTITY"].ToString(), PrintFont, br, 08, 120);
                                g.DrawString("Batch/Lot# : " + drPrintData["BATCHNO"].ToString(), PrintFont, br, 08, 140);// left qty  260
                                g.DrawString("Expiry : " + drPrintData["EXPIRY_DATE"].ToString(), PrintFont, br, 225, 130);
                                g.DrawString("Prepared By : ", PrintFont, br, 08, 160);// left qty  260
                                g.DrawString("Checked PH :", PrintFont, br, 08, 180);

                              
                                // g.DrawString("Refill : " + drPrintData["REFILL"].ToString(), PrintFont, br, 150, 150);
                                g.DrawString("KEEP OUT OF REACH OF CHILDREN", drawFont, br, 08, 200);
                                if (Convert.ToString(drPrintData["BARCODE"]).Trim() != string.Empty)
                                {
                                    var qrcode = new QRCodeWriter();
                                    var qrValue = drPrintData["BARCODE"].ToString();

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

                                    Image logo2 = Image.FromFile(strPath);// drPrintData["LOGO_PATH"].ToString()

                                    Rectangle destinationn = new Rectangle(225, 145, 50, 50);
                                    g.DrawImage(logo2, destinationn, 0, 0, logo2.Width, logo2.Height, GraphicsUnit.Pixel);


                                    // Rectangle rect2 = new Rectangle(05, 05, 283, 280);
                                    // g.DrawRectangle(Pens.Black, Rectangle.Round(rect2));
                                    logo2.Dispose();
                                    if (File.Exists(strPath))
                                    {
                                        File.Delete(strPath);
                                    }
                                }
                            }
                            else
                            {
                                //Brush br = new SolidBrush(Color.Black);
                                //PrintFont = new Font("Times New Roman", 8);
                                //string photoPath = string.Empty;
                                //Image logo1 = null;
                                //SolidBrush drawBrush = new SolidBrush(Color.Black);
                                //Font drawFont = new System.Drawing.Font("Times New Roman", 7, FontStyle.Bold);
                                //Font drawHeading = new System.Drawing.Font("Times New Roman", 10, FontStyle.Bold);
                                //StringFormat drawFormat = new StringFormat();

                                //drawFormat.FormatFlags = StringFormatFlags.DirectionRightToLeft;

                                ////if (Convert.ToInt32(drPrintData["ISINFUSION"]) == 0)
                                ////{
                                ////Rectangle destinationn = new Rectangle(175, 10, 30, 30);
                                ////g.DrawImage(logo1, destinationn, 0, 0, logo1.Width, logo1.Height, GraphicsUnit.Pixel);
                                ////logo1.Dispose();
                                //g.DrawString("Danat Al Emarat Hospital Women’s & Children", PrintFont, br, 25, 10); // Hospital name
                                //g.DrawString("24/7 Outpatient Pharmacy", PrintFont, br, 30, 35);//
                                //g.DrawString("Telephone: Extn 2222", PrintFont, br, 35, 60);//
                                //g.DrawString("Patient Name: ", PrintFont, br, 5, 85); // MRNO. Label
                                //g.DrawString(drPrintData["PATIENT_NAME"].ToString(), drawFont, drawBrush, 70, 85);
                                //g.DrawString("MRN # ", PrintFont, br, 5, 110); // MRNO. Label
                                //g.DrawString(drPrintData["MRNO"].ToString(), drawFont, drawBrush, 50, 110);
                                //g.DrawString("Allergy: ", PrintFont, br, 5, 135); // MRNO. Label
                                //g.DrawString(drPrintData["ALLERGY_DRUG_NAME"].ToString(), drawFont, drawBrush, 50, 135);
                                //g.DrawString("Weight: ", PrintFont, br, 5, 160); // MRNO. Label
                                //g.DrawString(drPrintData["WEIGHT"].ToString(), drawFont, drawBrush, 50, 160);

                                //g.DrawString("Drug Name: ", PrintFont, br, 5, 185); // MRNO. Label
                                //g.DrawString(drPrintData["MEDICINE_NAME"].ToString(), drawFont, drawBrush, 70, 185);
                                //g.DrawString("Dose: ", PrintFont, br, 5, 210); // MRNO. Label
                                //g.DrawString(drPrintData["DOSAGE"].ToString(), drawFont, drawBrush, 50, 210);

                                //g.DrawString("Route: ", PrintFont, br, 5, 235); // MRNO. Label
                                //g.DrawString(drPrintData["ROUTE"].ToString(), drawFont, drawBrush, 50, 235);
                                //g.DrawString("Frequency: ", PrintFont, br, 5, 260); // MRNO. Label
                                //g.DrawString(drPrintData["FREQUENCY_VALUE"].ToString(), drawFont, drawBrush, 50, 260);

                                //g.DrawString("Comment: ", PrintFont, br, 5, 285); // MRNO. Label
                                //g.DrawString(drPrintData["COMMENT"].ToString(), drawFont, drawBrush, 50, 285);

                                //g.DrawString("Due date/Time: ", PrintFont, br, 5, 310); // MRNO. Label
                                //g.DrawString(drPrintData["START_DATE"].ToString(), drawFont, drawBrush, 70, 310);
                                //g.DrawString("No Dose (s): ", PrintFont, br, 5, 335); // MRNO. Label
                                //g.DrawString(drPrintData["DURATION"].ToString(), drawFont, drawBrush, 50, 335);

                                //g.DrawString("Prepared by: ", PrintFont, br, 5, 360); // MRNO. Label
                                //g.DrawString("Checked PH: ", PrintFont, br, 5, 385); // MRNO. Label
                                //g.DrawString("RN Ckd: ", PrintFont, br, 5, 410); // MRNO. Label

                                //g.DrawString("Please return to Pharmacy when d/c or expired", PrintFont, br, 10, 435);//warning
                                ////}

                                string StrAllergy = string.Empty;
                                Brush br = new SolidBrush(Color.Black);
                                PrintFont = new Font("Times New Roman", 8);
                                string photoPath = string.Empty;
                                Image logo1 = null;
                                SolidBrush drawBrush = new SolidBrush(Color.Black);
                                Font drawFont = new System.Drawing.Font("Times New Roman", 8, FontStyle.Bold);
                                Font drawFontArab = new System.Drawing.Font("Arial", 8, FontStyle.Bold);
                                Font drawHeading = new System.Drawing.Font("Times New Roman", 9, FontStyle.Bold);
                                StringFormat drawFormat = new StringFormat();
                                drawFormat.FormatFlags = StringFormatFlags.DirectionRightToLeft;

                                //g.DrawString("Danat Al Emarat Hospital Women’s & Children", drawHeading, br, 130, 15); // Hospital name
                                // g.DrawString(drPrintData["Address1"].ToString(), drawHeading, br, 150, 25);//
                                // g.DrawString(drPrintData["Address2"].ToString(), drawHeading, br, 180, 35);//
                                string strName = Convert.ToString(drPrintData["PATIENT_NAME"]);
                                if (strName.Length > 25)
                                {
                                    strName = strName.Substring(0, 24);
                                    strName = strName + "..";
                                }
                                g.DrawString("Name : ", PrintFont, br, 06, 13);//Mrno
                                //g.DrawString("Pres.Name :", PrintFont, br, 08, 30);
                                //if (drPrintData["PROVIDER_NAME"] != DBNull.Value)
                                //{
                                //    string provider = "Dr." + drPrintData["PROVIDER_NAME"].ToString();
                                //    g.DrawString(provider, drawFont, br, 52, 30);
                                //}
                                g.DrawString("MRN : ", PrintFont, br, 225, 13);//clinic code 250
                                //g.DrawString("Weight : ", PrintFont, drawBrush, 06, 27);
                                //g.DrawString("Allergy : ", PrintFont, drawBrush, 90, 27);
                                g.DrawString(strName, drawFont, br, 45, 13);//Mrno
                                g.DrawString(drPrintData["MRNO"].ToString(), drawFont, br, 265, 13);//clinic code 250
                                bool IsSucess = false;
                                //if (dsTemp.Tables.Contains("EMR_PAT_WARNING") & dsTemp.Tables["EMR_PAT_WARNING"].Rows.Count > 0)
                                //{
                                //    bool IsMultiAllergy = false;
                                //    bool IsAllergyMorethenTwo = false;
                                //    foreach (DataRow dr in dsTemp.Tables["EMR_PAT_WARNING"].Rows)
                                //    {
                                //        IsSucess = true;
                                //        if (!IsAllergyMorethenTwo)
                                //        {
                                //            if (IsMultiAllergy)
                                //            {
                                //                StrAllergy = StrAllergy + " " + "-" + " " + Convert.ToString(dr["ALLERGY"]);
                                //                IsAllergyMorethenTwo = true;
                                //                IsSucess = false;
                                //            }
                                //            else
                                //            {
                                //                StrAllergy = StrAllergy + Convert.ToString(dr["ALLERGY"]);
                                //                IsMultiAllergy = true;
                                //            }
                                //        }

                                //    }
                                //    if (IsAllergyMorethenTwo & IsSucess)
                                //    {
                                //        string str = "++";
                                //        StrAllergy = StrAllergy + " " + str;
                                //    }
                                //if (Convert.ToString(drPrintData["ALLERGY_DRUG_NAME"]).Length < 28)
                                //{
                                //    g.DrawString(drPrintData["ALLERGY_DRUG_NAME"].ToString(), drawFont, drawBrush, 138, 27);
                                //}
                                //else if (Convert.ToString(drPrintData["ALLERGY_DRUG_NAME"]).Length > 28)
                                //{
                                //    g.DrawString(Convert.ToString(drPrintData["ALLERGY_DRUG_NAME"]).Substring(0, 28) + " ++", drawFont, drawBrush, 138, 27);
                                //}
                                //}
                                //  g.DrawString(drPrintData["ALLERGY"].ToString(), drawFont, drawBrush, 130, 27);
                                //g.DrawString(drPrintData["WEIGHT"].ToString(), drawFont, drawBrush, 52, 27);
                                //g.DrawString("Patient Name: " + drPrintData["PATIENT_NAME"].ToString(), PrintFont, br, 15, 50);//Mrno
                                //g.DrawString("MRN: " + drPrintData["MRNO"].ToString(), PrintFont, br, 150, 50);//clinic code 250
                                //g.DrawString("Allergy: " + drPrintData["ALLERGY"].ToString(), PrintFont, drawBrush, 320, 50, drawFormat);
                                //g.DrawString("Weight: " + drPrintData["Weight"].ToString(), PrintFont, drawBrush, 420, 50, drawFormat);
                                g.DrawString("Drug Name : ", PrintFont, br, 06, 38);//Mrno

                                string strQuantityLabel = drPrintData["QUANTITY_DISPLAY_NAME"].ToString();

                                //g.DrawStrin"g("Dose : ", PrintFont, br, 06, 60);//clinic code 250
                                g.DrawString("Dose" + ": ", PrintFont, br, 06, 60);//clinic code 250
                                if (Convert.ToString(drPrintData["MEDICINE_NAME"]).Length < 45)
                                {
                                    g.DrawString(drPrintData["MEDICINE_NAME"].ToString(), drawFont, br, 65, 38);
                                }
                                else
                                {
                                    g.DrawString(drPrintData["MEDICINE_NAME"].ToString().Substring(0, 44) + "..", drawFont, br, 65, 38);
                                }

                                //g.DrawString(drPrintData["DOSAGE"].ToString(), drawFont, drawBrush, 48, 60);
                                if (drPrintData["QUANTITY"] != DBNull.Value && drPrintData["QUANTITY_UNIT"] != DBNull.Value)
                                {
                                    g.DrawString(drPrintData["QUANTITY"].ToString() + drPrintData["QUANTITY_UNIT"].ToString(), drawFont, drawBrush, 48, 60);
                                }
                                g.DrawString("Batch/Lot : " + drPrintData["BATCHNO"].ToString(), PrintFont, br, 225, 60);
                                g.DrawString("Route : " + drPrintData["ROUTE"].ToString(), PrintFont, br, 06, 75);//pharmacy code
                                g.DrawString("Frequency : ", PrintFont, drawBrush, 225, 75);
                                g.DrawString(drPrintData["FREQUENCY_VALUE"].ToString(), drawFont, drawBrush, 280, 75);
                                if (drPrintData["COMMENT"].ToString().Length < 50)
                                {
                                    //g.DrawString("Comment : " + drPrintData["COMMENT"].ToString(), drawFont, br, 06, 90);
                                    if (drPrintData.Table.Columns.Contains("IS_ARABIC_NOTE") && drPrintData["IS_ARABIC_NOTE"] != DBNull.Value && Convert.ToInt32(drPrintData["IS_ARABIC_NOTE"]) == 1)
                                    {
                                        //string str = drPrintData["COMMENT"].ToString() + " : التعليمات";
                                        g.DrawString("تعليمات", drawFontArab, br, 300, 90, drawFormat);
                                        g.DrawString(" : ", drawFont, br, 262, 90, drawFormat);
                                        g.DrawString(drPrintData["COMMENT"].ToString(), drawFontArab, br, 260, 90, drawFormat);
                                    }
                                    else
                                    {
                                        g.DrawString("Direction : " + drPrintData["COMMENT"].ToString(), drawFont, br, 06, 90);
                                    }
                                }
                                else
                                {
                                    if (drPrintData.Table.Columns.Contains("IS_ARABIC_NOTE") && drPrintData["IS_ARABIC_NOTE"] != DBNull.Value && Convert.ToInt32(drPrintData["IS_ARABIC_NOTE"]) == 1)
                                    {
                                        string str = drPrintData["COMMENT"].ToString();
                                       // char[] array = str.ToCharArray();
                                        //Array.Reverse(array);
                                        //str= new String(array);

                                        string str1 = str.Substring(0, 48);
                                        int i = str.Length;
                                        if (i > 52)
                                        {
                                            string str2 = str.Substring(48, str.Length - 48);
                                            g.DrawString(str2, drawFontArab, br, 300, 100, drawFormat);
                                            //g.DrawString(str2, drawFontArab, br, 10, 100);
                                        }
                                        else
                                        {
                                            string str2 = str.Substring(48, str.Length - 48);

                                            g.DrawString(str2, drawFontArab, br, 260, 100);
                                        }
                                        g.DrawString("تعليمات", drawFontArab, br, 300, 90, drawFormat);
                                        g.DrawString(" : ", drawFontArab, br, 262, 90, drawFormat);
                                        g.DrawString(str1, drawFontArab, br, 260, 90, drawFormat);
                                    }
                                    else
                                    {
                                        string str = drPrintData["COMMENT"].ToString();
                                        if (str.Length > 65)
                                        {
                                            string str1 = str.Substring(0, 48);
                                            int i = str.Length;
                                            if (i > 52)
                                            {
                                                string str2 = str.Substring(48, str.Length - 48);
                                                g.DrawString(str2 + "..", drawFont, br, 06, 100);
                                            }
                                            else
                                            {
                                                string str2 = str.Substring(48, str.Length - 48);

                                                g.DrawString(str2, drawFont, br, 06, 100);
                                            }
                                            g.DrawString("Comment : " + str1 + "..", drawFont, br, 06, 90);
                                        }
                                        else {
                                            g.DrawString("Comment : " + str, drawFont, br, 06, 90);
                                        }
                                    }

                                }
                                g.DrawString("Room No: " + drPrintData["LOCATION"].ToString(), PrintFont, br, 06, 110);
                                g.DrawString("Dosage Form: " + drPrintData["QUANTITY_UNIT"].ToString(), PrintFont, br, 06, 130);
                                g.DrawString("Dispensing date : " + drPrintData["START_DATE"].ToString(), PrintFont, br, 06, 150);// left qty  260
                                g.DrawString("Quantity Dispensed: " + drPrintData["DELIVERY_QUANTITY"], PrintFont, br, 06, 170);
                                g.DrawString("Prepared By : ", PrintFont, br, 06, 190);// left qty  260
                                g.DrawString("Checked PH :", PrintFont, br, 06, 210);
                                g.DrawString("RN Checked: ", PrintFont, br, 06, 230);
                                g.DrawString("Expiry : " + drPrintData["EXPIRY_DATE"].ToString(), PrintFont, br, 225, 170);
                                // g.DrawString("Refill : " + drPrintData["REFILL"].ToString(), PrintFont, br, 150, 150);
                                g.DrawString("KEEP OUT OF REACH OF CHILDREN", drawFont, br, 06, 250);
                                if (Convert.ToString(drPrintData["BARCODE"]).Trim() != string.Empty)
                                {
                                    if (drPrintData["BARCODE"] != DBNull.Value)
                                    {
                                        var qrcode = new QRCodeWriter();
                                        var qrValue = drPrintData["BARCODE"].ToString();

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

                                        Image logo2 = Image.FromFile(strPath);// drPrintData["LOGO_PATH"].ToString()
                                        Rectangle destinationn = new Rectangle(225, 190, 50, 50);
                                        g.DrawImage(logo2, destinationn, 0, 0, logo2.Width, logo2.Height, GraphicsUnit.Pixel);


                                        // Rectangle rect2 = new Rectangle(05, 05, 283, 280);
                                        // g.DrawRectangle(Pens.Black, Rectangle.Round(rect2));
                                        logo2.Dispose();
                                        if (File.Exists(strPath))
                                        {
                                            File.Delete(strPath);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (SelectedServiceType == ServiceType.Bloodbank && SelectedPrintType == PrintType.Bloodbank) //Blood Bank label Print
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
                        //if (drPrintData["ORG_NAME"] != DBNull.Value)
                        //{
                        //    g.DrawString(Convert.ToString(drPrintData["ORG_NAME"]).ToUpper(), PrintFont1, br, 15, 90); // Hospital name
                        //}
                        //else
                        //{
                        //    g.DrawString("DR.S.FAKEEH HOSPITAL", PrintFont1, br, 15, 90); // Hospital name
                        //}
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
                    else if (Convert.ToString(drPrintData["BBLABEL_TYPE"]) == "2")//Replaced code from client side on 26-03-18
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
                            g.DrawString("DR.S.FAKEEH HOSPITAL", PrintFont2, br, 20, 35); // Hospital name
                        }
                        g.DrawString("Blood Bank", PrintFont2, br, 20, 55);
                        g.DrawString("COMPATIBLE WITH", PrintFont3, br, 70, 85);
                        g.DrawString("Name:", PrintFont, br, 15, 105);
                        if (drPrintData["PATIENT_NAME"].ToString().Length <= 35)
                        {
                            g.DrawString(Convert.ToString(drPrintData["PATIENT_NAME"]), PrintFont1, br, 65, 105);
                        }
                        else
                        {
                            g.DrawString(Convert.ToString(drPrintData["PATIENT_NAME"]).Substring(0, 34) + "..", PrintFont1, br, 65, 105);
                        }
                        g.DrawString("MRN:", PrintFont, br, 15, 125);
                        g.DrawString(Convert.ToString(drPrintData["MRNO"]).ToUpper(), PrintFont1, br, 65, 125);
                        g.DrawString("ABO/RH:", PrintFont, br, 15, 145);
                        g.DrawString(Convert.ToString(drPrintData["PATIENT_BLOOD"]), PrintFont1, br, 80, 145);
                        g.DrawString("Room:", PrintFont, br, 15, 160);
                        g.DrawString(Convert.ToString(drPrintData["PATIENT_ROOM_NAME"]).Replace("Bed:", ""), PrintFont1, br, 80, 160);

                        g.DrawString("ABO/RH2:", PrintFont, br, 15, 190);
                        g.DrawString(Convert.ToString(drPrintData["RECEIVED_BLOOD"]), PrintFont1, br, 90, 190);
                        g.DrawString("Product#:", PrintFont, br, 15, 210);
                        g.DrawString(Convert.ToString(drPrintData["PRODUCT_BARCODE"]), PrintFont1, br, 80, 210);
                        g.DrawString("Exp:", PrintFont, br, 140, 210);
                        g.DrawString(Convert.ToString(drPrintData["EXPIRY_DATE"]), PrintFont1, br, 180, 210);
                        g.DrawString("Actual Vol:", PrintFont, br, 15, 230);
                        g.DrawString(Convert.ToString(drPrintData["QUANTITY_AND_UNIT"]), PrintFont1, br, 85, 230);
                        g.DrawString("Type:", PrintFont, br, 140, 230);
                        g.DrawString(Convert.ToString(drPrintData["COMPONENT_NAME"]), PrintFont1, br, 180, 230);
                        g.DrawString("Date:", PrintFont, br, 15, 250);
                        g.DrawString(Convert.ToDateTime(drPrintData["CROSS_MATCH_DATE"]).ToString("dd/MMM/yyyy HH:mm"), PrintFont1, br, 60, 250);
                        g.DrawString("Crossmatched By:", PrintFont, br, 15, 270);
                        g.DrawString(Convert.ToString(drPrintData["CROSSMATCH_BY"]), PrintFont1, br, 123, 270);
                        g.DrawString(Convert.ToString(drPrintData["CROSSMATCH_BY"]), PrintFont1, br, 123, 270);
                        // the Symbol (š) and (œ} is used to delimit the barcode, and is required as the start and stop charachters by the Code128b Symbology.
                        //g.DrawString("š" + drPrintData["BB_COMPONENT_UNIT_NO"].ToString().Trim() + drPrintData["BB_COMPONENT_UNIT_NO"].ToString().Trim() + "œ", BarcodeFont, br, 75, 305);//6
                        //g.DrawString("š" + drPrintData["BB_COMPONENT_UNIT_NO"].ToString().Trim() + Convert.ToString((char)CheckSum(drPrintData["BB_COMPONENT_UNIT_NO"].ToString().Trim())) + "œ", BarcodeFont, br, 75, 305);//6
                        var qrcode = new QRCodeWriter();
                        var qrValue = drPrintData["BB_COMPONENT_UNIT_NO"].ToString().Trim();
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
                        //Rectangle destinationn = new Rectangle(350, 40, 50, 50);
                        //g.DrawImage(logo1, destinationn, 0, 0, logo1.Width, logo1.Height, GraphicsUnit.Pixel);
                        Rectangle destinationn1 = new Rectangle(305, 290, 50, 50);
                        g.DrawImage(logo1, destinationn1, 0, 0, logo1.Width, logo1.Height, GraphicsUnit.Pixel);
                        logo1.Dispose();
                        if (File.Exists(strPath))
                        {
                            File.Delete(strPath);
                        }
                        g.DrawString("Unit#:", PrintFont, br, 103, 325);
                        g.DrawString(Convert.ToString(drPrintData["BB_COMPONENT_UNIT_NO"]), PrintFont1, br, 150, 325);
                        Rectangle rect1 = new Rectangle(15, 35, 350, 308);
                        g.DrawRectangle(Pens.Black, Rectangle.Round(rect1)); // big rectangle
                        Rectangle rect2 = new Rectangle(15, 85, 350, 90);
                        g.DrawRectangle(Pens.Black, Rectangle.Round(rect2));

                    }
                }
                else if (SelectedServiceType == ServiceType.Common && SelectedPrintType == PrintType.PatientSlip)
                {
                    try
                    {
                        //NO NEED TO CHECK BARCODE COUNT NEED TO PRINT  ON 3*8 A4 PAPER
                        string Gender = Convert.ToString(drPrintData["GENDER"]).ToUpper();
                        Brush br = new SolidBrush(Color.Black);
                        string MrnoLabel = string.Empty;
                        MrnoLabel = "MRN :";
                        string mrno = Convert.ToString(drPrintData["MRNO"]);
                        //string EncounterNo = Convert.ToString(drPrintData["ENCOUNTER_NO"]);
                        string Insurance = drPrintData["INSURANCE"] != null && drPrintData["INSURANCE"] != DBNull.Value ? drPrintData["INSURANCE"].ToString() : string.Empty;// +"                  " + "Encounter No" + "  " + EncounterNo;
                        string Insurance2 = string.Empty;
                        if (Insurance.Length >= 36)
                        {
                            Insurance2 = Insurance.Substring(35, Insurance.Length - 35);
                            Insurance = Insurance.Substring(0, 35) + "..";
                        }
                        string Name = Convert.ToString(drPrintData["PATIENT_NAME"]);
                        if (Name.Length >= 40)
                        {
                            Name = Name.Substring(0, 39);
                            Name = Name + "..";
                        }
                        string doB = "DOB :" + Convert.ToString(drPrintData["DOB"]);
                        //NEED TO CALCULATE AGE FROM DATE OF BIRTH AND SHOW BELOW 
                        //BASED ON THE AGE IF ITS LESS THEN YEAR AGE WILL BE PREFIXED WITH D,
                        //IF ITS MORE THEN YEAR IT WILL BE PREFIXED WITH Y
                        string AGE = "AGE : " + drPrintData["AGE"] + "   " + "GENDER :" + Gender;
                        string InsuranceName = Insurance != string.Empty ? "INS:" + Insurance : string.Empty;
                        string InsuranceName2 = Insurance2 != string.Empty ? Insurance2 : string.Empty;
                        string EncounterNo = drPrintData["ENCOUNTER_NO"] != DBNull.Value ? Convert.ToString(drPrintData["ENCOUNTER_NO"]) : string.Empty;
                        string VisitDate = drPrintData["VISIT_DATE"] != DBNull.Value ? Convert.ToString(drPrintData["VISIT_DATE"]) : string.Empty;
                        string encounterNoLabel = string.Empty;
                        string visitDatelabel = string.Empty;
                        if (EncounterNo != string.Empty && VisitDate != string.Empty)
                        {
                            encounterNoLabel = "Vis : " + EncounterNo;
                            visitDatelabel = "Visit Date :" + VisitDate;
                        }

                        System.Drawing.Font drawFont = new System.Drawing.Font("Times New Roman", 8, FontStyle.Bold);
                        System.Drawing.Font drawName = new System.Drawing.Font("Times New Roman", 8, FontStyle.Bold);
                        System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
                        float y = 100.0f;
                        // StringFormat drawFormat = new StringFormat();
                        // drawFormat.FormatFlags = StringFormatFlags.;
                        g.DrawString(Name, drawFont, drawBrush, 10, 50);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 10, 65);
                        g.DrawString(doB, drawFont, drawBrush, 120, 65);
                        g.DrawString(AGE, drawFont, drawBrush, 10, 80);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 10, 100);//6
                        g.DrawString(InsuranceName, drawFont, drawBrush, 10, 125);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 32, 140);

                        /*Removed Below section as per the mail reveived from Client side
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 10, 140);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 100, 140);
                        */
                        //g.DrawString("š" + drPrintData["MRNO"].ToString().Trim() + Convert.ToString((char)CheckSum(drPrintData["MRNO"].ToString().Trim())) + "œ", BarcodeFont, br, 15, 12);//6
                        //g.DrawString("*" + drPrintData["MRNO"].ToString().Trim() + "*", BarcodeFont, br, 15, 1);//6

                        g.DrawString(Name, drawFont, drawBrush, 270, 50);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 270, 65);
                        g.DrawString(doB, drawFont, drawBrush, 380, 65);
                        g.DrawString(AGE, drawFont, drawBrush, 270, 80);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 270, 100);//6
                        g.DrawString(InsuranceName, drawFont, drawBrush, 270, 125);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 292, 140);
                        /*Removed Below section as per the mail reveived from Client side
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 270, 140);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 360, 140);
                        */

                        g.DrawString(Name, drawFont, drawBrush, 550, 50);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 550, 65);
                        g.DrawString(doB, drawFont, drawBrush, 660, 65);
                        g.DrawString(AGE, drawFont, drawBrush, 550, 80);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 550, 100);//6
                        g.DrawString(InsuranceName, drawFont, drawBrush, 550, 125);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 572, 140);
                        /*Removed Below section as per the mail reveived from Client side
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 550, 140);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 640, 140);
                         */

                        //first row ends here
                        g.DrawString(Name, drawFont, drawBrush, 10, 180);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 10, 195);
                        g.DrawString(doB, drawFont, drawBrush, 120, 195);
                        g.DrawString(AGE, drawFont, drawBrush, 10, 210);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 10, 230);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 10, 260);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 32, 275);
                        /*Removed Below section as per the mail reveived from Client side
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 10, 275);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 100, 275);
                        */

                        g.DrawString(Name, drawFont, drawBrush, 270, 180);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 270, 195);
                        g.DrawString(doB, drawFont, drawBrush, 380, 195);
                        g.DrawString(AGE, drawFont, drawBrush, 270, 210);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 270, 230);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 270, 260);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 292, 275);
                        /*Removed Below section as per the mail reveived from Client side
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 270, 275);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 360, 275);
                        */

                        g.DrawString(Name, drawFont, drawBrush, 550, 180);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 550, 195);
                        g.DrawString(doB, drawFont, drawBrush, 660, 195);
                        g.DrawString(AGE, drawFont, drawBrush, 550, 210);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 550, 230);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 550, 260);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 572, 275);
                        /*Removed Below section as per the mail reveived from Client side
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 550, 275);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 640, 275);
                        */

                        //2nd row end
                        g.DrawString(Name, drawFont, drawBrush, 10, 320);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 10, 335);
                        g.DrawString(doB, drawFont, drawBrush, 120, 335);
                        g.DrawString(AGE, drawFont, drawBrush, 10, 350);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 10, 370);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 10, 400);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 32, 415);
                        /*Removed Below section as per the mail reveived from Client side
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 10, 415);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 100, 415);
                        */

                        g.DrawString(Name, drawFont, drawBrush, 270, 320);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 270, 335);
                        g.DrawString(doB, drawFont, drawBrush, 380, 335);
                        g.DrawString(AGE, drawFont, drawBrush, 270, 350);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 270, 370);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 270, 400);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 292, 415);
                        /*Removed Below section as per the mail reveived from Client side
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 270, 415);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 360, 415);
                        */

                        g.DrawString(Name, drawFont, drawBrush, 550, 320);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 550, 335);
                        g.DrawString(doB, drawFont, drawBrush, 660, 335);
                        g.DrawString(AGE, drawFont, drawBrush, 550, 350);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 550, 370);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 550, 400);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 572, 415);
                        /*Removed Below section as per the mail reveived from Client side
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 550, 415);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 640, 415);
                        */

                        //third row end here
                        g.DrawString(Name, drawFont, drawBrush, 10, 460);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 10, 475);
                        g.DrawString(doB, drawFont, drawBrush, 120, 475);
                        g.DrawString(AGE, drawFont, drawBrush, 10, 490);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 10, 510);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 10, 540);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 32, 555);
                        /*Removed Below section as per the mail reveived from Client side
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 10, 555);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 100, 555);
                        */

                        g.DrawString(Name, drawFont, drawBrush, 270, 460);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 270, 475);
                        g.DrawString(doB, drawFont, drawBrush, 380, 475);
                        g.DrawString(AGE, drawFont, drawBrush, 270, 490);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 270, 510);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 270, 540);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 292, 555);
                        /*Removed Below section as per the mail reveived from Client side
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 270, 555);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 360, 555);
                        */

                        g.DrawString(Name, drawFont, drawBrush, 550, 460);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 550, 475);
                        g.DrawString(doB, drawFont, drawBrush, 660, 475);
                        g.DrawString(AGE, drawFont, drawBrush, 550, 490);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 550, 510);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 550, 535);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 572, 555);
                        /*Removed Below section as per the mail reveived from Client side
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 550, 550);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 640, 550);
                        */

                        //fourth ends here
                        g.DrawString(Name, drawFont, drawBrush, 10, 590);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 10, 605);
                        g.DrawString(doB, drawFont, drawBrush, 120, 605);
                        g.DrawString(AGE, drawFont, drawBrush, 10, 620);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 10, 640);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 10, 670);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 32, 685);
                        /*Removed Below section as per the mail reveived from Client side
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 10, 685);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 100, 685);
                        */

                        g.DrawString(Name, drawFont, drawBrush, 270, 590);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 270, 605);
                        g.DrawString(doB, drawFont, drawBrush, 380, 605);
                        g.DrawString(AGE, drawFont, drawBrush, 270, 620);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 270, 640);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 270, 670);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 292, 685);
                        /*Removed Below section as per the mail reveived from Client side
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 270, 685);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 360, 685);
                        */

                        g.DrawString(Name, drawFont, drawBrush, 550, 590);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 550, 605);
                        g.DrawString(doB, drawFont, drawBrush, 660, 605);
                        g.DrawString(AGE, drawFont, drawBrush, 550, 620);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 550, 640);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 550, 670);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 572, 685);
                        /*Removed Below section as per the mail reveived from Client side
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 550, 685);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 640, 685);
                        */

                        //fifth row ends here

                        g.DrawString(Name, drawFont, drawBrush, 10, 720);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 10, 735);
                        g.DrawString(doB, drawFont, drawBrush, 120, 735);
                        g.DrawString(AGE, drawFont, drawBrush, 10, 750);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 10, 770);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 10, 800);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 32, 815);
                        /*Removed Below section as per the mail reveived from Client side
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 10, 815);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 100, 815);
                        */

                        g.DrawString(Name, drawFont, drawBrush, 270, 720);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 270, 735);
                        g.DrawString(doB, drawFont, drawBrush, 380, 735);
                        g.DrawString(AGE, drawFont, drawBrush, 270, 750);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 270, 770);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 270, 800);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 292, 815);
                        /*Removed Below section as per the mail reveived from Client side
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 270, 815);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 360, 815);
                        */

                        g.DrawString(Name, drawFont, drawBrush, 550, 720);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 550, 735);
                        g.DrawString(doB, drawFont, drawBrush, 660, 735);
                        g.DrawString(AGE, drawFont, drawBrush, 550, 750);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 550, 770);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 550, 800);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 572, 815);
                        /*Removed Below section as per the mail reveived from Client side
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 550, 815);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 640, 815);
                        //sixth row ends here
                        */

                        g.DrawString(Name, drawFont, drawBrush, 10, 850);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 10, 865);
                        g.DrawString(doB, drawFont, drawBrush, 120, 865);
                        g.DrawString(AGE, drawFont, drawBrush, 10, 880);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 10, 900);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 10, 930);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 32, 945);
                        /*Removed Below section as per the mail reveived from Client side
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 10, 945);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 100, 945);
                        */

                        g.DrawString(Name, drawFont, drawBrush, 270, 850);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 270, 865);
                        g.DrawString(doB, drawFont, drawBrush, 380, 865);
                        g.DrawString(AGE, drawFont, drawBrush, 270, 880);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 270, 900);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 270, 930);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 292, 945);
                        /*Removed Below section as per the mail reveived from Client side
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 270, 945);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 360, 945);
                        */

                        g.DrawString(Name, drawFont, drawBrush, 550, 850);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 550, 865);
                        g.DrawString(doB, drawFont, drawBrush, 660, 865);
                        g.DrawString(AGE, drawFont, drawBrush, 550, 880);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 550, 900);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 550, 930);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 572, 945);
                        /*Removed Below section as per the mail reveived from Client side
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 550, 945);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 640, 945);
                        */

                        //seventh row ends here
                        g.DrawString(Name, drawFont, drawBrush, 10, 980);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 10, 995);
                        g.DrawString(doB, drawFont, drawBrush, 120, 995);
                        g.DrawString(AGE, drawFont, drawBrush, 10, 1010);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 10, 1030);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 10, 1060);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 32, 1075);
                        /*Removed Below section as per the mail reveived from Client side
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 10, 1075);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 100, 1075);
                        */

                        g.DrawString(Name, drawFont, drawBrush, 270, 980);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 270, 995);
                        g.DrawString(doB, drawFont, drawBrush, 380, 995);
                        g.DrawString(AGE, drawFont, drawBrush, 270, 1010);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 270, 1030);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 270, 1060);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 292, 1075);
                        /*Removed Below section as per the mail reveived from Client side
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 270, 1075);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 360, 1075);
                        */

                        g.DrawString(Name, drawFont, drawBrush, 550, 980);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 550, 995);
                        g.DrawString(doB, drawFont, drawBrush, 660, 995);
                        g.DrawString(AGE, drawFont, drawBrush, 550, 1010);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 550, 1030);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 550, 1060);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 572, 1075);
                        /*Removed Below section as per the mail reveived from Client side
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 550, 1075);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 640, 1075);
                        */

                    }
                    catch (Exception)
                    {

                        throw;
                    }
                    #region Commented
                    /*
                    //Graphics g = e.Graphics;
                    try
                    {
                        //NO NEED TO CHECK BARCODE COUNT NEED TO PRINT  ON 3*8 A4 PAPER
                        string Gender = Convert.ToString(drPrintData["GENDER"]).ToUpper();                        
                        Brush br = new SolidBrush(Color.Black);
                        string MrnoLabel = string.Empty;
                        MrnoLabel = "MRNO :";
                        string mrno = Convert.ToString(drPrintData["MRNO"]);
                        //string EncounterNo = Convert.ToString(drPrintData["ENCOUNTER_NO"]);
                        string Insurance = drPrintData["INSURANCE"] != null && drPrintData["INSURANCE"] != DBNull.Value ? drPrintData["INSURANCE"].ToString() : string.Empty;// +"                  " + "Encounter No" + "  " + EncounterNo;
                        string Insurance2 = string.Empty;
                        if(Insurance.Length >= 36)
                        {
                            Insurance2 = Insurance.Substring(35,Insurance.Length-35);
                            Insurance = Insurance.Substring(0,35)+"..";
                        }                        
                        string Name = Convert.ToString(drPrintData["PATIENT_NAME"]);
                        if (Name.Length >= 40)
                        {
                            Name = Name.Substring(0, 39);
                            Name = Name + "..";
                        }
                        string doB = "DOB :" + Convert.ToString(drPrintData["DOB"]);
                        //NEED TO CALCULATE AGE FROM DATE OF BIRTH AND SHOW BELOW 
                        //BASED ON THE AGE IF ITS LESS THEN YEAR AGE WILL BE PREFIXED WITH D,
                        //IF ITS MORE THEN YEAR IT WILL BE PREFIXED WITH Y
                        string AGE = "AGE : " + drPrintData["AGE"] + "   " + "GENDER :" + Gender;
                        string InsuranceName = Insurance != string.Empty ? "INS:" + Insurance : string.Empty;
                        string InsuranceName2 = Insurance2 != string.Empty ? Insurance2 : string.Empty;
                        string EncounterNo = drPrintData["ENCOUNTER_NO"] != DBNull.Value ? Convert.ToString(drPrintData["ENCOUNTER_NO"]) : string.Empty;
                        string VisitDate = drPrintData["VISIT_DATE"] != DBNull.Value ? Convert.ToString(drPrintData["VISIT_DATE"]) : string.Empty;
                        string encounterNoLabel=string.Empty;
                        string visitDatelabel = string.Empty;
                        if (EncounterNo != string.Empty && VisitDate != string.Empty)
                        {
                            encounterNoLabel = "Vis : " + EncounterNo;
                            visitDatelabel = "Visit Date :" + VisitDate;
                        }

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
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 32, 140);
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 10, 140);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 100, 140);
                        //g.DrawString("š" + drPrintData["MRNO"].ToString().Trim() + Convert.ToString((char)CheckSum(drPrintData["MRNO"].ToString().Trim())) + "œ", BarcodeFont, br, 15, 12);//6
                        //g.DrawString("*" + drPrintData["MRNO"].ToString().Trim() + "*", BarcodeFont, br, 15, 1);//6

                        g.DrawString(Name, drawFont, drawBrush, 270, 40);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 270, 55);
                        g.DrawString(doB, drawFont, drawBrush, 380, 55);
                        g.DrawString(AGE, drawFont, drawBrush, 270, 70);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 270, 90);//6
                        g.DrawString(InsuranceName, drawFont, drawBrush, 270, 125);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 292, 140);
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 270, 140);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 360, 140);

                        g.DrawString(Name, drawFont, drawBrush, 550, 40);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 550, 55);
                        g.DrawString(doB, drawFont, drawBrush, 660, 55);
                        g.DrawString(AGE, drawFont, drawBrush, 550, 70);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 550, 90);//6
                        g.DrawString(InsuranceName, drawFont, drawBrush, 550, 125);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 572, 140);
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 550, 140);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 640, 140);
                        //first row ends here
                        g.DrawString(Name, drawFont, drawBrush, 10, 170);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 10, 185);
                        g.DrawString(doB, drawFont, drawBrush, 120, 185);
                        g.DrawString(AGE, drawFont, drawBrush, 10, 200);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 10, 220);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 10, 260);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 32, 275);
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 10, 275);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 100, 275);

                        g.DrawString(Name, drawFont, drawBrush, 270, 170);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 270, 185);
                        g.DrawString(doB, drawFont, drawBrush, 380, 185);
                        g.DrawString(AGE, drawFont, drawBrush, 270, 200);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 270, 220);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 270, 260);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 292, 275);
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 270, 275);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 360, 275);

                        g.DrawString(Name, drawFont, drawBrush, 550, 170);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 550, 185);
                        g.DrawString(doB, drawFont, drawBrush, 660, 185);
                        g.DrawString(AGE, drawFont, drawBrush, 550, 200);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 550, 220);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 550, 260);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 572, 275);
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 550, 275);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 640, 275);
                        //2nd row end
                        g.DrawString(Name, drawFont, drawBrush, 10, 310);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 10, 325);
                        g.DrawString(doB, drawFont, drawBrush, 120, 325);
                        g.DrawString(AGE, drawFont, drawBrush, 10, 340);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 10, 360);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 10, 400);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 32, 415);
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 10, 415);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 100, 415);

                        g.DrawString(Name, drawFont, drawBrush, 270, 310);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 270, 325);
                        g.DrawString(doB, drawFont, drawBrush, 380, 325);
                        g.DrawString(AGE, drawFont, drawBrush, 270, 340);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 270, 360);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 270, 400);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 292, 415);
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 270, 415);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 360, 415);

                        g.DrawString(Name, drawFont, drawBrush, 550, 310);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 550, 325);
                        g.DrawString(doB, drawFont, drawBrush, 660, 325);
                        g.DrawString(AGE, drawFont, drawBrush, 550, 340);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 550, 360);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 550, 400);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 572, 415);
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 550, 415);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 640, 415);
                        //third row end here
                        g.DrawString(Name, drawFont, drawBrush, 10, 450);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 10, 465);
                        g.DrawString(doB, drawFont, drawBrush, 120, 465);
                        g.DrawString(AGE, drawFont, drawBrush, 10, 480);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 10, 500);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 10, 540);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 32, 555);
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 10, 555);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 100, 555);

                        g.DrawString(Name, drawFont, drawBrush, 270, 450);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 270, 465);
                        g.DrawString(doB, drawFont, drawBrush, 380, 465);
                        g.DrawString(AGE, drawFont, drawBrush, 270, 480);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 270, 500);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 270, 540);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 292, 555);
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 270, 555);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 360, 555);

                        g.DrawString(Name, drawFont, drawBrush, 550, 450);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 550, 465);
                        g.DrawString(doB, drawFont, drawBrush, 660, 465);
                        g.DrawString(AGE, drawFont, drawBrush, 550, 480);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 550, 500);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 550, 540);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 572, 555);
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 550, 555);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 640, 555);
                        //fourth ends here
                        g.DrawString(Name, drawFont, drawBrush, 10, 580);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 10, 595);
                        g.DrawString(doB, drawFont, drawBrush, 120, 595);
                        g.DrawString(AGE, drawFont, drawBrush, 10, 610);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 10, 630);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 10, 670);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 32, 685);
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 10, 685);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 100, 685);

                        g.DrawString(Name, drawFont, drawBrush, 270, 580);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 270, 595);
                        g.DrawString(doB, drawFont, drawBrush, 380, 595);
                        g.DrawString(AGE, drawFont, drawBrush, 270, 610);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 270, 630);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 270, 670);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 292, 685);
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 270, 685);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 360, 685);

                        g.DrawString(Name, drawFont, drawBrush, 550, 580);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 550, 595);
                        g.DrawString(doB, drawFont, drawBrush, 660, 595);
                        g.DrawString(AGE, drawFont, drawBrush, 550, 610);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 550, 630);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 550, 670);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 572, 685);
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 550, 685);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 640, 685);
                        //fifth row ends here

                        g.DrawString(Name, drawFont, drawBrush, 10, 710);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 10, 725);
                        g.DrawString(doB, drawFont, drawBrush, 120, 725);
                        g.DrawString(AGE, drawFont, drawBrush, 10, 740);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 10, 760);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 10, 800);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 32, 815);
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 10, 815);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 100, 815);

                        g.DrawString(Name, drawFont, drawBrush, 270, 710);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 270, 725);
                        g.DrawString(doB, drawFont, drawBrush, 380, 725);
                        g.DrawString(AGE, drawFont, drawBrush, 270, 740);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 270, 760);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 270, 800);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 292, 815);
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 270, 815);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 360, 815);

                        g.DrawString(Name, drawFont, drawBrush, 550, 710);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 550, 725);
                        g.DrawString(doB, drawFont, drawBrush, 660, 725);
                        g.DrawString(AGE, drawFont, drawBrush, 550, 740);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 550, 760);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 550, 800);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 572, 815);
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 550, 815);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 640, 815);
                        //sixth row ends here

                        g.DrawString(Name, drawFont, drawBrush, 10, 840);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 10, 855);
                        g.DrawString(doB, drawFont, drawBrush, 120, 855);
                        g.DrawString(AGE, drawFont, drawBrush, 10, 870);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 10, 890);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 10, 930);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 32, 945);
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 10, 945);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 100, 945);

                        g.DrawString(Name, drawFont, drawBrush, 270, 840);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 270, 855);
                        g.DrawString(doB, drawFont, drawBrush, 380, 855);
                        g.DrawString(AGE, drawFont, drawBrush, 270, 870);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 270, 890);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 270, 930);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 292, 945);
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 270, 945);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 360, 945);

                        g.DrawString(Name, drawFont, drawBrush, 550, 840);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 550, 855);
                        g.DrawString(doB, drawFont, drawBrush, 660, 855);
                        g.DrawString(AGE, drawFont, drawBrush, 550, 870);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 550, 890);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 550, 930);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 572, 945);
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 550, 945);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 640, 945);
                        //seventh row ends here
                        g.DrawString(Name, drawFont, drawBrush, 10, 970);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 10, 985);
                        g.DrawString(doB, drawFont, drawBrush, 120, 985);
                        g.DrawString(AGE, drawFont, drawBrush, 10, 1000);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 10, 1020);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 10, 1060);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 32, 1075);
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 10, 1075);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 100, 1075);

                        g.DrawString(Name, drawFont, drawBrush, 270, 970);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 270, 985);
                        g.DrawString(doB, drawFont, drawBrush, 380, 985);
                        g.DrawString(AGE, drawFont, drawBrush, 270, 1000);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 270, 1020);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 270, 1060);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 292, 1075);
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 270, 1075);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 360, 1075);

                        g.DrawString(Name, drawFont, drawBrush, 550, 970);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 550, 985);
                        g.DrawString(doB, drawFont, drawBrush, 660, 985);
                        g.DrawString(AGE, drawFont, drawBrush, 550, 1000);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 550, 1020);
                        g.DrawString(InsuranceName, drawFont, drawBrush, 550, 1060);
                        //g.DrawString(InsuranceName2, drawFont, drawBrush, 572, 1075);
                        g.DrawString(encounterNoLabel, drawFont, drawBrush, 550, 1075);
                        g.DrawString(visitDatelabel, drawFont, drawBrush, 640, 1075);

                    }
                    catch (Exception)
                    {

                        throw;
                    }
                    */
                    #endregion Commented
                }
                else if (SelectedServiceType == ServiceType.Common && SelectedPrintType == PrintType.PatientBand)
                {
                    //Graphics g = e.Graphics;
                    try
                    {
                        string Path = System.AppDomain.CurrentDomain.BaseDirectory + System.DateTime.Now.ToString("ddmmyyhhmmss");
                        string Gender = Convert.ToString(drPrintData["GENDER"]).ToUpper();
                        string subTitle = string.Empty;
                        if (Gender == "MALE")
                        {
                            Gender = "M";
                            subTitle = " B/o ";
                        }
                        else if (Gender == "FEMALE")
                        {
                            Gender = "F";
                            subTitle = " B/o ";
                        }
                        if (Convert.ToString(drPrintData["PATIENT_TYPE"]) == "0")//ISINFANT
                        {
                            string Strmrno = "MRN : " + Convert.ToString(drPrintData["MRNO"]);
                            if (drPrintData["MOTHER_MRNO"] != DBNull.Value) Strmrno += subTitle + Convert.ToString(drPrintData["MOTHER_MRNO"]);
                            string StrName = Convert.ToString(drPrintData["PATIENT_NAME"]).ToUpper();
                            string Strdob = "DOB : " + Convert.ToDateTime(drPrintData["DOB"]).ToString("dd-MMM-yyyy");
                            //NEED TO WRITE LOGIC TO SHOW AGE IN DAY FOR INFANT 
                            string Strage = "AGE : " + Convert.ToString(drPrintData["AGE"]) + "   " + "GENDER :" + Gender;
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
                        else if (Convert.ToString(drPrintData["PATIENT_TYPE"]) == "1")//ISCHILD
                        {

                            string Strmrno = "MRN : " + Convert.ToString(drPrintData["MRNO"]);
                            string StrName = Convert.ToString(drPrintData["PATIENT_NAME"]).ToUpper();
                            string Strdob = "DOB : " + Convert.ToDateTime(drPrintData["DOB"]).ToString("dd-MMM-yyyy");
                            //NEED TO WRITE LOGIC TO SHOW AGE IN NO YEAR FOR CHILD
                            string Strage = "AGE : " + Convert.ToString(drPrintData["AGE"]) + "   " + "GENDER :" + Gender;
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

                        else if (Convert.ToString(drPrintData["PATIENT_TYPE"]) == "2")//ISADULT
                        {

                            string Strmrno = "MRN : " + Convert.ToString(drPrintData["MRNO"]);
                            string StrName = Convert.ToString(drPrintData["PATIENT_NAME"]).ToUpper();
                            string Strdob = "DOB : " + Convert.ToDateTime(drPrintData["DOB"]).ToString("dd-MMM-yyyy");
                            //NEED TO WRITE LOGIC TO SHOW AGE IN NO OF YEARS
                            string Strage = "AGE : " + Convert.ToString(drPrintData["AGE"]) + "   " + "GENDER :" + Gender;
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
                else if (SelectedServiceType == ServiceType.Common && SelectedPrintType == PrintType.ERPCode)
                {
                    try
                    {
                        string str = string.Empty;
                        string str1 = string.Empty;
                        string str2 = string.Empty;
                        str = drPrintData["ITEM_NAME"].ToString();
                        string strPath = System.AppDomain.CurrentDomain.BaseDirectory + System.DateTime.Now.ToString("ddmmyyhhmmss");
                        Brush br = new SolidBrush(Color.Black);
                        PrintFont = new Font("Times New Roman", 8);
                        string photoPath = string.Empty;
                        Image logo1 = null;
                        SolidBrush drawBrush = new SolidBrush(Color.Black);
                        Font drawFont = new System.Drawing.Font("Times New Roman", 8, FontStyle.Regular);
                        Font drawHeading = new System.Drawing.Font("Times New Roman", 10, FontStyle.Bold);
                        StringFormat drawFormat = new StringFormat();
                        drawFormat.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                        g.DrawString("Danat Al Emarat Hospital" + "", drawHeading, br, 35, 15);// left qty  260
                        // g.DrawString("": " + "SFDSFDF", drawFont, br, 220, 30);// left qty  260
                        if (str.Length > 35)
                        {
                            str1 = str.Substring(0, 34);
                            str2 = str.Remove(0, 34);
                            g.DrawString(str1 + "", drawFont, br, 10, 40);
                            g.DrawString(str2 + "", drawFont, br, 10, 50);
                        }
                        else
                        {
                            g.DrawString(str + "", drawFont, br, 10, 50);//pharmacy code
                        }
                        //g.DrawString(drPrintData["ITEM_NAME"].ToString() + "", drawFont, br, 10, 50);//pharmacy code
                        g.DrawString("Price :  AED  " + drPrintData["PRICE"].ToString(), drawFont, br, 10, 85);
                        g.DrawString("Expiry :" + drPrintData["EXP_DATE"].ToString(), drawFont, br, 10, 100);//pharmacy code
                        g.DrawString("" + drPrintData["ERP_CODE"].ToString(), drawFont, br, 10, 115);//pharmacy code
                        var qrcode = new QRCodeWriter();
                        var qrValue = drPrintData["ERP_CODE"].ToString();

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
                            bitmap.Save(strPath, ImageFormat.Jpeg);

                        Image logo2 = Image.FromFile(strPath);// drPrintData["LOGO_PATH"].ToString()
                        Rectangle destinationn = new Rectangle(130, 67, 50, 50);
                        g.DrawImage(logo2, destinationn, 0, 0, logo2.Width, logo2.Height, GraphicsUnit.Pixel);
                        logo2.Dispose();
                        if (File.Exists(strPath))
                        {
                            File.Delete(strPath);
                        }



                        //Brush br = new SolidBrush(Color.Black);
                        //PrintFont = new Font("Courier New", 8);
                        //string photoPath = string.Empty;
                        //Image logo1 = null;
                        //SolidBrush drawBrush = new SolidBrush(Color.Black);
                        //Font drawFont = new System.Drawing.Font("Arial", 8, FontStyle.Bold);
                        //Font drawHeading = new System.Drawing.Font("Arial", 8, FontStyle.Bold);
                        //StringFormat drawFormat = new StringFormat();
                        //drawFormat.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                        ////g.DrawString("ERP Code of Item: " + drPrintData["ERP_CODE"].ToString(), drawFont, br, 120, 60);// left qty  260
                        ////g.DrawString("Name of Item   : " + drPrintData["ITEM_NAME"].ToString(), drawFont, br, 120, 80);
                        //string strItemName = drPrintData["ITEM_NAME"].ToString();
                        //Int32 xValue = 120, yValue = 60;
                        //string[] strMessage =new string[10];
                        //if (strItemName.Length > 35)
                        //{
                        //    string[] strMessageArray = strItemName.Split(' ');
                        //    string strTemp = string.Empty;
                        //    string str1 = string.Empty, str2 = string.Empty;
                        //    Int32 count = 0;
                        //    foreach (string item in strMessageArray)
                        //    {
                        //        if ((strTemp + " " + item).Length > 35)
                        //        {
                        //            strMessage[count] = strTemp;
                        //            strTemp = string.Empty;
                        //            count = count + 1;
                        //        }
                        //        strTemp = strTemp + " " + item;
                        //    }
                        //    if (strTemp != string.Empty)
                        //    {
                        //        strMessage[count] = strTemp;
                        //    }
                        //}
                        //else
                        //{
                        //    strMessage[0] = strItemName;
                        //}
                        //int i = 0;
                        //foreach (string item in strMessage)
                        //{
                        //    if (item == null)
                        //    {
                        //        break;
                        //    }
                        //    if (i == 0)
                        //    {
                        //        g.DrawString("Item Name: " + item, drawFont, br, 120, yValue);
                        //        i = i + 1;
                        //    }
                        //    else
                        //    {
                        //        yValue = yValue + 15;
                        //        g.DrawString(item, drawFont, br, 185, yValue);
                        //    }
                        //}
                        ////120,60
                        //yValue = yValue + 25;
                        //g.DrawString("Expiry Date: " + drPrintData["EXP_DATE"].ToString(), drawFont, br, 120, yValue);

                        //BarcodeFont = new Font(strBarcodeFont, 17);
                        //drawFont = new System.Drawing.Font("Arial", 12, FontStyle.Bold);
                        //yValue = yValue + 35;
                        //if (strBarcodeType.Equals("Code3of9"))
                        //{
                        //    // the Asterisk (*) is used to delimit the barcode, and is required as the start and stop charachters by the 3of9 Symbology.
                        //    g.DrawString("*" + drPrintData["ERP_CODE"].ToString().Trim() + "*", BarcodeFont, br, 135, yValue);
                        //    yValue = yValue + 30;
                        //    g.DrawString(drPrintData["ERP_CODE"].ToString(), drawFont, br, 135, yValue);
                        //}
                        //else if (strBarcodeType.Equals("Code128b"))
                        //{
                        //    g.DrawString("š" + drPrintData["ERP_CODE"].ToString().Trim() + Convert.ToString((char)CheckSum(drPrintData["ERP_CODE"].ToString().Trim())) + "œ", BarcodeFont, br, 135, yValue);
                        //    yValue = yValue + 30;
                        //    g.DrawString(drPrintData["ERP_CODE"].ToString(), drawFont, br, 135, yValue);
                        //}
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
                else if (SelectedServiceType == ServiceType.Common && SelectedPrintType == PrintType.CPOEAdminPatientSlip)
                {
                    try
                    {
                        string Gender = Convert.ToString(drPrintData["GENDER"]).ToUpper();
                        if (Gender.ToUpper() == "MALE")
                        {
                            Gender = "M";
                        }
                        else if (Gender.ToUpper() == "FEMALE")
                        {
                            Gender = "F";
                        }
                        Brush br = new SolidBrush(Color.Black);
                        string MrnoLabel = string.Empty;
                        MrnoLabel = "MRN :";
                        string mrno = Convert.ToString(drPrintData["MRNO"]);
                        string Name = Convert.ToString(drPrintData["PATIENT_NAME"]);
                        if (Name.Length >= 40)
                        {
                            Name = Name.Substring(0, 39);
                            Name = Name + "..";
                        }
                        string doB = "DOB :" + Convert.ToDateTime(drPrintData["DOB"]).ToString("dd-MM-yyyy");
                        string AGE = "AGE : " + drPrintData["AGE"] + "   " + "GENDER :" + Gender;
                        System.Drawing.Font drawFont = new System.Drawing.Font("Times New Roman", 8, FontStyle.Bold);
                        System.Drawing.Font drawName = new System.Drawing.Font("Times New Roman", 8, FontStyle.Bold);
                        System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
                        float y = 100.0f;
                        g.DrawString(Name, drawFont, drawBrush, 10, 50);
                        g.DrawString(MrnoLabel + mrno, drawFont, drawBrush, 10, 65);
                        g.DrawString(doB, drawFont, drawBrush, 120, 65);
                        g.DrawString(AGE, drawFont, drawBrush, 10, 80);
                        g.DrawString("š" + mrno + Convert.ToString((char)CheckSum(mrno.Trim())) + "œ", BarcodeFont, br, 10, 100);
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
                else if (SelectedServiceType == ServiceType.Cafeteria && SelectedPrintType == PrintType.Invoice)
                {
                    StringFormat format = new StringFormat(StringFormatFlags.DirectionRightToLeft);
                    // g.DrawString("سلام",bas,new SolidBrush(Color.Red),r1,format);
                    System.Drawing.Font Logo = new System.Drawing.Font("Times New Roman", 8, FontStyle.Bold);
                    System.Drawing.Font TitelFont = new System.Drawing.Font("Times New Roman", 8, FontStyle.Underline);
                    System.Drawing.Font BasicFont = new System.Drawing.Font("Times New Roman", 8, FontStyle.Regular);
                    System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
                    // System.Drawing.SolidBrush drawBrushEmail = new System.Drawing.SolidBrush(System.Drawing.Color.Blue);

                    string CURR_TYPE = string.Empty;
                    string BILLED_BY = string.Empty;
                    string FOOD_NAME = string.Empty;
                    string HospitalName = string.Empty;
                    //string Address = string.Empty;
                    //string AddressLine1 = string.Empty;
                    //string AddressLine2 = string.Empty;
                    string LogoPath = string.Empty;

                    string Titel = string.Empty;
                    string MRNO = "ID: " + Convert.ToString(drPrintData["MRNO"]);
                    string NAME = Convert.ToString(drPrintData["NAME"]);
                    string BILL_NO = "Bill No: " + Convert.ToString(drPrintData["BILL_NO"]);
                    string DATE = "Date: " + Convert.ToString(drPrintData["RECEIPT_DATE"]);
                    string TotalAmount = string.Empty;
                    string item = "Item Description";
                    string SlNO = "#";
                    string HeadAmount = "Amount";
                    string Total = "Total :";
                    string Sign = "Sign :";
                    int SRNO = 1;
                    int VerticalLine = 140;
                    int y = 118;
                    decimal AmountSum;
                    decimal AMOUNT;
                    //AmountSum = drBillDtls["GROSS_AMOUNT"] != DBNull.Value ? drBillDtls["GROSS_AMOUNT"].ToString().KIFormatDecimalPlace(CommonData.DecimalPlace).ToString() : "0.00";
                    AmountSum = dsData.Tables["CAFETERIA_BILL_DETAIL"].AsEnumerable().Sum(x => Convert.ToDecimal(x["AMOUNT"]));
                    TotalAmount = Convert.ToString(AmountSum).KIFormatDecimalPlace(CommonData.DecimalPlace);
                    if (NAME.Length > 60)
                    {
                        NAME = NAME.Substring(0, 58);
                    }

                    HospitalName = "Danat Al-Emarat Women & Children Hospital";
                    //Address = "2nd Street, Abu Dhabi, United Arab Emirates" + "\n" + "Phone: +971 2 614 9999	Fax: +971 2 651 0088" ;
                    //AddressLine2 = "email : info@danatalemarat.ae";
                    Titel = "CAFETERIA BILL";
                    CURR_TYPE = Convert.ToString(drPrintData["CURR_TYPE"]);
                    BILLED_BY = "Cashier :" + Convert.ToString(drPrintData["BILLED_BY"]);

                    Pen MyPen = new Pen(Color.Black, .5f);
                    g.DrawString(HospitalName, Logo, drawBrush, 25, 20);

                    // g.DrawString("سلام", BasicFont, new SolidBrush(Color.Red), r1, format);
                    //   g.DrawString(
                    //g.DrawString(Address, BasicFont, drawBrush, 220, 30);
                    ////g.DrawString(AddressLine1, BasicFont, drawBrush, 180, 45);
                    //g.DrawString(AddressLine2, BasicFont, drawBrushEmail, 220, 70);

                    g.DrawString(Titel, TitelFont, drawBrush, 80, 35);
                    g.DrawString(MRNO, BasicFont, drawBrush, 30, 55);
                    g.DrawString(NAME, BasicFont, drawBrush, 120, 55);
                    g.DrawString(BILL_NO, BasicFont, drawBrush, 30, 70);
                    g.DrawString(DATE, BasicFont, drawBrush, 150, 70);

                    g.DrawLine(MyPen, 28, 90, 270, 90);
                    g.DrawString(SlNO, BasicFont, drawBrush, 32, 95);
                    g.DrawString(item, BasicFont, drawBrush, 60, 95);
                    g.DrawString(HeadAmount, BasicFont, drawBrush, 218, 95);
                    g.DrawLine(MyPen, 28, 110, 270, 110);
                    if (NAME.Length > 60)
                    {
                        NAME = NAME.Substring(0, 58);
                    }
                    foreach (DataRow dr in dsData.Tables["CAFETERIA_BILL_DETAIL"].Rows)
                    {

                        FOOD_NAME = Convert.ToString(dr["FOOD_NAME"]);
                        if (FOOD_NAME.Length > 25)
                        {
                            FOOD_NAME = FOOD_NAME.Substring(0, 23) + "..";
                        }
                        AMOUNT = Convert.ToDecimal(dr["AMOUNT"]);
                        g.DrawString(FOOD_NAME, BasicFont, drawBrush, 55, y);
                        //g.DrawString(Convert.ToString(AMOUNT).KIFormatDecimalPlace(0), BasicFont, drawBrush, 235, y);
                        //if(AMOUNT.ToString().Length >=7)
                        //{
                        g.DrawString(Convert.ToString(AMOUNT).KIFormatDecimalPlace(CommonData.DecimalPlace), BasicFont, drawBrush, 270, y, format);
                        //}
                        //else
                        //{
                        //    g.DrawString(Convert.ToString(AMOUNT).KIFormatDecimalPlace(0), BasicFont, drawBrush, 242, y, format);
                        //}
                        //if (SRNO >= 10)
                        //{
                        //    g.DrawString(Convert.ToString(SRNO), BasicFont, drawBrush, 34, y);
                        //}
                        //else
                        //{
                        g.DrawString(Convert.ToString(SRNO), BasicFont, drawBrush, 48, y, format);
                        // }
                        //  g.DrawString(Convert.ToString(SRNO), BasicFont, drawBrush, 38, y);
                        // g.DrawLine(MyPen, 25, 95, 95, VerticalLine);
                        int i = VerticalLine - 5;
                        g.DrawLine(MyPen, 50, 90, 50, i);
                        g.DrawLine(MyPen, 215, 90, 215, VerticalLine);
                        g.DrawLine(MyPen, 271, 90, 271, VerticalLine);
                        y += 15;
                        VerticalLine += 15;
                        SRNO++;
                    }
                    VerticalLine -= 20;
                    g.DrawLine(MyPen, 28, VerticalLine, 270, VerticalLine);
                    VerticalLine += 20;
                    g.DrawLine(MyPen, 28, 90, 28, VerticalLine);
                    // g.DrawLine(MyPen, 270, 95, 275, VerticalLine);
                    g.DrawLine(MyPen, 215, 105, 215, VerticalLine);
                    g.DrawLine(MyPen, 271, 90, 271, VerticalLine);
                    g.DrawLine(MyPen, 28, VerticalLine, 270, VerticalLine);
                    g.DrawString(Total, BasicFont, drawBrush, 147, VerticalLine -= 15);
                    g.DrawString(CURR_TYPE, BasicFont, drawBrush, 180, VerticalLine);
                    g.DrawString(TotalAmount, Logo, drawBrush, 270, VerticalLine, format);
                    //g.DrawString(BILLED_BY, BasicFont, drawBrush, 80, VerticalLine += 35);
                    g.DrawString(Sign, BasicFont, drawBrush, 28, VerticalLine += 27);
                    g.DrawString(" ", BasicFont, drawBrush, 33, VerticalLine += 27);

                    //LogoPath=@"D:\LogoImage\Logo";
                    //string LogoKey = string.Empty;
                    //if (ConfigurationSettings.AppSettings["LogoKey"] != null)
                    //{
                    //    LogoKey = ConfigurationSettings.AppSettings["LogoKey"].ToString();
                    //}


                    //Image LogoCaf = Image.FromFile(LogoKey);
                    //Rectangle destinationn = new Rectangle(150,10,70,80);
                    //g.DrawImage(LogoCaf, destinationn, 0, 0, LogoCaf.Width, LogoCaf.Height, GraphicsUnit.Pixel);

                    //LogoCaf.Dispose();


                }
                else if (SelectedServiceType == ServiceType.CSSD)
                {
                    Brush br = new SolidBrush(Color.Black);
                    PrintFont = new Font("Times New Roman", 8);
                    string photoPath = string.Empty;
                    Image logo1 = null;
                    SolidBrush drawBrush = new SolidBrush(Color.Black);
                    Font drawFont = new System.Drawing.Font("Times New Roman", 8, FontStyle.Bold);
                    Font noteFont = new System.Drawing.Font("Times New Roman", 7, FontStyle.Regular);
                    Font drawHeading = new System.Drawing.Font("Times New Roman", 9, FontStyle.Bold | FontStyle.Underline);
                    StringFormat drawFormat = new StringFormat();
                    drawFormat.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                    if (drPrintData["ORG_NAME"] != DBNull.Value && Convert.ToString(drPrintData["ORG_NAME"]) != string.Empty)
                    {
                        g.DrawString(drPrintData["ORG_NAME"].ToString(), drawHeading, br, 87, 15);
                    }
                    else
                    {
                        g.DrawString("Danat Al-Emarat Hospital", drawHeading, br, 87, 15);
                    }
                    g.DrawString("Central Sterile Department", drawHeading, br, 85, 30);
                    g.DrawString("Name of the Set : ", PrintFont, br, 06, 50);
                    g.DrawString("Washed By Name & ID No : ", PrintFont, br, 06, 65);
                    g.DrawString("Packed By : ", PrintFont, drawBrush, 06, 80);
                    g.DrawString("Sterilizer Used : ", PrintFont, drawBrush, 06, 95);
                    g.DrawString("Sterilizer No :", PrintFont, drawBrush, 06, 110);
                    g.DrawString("Loading in the Sterilizer By : ", PrintFont, br, 06, 125);//Mrno
                    //g.DrawString("ID No of Staff who loaded : ", PrintFont, br, 06, 140);
                    //g.DrawString("Unloaded from the Sterilizer By :  ", PrintFont, br, 06, 155);
                    //g.DrawString("ID No of Staff who unloaded : ", PrintFont, br, 06, 170);
                    //g.DrawString("Date of Processed : ", PrintFont, br, 06, 185);                     
                    g.DrawString("Unloaded from the Sterilizer By :  ", PrintFont, br, 06, 140);
                    g.DrawString("Date of Processed : ", PrintFont, br, 06, 155);

                    if (drPrintData.Table.Columns.Contains("CLEANING_COMMENTS") &&
                        drPrintData["CLEANING_COMMENTS"] != DBNull.Value && Convert.ToString(drPrintData["CLEANING_COMMENTS"]) != string.Empty)
                    {
                        g.DrawString(drPrintData["CLEANING_COMMENTS"].ToString(), drawFont, br, 06, 170);
                    }

                    g.DrawString(drPrintData["SET_NAME"].ToString(), drawFont, br, 100, 50);
                    g.DrawString(drPrintData["CLEANED_BY"].ToString() + "(" + drPrintData["CLEANING_USER_ID"].ToString() + ")", drawFont, br, 135, 65);
                    g.DrawString(drPrintData["PACKED_BY"].ToString(), drawFont, br, 80, 80);
                    g.DrawString(drPrintData["STERILIZER"].ToString(), drawFont, br, 80, 95);
                    g.DrawString(drPrintData["STERILIZER_NO"].ToString(), drawFont, br, 80, 110);
                    //g.DrawString(drPrintData["STRLIZE_START_BY"].ToString(), drawFont, br, 160, 125);
                    //g.DrawString(drPrintData["STRLIZE_START_USER_ID"].ToString(), drawFont, br, 160, 140);
                    //g.DrawString(drPrintData["STRLIZE_END_BY"].ToString(), drawFont, br, 160, 155);
                    //g.DrawString(drPrintData["STRLIZE_END_USER_ID"].ToString(), drawFont, br, 160, 170);
                    //g.DrawString(drPrintData["STERIL_FINISHDATE"].ToString(), drawFont, br, 160, 185);
                    //g.DrawString("PRODUCT SHOULD BE REMAIN STERILE UNTIL SOME OF EVENT CAUSE THE", noteFont, br, 06, 300);
                    //g.DrawString("ITEMS TO BE CONTAMINATED OR PACKAGE BECOME TORN OR WET OR", noteFont, br, 06, 310);
                    //g.DrawString("DROPPED ON THE FLOOR.", noteFont, br, 06, 320);

                    g.DrawString(drPrintData["STRLIZE_START_BY"].ToString() + "(" + drPrintData["STRLIZE_START_USER_ID"].ToString() + ")", drawFont, br, 160, 125);
                    g.DrawString(drPrintData["STRLIZE_END_BY"].ToString() + "(" + drPrintData["STRLIZE_END_USER_ID"].ToString() + ")", drawFont, br, 160, 140);
                    g.DrawString(drPrintData["STERIL_FINISHDATE"].ToString(), drawFont, br, 160, 155);
                    g.DrawString("PRODUCT SHOULD BE REMAIN STERILE UNTIL SOME OF EVENT CAUSE THE", noteFont, br, 06, 270);
                    g.DrawString("ITEMS TO BE CONTAMINATED OR PACKAGE BECOME TORN OR WET OR", noteFont, br, 06, 280);
                    g.DrawString("DROPPED ON THE FLOOR.", noteFont, br, 06, 290);

                    //var qrcode = new QRCodeWriter();
                    if (Convert.ToString(drPrintData["ITEM_DETAILS"]).Trim() != string.Empty)
                    {
                        var qrValue = Convert.ToString(drPrintData["ITEM_DETAILS"]);
                        var barcodeWriter = new BarcodeWriter
                        {
                            Format = BarcodeFormat.QR_CODE,
                            Options = new EncodingOptions
                            {
                                Height = 500,
                                Width = 500,
                                Margin = 1,
                            }
                        };
                        using (var bitmap = barcodeWriter.Write(qrValue))
                            bitmap.Save(strPath, ImageFormat.Png);

                        Image logo2 = Image.FromFile(strPath);// drPrintData["LOGO_PATH"].ToString()
                        Rectangle destinationn = new Rectangle(250, 170, 100, 100);
                        g.DrawImage(logo2, destinationn, 0, 0, logo2.Width, logo2.Height, GraphicsUnit.Pixel);


                        // Rectangle rect2 = new Rectangle(05, 05, 283, 280);
                        // g.DrawRectangle(Pens.Black, Rectangle.Round(rect2));
                        logo2.Dispose();
                    }
                    if (File.Exists(strPath))
                    {
                        File.Delete(strPath);
                    }                
                }
                else
                {
                    string patName = string.Empty;
                    string address1 = string.Empty;
                    string address2 = string.Empty;
                    Brush br = new SolidBrush(Color.Black);
                    PrintFont = new Font("Times New Roman", 10);
                    string photoPath = string.Empty;
                    Image image = null;

                    // if (drPrintData["PHOTO_PATH"].ToString().Trim().Length > 0)
                    {
                        //CommonClient.Controls.CommonFunctions.CommonFunctions oCommonFunction = new Infologics.Medilogics.CommonClient.Controls.CommonFunctions.CommonFunctions();
                        //photoPath = oCommonFunction.DownLoadFileFromServer(drPrintData["PHOTO_PATH"].ToString());
                        try
                        {
                            image = Image.FromFile(photoPath);
                        }
                        catch (FileNotFoundException)
                        {

                            photoPath = string.Empty;
                        }
                    }

                    if (photoPath == string.Empty)
                    {
                        patName = drPrintData["FIRST_NAME"].ToString().Trim() + " " + drPrintData["MIDDLE_NAME"].ToString().Trim() + " " + drPrintData["LAST_NAME"].ToString().Trim();
                        patName = patName.ToUpper();
                        if (drPrintData["TITLE"].ToString().Trim() != string.Empty)
                        {
                            patName = drPrintData["TITLE"].ToString().Trim() + " " + patName;
                        }
                        if (patName.Length > 26)
                        {
                            patName = patName.Substring(0, 26);
                        }

                        address1 = drPrintData["ADDRESS1"].ToString().Trim();
                        if (address1.Length > 24)
                        {
                            address1 = address1.Substring(0, 24);
                        }

                        address2 = drPrintData["ADDRESS2"].ToString().Trim();
                        if (address2.Length > 24)
                        {
                            address2 = address2.Substring(0, 24);
                        }

                        if (strBarcodeType.Equals("Code3of9"))
                        {
                            // the Asterisk (*) is used to delimit the barcode, and is required as the start and stop charachters by the 3of9 Symbology.
                            g.DrawString("*" + drPrintData["MRNO"].ToString().Trim() + "*", BarcodeFont, br, 110, 40);
                        }
                        else if (strBarcodeType.Equals("Code128b"))
                        {
                            // the Symbol (š) and (œ} is used to delimit the barcode, and is required as the start and stop charachters by the Code128b Symbology.
                            g.DrawString("š" + drPrintData["MRNO"].ToString().Trim() + Convert.ToString((char)CheckSum(drPrintData["MRNO"].ToString().Trim())) + "œ", BarcodeFont, br, 110, 40);
                        }

                        PrintFont = new Font("Times New Roman", 12, FontStyle.Bold);
                        g.DrawString(patName, PrintFont, br, 30, 75);

                        PrintFont = new Font("Times New Roman", 10, FontStyle.Bold);
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

                    }
                    else
                    {
                        Rectangle destination = new Rectangle(10, 10, 80, 80);
                        g.DrawImage(image, destination, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);


                        patName = drPrintData["FIRST_NAME"].ToString().Trim() + " " + drPrintData["MIDDLE_NAME"].ToString().Trim() + " " + drPrintData["LAST_NAME"].ToString().Trim();
                        patName = patName.ToUpper();
                        if (drPrintData["TITLE"].ToString().Trim() != string.Empty)
                        {
                            patName = drPrintData["TITLE"].ToString().Trim() + " " + patName;
                        }
                        if (patName.Length > 26)
                        {
                            patName = patName.Substring(0, 26);
                        }

                        address1 = drPrintData["ADDRESS1"].ToString().Trim();
                        if (address1.Length > 24)
                        {
                            address1 = address1.Substring(0, 24);
                        }

                        address2 = drPrintData["ADDRESS2"].ToString().Trim();
                        if (address2.Length > 24)
                        {
                            address2 = address2.Substring(0, 24);
                        }

                        if (strBarcodeType.Equals("Code3of9"))
                        {
                            // the Asterisk (*) is used to delimit the barcode, and is required as the start and stop charachters by the 3of9 Symbology.
                            g.DrawString("*" + drPrintData["MRNO"].ToString().Trim() + "*", BarcodeFont, br, 110, 40);
                        }
                        else if (strBarcodeType.Equals("Code128b"))
                        {
                            // the Symbol (š) and (œ} is used to delimit the barcode, and is required as the start and stop charachters by the Code128b Symbology.
                            g.DrawString("š" + drPrintData["MRNO"].ToString().Trim() + Convert.ToString((char)CheckSum(drPrintData["MRNO"].ToString().Trim())) + "œ", BarcodeFont, br, 110, 40);
                        }

                        PrintFont = new Font("Times New Roman", 12, FontStyle.Bold);
                        g.DrawString(patName, PrintFont, br, 30, 75);

                        PrintFont = new Font("Times New Roman", 10, FontStyle.Bold);
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
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        //private string QuantityLeft(DataRow dr)
        //{
        //    bool OrderedQtyUnitFound = false;
        //    decimal PerUnitTotalQuantity = 1;
        //    decimal quantityLeft = 0;
        //    string strQuantityLeft = string.Empty;
        //    bool processed = false;

        //    if (dr["QUANTITY_UNIT"] == dr["DISPENSE_QUANTITY_UNIT"])
        //    {
        //        if (dr["QUANTITY"].KIIsNotNullOrEmpty() && dr["DISPENSE_QUANTITY"].KIIsNotNullOrEmpty())
        //        {
        //            quantityLeft = Convert.ToDecimal(dr["QUANTITY"]) - Convert.ToDecimal(dr["DISPENSE_QUANTITY"]);
        //            processed = true;
        //        }
        //    }
        //    else
        //    {
        //        DataTable dtCriteria = new DataTable("Criteria");
        //        dtCriteria.Columns.Add("INV_MAST_SERVICE_ID");
        //        dtCriteria.Rows.Add();
        //        dtCriteria.Rows[0]["INV_MAST_SERVICE_ID"] = Convert.ToString(dr["BRAND_ID"]);

        //        Infologics.Medilogics.CommonShared.BillingMain.MainBillingShared objBillingShared = new Infologics.Medilogics.CommonShared.BillingMain.MainBillingShared();
        //        DataTable dtPhUnitConversion = objBillingShared.FetchPharmacyUnitSales(dtCriteria);

        //        if (dtPhUnitConversion != null && dtPhUnitConversion.Rows.Count > 0)
        //        {
        //            dtPhUnitConversion = SortTableAscending(dtPhUnitConversion, "DISPLAY_ORDER");
        //            foreach (DataRow drPhUnit in dtPhUnitConversion.Rows)
        //            {
        //                if (Convert.ToString(drPhUnit["TO_UNIT_NAME"]).Equals(Convert.ToString(dr["DISPENSE_QUANTITY_UNIT"])))
        //                {
        //                    OrderedQtyUnitFound = true;
        //                }
        //                if (OrderedQtyUnitFound == true)
        //                {
        //                    PerUnitTotalQuantity = PerUnitTotalQuantity * Convert.ToDecimal(drPhUnit["CONVERSION_FACTOR"]);
        //                }
        //                if (Convert.ToString(drPhUnit["TO_UNIT_NAME"]).Equals(Convert.ToString(dr["QUANTITY_UNIT"])))
        //                {
        //                    break;
        //                }
        //            }
        //            if (dr["DISPENSE_QUANTITY"] != DBNull.Value)
        //            {
        //                quantityLeft = (PerUnitTotalQuantity * Convert.ToDecimal(dr["QUANTITY"])) - Convert.ToDecimal(dr["DISPENSE_QUANTITY"]);
        //            }


        //            processed = true;
        //        }
        //    }
        //    if (processed == true)
        //    {
        //        strQuantityLeft = quantityLeft.ToString();
        //    }
        //    return strQuantityLeft;
        //}
        private DataTable SortTableAscending(DataTable dtTable, string FieldName)
        {
            try
            {
                DataTable dt = new DataTable();
                if (dtTable != null && dtTable.Rows.Count > 0 && dtTable.Columns.Contains(FieldName))
                {
                    var query = from c in dtTable.AsEnumerable()
                                orderby c["DISPLAY_ORDER"] ascending
                                // orderby c.Field.(FieldName) ascending//descending
                                select c;
                    dt = query.CopyToDataTable();
                }
                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }
        DataSet dsData = new DataSet();
        private DataTable SelectDataToPrint(DataSet dsPrintData)
        {
            DataTable dtData = new DataTable("PrintData");
            dsData = dsPrintData.Copy();
            if (SelectedServiceType == ServiceType.Pharmacy && SelectedPrintType == PrintType.Prescription)
            {
                if (dsData.Tables.Contains("PH_PAT_DTLS_ORDER"))
                {
                    dtData.Columns.Add("IS_OP", typeof(String));
                    dtData.Columns.Add("PATIENT_NAME", typeof(String));
                    dtData.Columns.Add("MRNO", typeof(String));
                    dtData.Columns.Add("ALLERGY_DRUG_NAME", typeof(String));
                    dtData.Columns.Add("WEIGHT", typeof(String));
                    dtData.Columns.Add("MEDICINE_NAME", typeof(String));
                    dtData.Columns.Add("DOSAGE", typeof(String));
                    dtData.Columns.Add("ROUTE", typeof(String));
                    dtData.Columns.Add("FREQUENCY_VALUE", typeof(String));
                    dtData.Columns.Add("COMMENT", typeof(String));
                    dtData.Columns.Add("BATCHNO", typeof(String));
                    dtData.Columns.Add("EXPIRY_DATE", typeof(String));
                    dtData.Columns.Add("QUANTITY", typeof(String));
                    dtData.Columns.Add("ISINFUSION", typeof(String));
                    dtData.Columns.Add("START_DATE", typeof(String));
                    dtData.Columns.Add("ADMIN_TIME", typeof(String));
                    dtData.Columns.Add("DURATION", typeof(String));
                    dtData.Columns.Add("GENERIC_NAME", typeof(String));
                    dtData.Columns.Add("DILUENT", typeof(String));
                    dtData.Columns.Add("QUANTITY_UNIT", typeof(String));
                    dtData.Columns.Add("INFUSE_OVER", typeof(String));
                    dtData.Columns.Add("RATE", typeof(String));
                    dtData.Columns.Add("ISCONSUMABLE", typeof(String));
                    dtData.Columns.Add("BARCODE", typeof(String));
                    dtData.Columns.Add("REFILL", typeof(String));
                    dtData.Columns.Add("TOTAL_QUANTITY", typeof(String));
                    dtData.Columns.Add("NUMBER_OF_DOSE", typeof(String));
                    dtData.Columns.Add("DOSE_DISPLAY_NAME", typeof(String));
                    dtData.Columns.Add("QUANTITY_DISPLAY_NAME", typeof(String));
                    dtData.Columns.Add("DATE", typeof(String));
                    dtData.Columns.Add("PROVIDER_NAME", typeof(String));
                    dtData.Columns.Add("DELIVERY_QUANTITY", typeof(String));
                    dtData.Columns.Add("IS_ARABIC_NOTE");
                    dtData.Columns.Add("LOCATION");
                    dtData.Columns.Add("FORM");
                    dtData.Columns.Add("VOLUMN");
                    DataRow drPharama = null;
                    foreach (DataRow dr in dsData.Tables["PH_PAT_DTLS_ORDER"].Rows)  // edited sajin
                    {
                        drPharama = dtData.NewRow();
                        drPharama["IS_OP"] = dr["IS_OP"].ToString();
                        drPharama["PATIENT_NAME"] = dr["PATIENT_NAME"].ToString();
                        drPharama["MRNO"] = dr["MRNO"].ToString();
                        drPharama["ALLERGY_DRUG_NAME"] = dr["ALLERGY_DRUG_NAME"].ToString();
                        drPharama["WEIGHT"] = dr["WEIGHT"].ToString();
                        drPharama["MEDICINE_NAME"] = dr["MEDICINE_NAME"].ToString();
                        drPharama["DOSAGE"] = dr["DOSE_FOR_PRESCRIPTION"].ToString();
                        drPharama["ROUTE"] = dr["ROUTE"].ToString();
                        if (dr.Table.Columns.Contains("IS_ARABIC_NOTE") && dr["IS_ARABIC_NOTE"] != DBNull.Value)
                        {
                            drPharama["IS_ARABIC_NOTE"] = dr["IS_ARABIC_NOTE"];
                        }
                        if (dr.Table.Columns.Contains("VOLUMN") && dr["VOLUMN"] != DBNull.Value)
                        {
                            drPharama["VOLUMN"] = dr["VOLUMN"];
                        }
                        if (dr.Table.Columns.Contains("FORM") && dr["FORM"] != DBNull.Value)
                        {
                            drPharama["FORM"] = dr["FORM"];
                        }
                        if (dr.Table.Columns.Contains("PROVIDER_NAME") && dr["PROVIDER_NAME"] != DBNull.Value)
                        {
                            drPharama["PROVIDER_NAME"] = dr["PROVIDER_NAME"];
                        }
                        if (dr.Table.Columns.Contains("ENTERED_BY_NAME") && dr["ENTERED_BY_NAME"] != DBNull.Value)
                        {
                            drPharama["PROVIDER_NAME"] = dr["ENTERED_BY_NAME"];
                        }
                        if (dr.Table.Columns.Contains("LOCATION") && dr["LOCATION"] != DBNull.Value)
                        {
                            drPharama["LOCATION"] = dr["LOCATION"];
                        }
                        if (dr.Table.Columns.Contains("DELIVERY_QUANTITY") && dr["DELIVERY_QUANTITY"] != DBNull.Value)
                        {
                            drPharama["DELIVERY_QUANTITY"] = dr["DELIVERY_QUANTITY"];
                        }
                        if (Convert.ToInt32(dr["ISINFUSION"]) == 1)
                        {
                            drPharama["FREQUENCY_VALUE"] = dr["FREQUENCY_NAME"].ToString();
                        }
                        else
                        {
                            if (Convert.ToInt32(dr["IS_OP"]) == 1 && dr["FREQUENCY_DESC"] != DBNull.Value)
                            {
                                drPharama["FREQUENCY_VALUE"] = dr["FREQUENCY_DESC"].ToString();
                            }
                            else if (Convert.ToInt32(dr["IS_OP"]) == 1 && dr.Table.Columns.Contains("FREQUENCY_FREETEXT") && dr["FREQUENCY_FREETEXT"] != DBNull.Value)
                            {
                                drPharama["FREQUENCY_VALUE"] = dr["FREQUENCY_FREETEXT"].ToString();
                            }
                            else if (dr.Table.Columns.Contains("FREQUENCY_NAME") && dr["FREQUENCY_NAME"] != DBNull.Value)
                            {
                                if (dr.Table.Columns.Contains("FREQUENCY_DESC") && dr["FREQUENCY_DESC"] != DBNull.Value)
                                {
                                    drPharama["FREQUENCY_VALUE"] = dr["FREQUENCY_DESC"].ToString();
                                }
                                else
                                {
                                    drPharama["FREQUENCY_VALUE"] = dr["FREQUENCY_NAME"].ToString();
                                }
                            }
                        }
                        if (dr.Table.Columns.Contains("FREQUENCY_FREETEXT") && dr["FREQUENCY_FREETEXT"] != DBNull.Value
                            && dr["FREQUENCY_NAME"] == DBNull.Value && dr["FREQUENCY_DESC"] == DBNull.Value && Convert.ToInt32(dr["ISINFUSION"]) != 1)
                        {
                            drPharama["FREQUENCY_VALUE"] = dr["FREQUENCY_FREETEXT"].ToString();
                        }
                        //if (dr.Table.Columns.Contains("ADMINISTRATION_INSTRUCTION")) //dr.Table.Columns.Contains("PHARMACY_NOTE") && 
                        //{
                        //    // string strPharmacyNote = dr["PHARMACY_NOTE"].ToString().Trim();
                        //    //string strAdminInstruction = dr["ADMINISTRATION_INSTRUCTION"].ToString().Trim() != string.Empty ? (dr["ADMINISTRATION_INSTRUCTION"].ToString().Trim() + ".") : string.Empty;
                        //    string strRemarks = dr["PHARMACY_NOTE"].ToString().Trim() != string.Empty ? (dr["PHARMACY_NOTE"].ToString().Trim() + ".") : string.Empty;
                        //    //string strComment = strPharmacyNote != string.Empty ? (strPharmacyNote + "." + "\n" + strAdminInstruction + strRemarks) : (strAdminInstruction + strRemarks);
                        //    string strComment = (strRemarks);

                        //    drPharama["COMMENT"] = strComment;
                        //}
                        //else
                        //{
                        //    drPharama["COMMENT"] = dr["REMARKS"].ToString().Trim();
                        //}
                        if (Convert.ToInt16(drPharama["IS_ARABIC_NOTE"] != DBNull.Value ? drPharama["IS_ARABIC_NOTE"] : 0) == 1)
                        {
                            DataTable dt = dsData.Tables["PH_PAT_DTLS_ORDER"];
                            drPharama["COMMENT"] = dt.Rows[0]["PHARMACY_ARABIC_NOTE"];
                        }
                        else
                        {
                            drPharama["COMMENT"] = dr["PHARMACY_NOTE"];
                        }
                        drPharama["BATCHNO"] = dr["BATCHNO"].ToString();
                        if (dr["EXP_DATE"] != DBNull.Value)
                        {
                            //Int32 ts = Convert.ToDateTime(dr["EXP_DATE"]).Date.Hour;
                            //if (ts == 0)
                            //{
                            //    drPharama["EXPIRY_DATE"] = Convert.ToDateTime(dr["EXP_DATE"]).ToString("dd-MMM-yyyy") + " 23:59";
                            //}
                            //else
                            //{
                            //    drPharama["EXPIRY_DATE"] = Convert.ToDateTime(dr["EXP_DATE"]).ToString("dd-MMM-yyyy HH:mm");
                            //}
                            drPharama["EXPIRY_DATE"] = Convert.ToDateTime(dr["EXP_DATE"]).ToString("dd-MMM-yyyy");
                        }

                        drPharama["DATE"] = Convert.ToDateTime(dr["START_DATE"]).ToString("dd-MMM-yyyy");

                        drPharama["QUANTITY"] = Convert.ToString(dr["QUANTITY"]).Trim().StartsWith(".") ? ("0" + Convert.ToString(dr["QUANTITY"])) : dr["QUANTITY"].ToString();
                        drPharama["QUANTITY_UNIT"] = dr["QUANTITY_UNIT"].ToString();
                        drPharama["ISINFUSION"] = dr["ISINFUSION"].ToString();
                        drPharama["START_DATE"] = Convert.ToDateTime(dr["START_DATE"]).ToString("dd-MMM-yyyy HH:mm");
                        drPharama["ADMIN_TIME"] = dr["ADMIN_TIME"].ToString();
                        drPharama["DURATION"] = dr["DURATION"].ToString();
                        drPharama["GENERIC_NAME"] = dr["GENERIC_NAME"].ToString();
                        drPharama["DILUENT"] = dr["FLUID_NAME"].ToString();
                        drPharama["QUANTITY_UNIT"] = dr["QUANTITY_UNIT"].ToString();
                        drPharama["INFUSE_OVER"] = dr["DURATION_DATA"].ToString();
                        drPharama["RATE"] = dr["RATE"].ToString();
                        drPharama["ISCONSUMABLE"] = dr["ISCONSUMABLE"].ToString();
                        drPharama["BARCODE"] = dr["BARCODE"].ToString();
                        drPharama["REFILL"] = dr["REFILL"].ToString();
                        drPharama["TOTAL_QUANTITY"] = dr["TOTAL_QUANTITY"].ToString();
                        drPharama["NUMBER_OF_DOSE"] = Convert.ToInt64(dr["NUMBER_OF_DOSE"]) == 0 ? "" : dr["NUMBER_OF_DOSE"];
                        if (dsPrintData.Tables.Contains("CPOE_MEDICINE_DISPLAY_NAME"))
                        {
                            drPharama["DOSE_DISPLAY_NAME"] = dsPrintData.Tables["CPOE_MEDICINE_DISPLAY_NAME"].Rows[0]["DOSE_DISPLAY_NAME"].ToString();
                            drPharama["QUANTITY_DISPLAY_NAME"] = dsPrintData.Tables["CPOE_MEDICINE_DISPLAY_NAME"].Rows[0]["QUANTITY_DISPLAY_NAME"].ToString();
                        }
                        dtData.Rows.Add(drPharama);
                    }
                }
            }
            else if (SelectedServiceType == ServiceType.Bloodbank && SelectedPrintType == PrintType.Bloodbank) //Blood Bank Label Print
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
            else if (SelectedServiceType == ServiceType.Common && SelectedPrintType == PrintType.PatientSlip)
            {
                if (dsData.Tables["PATIENT_SLIP_DATA"] != null)
                {
                    //Create table to print                
                    dtData.Columns.Add("PATIENT_NAME", typeof(String));
                    dtData.Columns.Add("MRNO", typeof(String));
                    dtData.Columns.Add("AGE", typeof(String));
                    dtData.Columns.Add("DOB", typeof(String));
                    dtData.Columns.Add("GENDER", typeof(String));
                    dtData.Columns.Add("BARCODE_COUNT", typeof(Decimal));
                    dtData.Columns.Add("IDENTIFYING_DOCUMENT", typeof(Decimal));
                    dtData.Columns.Add("DOCUMENT_NO", typeof(String));
                    dtData.Columns.Add("NATIONALITY", typeof(String));
                    dtData.Columns.Add("MOBILEPHONE", typeof(String));
                    dtData.Columns.Add("HOMEPHONE", typeof(String));
                    //dtData.Columns.Add("VISIT_NO", typeof(Decimal));
                    dtData.Columns.Add("START_DATE", typeof(String));
                    dtData.Columns.Add("INSURANCE", typeof(String));
                    dtData.Columns.Add("SITE", typeof(String));
                    dtData.Columns.Add("CPR", typeof(String));
                    dtData.Columns.Add("REGISTERED_SINCE", typeof(String));
                    dtData.Columns.Add("VISIT_DATE", typeof(String));
                    dtData.Columns.Add("ENCOUNTER_NO", typeof(String));

                    //Add necessary data from dataset to datatable to print
                    foreach (DataRow dr in dsData.Tables["PATIENT_SLIP_DATA"].Rows)
                    {
                        DataRow drNew = dtData.NewRow();
                        drNew["PATIENT_NAME"] = dr["PATIENT_NAME"].ToString();
                        drNew["MRNO"] = dr["MRNO"].ToString();
                        drNew["AGE"] = dr["AGE"].ToString();//Change the Age Format
                        drNew["DOB"] = Convert.ToDateTime(dr["DOB"]).ToString("dd-MM-yyyy");
                        if (dsData.Tables["PATIENT_SLIP_DATA"].Columns.Contains("INSURANCE")) drNew["INSURANCE"] = dr["INSURANCE"];
                        if (dr["GENDER"].ToString().ToUpper() == "MALE") drNew["GENDER"] = "M";
                        else if (dr["GENDER"].ToString().ToUpper() == "FEMALE") drNew["GENDER"] = "F";
                        else drNew["GENDER"] = "Unknown";
                        drNew["BARCODE_COUNT"] = Convert.ToDecimal(dr["BARCODE_COUNT"].ToString());
                        if (dr["IDENTIFYING_DOCUMENT"] != DBNull.Value)
                        {
                            drNew["IDENTIFYING_DOCUMENT"] = Convert.ToDecimal(dr["IDENTIFYING_DOCUMENT"].ToString());
                            drNew["DOCUMENT_NO"] = dr["DOCUMENT_NO"].ToString();
                        }
                        if (dsData.Tables.Contains("PAT_MAST_PATIENT") && dsData.Tables["PAT_MAST_PATIENT"].KIIsNotNullAndRowCount())
                        {
                            drNew["REGISTERED_SINCE"] = Convert.ToDateTime(dsData.Tables["PAT_MAST_PATIENT"].Rows[0]["REGISTERED_SINCE"]).ToString("dd-MMM-yyyy");
                        }
                        if (dsData.Tables["PATIENT_SLIP_DATA"].Columns.Contains("VISIT_DATE")) drNew["VISIT_DATE"] = dr["VISIT_DATE"];
                        if (dsData.Tables["PATIENT_SLIP_DATA"].Columns.Contains("ENCOUNTER_NO")) drNew["ENCOUNTER_NO"] = dr["ENCOUNTER_NO"];
                        //drNew["MOBILEPHONE"] = dr["MOBILEPHONE"].ToString();
                        //drNew["HOMEPHONE"] = dr["HOMEPHONE"].ToString();
                        //if (dr["VISIT_NO"] != DBNull.Value)
                        //    drNew["VISIT_NO"] = dr["VISIT_NO"].ToString();
                        //else
                        //    drNew["VISIT_NO"] = DBNull.Value;
                        //drNew["START_DATE"] = dr["START_DATE"].ToString();
                        //drNew["INSURANCE"] = dr["INSURANCE"].ToString();
                        //drNew["SITE"] = dr["SITE"].ToString();
                        //drNew["CPR"] = dr["CPR"].ToString();
                        //drNew["NATIONALITY"] = dr["NATIONALITY"].ToString();
                        dtData.Rows.Add(drNew);
                    }
                    ////
                }
            }
            else if (SelectedServiceType == ServiceType.Common && SelectedPrintType == PrintType.PatientBand)
            {
                if (dsData.Tables["PATIENT_BAND_DATA"] != null)
                {
                    dtData.Columns.Add("GENDER", typeof(String));
                    dtData.Columns.Add("PATIENT_TYPE", typeof(String));
                    dtData.Columns.Add("MRNO", typeof(String));
                    dtData.Columns.Add("PATIENT_NAME", typeof(String));
                    dtData.Columns.Add("DOB", typeof(String));
                    dtData.Columns.Add("AGE", typeof(String));
                    dtData.Columns.Add("MOTHER_MRNO", typeof(String));
                    //Add necessary data from dataset to datatable to print
                    foreach (DataRow dr in dsData.Tables["PATIENT_BAND_DATA"].Rows)
                    {
                        DataRow drNew = dtData.NewRow();
                        drNew["GENDER"] = dr["GENDER"];
                        drNew["PATIENT_TYPE"] = dr["PATIENT_TYPE"];
                        drNew["MRNO"] = dr["MRNO"];
                        drNew["PATIENT_NAME"] = dr["PATIENT_NAME"];
                        drNew["DOB"] = dr["DOB"];
                        drNew["AGE"] = dr["AGE"];
                        if (dr.Table.Columns.Contains("MOTHER_MRNO") && dr["MOTHER_MRNO"] != DBNull.Value)
                        {
                            drNew["MOTHER_MRNO"] = dr["MOTHER_MRNO"];
                        }
                        else
                        {
                            drNew["MOTHER_MRNO"] = DBNull.Value;
                        }
                        dtData.Rows.Add(drNew);
                    }
                }
            }
            else if (SelectedServiceType == ServiceType.Common && SelectedPrintType == PrintType.ERPCode)
            {
                if (dsData.Tables["BARCODE_PRINT"] != null)
                {
                    dtData.Columns.Add("ERP_CODE", typeof(String));
                    dtData.Columns.Add("ITEM_NAME", typeof(String));
                    dtData.Columns.Add("BARCODE_COUNT", typeof(String));
                    dtData.Columns.Add("EXP_DATE", typeof(String));
                    dtData.Columns.Add("UOM", typeof(String));
                    dtData.Columns.Add("PRICE", typeof(String));
                    //Add necessary data from dataset to datatable to print
                    foreach (DataRow dr in dsData.Tables["BARCODE_PRINT"].Rows)
                    {
                        DataRow drNew = dtData.NewRow();
                        drNew["ERP_CODE"] = dr["BARCODE"];
                        drNew["ITEM_NAME"] = dr["ITEM_NAME"];
                        drNew["EXP_DATE"] = dr["EXPDATE"] != DBNull.Value ? Convert.ToDateTime(dr["EXPDATE"]).ToString("dd-MM-yyyy") : null;
                        drNew["BARCODE_COUNT"] = dr["PRINT_COUNT"];
                        drNew["UOM"] = dr["UOM"];
                        drNew["PRICE"] = dr["PRICE"];
                        dtData.Rows.Add(drNew);
                    }
                }
            }
            else if (SelectedServiceType == ServiceType.Common && SelectedPrintType == PrintType.CPOEAdminPatientSlip)
            {
                if (dsData.Tables["PATIENT_SLIP_DATA"] != null)
                {
                    //Create table to print                
                    dtData.Columns.Add("PATIENT_NAME", typeof(String));
                    dtData.Columns.Add("MRNO", typeof(String));
                    dtData.Columns.Add("AGE", typeof(String));
                    dtData.Columns.Add("DOB", typeof(String));
                    dtData.Columns.Add("GENDER", typeof(String));
                    dtData.Columns.Add("BARCODE_COUNT", typeof(Decimal));
                    //Add necessary data from dataset to datatable to print
                    foreach (DataRow dr in dsData.Tables["PATIENT_SLIP_DATA"].Rows)
                    {
                        DataRow drNew = dtData.NewRow();
                        drNew["PATIENT_NAME"] = dr["PATIENT_NAME"].ToString();
                        drNew["MRNO"] = dr["MRNO"].ToString();
                        drNew["AGE"] = dr["AGE"].ToString();
                        drNew["DOB"] = dr["DOB"].ToString();
                        drNew["GENDER"] = dr["GENDER"].ToString();
                        drNew["BARCODE_COUNT"] = Convert.ToDecimal(dr["BARCODE_COUNT"].ToString());
                        dtData.Rows.Add(drNew);
                    }
                    ////
                }
            }
            else if (SelectedServiceType == ServiceType.Cafeteria && SelectedPrintType == PrintType.Invoice)
            {
                if (dsData.Tables["CAFETERIA_BILL_DETAIL"] != null && dsData.Tables["CAFETERIA_BILL_DETAIL"].Rows.Count > 0)
                {
                    dtData.Columns.Add("MRNO");
                    dtData.Columns.Add("BILLED_BY");
                    dtData.Columns.Add("CURR_TYPE");
                    dtData.Columns.Add("NAME");
                    dtData.Columns.Add("RECEIPT_DATE");
                    dtData.Columns.Add("BILL_NO");
                    DataRow drNew = dtData.NewRow();
                    drNew["MRNO"] = dsData.Tables["CAFETERIA_BILL_DETAIL"].Rows[0]["MRNO"];
                    drNew["NAME"] = dsData.Tables["CAFETERIA_BILL_DETAIL"].Rows[0]["NAME"];
                    drNew["BILL_NO"] = dsData.Tables["CAFETERIA_BILL_DETAIL"].Rows[0]["BILL_NO"];
                    drNew["RECEIPT_DATE"] = dsData.Tables["CAFETERIA_BILL_DETAIL"].Rows[0]["RECEIPT_DATE"];
                    drNew["BILLED_BY"] = dsData.Tables["CAFETERIA_BILL_DETAIL"].Rows[0]["BILLED_BY"];
                    drNew["CURR_TYPE"] = dsData.Tables["CAFETERIA_BILL_DETAIL"].Rows[0]["CURR_TYPE"];
                    dtData.Rows.Add(drNew);
                }
            }
            else if (SelectedServiceType == ServiceType.CSSD)
            {
                if (dsData.Tables.Contains("PRINT_DATA") && dsData.Tables["PRINT_DATA"] != null && dsData.Tables["PRINT_DATA"].Rows.Count > 0)
                {
                    dtData = dsData.Tables["PRINT_DATA"].Copy();
                    dtData.TableName = "PrintData";
                }
            }
            else
            {
                DataTable dtTemp = new DataTable();
                DataRow[] drArray;
                if (dsData.Tables["PAT_PATIENT_NAME"] != null
                    && dsData.Tables["REG_PATIENT_REGISTRATION"] != null
                    && dsData.Tables["PAT_PATIENT_NAME"].Rows.Count > 0 && dsData.Tables["REG_PATIENT_REGISTRATION"].Rows.Count > 0)
                {
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
                    dtData.Columns.Add("PHOTO_PATH", System.Type.GetType("System.String"));
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
                            drData["ADDRESS1"] = dtTemp.Rows[0]["ADDRESS1"].ToString();
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
                        drData["REG_DATE"] = dtTemp.Rows[0]["REGISTRATION_DATE"].ToString();

                        //PhotoPath
                        drArray = dsData.Tables["PAT_MAST_PATIENT"].Select("MRNO='" + dr["MRNO"].ToString() + "'");
                        dtTemp = dsData.Tables["PAT_MAST_PATIENT"].Clone();
                        drArray.CopyToDataTable(dtTemp, LoadOption.OverwriteChanges);
                        drData["PHOTO_PATH"] = dtTemp.Rows[0]["PHOTO"].ToString();

                        //drArray = dsData.Tables["PAT_MAST_PATIENT"].Select("MRNO='" + dr["MRNO"].ToString() + "'");
                        //dtTemp = dsData.Tables["PAT_MAST_PATIENT"].Clone();
                        //drArray.CopyToDataTable(dtTemp, LoadOption.OverwriteChanges);
                        //drData["REG_DATE"] = dtTemp.Rows[0]["REGISTERED_SINCE"].ToString();

                        dtData.Rows.Add(drData);
                    }
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
            objPrintDoc.PrintPage -= new PrintPageEventHandler(AlignGraphicsData);
        }

        #endregion

    }
}