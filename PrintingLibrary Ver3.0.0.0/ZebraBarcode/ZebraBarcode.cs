//-----------------------------------------------------------------------
// <copyright file="BLZebraBarcode.cs" company="Kameda Infologics PVT Ltd">
//     Copyright (c) Kameda Infologics Pvt Ltd. All rights reserved.
// </copyright>
// <author>Biju S J</author>
//<Date>20-Dec-2010<Date>
//-----------------------------------------------------------------------


namespace Infologics.Medilogics.PrintingLibrary.ZebraBarcode
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Infologics.Medilogics.PrintingLibrary.Main;
    using System.Data;
    using System.Drawing.Printing;
    using System.Windows.Forms;
    using Infologics.Medilogics.Enumerators.General;
    using Infologics.Medilogics.General.Control;
    using Infologics.Medilogics.CommonClient.Controls.CommonFunctions;
    using System.Globalization;
    using System.Threading;
    using System.Reflection;
    using System.ComponentModel;
    using System.Drawing;
    //using System.Windows.Media;

    /// <summary>
    /// 
    /// </summary>
    public class ZebraBarcode : IPrinting
    {
        #region IPrinting Members

        public bool Print(System.Data.DataSet dsData, ServiceType serviceType, string PrinterName)
        {
            bool printStatus = false;
            PrintType prntype = PrintType.BarCode; //default

            printStatus = RawPrinting(dsData, serviceType, PrinterName, prntype);
            return printStatus;
        }
        public bool Print(DataSet dsData, ServiceType serviceType, string PrinterName, PrintType printType)
        {
            bool printStatus = false;
            printStatus = RawPrinting(dsData, serviceType, PrinterName, printType);
            return printStatus;
        }

        #endregion
        private static string CR = Convert.ToString((char)0x0D);
        private static string DoubleQuotes = Convert.ToString((char)0x22);
        private long pos = 0;

        /// <summary>
        /// Function to print data in a raw data printer
        /// </summary>
        /// <param name="dsData">Dataset which contains data to print</param>
        /// <param name="serviceType">Type Of Service</param>
        /// <param name="rawPrinterName">Raw Printer Name</param>
        /// <returns></returns>
        private bool RawPrinting(DataSet dsData, ServiceType serviceType, string rawPrinterName, PrintType printType)
        {
            bool printStatus = false;
            string printData = String.Empty;
            string docName = String.Empty;
            int printCount = 0;
            DataTable dtPrintData = new DataTable();
            PrintDialog objPd = new PrintDialog();
            objPd.PrinterSettings = new PrinterSettings();
            if (rawPrinterName == string.Empty)
            {
                objPd.ShowDialog();
            }
            else
            {
                objPd.PrinterSettings.PrinterName = rawPrinterName;
            }

            if (serviceType == ServiceType.Investigation && printType == PrintType.BarCode)
            {
                docName = "Investigation";
                if (dsData != null)
                {
                    dtPrintData = SelectDataToPrint(dsData, serviceType, printType);
                    if (dtPrintData != null)
                    {
                        foreach (DataRow dr in dtPrintData.Rows)
                        {
                            printData = AlignDataToPrint(dr, serviceType, printType);
                            if (printData.Length > 0)
                            {
                                printCount = Convert.ToInt32(dr["BARCODE_COUNT"].ToString());
                                for (int j = 0; j < printCount; j++)
                                {
                                    printStatus = RawPrinterHelper.SendStringToPrinter(objPd.PrinterSettings.PrinterName, printData, docName);
                                    if (printStatus == false)
                                    {
                                        break;
                                    }
                                }
                                if (printStatus == false)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else if (serviceType == ServiceType.Consultation && printType == PrintType.BarCode)
            {
                docName = "Consultation";
                if (dsData != null)
                {
                    dtPrintData = SelectDataToPrint(dsData, serviceType, printType);
                    if (dtPrintData != null)
                    {
                        foreach (DataRow dr in dtPrintData.Rows)
                        {
                            printData = AlignDataToPrint(dr, serviceType, printType);
                            if (printData.Length > 0)
                            {
                                printCount = Convert.ToInt32(dr["BARCODE_COUNT"].ToString());
                                for (int j = 0; j < printCount; j++)
                                {
                                    printStatus = RawPrinterHelper.SendStringToPrinter(objPd.PrinterSettings.PrinterName, printData, docName);
                                    if (printStatus == false)
                                    {
                                        break;
                                    }
                                }
                                if (printStatus == false)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else if (serviceType == ServiceType.Common && printType == PrintType.PatientSlip)
            {
                docName = "PatientBarcode";
                if (dsData != null)
                {
                    dtPrintData = SelectDataToPrint(dsData, serviceType, printType);
                    if (dtPrintData != null)
                    {
                        foreach (DataRow dr in dtPrintData.Rows)
                        {
                            printData = AlignDataToPrint(dr, serviceType, printType);
                            if (printData.Length > 0)
                            {
                                printCount = Convert.ToInt32(dr["BARCODE_COUNT"].ToString());
                                for (int j = 0; j < printCount; j++)
                                {
                                    printStatus = RawPrinterHelper.SendStringToPrinter(objPd.PrinterSettings.PrinterName, printData, docName);
                                    if (printStatus == false)
                                    {
                                        break;
                                    }
                                }
                                if (printStatus == false)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else if (serviceType == ServiceType.Pharmacy && printType == PrintType.Prescription)
            {
                docName = "Prescription";
                if (dsData != null)
                {
                    //dtPrintData = SelectDataToPrint(dsData, serviceType, printType);
                    dtPrintData = dsData.Tables["PH_PAT_DTLS_ORDER"].Copy();
                    if (dtPrintData != null)
                    {
                        DataColumn dcBarCode = new DataColumn("BARCODE_COUNT", typeof(Int32));
                        dcBarCode.DefaultValue = 1;
                        dtPrintData.Columns.Add(dcBarCode);
                       // SetData(dsData);
                        foreach (DataRow dr in dtPrintData.Rows)
                        {
                            
                            printData = AlignDataToPrint(dr, serviceType, printType);
                            if (printData.Length > 0)
                            {
                                printCount = Convert.ToInt32(dr["BARCODE_COUNT"].ToString());
                                for (int j = 0; j < printCount; j++)
                                {
                                    printStatus = RawPrinterHelper.SendStringToPrinter(objPd.PrinterSettings.PrinterName, printData, docName);
                                    if (printStatus == false)
                                    {
                                        break;
                                    }
                                }
                                if (printStatus == false)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else if (serviceType == ServiceType.Common && printType == PrintType.CPOEAdminPatientSlip)
            {
                docName = "Patient Slip";
                if (dsData != null)
                {
                    dtPrintData = SelectDataToPrint(dsData, serviceType, printType);
                    if (dtPrintData != null)
                    {
                        foreach (DataRow dr in dtPrintData.Rows)
                        {
                            printData = AlignDataToPrint(dr, serviceType, printType);
                            if (printData.Length > 0)
                            {
                                printCount = Convert.ToInt32(dr["BARCODE_COUNT"].ToString());
                                for (int j = 0; j < printCount; j++)
                                {
                                    printStatus = RawPrinterHelper.SendStringToPrinter(objPd.PrinterSettings.PrinterName, printData, docName);
                                    if (printStatus == false)
                                    {
                                        break;
                                    }
                                }
                                if (printStatus == false)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else if (serviceType == ServiceType.Common && printType == PrintType.PatientBand)
            {
                docName = "Patient Band";
                if (dsData != null)
                {
                    dtPrintData = SelectDataToPrint(dsData, serviceType, printType);
                    if (dtPrintData != null)
                    {
                        foreach (DataRow dr in dtPrintData.Rows)
                        {
                            printData = AlignDataToPrint(dr, serviceType, printType);
                            if (printData.Length > 0)
                            {
                                printCount = Convert.ToInt32(dr["BARCODE_COUNT"].ToString());
                                for (int j = 0; j < printCount; j++)
                                {
                                    printStatus = RawPrinterHelper.SendStringToPrinter(objPd.PrinterSettings.PrinterName, printData, docName);
                                    if (printStatus == false)
                                    {
                                        break;
                                    }
                                }
                                if (printStatus == false)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            else if (serviceType == ServiceType.CSSD && printType == PrintType.BarCode)
            {
                docName = "CSSD";
                if (dsData != null)
                {
                    dtPrintData = SelectDataToPrint(dsData, serviceType, printType);
                    if (dtPrintData != null)
                    {
                        foreach (DataRow dr in dtPrintData.Rows)
                        {
                            printData = AlignDataToPrint(dr, serviceType, printType);
                            if (printData.Length > 0)
                            {
                                printCount = Convert.ToInt32(dr["BARCODE_COUNT"].ToString());
                                for (int j = 0; j < printCount; j++)
                                {
                                    printStatus = RawPrinterHelper.SendStringToPrinter(objPd.PrinterSettings.PrinterName, printData, docName);
                                    if (printStatus == false)
                                    {
                                        break;
                                    }
                                }
                                if (printStatus == false)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return printStatus;
        }

        private void SetData( DataSet dsData)
        {
            if (dsData.Tables.Contains("PH_PAT_DTLS_ORDER") && dsData.Tables["PH_PAT_DTLS_ORDER"] != null && dsData.Tables["PH_PAT_DTLS_ORDER"].Rows.Count > 0)
            {
                if (!dsData.Tables["PH_PAT_DTLS_ORDER"].Columns.Contains("BATCHNO"))
                {
                    dsData.Tables["PH_PAT_DTLS_ORDER"].Columns.Add("BATCHNO");
                    if (!dsData.Tables["PH_PAT_DTLS_ORDER"].Columns.Contains("EXP_DATE"))
                    {
                        dsData.Tables["PH_PAT_DTLS_ORDER"].Columns.Add("EXP_DATE");
                    }

                    
                    if (dsData.Tables.Contains("INV_PAT_BILLING") && dsData.Tables["INV_PAT_BILLING"] != null && dsData.Tables["INV_PAT_BILLING"].Rows.Count > 0)
                    {
                        foreach (DataRow drdtls in dsData.Tables["PH_PAT_DTLS_ORDER"].Rows)
                        {
                            DataRow[] drBatch = dsData.Tables["INV_PAT_BILLING"].Select("EMR_PAT_DTLS_MEDICATION_ID'" +drdtls["EMR_PAT_DTLS_PH_ORDER"]+"'");
                            if (drBatch != null && drBatch.Count() > 0)
                            {
                                drdtls["BATCHNO"] = drBatch[0]["BATCHNO"];
                                drdtls["EXP_DATE"] = drBatch[0]["EXP_DATE"];

                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Select necessary data to print from dataset and copy to datatable
        /// </summary>
        /// <param name="dsData">Dataset from which data required to print is to select</param>
        /// <returns>Datatable which contains only necessary data to print</returns>
        private DataTable SelectDataToPrint(DataSet dsData, ServiceType serviceType, PrintType printType)
        {
            DataTable dtPrintData = new DataTable("PrintData");
            if (serviceType == ServiceType.Investigation && printType == PrintType.BarCode)
            {
                if (dsData.Tables["LIS_MAST_SAMPLE_COLLECTION"] != null)
                {
                    //Create table to print                
                    dtPrintData.Columns.Add("LIS_SAMPLE_NO", typeof(String));
                    dtPrintData.Columns.Add("MRNO", typeof(String));
                    dtPrintData.Columns.Add("PATIENT_NAME", typeof(String));
                    dtPrintData.Columns.Add("BILL_NO", typeof(String));
                    dtPrintData.Columns.Add("ISSTICKER", typeof(String));
                    dtPrintData.Columns.Add("BARCODE_COUNT", typeof(Decimal));
                    dtPrintData.Columns.Add("SPECIMEN_NAME", typeof(String));
                    dtPrintData.Columns.Add("ISPATIENT", typeof(Decimal));
                    dtPrintData.Columns.Add("GENDER", typeof(String));
                    dtPrintData.Columns.Add("AGE", typeof(String));
                    dtPrintData.Columns.Add("DOB", typeof(String));
                    dtPrintData.Columns.Add("NATIONALITY", typeof(String));
                    dtPrintData.Columns.Add("MOBILEPHONE", typeof(String));
                    dtPrintData.Columns.Add("HOMEPHONE", typeof(String));
                    dtPrintData.Columns.Add("VISIT_NO", typeof(Decimal));
                    dtPrintData.Columns.Add("START_DATE", typeof(String));
                    dtPrintData.Columns.Add("INSURANCE", typeof(String));
                    dtPrintData.Columns.Add("SITE", typeof(String));
                    dtPrintData.Columns.Add("CPR", typeof(String));


                    dtPrintData.Columns.Add("TUBE_COLOUR");
                    dtPrintData.Columns.Add("WITHDRAWAL_DATE");
                    dtPrintData.Columns.Add("WITHDRAWAL_ID");
                    //Add necessary data from dataset to datatable to print
                    
                    foreach (DataRow dr in dsData.Tables["LIS_MAST_SAMPLE_COLLECTION"].Rows)
                    {
                        DataRow drNew = dtPrintData.NewRow();
                        drNew["LIS_SAMPLE_NO"] = dr["LIS_SAMPLE_NO"].ToString();
                        drNew["MRNO"] = dr["MRNO"].ToString();
                        drNew["PATIENT_NAME"] = dr["PATIENT_NAME"].ToString();
                        drNew["BILL_NO"] = dr["BILL_NO"].ToString();
                        drNew["ISSTICKER"] = dr["ISSTICKER"].ToString();
                        drNew["BARCODE_COUNT"] = Convert.ToDecimal(dr["BARCODE_COUNT"].ToString());
                        drNew["SPECIMEN_NAME"] = dr["SPECIMEN_NAME"].ToString();
                        drNew["AGE"] = dr["AGE"].ToString();
                        drNew["GENDER"] = dr["GENDER"].ToString();
                        drNew["ISPATIENT"] = Convert.ToDecimal(dr["ISPATIENT"].ToString());
                        drNew["DOB"] = dr["DOB"].ToString();
                        drNew["NATIONALITY"] = dr["NATIONALITY"].ToString();
                        drNew["MOBILEPHONE"] = dr["MOBILEPHONE"].ToString();
                        drNew["HOMEPHONE"] = dr["HOMEPHONE"].ToString();
                        drNew["VISIT_NO"] = Convert.ToDecimal(dr["VISIT_NO"].ToString());
                        drNew["START_DATE"] = dr["START_DATE"].ToString();
                        drNew["INSURANCE"] = dr["INSURANCE"].ToString();
                        drNew["SITE"] = dr["SITE"].ToString();
                        drNew["CPR"] = dr["CPR"].ToString();

                        if (dsData.Tables["LOGIN_USER"] != null && dsData.Tables["LOGIN_USER"].Rows.Count > 0)
                            drNew["WITHDRAWAL_ID"] = dsData.Tables["LOGIN_USER"].Rows[0]["LOGIN_USER_ID"];
                        if (drNew["ISSTICKER"].ToString()!="1")
                            drNew["WITHDRAWAL_DATE"] = dr["COLLECTION_TIME"].ToString();
                        DataRow[] drArray = dsData.Tables["LIS_DTLS_SAMPLE_CONTAINERS"] != null ? dsData.Tables["LIS_DTLS_SAMPLE_CONTAINERS"].Select("LIS_MAST_SAMPLE_COLLECTION_ID=" + dr["LIS_MAST_SAMPLE_COLLECTION_ID"]) : null;
                        if (drArray != null && drArray.Count() > 0)
                        {
                            //Color color = (Color)ColorConverter.ConvertFromString(Convert.ToString(drArray[0]["CONTAINER_COLOUR"]));
                            Color color = System.Drawing.ColorTranslator.FromHtml(Convert.ToString(drArray[0]["CONTAINER_COLOUR"]));
                            drNew["TUBE_COLOUR"] = color.Name;
                        }                                              
                        dtPrintData.Rows.Add(drNew);
                    }
                    ////
                }
            }
            else if (serviceType == ServiceType.Consultation && printType == PrintType.BarCode)
            {
                if (dsData.Tables["CONSULTATION_TABLE"] != null)
                {
                    //Create table to print                
                    dtPrintData.Columns.Add("TOKEN_NO", typeof(String));
                    dtPrintData.Columns.Add("MRNO", typeof(String));
                    dtPrintData.Columns.Add("NAME", typeof(String));
                    dtPrintData.Columns.Add("PROVIDER_NAME", typeof(String));
                    dtPrintData.Columns.Add("PROVIDER_ID", typeof(String));
                    dtPrintData.Columns.Add("ISAPPOINTMENT", typeof(String));
                    dtPrintData.Columns.Add("ISDOCTOR", typeof(String));
                    dtPrintData.Columns.Add("APPOINTMENT_TIME", typeof(String));
                    dtPrintData.Columns.Add("ROOM_NO", typeof(String));
                    dtPrintData.Columns.Add("BARCODE_COUNT", typeof(Decimal));
                    //Add necessary data from dataset to datatable to print
                    foreach (DataRow dr in dsData.Tables["CONSULTATION_TABLE"].Rows)
                    {
                        DataRow drNew = dtPrintData.NewRow();
                        drNew["TOKEN_NO"] = dr["TOKEN_NO"].ToString();
                        drNew["MRNO"] = dr["MRNO"].ToString();
                        drNew["NAME"] = dr["NAME"].ToString();
                        drNew["PROVIDER_NAME"] = dr["PROVIDER"].ToString();
                        drNew["PROVIDER_ID"] = dr["PROVIDER_ID"].ToString();
                        //drNew["ISAPPOINTMENT"] = dr["ISAPPOINTMENT"].ToString();
                        //drNew["ISDOCTOR"] = dr["ISDOCTOR"].ToString();
                        drNew["APPOINTMENT_TIME"] = dr["APPOINTMENT_TIME"].ToString();
                        drNew["ROOM_NO"] = dr["ROOM_NO"].ToString();
                        drNew["BARCODE_COUNT"] = 1;// Convert.ToDecimal(dr["BARCODE_COUNT"].ToString());
                        dtPrintData.Rows.Add(drNew);
                    }
                    ////
                }
            }
            else if (serviceType == ServiceType.Common && printType == PrintType.PatientSlip)
            {
                if (dsData.Tables["PATIENT_SLIP_DATA"] != null)
                {
                    //Create table to print                
                    dtPrintData.Columns.Add("PATIENT_NAME", typeof(String));
                    dtPrintData.Columns.Add("MRNO", typeof(String));
                    dtPrintData.Columns.Add("AGE", typeof(String));
                    dtPrintData.Columns.Add("DOB", typeof(String));
                    dtPrintData.Columns.Add("GENDER", typeof(String));
                    dtPrintData.Columns.Add("BARCODE_COUNT", typeof(Decimal));
                    dtPrintData.Columns.Add("IDENTIFYING_DOCUMENT", typeof(Decimal));
                    dtPrintData.Columns.Add("DOCUMENT_NO", typeof(String));
                    dtPrintData.Columns.Add("NATIONALITY", typeof(String));
                    dtPrintData.Columns.Add("MOBILEPHONE", typeof(String));
                    dtPrintData.Columns.Add("HOMEPHONE", typeof(String));
                    dtPrintData.Columns.Add("VISIT_NO", typeof(Decimal));
                    dtPrintData.Columns.Add("START_DATE", typeof(String));
                    dtPrintData.Columns.Add("INSURANCE", typeof(String));
                    dtPrintData.Columns.Add("SITE", typeof(String));
                    dtPrintData.Columns.Add("CPR", typeof(String));
                    dtPrintData.Columns.Add("REGISTERED_SINCE", typeof(String));

                    //Add necessary data from dataset to datatable to print
                    foreach (DataRow dr in dsData.Tables["PATIENT_SLIP_DATA"].Rows)
                    {
                        DataRow drNew = dtPrintData.NewRow();
                        drNew["PATIENT_NAME"] = dr["PATIENT_NAME"].ToString();
                        drNew["MRNO"] = dr["MRNO"].ToString();
                        drNew["AGE"] = dr["AGE"].ToString();
                        drNew["DOB"] = Convert.ToDateTime(dr["DOB"]).ToString("dd-MMM-yyyy");
                        drNew["GENDER"] = dr["GENDER"].ToString();
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
                        dtPrintData.Rows.Add(drNew);
                    }
                    ////
                }
            }
            else if (serviceType == ServiceType.Pharmacy && printType == PrintType.Prescription)
            {
                Common objComman = new Common();
                if (dsData.Tables["INV_PAT_BILLING"] != null && dsData.Tables["Detail"] != null)
                {
                    var BrandQuery = from emr in dsData.Tables["Detail"].AsEnumerable().Where(r => r.RowState != DataRowState.Deleted)
                                     join inv in dsData.Tables["INV_PAT_BILLING"].AsEnumerable().Where(r => r.RowState != DataRowState.Deleted)
                                     on emr.Field<Decimal?>("EMR_PAT_DTLS_INV_ORDER_ID") equals
                                     inv.Field<Decimal?>("EMR_PAT_DTLS_MEDICATION_ID")
                                     select new
                                     {
                                         EMR_PAT_DTLS_INV_ORDER_ID = emr.Field<decimal?>("EMR_PAT_DTLS_INV_ORDER_ID"),
                                         MEDICINE_TYPE = emr.Field<decimal?>("MEDICINE_TYPE"),
                                         BRAND_GENERIC = emr.Field<string>("NAME"),
                                         ROUTE = emr.Field<string>("ROUTE"),
                                         FORM = emr.Field<string>("FORM"),
                                         DURATION = emr.Field<decimal?>("DURATION"),
                                         ADMINISTRATION_INSTRUCTION = emr.Field<string>("ADMINISTRATION_INSTRUCTION"),
                                         DURATION_TYPE = emr.Field<decimal?>("DURATION_TYPE"),
                                         QUANTITY = emr.Field<decimal?>("QUANTITY"),
                                         QUANTITY_UNIT = emr.Field<string>("QUANTITY_UNIT"),
                                         //FREQUENCY = emr.Field<decimal?>("FREQUENCY"),
                                         FREQUENCY = emr.Field<string>("FIELD10"),
                                         FREQUENCY_VALUE = emr.Field<decimal?>("FREQUENCY_VALUE"),
                                         REMARKS = emr.Field<string>("REMARKS"),
                                         ADMIN_TIME = emr.Field<string>("ADMIN_TIME"),
                                         START_DATE = emr.Field<string>("START_DATE"),
                                         SPECIAL_INSTRUCTIONS = emr.Field<string>("CONDITIONAL_FREQUENCY"),
                                         ISLIFELONG = emr.Field<decimal?>("ISLIFELONG"),
                                         BRAND_NAME = inv.Field<string>("NAME"),
                                         GEN_PAT_BILLING_ID = inv.Field<decimal?>("GEN_PAT_BILLING_ID")

                                     };
                    dtPrintData = objComman.LINQToDataTable(BrandQuery);

                    //Join to get the dosage .
                    var DosageQuery = from Mast in dtPrintData.AsEnumerable().Where(r => r.RowState != DataRowState.Deleted)
                                      join emr in dsData.Tables["EMR_LOOKUP"].AsEnumerable().Where(r => r.RowState != DataRowState.Deleted)
                                     on Mast.Field<Decimal?>("FREQUENCY") equals
                                     emr.Field<Decimal?>("FIELD10")
                                      //emr.Field<Decimal?>("EMR_LOOKUP_ID")
                                      select new
                                      {
                                          EMR_PAT_DTLS_INV_ORDER_ID = Mast.Field<decimal?>("EMR_PAT_DTLS_INV_ORDER_ID"),
                                          MEDICINE_TYPE = Mast.Field<decimal?>("MEDICINE_TYPE"),
                                          BRAND_GENERIC = Mast.Field<string>("BRAND_GENERIC"),
                                          ROUTE = Mast.Field<string>("ROUTE"),
                                          FORM = Mast.Field<string>("FORM"),
                                          DURATION = Mast.Field<decimal?>("DURATION"),
                                          ADMINISTRATION_INSTRUCTION = Mast.Field<string>("ADMINISTRATION_INSTRUCTION"),
                                          DURATION_TYPE = Mast.Field<decimal?>("DURATION_TYPE"),
                                          QUANTITY = Mast.Field<decimal?>("QUANTITY"),
                                          QUANTITY_UNIT = Mast.Field<string>("QUANTITY_UNIT"),
                                          FREQUENCY = emr.Field<string>("FIELD10"),
                                          FREQUENCY_VALUE = Mast.Field<decimal?>("FREQUENCY_VALUE"),
                                          REMARKS = Mast.Field<string>("REMARKS"),
                                          ADMIN_TIME = Mast.Field<string>("ADMIN_TIME"),
                                          START_DATE = Mast.Field<string>("START_DATE"),
                                          SPECIAL_INSTRUCTIONS = Mast.Field<string>("SPECIAL_INSTRUCTIONS"),
                                          ISLIFELONG = Mast.Field<decimal?>("ISLIFELONG"),
                                          BRAND_NAME = Mast.Field<string>("BRAND_NAME"),
                                          DOSE_VALUE = emr.Field<string>("LOOKUP_VALUE"),
                                          DOSE_TYPE = emr.Field<string>("FIELD2"),
                                          DOSE_PRN = emr.Field<string>("FIELD5"),
                                          GEN_PAT_BILLING_ID = Mast.Field<decimal?>("GEN_PAT_BILLING_ID")
                                      };
                    dtPrintData = objComman.LINQToDataTable(DosageQuery);

                    //Join to get the duration type from enum.

                    CommonFunctions objCommon = new CommonFunctions();
                    DataTable dtDurationType = objCommon.EnumToDataTable(typeof(Enumerators.EMR.DurationType));

                    var DurationQuery = from Mast in dtPrintData.AsEnumerable().Where(r => r.RowState != DataRowState.Deleted)
                                        join dur in dtDurationType.AsEnumerable().Where(r => r.RowState != DataRowState.Deleted)
                                        on Convert.ToString(Mast["DURATION_TYPE"]) equals
                                        Convert.ToString(dur["KEY"]) into joinedPatBilling
                                        from dur in joinedPatBilling.DefaultIfEmpty()
                                        select new
                                        {
                                            EMR_PAT_DTLS_INV_ORDER_ID = Mast.Field<decimal?>("EMR_PAT_DTLS_INV_ORDER_ID"),
                                            MEDICINE_TYPE = Mast.Field<decimal?>("MEDICINE_TYPE"),
                                            BRAND_GENERIC = Mast.Field<string>("BRAND_GENERIC"),
                                            ROUTE = Mast.Field<string>("ROUTE"),
                                            FORM = Mast.Field<string>("FORM"),
                                            DURATION = Mast.Field<decimal?>("DURATION"),
                                            ADMINISTRATION_INSTRUCTION = Mast.Field<string>("ADMINISTRATION_INSTRUCTION"),
                                            DURATION_TYPE = Mast.Field<decimal?>("DURATION_TYPE"),
                                            QUANTITY = Mast.Field<decimal?>("QUANTITY"),
                                            QUANTITY_UNIT = Mast.Field<string>("QUANTITY_UNIT"),
                                            FREQUENCY = Mast.Field<string>("FREQUENCY"),
                                            FREQUENCY_VALUE = Mast.Field<decimal?>("FREQUENCY_VALUE"),
                                            REMARKS = Mast.Field<string>("REMARKS"),
                                            ADMIN_TIME = Mast.Field<string>("ADMIN_TIME"),
                                            START_DATE = Mast.Field<string>("START_DATE"),
                                            SPECIAL_INSTRUCTIONS = Mast.Field<string>("SPECIAL_INSTRUCTIONS"),
                                            ISLIFELONG = Mast.Field<decimal?>("ISLIFELONG"),
                                            BRAND_NAME = Mast.Field<string>("BRAND_NAME"),
                                            DOSE_VALUE = Mast.Field<string>("DOSE_VALUE"),
                                            DOSE_TYPE = Mast.Field<string>("DOSE_TYPE"),
                                            DOSE_PRN = Mast.Field<string>("DOSE_PRN"),
                                            DURATION_TYPE_VALUE = dur != null ? dur.Field<string>("VALUE") : null,
                                            GEN_PAT_BILLING_ID = Mast.Field<decimal?>("GEN_PAT_BILLING_ID")
                                        };
                    dtPrintData = objComman.LINQToDataTable(DurationQuery);
                    if (!dtPrintData.Columns.Contains("BILL_NO"))
                    {
                        dtPrintData.Columns.Add("BILL_NO", typeof(string));
                    }
                    if (!dtPrintData.Columns.Contains("BILL_DATE"))
                    {
                        dtPrintData.Columns.Add("BILL_DATE", typeof(string));
                    }
                    if (!dtPrintData.Columns.Contains("MRNO"))
                    {
                        dtPrintData.Columns.Add("MRNO", typeof(string));
                    }
                    if (!dtPrintData.Columns.Contains("AGE"))
                    {
                        dtPrintData.Columns.Add("AGE", typeof(string));
                    }
                    if (!dtPrintData.Columns.Contains("FIRST_NAME"))
                    {
                        dtPrintData.Columns.Add("FIRST_NAME", typeof(string));
                    }
                    if (!dtPrintData.Columns.Contains("MIDDLE_NAME"))
                    {
                        dtPrintData.Columns.Add("MIDDLE_NAME", typeof(string));
                    }
                    if (!dtPrintData.Columns.Contains("LAST_NAME"))
                    {
                        dtPrintData.Columns.Add("LAST_NAME", typeof(string));
                    }
                    if (!dtPrintData.Columns.Contains("GENDER"))
                    {
                        dtPrintData.Columns.Add("GENDER", typeof(string));
                    }
                    if (!dtPrintData.Columns.Contains("PROVIDER"))
                    {
                        dtPrintData.Columns.Add("PROVIDER", typeof(string));
                    }
                    if (!dtPrintData.Columns.Contains("DEPARTMENT"))
                    {
                        dtPrintData.Columns.Add("DEPARTMENT", typeof(string));
                    }
                    foreach (DataRow drRow in dtPrintData.Rows)
                    {
                        drRow["GEN_PAT_BILLING_ID"] = dsData.Tables["GEN_PAT_BILLING"].Rows[0]["GEN_PAT_BILLING_ID"];
                        drRow["BILL_DATE"] = dsData.Tables["GEN_PAT_BILLING"].Rows[0]["BILL_DATE"];
                        drRow["BILL_NO"] = dsData.Tables["GEN_PAT_BILLING"].Rows[0]["BILL_NO"];
                        drRow["MRNO"] = dsData.Tables["PAT_PATIENT_NAME"].Rows[0]["MRNO"];
                        drRow["AGE"] = dsData.Tables["PAT_PATIENT_NAME"].Rows[0]["AGE"];
                        drRow["FIRST_NAME"] = dsData.Tables["PAT_PATIENT_NAME"].Rows[0]["FIRST_NAME"];
                        drRow["MIDDLE_NAME"] = dsData.Tables["PAT_PATIENT_NAME"].Rows[0]["MIDDLE_NAME"];
                        drRow["LAST_NAME"] = dsData.Tables["PAT_PATIENT_NAME"].Rows[0]["LAST_NAME"];
                        drRow["GENDER"] = dsData.Tables["PAT_PATIENT_NAME"].Rows[0]["GENDER"];
                        drRow["PROVIDER"] = dsData.Tables["BILL_COMMON_DETAILS"].Rows[0]["PROVIDER_NAME"];
                        drRow["DEPARTMENT"] = dsData.Tables["BILL_COMMON_DETAILS"].Rows[0]["DEPARTMENT_NAME"];
                    }
                }
            }
            else if (serviceType == ServiceType.Common && printType == PrintType.CPOEAdminPatientSlip)
            {
                if (dsData.Tables["PATIENT_SLIP_DATA"] != null)
                {
                    //Create table to print                
                    dtPrintData.Columns.Add("PATIENT_NAME", typeof(String));
                    dtPrintData.Columns.Add("MRNO", typeof(String));
                    dtPrintData.Columns.Add("AGE", typeof(String));
                    dtPrintData.Columns.Add("DOB", typeof(String));
                    dtPrintData.Columns.Add("GENDER", typeof(String));
                    dtPrintData.Columns.Add("BARCODE_COUNT", typeof(Decimal));
                    dtPrintData.Columns.Add("IDENTIFYING_DOCUMENT", typeof(Decimal));
                    dtPrintData.Columns.Add("DOCUMENT_NO", typeof(String));
                    dtPrintData.Columns.Add("NATIONALITY", typeof(String));
                    //dtPrintData.Columns.Add("")
                    //Add necessary data from dataset to datatable to print
                    foreach (DataRow dr in dsData.Tables["PATIENT_SLIP_DATA"].Rows)
                    {
                        DataRow drNew = dtPrintData.NewRow();
                        drNew["PATIENT_NAME"] = dr["PATIENT_NAME"].ToString();
                        drNew["MRNO"] = dr["MRNO"].ToString();
                        drNew["AGE"] = dr["AGE"].ToString();
                        drNew["DOB"] = dr["DOB"].ToString();
                        drNew["GENDER"] = dr["GENDER"].ToString();
                        drNew["BARCODE_COUNT"] = Convert.ToDecimal(dr["BARCODE_COUNT"].ToString());
                        dtPrintData.Rows.Add(drNew);
                    }
                    ////
                }
            }
            else if (serviceType == ServiceType.Common && printType == PrintType.PatientBand)
            {
                if (dsData.Tables.Contains("PATIENT_BAND_DATA"))
                {
                    //Create table to print                
                    dtPrintData.Columns.Add("PATIENT_NAME", typeof(String));
                    dtPrintData.Columns.Add("MRNO", typeof(String));
                    dtPrintData.Columns.Add("DOB", typeof(String));
                    dtPrintData.Columns.Add("AGE", typeof(String));
                    dtPrintData.Columns.Add("GENDER", typeof(String));
                    dtPrintData.Columns.Add("BARCODE_COUNT", typeof(Decimal));
                    //Add necessary data from dataset to datatable to print
                    foreach (DataRow dr in dsData.Tables["PATIENT_BAND_DATA"].Rows)
                    {
                        DataRow drNew = dtPrintData.NewRow();
                        if (dr["PATIENT_NAME"] != DBNull.Value)
                        {
                            drNew["PATIENT_NAME"] = dr["PATIENT_NAME"].ToString();
                        }
                        if (dr["MRNO"] != DBNull.Value)
                        {
                            drNew["MRNO"] = dr["MRNO"].ToString();
                        }
                        if (dr["DOB"] != DBNull.Value)
                        {
                            drNew["DOB"] = Convert.ToDateTime(dr["DOB"]).ToString("dd-MMM-yyyy");
                        }
                        if (dr["AGE"] != DBNull.Value)
                        {
                            drNew["AGE"] = dr["AGE"].ToString();
                        }
                        if (dr["GENDER"] != DBNull.Value)
                        {
                            drNew["GENDER"] = dr["GENDER"].ToString();
                        }
                        drNew["BARCODE_COUNT"] = Convert.ToDecimal(dr["BARCODE_COUNT"].ToString());
                        dtPrintData.Rows.Add(drNew);
                    }
                    ////
                }
            }
            else if (serviceType == ServiceType.CSSD && printType == PrintType.BarCode)
            {
                if (dsData.Tables.Contains("CSSD_INDIVIDUAL_ITEM") && Convert.ToString(dsData.Tables["CSSD_INDIVIDUAL_ITEM"].Rows[0]["BARCODE_TYPE"])== "0")
                {
                    //Create table to print                
                    dtPrintData.Columns.Add("CSSD_ITEM_NAME", typeof(Decimal));
                    dtPrintData.Columns.Add("CSSD_INDIVIDUAL_ITEM_NAME", typeof(String));
                    dtPrintData.Columns.Add("BARCODE_TYPE", typeof(Decimal));
                    dtPrintData.Columns.Add("BARCODE_COUNT", typeof(Decimal));
                    //Add necessary data from dataset to datatable to print
                    foreach (DataRow dr in dsData.Tables["CSSD_MAST_PROCESS"].Rows)
                    {
                        DataRow drNew = dtPrintData.NewRow();
                        if (dr["CSSD_ITEM_NAME"] != DBNull.Value)
                        {
                            drNew["CSSD_ITEM_NAME"] = dr["CSSD_ITEM_NAME"].ToString();
                        }
                        if (dr["CSSD_INDIVIDUAL_ITEM_NAME"] != DBNull.Value)
                        {
                            drNew["CSSD_INDIVIDUAL_ITEM_NAME"] = dr["CSSD_INDIVIDUAL_ITEM_NAME"].ToString();
                        }
                        if (dr["BARCODE_TYPE"] != DBNull.Value)
                        {
                            drNew["BARCODE_TYPE"] = dr["BARCODE_TYPE"].ToString();
                        }
                        drNew["BARCODE_COUNT"] = Convert.ToDecimal(dr["BARCODE_COUNT"].ToString());
                        dtPrintData.Rows.Add(drNew);
                    }
                }
                else if (dsData.Tables.Contains("CSSD_MAST_PROCESS") && Convert.ToString(dsData.Tables["CSSD_MAST_PROCESS"].Rows[0]["BARCODE_TYPE"]) == "1")
                {
                    //Create table to print                
                    dtPrintData.Columns.Add("CSSD_MAST_PROCESS_ID", typeof(Decimal));
                    dtPrintData.Columns.Add("CATEGORY_NAME", typeof(String));
                    dtPrintData.Columns.Add("SET_NAME", typeof(String));
                    dtPrintData.Columns.Add("EXPIRY_DATE", typeof(String));
                    dtPrintData.Columns.Add("BARCODE_TYPE", typeof(Decimal));
                    dtPrintData.Columns.Add("BARCODE_COUNT", typeof(Decimal));
                    //Add necessary data from dataset to datatable to print
                    foreach (DataRow dr in dsData.Tables["CSSD_MAST_PROCESS"].Rows)
                    {
                        DataRow drNew = dtPrintData.NewRow();
                        if (dr["CSSD_MAST_PROCESS_ID"] != DBNull.Value)
                        {
                            drNew["CSSD_MAST_PROCESS_ID"] = dr["CSSD_MAST_PROCESS_ID"].ToString();
                        }
                        if (dr["CATEGORY_NAME"] != DBNull.Value)
                        {
                            drNew["CATEGORY_NAME"] = dr["CATEGORY_NAME"].ToString();
                        }
                        if (dr["SET_NAME"] != DBNull.Value)
                        {
                            drNew["SET_NAME"] = dr["SET_NAME"].ToString();
                        }
                        if (dr["EXPIRY_DATE"] != DBNull.Value)
                        {
                            drNew["EXPIRY_DATE"] = Convert.ToDateTime(dr["EXPIRY_DATE"]).ToString("dd/MM/yyyy");
                        }
                        if (dr["BARCODE_TYPE"] != DBNull.Value)
                        {
                            drNew["BARCODE_TYPE"] = dr["BARCODE_TYPE"].ToString();
                        }
                        drNew["BARCODE_COUNT"] = Convert.ToDecimal(dr["BARCODE_COUNT"].ToString());
                        dtPrintData.Rows.Add(drNew);
                    }
                    ////
                }
            }
            return dtPrintData;
        }
        /// <summary>
        /// Allign the sample data in specified format to print in Datamax Printer
        /// </summary>
        /// <param name="dr">Datarow which contains data to print</param>
        /// <returns>String arranged in specified format</returns>
        private string AlignDataToPrint(DataRow dr, ServiceType serviceType, PrintType printType)
        {
            string printData = string.Empty;
            string strName = string.Empty;
            string strAge = string.Empty;

            string strGender = string.Empty;

            try
            {
                if (serviceType == ServiceType.Investigation && printType == PrintType.BarCode)
                {
                    strName = dr["PATIENT_NAME"].ToString() + "(" + dr["AGE"].ToString() + "/" + dr["GENDER"].ToString() + ")";
                    //strAge = dr["AGE"].ToString();
                    //strGender = dr["GENDER"].ToString();
                    if (strName.Length > 20)
                    {
                        strName = strName.Substring(0, 18);
                        strName = strName + "..";
                    }
                    printData = printData + "^XA";      //START
                    printData = printData + "^PRC";     //Print Rate


                    printData = printData + "^LH02,15^FO01,35^BY2^BCN,65,Y,Y,N^FR^FD>:" + dr["LIS_SAMPLE_NO"].ToString() + "^FS";  //BARCODE

                    if (Convert.ToInt16(dr["ISPATIENT"]) == 1)
                    {
                        printData = printData + "^LH05,90^FO01,40^A0N,18,18^CI130^FR^FD" + "MRNO: " + dr["MRNO"].ToString() + "^FS";
                    }
                    else
                    {
                        printData = printData + "^LH05,90^FO01,40^A0N,18,18^CI130^FR^FD" + "OUTSIDER" + "^FS";
                    }
                    printData = printData + "^LH05,110^FO01,40^A0N,18,18^CI130^FR^FD" + "Name: " + strName + "^FS";
                    printData = printData + "^LH05,130^FO01,40^A0N,18,18^CI130^FR^FD" + "DOB: " + dr["DOB"].ToString() + "^FS";
                    printData = printData + "^LH05,150^FO01,40^A0N,18,18^CI130^FR^FD" + "Specimen: " + dr["SPECIMEN_NAME"].ToString() + "^FS";
                    printData = printData + "^LH05,170^FO01,40^A0N,18,18^CI130^FR^FD" + "Tube Color: " + dr["TUBE_COLOUR"].ToString() + "^FS";
                    printData = printData + "^LH05,190^FO01,40^A0N,18,18^CI130^FR^FD" + "Withdrawal Date: " + dr["WITHDRAWAL_DATE"].ToString() + "^FS";
                    printData = printData + "^LH05,210^FO01,40^A0N,18,18^CI130^FR^FD" + "Withdrawl_ID: " + dr["WITHDRAWAL_ID"].ToString() + "^FS";


                    printData = printData + "^PQ" + "1" + ",0,0,N"; //Number of copy
                    printData = printData + "^XZ";      //end
                    /*
                     * Commented By Alif M
                     * 
                     * strName = dr["PATIENT_NAME"].ToString();
                    strAge = dr["AGE"].ToString();
                    strGender = dr["GENDER"].ToString();
                    if (strName.Length > 20)
                    {
                        strName = strName.Substring(0, 18);
                        strName = strName + "..";
                    }
                    printData = printData + "^XA";      //START
                    printData = printData + "^PRC";     //Print Rate
                    if (Convert.ToInt16(dr["ISSTICKER"]) == 1)
                    {
                        if (dr["BILL_NO"] != DBNull.Value || dr["BILL_NO"].ToString() != String.Empty)
                        {
                            printData = printData + "^LH05,40^FO01,40^A0N,20,20^CI130^FR^FD" + "Bill No: " + dr["BILL_NO"].ToString() + "^FS";
                        }
                        else
                        {
                            printData = printData + "^LH05,40^FO01,40^A0N,20,20^CI130^FR^FD" + "Not Billed " + "^FS";
                        }
                        printData = printData + "^LH05,90^FO01,40^A0N,20,20^CI130^FR^FD" + "Name: " + strName + "^FS";
                        if (Convert.ToInt16(dr["ISPATIENT"]) == 1)
                        {
                            printData = printData + "^LH05,65^FO01,40^A0N,20,20^CI130^FR^FD" + "MRNO: " + dr["MRNO"].ToString() + "^FS";
                        }
                        else
                        {
                            printData = printData + "^LH05,65^FO01,40^A0N,20,20^CI130^FR^FD" + "OUTSIDER" + "^FS";
                        }
                        //printData = printData + "^LH05,115^FO01,40^A0N,20,20^CI130^FR^FD" + "DOB: " + dr["DOB"] + "^FS";
                        printData = printData + "^LH25,125^FO01,40^A0N,20,20^CI130^FR^FD" + "Age: " + strAge + "^FS";
                        printData = printData + "^LH34,145^FO01,40^A0N,20,20^CI130^FR^FD" + "Gender: " + strGender + "^FS";
                        //printData = printData + "^LH75,115^FO01,40^A0N,20,20^CI130^FR^FD" + "Nationality: " + dr["NATIONALITY"] + "^FS";
                        if (dr.Table.Columns.Contains("SPECIMEN_NAME") && dr["SPECIMEN_NAME"] != DBNull.Value)
                        {
                            printData = printData + "^LH05,165^FO01,40^A0N,20,20^CI130^FR^FD" + "Specimen: " + dr["SPECIMEN_NAME"].ToString() + "^FS";
                        }
                        //if (dr["MOBILEPHONE"].ToString() != String.Empty)
                        //    printData = printData + "^LH05,115^FO01,40^A0N,20,20^CI130^FR^FD" + "Mobile: " + dr["MOBILEPHONE"].ToString() + "^FS";
                        //if (dr["HOMEPHONE"].ToString() != String.Empty)
                        //    printData = printData + "^LH75,115^FO01,40^A0N,20,20^CI130^FR^FD" + "Home: " + dr["HOMEPHONE"].ToString() + "^FS";
                        //if (dr["CPR"].ToString() != String.Empty)
                        //    printData = printData + "^LH75,115^FO01,40^A0N,20,20^CI130^FR^FD" + "CPR: " + dr["CPR"].ToString() + "^FS";
                        //if (dr["VISIT_NO"].ToString() != String.Empty)
                        //    printData = printData + "^LH05,115^FO01,40^A0N,20,20^CI130^FR^FD" + "Encounter: " + dr["VISIT_NO"].ToString() + "^FS";
                        //if (dr["INSURANCE"].ToString() != String.Empty)
                        //    printData = printData + "^LH05,115^FO01,40^A0N,20,20^CI130^FR^FD" + "Insurance: " + dr["INSURANCE"].ToString() + "^FS";
                        if (dr["SITE"].ToString() != String.Empty)
                            printData = printData + "^LH05,115^FO01,40^A0N,20,20^CI130^FR^FD" + "Site: " + dr["SITE"].ToString() + "^FS";
                    }
                    else
                    {
                        printData = printData + "^LH02,15^FO01,35^BY2^BCN,65,Y,Y,N^FR^FD>:" + dr["LIS_SAMPLE_NO"].ToString() + "^FS";  //BARCODE

                        if (Convert.ToInt16(dr["ISPATIENT"]) == 1)
                        {
                            printData = printData + "^LH05,90^FO01,40^A0N,18,18^CI130^FR^FD" + "MRNO: " + dr["MRNO"].ToString() + "^FS";
                        }
                        else
                        {
                            printData = printData + "^LH05,90^FO01,40^A0N,18,18^CI130^FR^FD" + "OUTSIDER" + "^FS";
                        }
                        printData = printData + "^LH05,110^FO01,40^A0N,18,18^CI130^FR^FD" + "Name: " + strName + "^FS";
                        printData = printData + "^LH05,130^FO01,40^A0N,18,18^CI130^FR^FD" + "Age: " + strAge + "^FS";
                        printData = printData + "^LH05,150^FO01,40^A0N,18,18^CI130^FR^FD" + "Gender: " + strGender + "^FS";
                        printData = printData + "^LH05,170^FO01,40^A0N,18,18^CI130^FR^FD" + "Specimen: " + dr["SPECIMEN_NAME"].ToString() + "^FS";
                    }

                    printData = printData + "^PQ" + "1" + ",0,0,N"; //Number of copy
                    printData = printData + "^XZ";      //end */

                }
                else if (serviceType == ServiceType.Consultation && printType == PrintType.BarCode)
                {
                    strName = dr["PROVIDER_NAME"].ToString();
                    if (strName.Length > 35)
                    {
                        strName = strName.Substring(0, 33);
                        strName = strName + "..";
                    }
                    printData = printData + "^XA";      //START
                    printData = printData + "^PRC";     //Print Rate

                    printData = printData + "^LH6,30^FO01,40^A0N,30,30^CI130^FR^FD" + "MRNO: " + dr["MRNO"].ToString() + "^FS";
                    printData = printData + "^LH6,50^FO01,40^A0N,30,30^CI130^FR^FD" + "NAME: " + dr["NAME"].ToString() + "^FS";
                    printData = printData + "^LH6,80^FO01,40^A0N,25,25^CI130^FR^FD" + "Dr." + strName + " " + dr["PROVIDER_ID"].ToString() + "^FS";
                    //if (Convert.ToInt16(dr["ISDOCTOR"]) == 1)
                    //{
                    //    printData = printData + "^LH6,80^FO01,40^A0N,25,25^CI130^FR^FD" + "Dr." + strName + "^FS";
                    //}
                    //else
                    //{
                    //    printData = printData + "^LH6,80^FO01,40^A0N,25,25^CI130^FR^FD" + "Provider: " + strName + "^FS";
                    //}


                    printData = printData + "^LH6,110^FO01,40^A0N,25,25^CI130^FR^FD" + "App. Time: " + dr["APPOINTMENT_TIME"].ToString() + "^FS";
                    printData = printData + "^LH6,140^FO01,40^A0N,70,70^CI130^FR^FD" + "Token : " + dr["TOKEN_NO"].ToString() + "^FS";
                    printData = printData + "^LH6,170^FO01,40^A0N,25,25^CI130^FR^FD" + "Doctor Room No: " + dr["ROOM_NO"].ToString() + "^FS";
                    //if (Convert.ToInt16(dr["ISAPPOINTMENT"]) == 1)
                    //{
                    //    printData = printData + "^LH6,140^FO01,40^A0N,25,25^CI130^FR^FD" + "App. Date: " + dr["APPOINTMENT_TIME"].ToString() + "^FS";
                    //    printData = printData + "^LH280,60^FO01,40^A0N,70,70^CI130^FR^FD" + dr["TOKEN_NO"].ToString() + "^FS";
                    //}
                    //else
                    //{
                    //    printData = printData + "^LH280,60^FO01,40^A0N,70,70^CI130^FR^FD" + dr["TOKEN_NO"].ToString() + "^FS";
                    //}

                    printData = printData + "^PQ" + "1" + ",0,0,N"; //Number of copy
                    printData = printData + "^XZ";      //end
                }
                else if (serviceType == ServiceType.Common && printType == PrintType.PatientSlip)
                {
                    strName = dr["PATIENT_NAME"].ToString();
                    if (strName.Length > 35)
                    {
                        strName = strName.Substring(0, 33);
                        strName = strName + "..";
                    }
                    string description = string.Empty;
                    if (dr["IDENTIFYING_DOCUMENT"] != DBNull.Value)
                    {
                        description = GetEnumDescription((IdentifyingDocument)Convert.ToInt16(dr["IDENTIFYING_DOCUMENT"].ToString()));
                    }

                    printData = printData + "^XA";      //START
                    printData = printData + "^PRC";     //Print Rate

                    printData = printData + "^LH06,5^FO01,40^A0N,25,25^CI130^FR^FD" + "Name : " + strName + "^FS";
                    printData = printData + "^LH06,30^FO01,40^A0N,30,30^CI130^FR^FD" + "MRNO: " + dr["MRNO"].ToString() + "^FS";
                    printData = printData + "^LH06,50^FO01,35^BY2^BCR,90,N,N,N^FR^FD>:" + dr["MRNO"].ToString() + "^FS";  //BARCODE
                    printData = printData + "^LH06,60^FO01,40^A0N,25,25^CI130^FR^FD" + "DOB: " + dr["DOB"].ToString() + " " + "Gender/Age: " + dr["GENDER"].ToString() + "/" + dr["AGE"].ToString() + "^FS";
                    printData = printData + "^LH06,90^FO01,40^A0N,25,25^CI130^FR^FD" + "Registered Since: " + dr["REGISTERED_SINCE"].ToString();
                    //printData = printData + "^LH06,85^FO01,40^A0N,25,25^CI130^FR^FD" + "Nationality: " + dr["NATIONALITY"].ToString() + "^FS";rNew["REGISTERED_SINCE"]
                    //printData = printData + "^LH06,115^FO01,40^A0N,25,25^CI130^FR^FD" + "CPR: " + dr["CPR"].ToString() + "^FS";
                    //printData = printData + "^LH06,145^FO01,40^A0N,25,25^CI130^FR^FD" + "Mobile: " + dr["MOBILEPHONE"].ToString() + " " + "Home: " + dr["HOMEPHONE"].ToString() + "^FS";
                    //printData = printData + "^LH06,175^FO01,40^A0N,25,25^CI130^FR^FD" + "Encounter: " + dr["VISIT_NO"].ToString() + " " + dr["START_DATE"].ToString() + "^FS";
                    //printData = printData + "^LH06,205^FO01,40^A0N,25,25^CI130^FR^FD" + "Insurance: " + dr["INSURANCE"].ToString() + "^FS";
                    //printData = printData + "^LH06,235^FO01,40^A0N,25,25^CI130^FR^FD" + "Site: " + dr["SITE"].ToString() + "^FS";
                    //if (dr["IDENTIFYING_DOCUMENT"] != DBNull.Value)
                    //{
                    //    printData = printData + "^LH06,200^FO01,40^A0N,25,25^CI130^FR^FD" + description + ": " + dr["DOCUMENT_NO"].ToString() + "^FS";
                    //}

                    printData = printData + "^PQ" + "1" + ",0,0,N"; //Number of copy
                    printData = printData + "^XZ";      //end
                }
                else if (serviceType == ServiceType.Pharmacy && printType == PrintType.Prescription)
                {
                    // pos = 170;
                    printData = string.Empty;
                    //  printData = "N\n";
                    // string data = string.Empty;

                    printData = printData + "^XA";      //START
                    printData = printData + "^PRC";
                    //printData = printData + "^LH36,10^FO01,30^A0N,30,30^CI130^FR^FD" + dr["HOSPITAL_NAME"].ToString() + "^FS";
                    printData = printData + "^LH06,20^FO01,30^A0N,30,30^CI130^FR^FD" + "MRNO:" + dr["MRNO"].ToString() + "^FS"; // "MRNO: " + 
                    printData = printData + "^LH60,20^FO01,30^A0N,30,30^CI130^FR^FD" + "Date:" + Convert.ToDateTime(dr["ENTRY_DATE"]).ToString("dd-MMM-yyyy") + "^FS";
                    printData = printData + "^LH06,50^FO01,30^A0N,25,25^CI130^FR^FD" + "NAME:" + dr["PATIENT_NAME"].ToString() + "^FS"; // "Name : " + " " + dr["MIDDLE_NAME"].ToString() + 
                    //printData = printData + "^LH06,75^FO01,35^BY2^BCN,65,N,N,N^FR^FD>:" + dr["MRNO"].ToString() + "^FS";  //BARCODE
                    // printData = printData + "^LH06,65^FO01,30^A0N,25,25^CI130^FR^FD" + dr["AGE"].ToString() + " " + dr["GENDER"].ToString().First() + " " + dr["BILL_NO"].ToString() + " " + Convert.ToDateTime(dr["BILL_DATE"]).ToString("dd-MMM-yyyy HH:MM") + "^FS";
                    printData = printData + "^LH06,75^FO01,30^A0N,25,25^CI130^FR^FD" + "IOS:" + dr["MEDICINE_NAME"].ToString() + "^FS";
                    //To generate the Medicine Name.
                    string data = string.Empty;

                    //if (Convert.ToString(dr["MEDICINE_TYPE"]).Trim() == "0")
                    //{
                    //    printData = printData + "^LH06,115^FO01,40^A0N,25,25^CI130^FR^FD" + "* * *" + dr["BRAND_GENERIC"].ToString() + "* * *" + "^FS";
                    //}
                    //else
                    //{
                    //    printData = printData + "^LH06,140^FO01,40^A0N,25,25^CI130^FR^FD" + "* * *" + dr["BRAND_NAME"].ToString() + "* * *" + "^FS";
                    //}

                    //printData = printData + "^LH06,145^FO01,40^A0N,25,25^CI130^FR^FD" + "Take " + dr["QUANTITY"] + " " + dr["FORM"] + " " + dr["FREQUENCY"] + "  for " + dr["DURATION"] + " days" + "^FS";

                    //printData = printData + "^LH06,160^FO01,40^A0N,25,25^CI130^FR^FD" + "Take " + dr["QUANTITY"] + " " + dr["FORM"] + " " + dr["FREQUENCY"] + " times a day for " + dr["DURATION"] + " days" + "^FS";
                    //printData += AlignElements(data, serviceType, printType, CaseType.Normal);
                    //printData += CR;
                    //data = string.Empty;


                    //printData = printData + "^LH06,205^FO01,40^A0N,25,25^CI130^FR^FD" + "Insurance: " + dr["INSURANCE"].ToString() + "^FS";
                    //printData = printData + "^LH06,235^FO01,40^A0N,25,25^CI130^FR^FD" + "Site: " + dr["SITE"].ToString() + "^FS";
                    ////To generate MRNO.
                    //data = dr["MRNO"].ToString();
                    //printData += AlignElements(data, serviceType, printType, CaseType.Normal);
                    //printData += CR;
                    //data = string.Empty;
                    ////To generate Patient Name.
                    //data = dr["FIRST_NAME"].ToString() + " " + dr["MIDDLE_NAME"].ToString() + " " + dr["LAST_NAME"].ToString();
                    //printData += AlignElements(data, serviceType, printType, CaseType.Normal);
                    //printData += CR;
                    //data = string.Empty;
                    ////To generate Patient Age, Bill No and Bill Date.
                    //data = dr["AGE"].ToString() + " " + dr["GENDER"].ToString().First() + " " + dr["BILL_NO"].ToString() + " " + Convert.ToDateTime(dr["BILL_DATE"]).ToString("dd-MMM-yyyy HH:MM");
                    //printData += AlignElements(data, serviceType, printType, CaseType.Normal);
                    //printData += CR;
                    //data = string.Empty;
                    ////To generate the Provider Name and Department
                    //data = dr["PROVIDER"].ToString() + " " + dr["DEPARTMENT"].ToString();
                    //printData += AlignElements(data, serviceType, printType, CaseType.Normal);
                    //printData += CR;
                    //data = string.Empty;
                    ////To generate the Medicine Name.
                    //if (Convert.ToString(dr["MEDICINE_TYPE"]).Trim() == "0")
                    //{
                    //    data = "* * *" + dr["BRAND_GENERIC"].ToString() + "* * * ";
                    //}
                    //else
                    //{
                    //    data = "* * *" + dr["BRAND_NAME"].ToString() + "* * * ";
                    //}
                    //printData += AlignElements(data, serviceType, printType, CaseType.Normal);
                    //printData += CR;
                    //data = string.Empty;


                    //To generate Insturction.
                    //data = "Take " + dr["QUANTITY"] + " " + dr["FORM"] + " " + dr["FREQUENCY"] + " times a day for " + dr["DURATION"] + " days";
                    //printData += AlignElements(data, serviceType, printType, CaseType.Normal);
                    //printData += CR;
                    //data = string.Empty;

                    ////To generate the Medicine Name.
                    //if (Convert.ToString(dr["MEDICINE_TYPE"]).Trim() == "0")
                    //{
                    //    data = "Medicine Name: " + dr["BRAND_GENERIC"].ToString();
                    //}
                    //else
                    //{
                    //    data = "Medicine Name: " + dr["BRAND_NAME"].ToString();
                    //}
                    //printData += AlignElements(data, serviceType, printType, CaseType.Normal);
                    //printData += CR;
                    //data = string.Empty;
                    ////To generate the Dosage [eg:2ml 1-0-1]
                    //if (Convert.ToString(dr["DOSE_PRN"]).Trim() == "0")
                    //{
                    //    data = "Dosage: ";
                    //    if (dr["QUANTITY"] != DBNull.Value)
                    //    {
                    //        data += dr["QUANTITY"].ToString() + dr["QUANTITY_UNIT"].ToString() + ", ";
                    //    }
                    //    if (Convert.ToString(dr["DOSE_TYPE"]).Trim() == "1")
                    //    {
                    //        data += Convert.ToString(dr["FREQUENCY_VALUE"]) + Convert.ToString(dr["DOSE_VALUE"] + ", ");
                    //    }
                    //    else
                    //    {
                    //        data += Convert.ToString(dr["DOSE_VALUE"]);
                    //        if (dr["ADMIN_TIME"] != DBNull.Value)
                    //        {
                    //            data += " at " + Convert.ToString(dr["ADMIN_TIME"]);
                    //        }
                    //    }
                    //    printData += AlignElements(data, serviceType, printType, CaseType.Normal);
                    //    printData += CR;
                    //    data = string.Empty;
                    //}
                    ////To generate the Start Date,Duration & Route
                    //if (Convert.ToDateTime(dr["START_DATE"]) > System.DateTime.Now.Date)
                    //{
                    //    data = "Start Date:" + Convert.ToString(dr["START_DATE"]) + " ";

                    //    //Duration.
                    //    if (Convert.ToString(dr["ISLIFELONG"]).Trim() == "1")
                    //    {
                    //        data += "Duration: Life Long";
                    //    }
                    //    else if (dr["DURATION"] != DBNull.Value)
                    //    {
                    //        data += "Duration: ";
                    //        data += Convert.ToString(dr["DURATION"]) + " " + Convert.ToString(dr["DURATION_TYPE_VALUE"]);
                    //    }
                    //    printData += AlignElements(data, serviceType, printType, CaseType.Normal);
                    //    printData += CR;
                    //    data = string.Empty;
                    //    //Route.
                    //    data = "Route: " + Convert.ToString(dr["ROUTE"]) + " " + Convert.ToString(dr["FORM"]);
                    //    printData += AlignElements(data, serviceType, printType, CaseType.Normal);
                    //    printData += CR;
                    //}
                    //else
                    //{
                    //    //Duration.
                    //    if (Convert.ToString(dr["ISLIFELONG"]).Trim() == "1")
                    //    {
                    //        data += "Duration: Life Long";
                    //    }
                    //    else if (dr["DURATION"] != DBNull.Value)
                    //    {
                    //        data += "Duration: ";
                    //        data += Convert.ToString(dr["DURATION"]) + " " + Convert.ToString(dr["DURATION_TYPE_VALUE"]);
                    //    }
                    //    //Route.
                    //    data += "Route: " + Convert.ToString(dr["ROUTE"]) + " " + Convert.ToString(dr["FORM"]);
                    //    printData += AlignElements(data, serviceType, printType, CaseType.Normal);
                    //    printData += CR;
                    //}
                    //data = string.Empty;

                    //string data = string.Empty;
                    //To generate the instructions.
                    //if (dr["SPECIAL_INSTRUCTIONS"] != DBNull.Value)
                    //{
                    //    data = Convert.ToString(dr["SPECIAL_INSTRUCTIONS"]);

                    //}
                    //if (dr["ADMINISTRATION_INSTRUCTION"] != DBNull.Value)
                    //{
                    //    data += Convert.ToString(dr["ADMINISTRATION_INSTRUCTION"]);
                    //}
                    //if (dr["REMARKS"] != DBNull.Value)
                    //{
                    //    data += ", " + Convert.ToString(dr["REMARKS"]);
                    //}
                    if (dr.Table.Columns.Contains("DURATION_VALUE")&&dr.Table.Columns.Contains("FREQ_VALUE"))
                    {
                        printData = printData + "^LH06,100^FO01,30^A0N,25,25^CI130^FR^FD" + "Dose:" + dr["DOSE"].ToString() + " " + dr["FREQ_VALUE"] + " " + "by" + " " + dr["ROUTE"] + " " + "for" + " " + dr["DURATION"] + " " + dr["DURATION_VALUE"] + "^FS";
                        //printData = printData + "^LH06,100^FO01,30^A0N,25,25^CI130^FR^FD" + "Dose:" + dr["DOSE"].ToString() + " " + dr["DOSE"].ToString() + " " + dr["FREQUENCY"] + " " + "by" + " " + dr["ROUTE"] + " " + "for" + " " + dr["DURATION"] + " " + dr["DURATION_VALUE"] + "^FS";
                    }

                    if (dr.Table.Columns.Contains("ADMINISTRATION_INSTRUCTION"))
                    {
                        printData = printData + "^LH06,130^FO01,30^A0N,25,25^CI130^FR^FD" + "Instruction:" + dr["ADMINISTRATION_INSTRUCTION"].ToString() + "^FS";
                    }

                    if (dr.Table.Columns.Contains("BATCHNO"))
                    {
                        printData = printData + "^LH06,150^FO01,30^A0N,25,25^CI130^FR^FD" + "Batch No.:" + dr["BATCHNO"].ToString() + "^FS";
                    }

                    if (dr.Table.Columns.Contains("EXP_DATE"))
                    {
                        printData = printData + "^LH06,180^FO01,30^A0N,25,25^CI130^FR^FD" + "Exp Date:" + Convert.ToDateTime(dr["EXP_DATE"]).ToString("dd-MMM-yyyy")+ "^FS";
                    }

                    printData = printData + "^LH06,200^FO01,30^A0N,25,25^CI130^FR^FD" + data + "^FS";
                    //printData += AlignElements(data, serviceType, printType, CaseType.Normal);
                    printData += "^PQ" + "1" + ",0,0,N"; //Number of copy
                    printData += "^XZ";      //end              
                }
                else if (serviceType == ServiceType.Common && printType == PrintType.CPOEAdminPatientSlip)
                {
                    CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
                    TextInfo textInfo = cultureInfo.TextInfo;
                    strName = textInfo.ToTitleCase(dr["PATIENT_NAME"].ToString().ToLower());

                    if (strName.Length > 28)
                    {
                        strName = strName.Substring(0, 26);
                        strName = strName + "..";
                    }
                    printData = printData + "^XA";      //START
                    printData = printData + "^PRC";     //Print Rate

                    printData = printData + "^LH06,15^FO01,40^A0N,25,25^CI130^FR^FD" + "NAME : " + strName + "^FS";
                    printData = printData + "^LH06,45^FO01,40^A0N,25,25^CI130^FR^FD" + "MRNO: " + dr["MRNO"].ToString() + "^FS";
                    printData = printData + "^LH06,75^FO01,40^A0N,25,25^CI130^FR^FD" + "Age: " + dr["AGE"].ToString() + "^FS";
                    printData = printData + "^LH06,105^FO01,40^A0N,25,25^CI130^FR^FD" + "Gender: " + dr["GENDER"].ToString() + "^FS";

                    printData = printData + "^PQ" + "1" + ",0,0,N"; //Number of copy
                    printData = printData + "^XZ";      //end
                }
                else if (serviceType == ServiceType.Common && printType == PrintType.PatientBand)
                {
                    CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
                    TextInfo textInfo = cultureInfo.TextInfo;
                    strName = textInfo.ToTitleCase(dr["PATIENT_NAME"].ToString().ToLower());

                    //if (strName.Length > 28)
                    //{
                    //    strName = strName.Substring(0, 26);
                    //    strName = strName + "..";
                    //}
                    printData = printData + "^XA";      //START
                    printData = printData + "^PRC";     //Print Rate

                    printData = printData + "^LH240,00^FO01,40^A0R,30,30^CI130^FR^FD" + "PATIENT NAME: " + strName + "^FS";
                    printData = printData + "^LH200,00^FO01,40^A0R,30,30^CI130^FR^FD" + "MRNO: " + dr["MRNO"].ToString() + "^FS";
                    printData = printData + "^LH160,00^FO01,40^A0R,30,30^CI130^FR^FD" + "DOB: " + dr["DOB"].ToString() + "^FS";
                    printData = printData + "^LH120,00^FO01,40^A0R,30,30^CI130^FR^FD" + "AGE & SEX: " + dr["AGE"].ToString() + " " + dr["GENDER"].ToString() + "^FS";
                    printData = printData + "^LH160,410^FO01,35^BY2^BCR,90,N,N,N^FR^FD>:" + dr["MRNO"].ToString() + "^FS";
                    //printData = printData + "^LH155,400^FO01,35^BY2^BCR,65,N,N,N^FR^FD>:" + dr["MRNO"].ToString() + "^FS";


                    printData = printData + "^PQ" + "1" + ",0,0,N"; //Number of copy
                    printData = printData + "^XZ";      //end
                }
                else if (serviceType == ServiceType.CSSD && printType == PrintType.BarCode)
                {
                    if (Convert.ToString(dr["BARCODE_TYPE"])=="0")
                    {
                        printData = printData + "^LH02,15^FO01,35^BY2^BCN,65,Y,Y,N^FR^FD>:" + dr["CSSD_INDIVIDUAL_ITEM_NAME"].ToString() + "^FS";  //BARCODE
                        printData = printData + "^LH05,110^FO01,40^A0N,18,18^CI130^FR^FD" + "Item Category: " + dr["CSSD_ITEM_NAME"].ToString() + "^FS";
                        printData = printData + "^LH25,110^FO01,40^A0N,18,18^CI130^FR^FD" + "Item: " + dr["CSSD_INDIVIDUAL_ITEM_NAME"].ToString() + "^FS";

                        printData = printData + "^PQ" + "1" + ",0,0,N"; //Number of copy
                        printData = printData + "^XZ";      //end
                    }

                    else if (Convert.ToString(dr["BARCODE_TYPE"]) == "1")
                    {
                        printData = printData + "^LH02,15^FO01,35^BY2^BCN,65,Y,Y,N^FR^FD>:" + dr["CSSD_MAST_PROCESS_ID"].ToString() + "^FS";  //BARCODE
                        printData = printData + "^LH05,110^FO01,40^A0N,18,18^CI130^FR^FD" + "Category Name: " + dr["CATEGORY_NAME"].ToString() + "^FS";
                        printData = printData + "^LH25,110^FO01,40^A0N,18,18^CI130^FR^FD" + "Set Name: " + dr["SET_NAME"].ToString() + "^FS";
                        printData = printData + "^LH55,110^FO01,40^A0N,18,18^CI130^FR^FD" + "Expiry Date: " + dr["EXPIRY_DATE"].ToString() + "^FS";


                        printData = printData + "^PQ" + "1" + ",0,0,N"; //Number of copy
                        printData = printData + "^XZ";      //end
                    }
                }
                return printData;
            }
            catch (Exception)
            {
                throw;
            }

        }
        /// <summary>
        /// Align the position of the data to be printed.
        /// </summary>
        /// <param name="name">string data</param>
        /// <param name="serviceType">Service Type</param>
        /// <param name="printType">Print Type</param>
        /// <param name="_caseType">Character Case Type</param>
        /// <returns></returns>
        private string AlignElements(string name, ServiceType serviceType, PrintType printType, CaseType _caseType)
        {
            try
            {
                string _name = name;
                string print_data = string.Empty;
                int caseParse = 0;
                if (serviceType == ServiceType.Pharmacy && printType == PrintType.Prescription)
                {
                    switch (_caseType)
                    {
                        case CaseType.Normal:
                            {
                                caseParse = 57;
                                break;
                            }
                        case CaseType.Upper:
                            {
                                caseParse = 42;
                                break;
                            }
                        case CaseType.Lower:
                            {
                                break;
                            }
                    }

                    //^LH06,15^FO01,40^A0N,25,25^CI130^FR^FD
                    while (_name.Length > caseParse)
                    {
                        if (pos >= 170)
                        {
                            print_data += "^XA";      //START
                            print_data += "^PRC";     //Print Rate

                            //print_data += "E" + CR;
                            //print_data += STX + "L" + CR;
                            //print_data += "D11" + CR;
                            pos = 20;
                        }
                        else
                        {
                            pos -= 20;
                            print_data += "^LH12," + (pos).ToString() + "^FO01,40^A0N,25,25^CI130^FR^FD";
                            print_data += DoubleQuotes + _name.Substring(0, caseParse) + "-" + DoubleQuotes + "^FS";
                            _name = _name.Substring(caseParse, _name.Length - caseParse);
                        }
                    }
                    if (pos >= 170)
                    {
                        //print_data += "E" + CR;
                        //print_data += STX + "L" + CR;
                        //print_data += "D11" + CR;
                        print_data += "^XA";      //START
                        print_data += "^PRC";     //Print Rate
                        pos = 20;
                    }
                    pos += 20;
                    print_data += "^LH12," + (pos).ToString() + "^FO01,40^A0N,25,25^CI130^FR^FD";
                    print_data += DoubleQuotes + _name.Substring(0, _name.Length);
                    //print_data += CR;
                }
                return print_data;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Gets the enum description.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

        /// <summary>
        /// Enumerator for the character chasing in printing.
        /// </summary>
        public enum CaseType : short
        {
            /// <summary>
            /// Represents Normal Case Letter.
            /// </summary>
            Normal,
            /// <summary>
            /// Represents Upper Case Letter.
            /// </summary>
            Upper,
            /// <summary>
            /// Represents Lower Case Letter.
            /// </summary>
            Lower,
        }



        public string data { get; set; }
    }
}
