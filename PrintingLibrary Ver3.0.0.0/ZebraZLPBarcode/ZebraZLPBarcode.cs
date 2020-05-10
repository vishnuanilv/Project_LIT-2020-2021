using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infologics.Medilogics.PrintingLibrary.Main;
using System.Data;
using System.Drawing.Printing;
using System.Windows.Forms;
using Infologics.Medilogics.Enumerators.General;
using Infologics.Medilogics.CommonClient.Controls.CommonFunctions;
using Infologics.Medilogics.General.Control;
using System.Globalization;
using System.Threading;
using System.Reflection;
using System.ComponentModel;
using System.Drawing;
using Infologics.Medilogics.Enumerators.Investigations;
using System.Configuration;
using System.IO;

namespace Infologics.Medilogics.PrintingLibrary.ZebraZLPBarcode
{
    public class ZebraZLPBarcode : IPrinting
    {
        #region IPrinting Members
        private static string DoubleQuotes = Convert.ToString((char)0x22);
        private static string CR = Convert.ToString((char)0x0D);
        private long pos = 0;

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
                                //WriteFileLog(printData);
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
                    dtPrintData = SelectDataToPrint(dsData, serviceType, printType);
                    if (dtPrintData != null)
                    {
                        DataColumn dcBarCode = new DataColumn("BARCODE_COUNT", typeof(Int32));
                        dcBarCode.DefaultValue = 1;
                        dtPrintData.Columns.Add(dcBarCode);
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
            return printStatus;
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
                    dtPrintData.Columns.Add("PHYSICIAN_ID");
                    dtPrintData.Columns.Add("CONTAINER_NAME");
                    dtPrintData.Columns.Add("WITHDRAWAL_DATE", typeof(DateTime));
                    dtPrintData.Columns.Add("WITHDRAWAL_ID");
                    dtPrintData.Columns.Add("PRIORITY");
                    dtPrintData.Columns.Add("NURSING_STATION");
                    //Add necessary data from dataset to datatable to print
                    foreach (DataRow dr in dsData.Tables["LIS_MAST_SAMPLE_COLLECTION"].Rows)
                    {
                        //Added by Alif M on 10-02-2016 --Added for fetching Patient Details
                        string patientName = string.Empty;
                        Assembly asm = Assembly.LoadFile(AppDomain.CurrentDomain.BaseDirectory+"Infologics.Medilogics.CommonShared.FOMain.dll");
                        object obj = new object();
                        obj = asm.CreateInstance("Infologics.Medilogics.CommonShared.FOMain.MainFOShared");
                        MethodInfo mStart = null;
                        mStart = obj.GetType().GetMethod("PatientDemographicsNewWithDatatable");
                        DataTable dtCriteria = new DataTable();
                        dtCriteria.Columns.Add("MRNO");
                        dtCriteria.Columns.Add("MODE");

                        if (Convert.ToInt16(dr["ISPATIENT"].ToString())==1)
                        {
                            dtCriteria.Rows.Add(dr["MRNO"].ToString(), 1);
                        }
                        else
                        {
                            dtCriteria.Rows.Add(dr["PROFILE_ID"].ToString(), 2);
                        }                        
                        object[] paramDll = new object[1];
                        paramDll[0] = dtCriteria;
                        DataTable dtPatDemographicsData = (DataTable)mStart.Invoke(obj, paramDll);
                        if (dtPatDemographicsData.Rows.Count > 0)
                        {
                            if (dtPatDemographicsData.Rows[0]["FIRST_NAME"] != DBNull.Value)
                            {
                                patientName = dtPatDemographicsData.Rows[0]["FIRST_NAME"].ToString();
                            }
                            if (dtPatDemographicsData.Rows[0]["MIDDLE_NAME"] != DBNull.Value)
                            {
                                if (patientName == string.Empty)
                                {
                                    patientName = dtPatDemographicsData.Rows[0]["MIDDLE_NAME"].ToString();
                                }
                                else
                                {
                                    patientName += " " + dtPatDemographicsData.Rows[0]["MIDDLE_NAME"].ToString();
                                }
                            }
                            if (dtPatDemographicsData.Rows[0]["LAST_NAME"] != DBNull.Value)
                            {


                                if (patientName == string.Empty)
                                {
                                    patientName = dtPatDemographicsData.Rows[0]["LAST_NAME"].ToString();
                                }
                                else
                                {
                                    patientName += " " + dtPatDemographicsData.Rows[0]["LAST_NAME"].ToString();
                                }
                            }
                        }
                        //                        
                        string priority = string.Empty;
                        string nursingStation = string.Empty;
                        if (dsData.Tables.Contains("LIS_DTLS_SAMPLE_ORDERBILL") && dsData.Tables["LIS_DTLS_SAMPLE_ORDERBILL"] != null && dsData.Tables["LIS_DTLS_SAMPLE_ORDERBILL"].Rows.Count > 0)
                        {
                            var selectedRow = from dtlsSampleCollection in dsData.Tables["LIS_DTLS_SAMPLE_COLLECTION"].AsEnumerable()
                                              join dtlsOrderBill in dsData.Tables["LIS_DTLS_SAMPLE_ORDERBILL"].AsEnumerable()
                                              on Convert.ToInt64(dtlsSampleCollection["LIS_DTLS_SAMPLE_ORDERBILL_ID"]) equals Convert.ToInt64(dtlsOrderBill["LIS_DTLS_SAMPLE_ORDERBILL_ID"])
                                              join mastSampleCollection in dsData.Tables["LIS_MAST_SAMPLE_COLLECTION"].AsEnumerable()
                                              on Convert.ToInt64(dtlsOrderBill["LIS_MAST_SAMPLE_COLLECTION_ID"]) equals Convert.ToInt64(mastSampleCollection["LIS_MAST_SAMPLE_COLLECTION_ID"])
                                              where Convert.ToInt64(dtlsOrderBill["LIS_MAST_SAMPLE_COLLECTION_ID"]) == Convert.ToInt64(dr["LIS_MAST_SAMPLE_COLLECTION_ID"])
                                              select dtlsSampleCollection;
                            if (selectedRow.Count() > 0)
                            {
                                DataTable dtTemp = selectedRow.CopyToDataTable();
                                int priorityEnum = 0;
                                if (dtTemp.Columns.Contains("PRIORITY") && dtTemp.Rows[0]["PRIORITY"] != DBNull.Value && int.TryParse(dtTemp.Rows[0]["PRIORITY"].ToString(), out priorityEnum))
                                {
                                    if (priorityEnum == 1 )
                                    {
                                        priority = ((TestPriority)priorityEnum).ToString();
                                    }
                                    else if (priorityEnum == 2)
                                    {
                                        priority = "URGENT";
                                    }
                                }
                                //Added By Alif M On 2016-04-05
                                nursingStation = FetchNursingStation(dtTemp.Rows[0],dr);
                                //
                            }
                        }

                        if (dsData.Tables.Contains("LIS_DTLS_SAMPLE_CONTAINERS") && dsData.Tables["LIS_DTLS_SAMPLE_CONTAINERS"] != null && dsData.Tables["LIS_DTLS_SAMPLE_CONTAINERS"].Rows.Count > 0)
                        {
                            var selectedRow = from sampleContainers in dsData.Tables["LIS_DTLS_SAMPLE_CONTAINERS"].AsEnumerable()
                                              where Convert.ToString(sampleContainers["LIS_MAST_SAMPLE_COLLECTION_ID"]) == Convert.ToString(dr["LIS_MAST_SAMPLE_COLLECTION_ID"])
                                              select sampleContainers;
                            if (selectedRow.Count() > 0)
                            {
                                DataTable dtSelectedContainer = selectedRow.CopyToDataTable();
                                foreach (DataRow drContainer in dtSelectedContainer.Rows)
                                {
                                    DataRow drNew = dtPrintData.NewRow();
                                    drNew["LIS_SAMPLE_NO"] = dr["LIS_SAMPLE_NO"].ToString();
                                    drNew["MRNO"] = dr["MRNO"].ToString();
                                    drNew["PATIENT_NAME"] = patientName;//dr["PATIENT_NAME"].ToString();
                                    drNew["BILL_NO"] = dr["BILL_NO"].ToString();
                                    drNew["ISSTICKER"] = dr["ISSTICKER"].ToString();
                                    drNew["BARCODE_COUNT"] = Convert.ToDecimal(dr["BARCODE_COUNT"].ToString());
                                    drNew["SPECIMEN_NAME"] = dr["SPECIMEN_NAME"].ToString();
                                    drNew["AGE"] = dr["AGE"].ToString();
                                    drNew["GENDER"] = dr["GENDER"].ToString();
                                    drNew["ISPATIENT"] = Convert.ToDecimal(dr["ISPATIENT"].ToString());
                                    drNew["DOB"] = dr["DOB"].ToString();
                                    if (dsData.Tables["LOGIN_USER"] != null && dsData.Tables["LOGIN_USER"].Rows.Count > 0)
                                        drNew["WITHDRAWAL_ID"] = dsData.Tables["LOGIN_USER"].Rows[0]["LOGIN_USER_ID"];
                                    if (drNew["ISSTICKER"].ToString() != "1")
                                    {
                                        drNew["WITHDRAWAL_DATE"] = dr["COLLECTION_TIME"];
                                        drNew["PHYSICIAN_ID"] = dr["PROVIDER_ID"].ToString();
                                    }
                                    drNew["CONTAINER_NAME"] = drContainer["CONTAINER_NAME"];
                                    //DataRow[] drArray = dsData.Tables["LIS_DTLS_SAMPLE_CONTAINERS"] != null ? dsData.Tables["LIS_DTLS_SAMPLE_CONTAINERS"].Select("LIS_MAST_SAMPLE_COLLECTION_ID=" + dr["LIS_MAST_SAMPLE_COLLECTION_ID"]) : null;
                                    //if (drArray != null && drArray.Count() > 0)
                                    //{
                                    //    //Color color = (Color)ColorConverter.ConvertFromString(Convert.ToString(drArray[0]["CONTAINER_COLOUR"]));
                                    //    Color color = System.Drawing.ColorTranslator.FromHtml(Convert.ToString(drArray[0]["CONTAINER_COLOUR"]));
                                    //    drNew["TUBE_COLOUR"] = color.Name;
                                    //}
                                    if (priority != string.Empty) drNew["PRIORITY"] = priority;
                                    if (nursingStation != string.Empty) drNew["NURSING_STATION"] = nursingStation;
                                    dtPrintData.Rows.Add(drNew);
                                }
                            }
                        }
                        else
                        {
                            DataRow drNew = dtPrintData.NewRow();
                            drNew["LIS_SAMPLE_NO"] = dr["LIS_SAMPLE_NO"].ToString();
                            drNew["MRNO"] = dr["MRNO"].ToString();
                            drNew["PATIENT_NAME"] = patientName;
                            drNew["BILL_NO"] = dr["BILL_NO"].ToString();
                            drNew["ISSTICKER"] = dr["ISSTICKER"].ToString();
                            drNew["BARCODE_COUNT"] = Convert.ToDecimal(dr["BARCODE_COUNT"].ToString());
                            drNew["SPECIMEN_NAME"] = dr["SPECIMEN_NAME"].ToString();
                            drNew["AGE"] = dr["AGE"].ToString();
                            drNew["GENDER"] = dr["GENDER"].ToString();
                            drNew["ISPATIENT"] = Convert.ToDecimal(dr["ISPATIENT"].ToString());
                            drNew["DOB"] = dr["DOB"].ToString();
                            if (dsData.Tables["LOGIN_USER"] != null && dsData.Tables["LOGIN_USER"].Rows.Count > 0)
                                drNew["WITHDRAWAL_ID"] = dsData.Tables["LOGIN_USER"].Rows[0]["LOGIN_USER_ID"];
                            if (drNew["ISSTICKER"].ToString() != "1")
                            {
                                drNew["WITHDRAWAL_DATE"] = dr["COLLECTION_TIME"].ToString();
                                drNew["PHYSICIAN_ID"] = dr["PROVIDER_ID"].ToString();
                            }
                            drNew["CONTAINER_NAME"] = " ";
                            if (priority != string.Empty) drNew["PRIORITY"] = priority;
                            dtPrintData.Rows.Add(drNew);
                        }
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
                    dtPrintData.Columns.Add("PROVIDER_NAME", typeof(String));
                    dtPrintData.Columns.Add("ISAPPOINTMENT", typeof(String));
                    dtPrintData.Columns.Add("ISDOCTOR", typeof(String));
                    dtPrintData.Columns.Add("APPOINTMENT_TIME", typeof(String));
                    dtPrintData.Columns.Add("ROOM_NO", typeof(String));
                    dtPrintData.Columns.Add("BARCODE_COUNT", typeof(Decimal));
                    dtPrintData.Columns.Add("IDENTIFYING_DOCUMENT", typeof(Decimal));
                    dtPrintData.Columns.Add("DOCUMENT_NO", typeof(String));
                    dtPrintData.Columns.Add("NATIONALITY", typeof(String));
                    //Add necessary data from dataset to datatable to print
                    foreach (DataRow dr in dsData.Tables["CONSULTATION_TABLE"].Rows)
                    {
                        DataRow drNew = dtPrintData.NewRow();
                        drNew["TOKEN_NO"] = dr["TOKEN_NO"].ToString();
                        drNew["MRNO"] = dr["MRNO"].ToString();
                        drNew["PROVIDER_NAME"] = dr["PROVIDER_NAME"].ToString();
                        drNew["ISAPPOINTMENT"] = dr["ISAPPOINTMENT"].ToString();
                        drNew["ISDOCTOR"] = dr["ISDOCTOR"].ToString();
                        drNew["APPOINTMENT_TIME"] = dr["APPOINTMENT_TIME"].ToString();
                        drNew["ROOM_NO"] = dr["ROOM_NO"].ToString();
                        drNew["BARCODE_COUNT"] = Convert.ToDecimal(dr["BARCODE_COUNT"].ToString());
                        if (dr["IDENTIFYING_DOCUMENT"] != DBNull.Value)
                        {
                            drNew["IDENTIFYING_DOCUMENT"] = Convert.ToDecimal(dr["IDENTIFYING_DOCUMENT"].ToString());
                            drNew["DOCUMENT_NO"] = dr["DOCUMENT_NO"].ToString();
                        }
                        drNew["NATIONALITY"] = dr["NATIONALITY"];
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
                                         FREQUENCY = emr.Field<decimal?>("FREQUENCY"),
                                         FREQUENCY_VALUE = emr.Field<decimal?>("FREQUENCY_VALUE"),
                                         REMARKS = emr.Field<string>("REMARKS"),
                                         ADMIN_TIME = emr.Field<string>("ADMIN_TIME"),
                                         START_DATE = emr.Field<DateTime?>("START_DATE"),
                                         SPECIAL_INSTRUCTIONS = emr.Field<string>("CONDITIONAL_FREQUENCY"),
                                         ISLIFELONG = emr.Field<decimal?>("ISLIFELONG"),
                                         BRAND_NAME = inv.Field<string>("NAME")

                                     };
                    dtPrintData = objComman.LINQToDataTable(BrandQuery);

                    //Join to get the dosage .
                    var DosageQuery = from Mast in dtPrintData.AsEnumerable().Where(r => r.RowState != DataRowState.Deleted)
                                      join emr in dsData.Tables["EMR_LOOKUP"].AsEnumerable().Where(r => r.RowState != DataRowState.Deleted)
                                     on Mast.Field<Decimal?>("FREQUENCY") equals
                                     emr.Field<Decimal?>("EMR_LOOKUP_ID")
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
                                          FREQUENCY = Mast.Field<decimal?>("FREQUENCY"),
                                          FREQUENCY_VALUE = Mast.Field<decimal?>("FREQUENCY_VALUE"),
                                          REMARKS = Mast.Field<string>("REMARKS"),
                                          ADMIN_TIME = Mast.Field<string>("ADMIN_TIME"),
                                          START_DATE = Mast.Field<DateTime?>("START_DATE"),
                                          SPECIAL_INSTRUCTIONS = Mast.Field<string>("SPECIAL_INSTRUCTIONS"),
                                          ISLIFELONG = Mast.Field<decimal?>("ISLIFELONG"),
                                          BRAND_NAME = Mast.Field<string>("BRAND_NAME"),
                                          DOSE_VALUE = emr.Field<string>("LOOKUP_VALUE"),
                                          DOSE_TYPE = emr.Field<string>("FIELD2"),
                                          DOSE_PRN = emr.Field<string>("FIELD5")
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
                                            FREQUENCY = Mast.Field<decimal?>("FREQUENCY"),
                                            FREQUENCY_VALUE = Mast.Field<decimal?>("FREQUENCY_VALUE"),
                                            REMARKS = Mast.Field<string>("REMARKS"),
                                            ADMIN_TIME = Mast.Field<string>("ADMIN_TIME"),
                                            START_DATE = Mast.Field<DateTime?>("START_DATE"),
                                            SPECIAL_INSTRUCTIONS = Mast.Field<string>("SPECIAL_INSTRUCTIONS"),
                                            ISLIFELONG = Mast.Field<decimal?>("ISLIFELONG"),
                                            BRAND_NAME = Mast.Field<string>("BRAND_NAME"),
                                            DOSE_VALUE = Mast.Field<string>("DOSE_VALUE"),
                                            DOSE_TYPE = Mast.Field<string>("DOSE_TYPE"),
                                            DOSE_PRN = Mast.Field<string>("DOSE_PRN"),
                                            DURATION_TYPE_VALUE = dur != null ? dur.Field<string>("VALUE") : null
                                        };
                    dtPrintData = objComman.LINQToDataTable(DurationQuery);
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
            return dtPrintData;
        }
        /// <summary>
        /// Allign the sample data in specified format to print in Datamax Printer
        /// </summary>
        /// <param name="dr">DataRow</param>
        /// <param name="serviceType">Service Type</param>
        /// <param name="printType">Print Type</param>
        /// <returns>String data to be printed</returns>
        private string AlignDataToPrint(DataRow dr, ServiceType serviceType, PrintType printType)
        {
            string printData = string.Empty;
            string strName = string.Empty;
            try
            {
                if (serviceType == ServiceType.Investigation && printType == PrintType.BarCode)
                {
                    #region CommentedByAlif
                    /*
                     * Commented By Alif M on 24-07-2015
                    strName = dr["PATIENT_NAME"].ToString();
                    if (strName.Length > 20)
                    {
                        strName = strName.Substring(0, 18);
                        strName = strName + "..";
                    }
                    printData = printData + "N\n";      //START

                    if (Convert.ToInt16(dr["ISSTICKER"]) == 1)
                    {
                        if (dr["BILL_NO"] != DBNull.Value || dr["BILL_NO"].ToString() != String.Empty)
                        {
                            printData = printData + "A110,10,0,2,1,1,N," + DoubleQuotes + "Bill No:" + dr["BILL_NO"].ToString() + DoubleQuotes + "\n";
                        }
                        else
                        {
                            printData = printData + "A110,10,0,2,1,1,N," + DoubleQuotes + "Not Billed" + DoubleQuotes + "\n";
                        }
                        if (Convert.ToInt16(dr["ISPATIENT"]) == 1)
                        {
                            printData = printData + "A170,40,0,2,1,1,N," + DoubleQuotes + "MRNO:" + dr["MRNO"].ToString() + DoubleQuotes + "\n";
                        }
                        else
                        {
                            printData = printData + "A170,40,0,2,1,1,N," + DoubleQuotes + "OUTSIDER" + DoubleQuotes + "\n";
                        }
                        printData = printData + "A170,70,0,2,1,1,N," + DoubleQuotes + "Name:" + strName + DoubleQuotes + "\n";
                        if (dr.Table.Columns.Contains("SPECIMEN_NAME") && dr["SPECIMEN_NAME"] != DBNull.Value && dr["SPECIMEN_NAME"].ToString().Trim().Length > 0)
                        {
                            printData = printData + "A170,100,0,2,1,1,N," + DoubleQuotes + "Specimen:" + dr["SPECIMEN_NAME"].ToString() + DoubleQuotes + "\n";
                        }
                    }
                    else
                    {
                        printData = printData + "B170,00,0,1,2,5,60,B," + DoubleQuotes + dr["LIS_SAMPLE_NO"].ToString() + DoubleQuotes + "\n";

                        if (Convert.ToInt16(dr["ISPATIENT"]) == 1)
                        {
                            printData = printData + "A170,95,0,2,1,1,N," + DoubleQuotes + "MRNO:" + dr["MRNO"].ToString() + DoubleQuotes + "\n";
                        }
                        else
                        {
                            printData = printData + "A170,100,0,2,1,1,N," + DoubleQuotes + "OUTSIDER" + DoubleQuotes + "\n";
                        }
                        printData = printData + "A170,115,0,2,1,1,N," + DoubleQuotes + "Name:" + strName + DoubleQuotes + "\n";
                        printData = printData + "A170,138,0,2,1,1,N," + DoubleQuotes + "Specimen:" + dr["SPECIMEN_NAME"].ToString() + DoubleQuotes + "\n";
                    }
                    printData = printData + "P1\n"; //Number of copy as 1
                    */
                    #endregion
                    bool isZebraGK420 = false;
                    strName = dr["PATIENT_NAME"].ToString();
                    if (strName.Length > 30)
                    {
                        strName = strName.Substring(0, 28);
                        strName += "..";
                    }
                    printData = printData + "\nN\n";      //START
                    if (Convert.ToInt16(dr["ISSTICKER"]) == 1)
                    {
                        if (Convert.ToInt16(dr["ISPATIENT"]) == 1)
                        {
                            printData = printData + "A110,20,0,2,1,1,N," + DoubleQuotes + "MRN:" + dr["MRNO"].ToString() + DoubleQuotes + "\n";
                        }
                        else
                        {
                            printData = printData + "A110,20,0,2,1,1,N," + DoubleQuotes + "OUTSIDER" + DoubleQuotes + "\n";
                        }
                        printData = printData + "A160,190,0,2,1,1,N," + DoubleQuotes + "DOB:" + dr["DOB"].ToString() + DoubleQuotes + "\n";
                        printData = printData + "A110,40,0,2,1,1,N," + DoubleQuotes + strName + DoubleQuotes + "\n";
                        printData = printData + "A10,40,0,2,1,1,N," + DoubleQuotes + "AGE:" + Convert.ToString(dr["AGE"]) + DoubleQuotes + "\n";
                        printData = printData + "A220,40,0,2,1,1,N," + DoubleQuotes + "GENDER:" + Convert.ToString(dr["GENDER"]) + DoubleQuotes + "\n";
                        printData = printData + "A110,55,0,2,1,1,N," + DoubleQuotes + dr["SPECIMEN_NAME"].ToString() + DoubleQuotes + "\n";


                    }
                    else
                    {
                        #region CommentedByAlif
                        /*
                         * Old Code Worked on DAE
                        if (Convert.ToInt16(dr["ISPATIENT"]) == 1)
                        {
                            printData = printData + "A10,00,0,2,1,1,N," + DoubleQuotes + "MRN:" + dr["MRNO"].ToString() + DoubleQuotes + "\n";
                        }
                        else
                        {
                            printData = printData + "A10,00,0,2,1,1,N," + DoubleQuotes + "OUTSIDER" + DoubleQuotes + "\n";
                        }
                        printData = printData + "A220,00,0,2,1,1,N," + DoubleQuotes + "DOB:" + dr["DOB"].ToString() + DoubleQuotes + "\n";
                        printData = printData + "A10,20,0,2,1,1,N," + DoubleQuotes + strName + DoubleQuotes + "\n";
                        printData = printData + "A10,40,0,2,1,1,N," + DoubleQuotes + "AGE:" + Convert.ToString(dr["AGE"]) + DoubleQuotes + "\n";
                        printData = printData + "A220,40,0,2,1,1,N," + DoubleQuotes + "GENDER:" + Convert.ToString(dr["GENDER"]) + DoubleQuotes + "\n";
                        printData = printData + "A10,55,0,2,1,1,N," + DoubleQuotes + "PHY(ID):" + dr["PHYSICIAN_ID"].ToString() + DoubleQuotes + "\n";
                        printData = printData + "B120,70,0,1,2,5,70,B," + DoubleQuotes + dr["LIS_SAMPLE_NO"].ToString() + DoubleQuotes + "\n";  //barcode
                        printData = printData + "A10,180,0,2,1,1,N," + DoubleQuotes + dr["WITHDRAWAL_DATE"].ToString() + DoubleQuotes + "\n";
                        printData = printData + "A280,180,0,2,1,1,N," + DoubleQuotes + "ID:" + dr["WITHDRAWAL_ID"].ToString() + DoubleQuotes + "\n";
                        printData = printData + "A10,200,0,2,1,1,N," + DoubleQuotes + dr["SPECIMEN_NAME"].ToString() + DoubleQuotes + "\n";
                        printData = printData + "A280,200,0,2,1,1,N," + DoubleQuotes + dr["TUBE_COLOUR"].ToString() + DoubleQuotes + "\n";
                        printData = printData + "P1\n"; //Number of copy as 1
                         */
                        #endregion
                        bool isStat = false;
                        if (dr.Table.Columns.Contains("PRIORITY") && dr["PRIORITY"] != DBNull.Value)
                        {
                            isStat = true;
                        }
                        //Check the column value wether is it stat or normal
                        if (ConfigurationManager.AppSettings["IsNotZebraGK420"] != null)
                        {
                            isZebraGK420 = true;
                        }

                        if (isZebraGK420==false)
                        {

                            if (!isStat)
                            {
                                string StrContanier = Convert.ToString(dr["CONTAINER_NAME"]);
                                if (StrContanier.Length > 16)
                                {
                                    StrContanier = StrContanier.Substring(0, 14) + "...";
                                }
                                string StrSpecimen = dr["SPECIMEN_NAME"].ToString();
                                if (StrSpecimen.Length > 16)
                                {
                                    StrSpecimen = StrSpecimen.Substring(0, 16) + "...";
                                }
                                string strGender = Convert.ToString(dr["GENDER"]);
                                if (strGender.ToUpper() == "MALE")
                                {
                                    strGender = "M";
                                }
                                else if (strGender.ToUpper() == "FEMALE")
                                {
                                    strGender = "F";
                                }
                                string strWithdrawalDate = Convert.ToDateTime(dr["WITHDRAWAL_DATE"]).ToString("dd-MM-yy HH:mm");
                                //if (DateTime.TryParse(,out dtWithdrawalDate))
                                //{                            
                                //    strWithdrawalDate = dtWithdrawalDate.ToString("dd-MM-yyyy HH:mm");
                                //}
                                DateTime dtDOB = DateTime.Now;
                                string strDOB = string.Empty;
                                //if (DateTime.TryParse(dr["DOB"].ToString(), out dtDOB))
                                //{
                                //    strDOB = GetDateInSpecificStringFormat(dtDOB, DateTime.Now);
                                //}
                                strDOB = Convert.ToDateTime(dr["DOB"]).ToString("dd-MM-yyyy");//dr["DOB"].ToString();
                                printData = printData + "B170,100,0,1,2,5,70,B," + DoubleQuotes + dr["LIS_SAMPLE_NO"].ToString() + DoubleQuotes + "\n";

                                if (Convert.ToInt16(dr["ISPATIENT"]) == 1)
                                {
                                    printData = printData + "A110,30,0,2,1,1,N," + DoubleQuotes + "MRN:" + dr["MRNO"].ToString() + DoubleQuotes + "\n";
                                }
                                else
                                {
                                    printData = printData + "A20,30,0,2,1,1,N," + DoubleQuotes + "OUTSIDER" + DoubleQuotes + "\n";
                                }
                                printData = printData + "A290,30,0,2,1,1,N," + DoubleQuotes + "" + strDOB + ": " + strGender + DoubleQuotes + "\n";
                                printData = printData + "A110,50,0,2,1,1,N," + DoubleQuotes + strName + DoubleQuotes + "\n";
                                printData = printData + "A110,75,0,2,1,1,N," + DoubleQuotes + "PHY(ID):" + dr["PHYSICIAN_ID"].ToString() + DoubleQuotes + "\n";
                                printData = printData + "A320,75,0,2,1,1,N," + DoubleQuotes + "CID:" + dr["WITHDRAWAL_ID"].ToString() + DoubleQuotes + "\n";
                                printData = printData + "A110,200,0,2,1,1,N," + DoubleQuotes + strWithdrawalDate + DoubleQuotes + "\n";
                                printData = printData + "A340,200,0,2,1,1,N," + DoubleQuotes + Convert.ToString(dr["NURSING_STATION"]) + DoubleQuotes + "\n";
                                printData = printData + "A110,220,0,2,1,1,N," + DoubleQuotes + StrSpecimen + DoubleQuotes + "\n";
                                printData = printData + "A320,220,0,2,1,1,N," + DoubleQuotes + StrContanier + DoubleQuotes + "\n";
                                printData = printData + "P1\n"; //Number of copy as 1
                            }
                            else
                            {
                                string Stat = dr["PRIORITY"].ToString();//string.Empty;//Here assigned the STAT Value
                                string StrContanier = Convert.ToString(dr["CONTAINER_NAME"]);
                                if (StrContanier.Length > 16)
                                {
                                    StrContanier = StrContanier.Substring(0, 14) + "...";
                                }
                                string StrSpecimen = dr["SPECIMEN_NAME"].ToString();
                                if (StrSpecimen.Length > 16)
                                {
                                    StrSpecimen = StrSpecimen.Substring(0, 16) + "...";
                                }
                                string strGender = Convert.ToString(dr["GENDER"]);
                                if (strGender.ToUpper() == "MALE")
                                {
                                    strGender = "M";
                                }
                                else if (strGender.ToUpper() == "FEMALE")
                                {
                                    strGender = "F";
                                }
                                string strWithdrawalDate = Convert.ToDateTime(dr["WITHDRAWAL_DATE"]).ToString("dd-MM-yy HH:mm");
                                //if (DateTime.TryParse(,out dtWithdrawalDate))
                                //{                            
                                //    strWithdrawalDate = dtWithdrawalDate.ToString("dd-MM-yyyy HH:mm");
                                //}
                                DateTime dtDOB = DateTime.Now;
                                string strDOB = string.Empty;
                                //if (DateTime.TryParse(dr["DOB"].ToString(), out dtDOB))
                                //{
                                //    strDOB = GetDateInSpecificStringFormat(dtDOB, DateTime.Now);
                                //}
                                strDOB = Convert.ToDateTime(dr["DOB"]).ToString("dd-MM-yyyy");//dr["DOB"].ToString();
                                printData = printData + "B170,100,0,1,2,5,70,B," + DoubleQuotes + dr["LIS_SAMPLE_NO"].ToString() + DoubleQuotes + "\n";

                                if (Convert.ToInt16(dr["ISPATIENT"]) == 1)
                                {
                                    printData = printData + "A110,30,0,2,1,1,N," + DoubleQuotes + "MRN:" + dr["MRNO"].ToString() + DoubleQuotes + "\n";
                                }
                                else
                                {
                                    printData = printData + "A120,30,0,2,1,1,N," + DoubleQuotes + "OUTSIDER" + DoubleQuotes + "\n";
                                }
                                printData = printData + "A290,30,0,2,1,1,N," + DoubleQuotes + "" + strDOB + ": " + strGender + DoubleQuotes + "\n";
                                printData = printData + "A110,50,0,2,1,1,N," + DoubleQuotes + strName.ToUpper() + DoubleQuotes + "\n";
                                printData = printData + "A110,75,0,2,1,1,N," + DoubleQuotes + "PHY(ID):" + dr["PHYSICIAN_ID"].ToString() + DoubleQuotes + "\n";
                                printData = printData + "A275,75,0,2,1,1,N," + DoubleQuotes + "CID:" + dr["WITHDRAWAL_ID"].ToString()  + DoubleQuotes + "\n";
                                printData = printData + "A390,75,0,3,1,1,N," + DoubleQuotes +    Stat.ToUpper() + DoubleQuotes + "\n";
                                printData = printData + "A110,200,0,2,1,1,N," + DoubleQuotes + strWithdrawalDate + DoubleQuotes + "\n";
                                printData = printData + "A340,200,0,2,1,1,N," + DoubleQuotes + Convert.ToString(dr["NURSING_STATION"]) + DoubleQuotes + "\n";
                                printData = printData + "A110,220,0,2,1,1,N," + DoubleQuotes + StrSpecimen.ToUpper() + DoubleQuotes + "\n";
                                printData = printData + "A320,220,0,2,1,1,N," + DoubleQuotes + StrContanier.ToUpper() + DoubleQuotes + "\n";
                                //printData = printData + "A20,240,0,2,1,1,N," + DoubleQuotes +  Stat.ToUpper() + DoubleQuotes + "\n";
                                printData = printData + "P1\n"; //Number of copy as 1
                            }
                        }
                        else
                        {
                            if (!isStat)
                            {
                                string StrContanier = Convert.ToString(dr["CONTAINER_NAME"]);
                                if (StrContanier.Length > 16)
                                {
                                    StrContanier = StrContanier.Substring(0, 14) + "...";
                                }
                                string StrSpecimen = dr["SPECIMEN_NAME"].ToString();
                                if (StrSpecimen.Length > 16)
                                {
                                    StrSpecimen = StrSpecimen.Substring(0, 16) + "...";
                                }
                                string strGender = Convert.ToString(dr["GENDER"]);
                                if (strGender.ToUpper() == "MALE")
                                {
                                    strGender = "M";
                                }
                                else if (strGender.ToUpper() == "FEMALE")
                                {
                                    strGender = "F";
                                }
                                string strWithdrawalDate = Convert.ToDateTime(dr["WITHDRAWAL_DATE"]).ToString("dd-MM-yy HH:mm");
                                //if (DateTime.TryParse(,out dtWithdrawalDate))
                                //{                            
                                //    strWithdrawalDate = dtWithdrawalDate.ToString("dd-MM-yyyy HH:mm");
                                //}
                                DateTime dtDOB = DateTime.Now;
                                string strDOB = string.Empty;
                                //if (DateTime.TryParse(dr["DOB"].ToString(), out dtDOB))
                                //{
                                //    strDOB = GetDateInSpecificStringFormat(dtDOB, DateTime.Now);
                                //}
                                strDOB = Convert.ToDateTime(dr["DOB"]).ToString("dd-MM-yyyy");//dr["DOB"].ToString();
                                printData = printData + "B40,100,0,1,2,5,70,B," + DoubleQuotes + dr["LIS_SAMPLE_NO"].ToString() + DoubleQuotes + "\n";

                                if (Convert.ToInt16(dr["ISPATIENT"]) == 1)
                                {
                                    printData = printData + "A10,30,0,2,1,1,N," + DoubleQuotes + "MRN:" + dr["MRNO"].ToString() + DoubleQuotes + "\n";
                                }
                                else
                                {
                                    printData = printData + "A10,30,0,2,1,1,N," + DoubleQuotes + "OUTSIDER" + DoubleQuotes + "\n";
                                }
                                printData = printData + "A180,30,0,2,1,1,N," + DoubleQuotes + "" + strDOB + ": " + strGender + DoubleQuotes + "\n";
                                printData = printData + "A10,50,0,2,1,1,N," + DoubleQuotes + strName + DoubleQuotes + "\n";
                                printData = printData + "A10,75,0,2,1,1,N," + DoubleQuotes + "PHY(ID):" + dr["PHYSICIAN_ID"].ToString() + DoubleQuotes + "\n";
                                printData = printData + "A255,75,0,2,1,1,N," + DoubleQuotes + "CID:" + dr["WITHDRAWAL_ID"].ToString() + DoubleQuotes + "\n";
                                printData = printData + "A10,200,0,2,1,1,N," + DoubleQuotes + strWithdrawalDate + DoubleQuotes + "\n";
                                printData = printData + "A190,200,0,2,1,1,N," + DoubleQuotes + Convert.ToString(dr["NURSING_STATION"]) + DoubleQuotes + "\n";
                                printData = printData + "A10,220,0,2,1,1,N," + DoubleQuotes + StrSpecimen + DoubleQuotes + "\n";
                                printData = printData + "A190,220,0,2,1,1,N," + DoubleQuotes + StrContanier + DoubleQuotes + "\n";
                                printData = printData + "P1\n"; //Number of copy as 1

                            }
                            else
                            {
                                string Stat = dr["PRIORITY"].ToString();//string.Empty;
                                string StrContanier = Convert.ToString(dr["CONTAINER_NAME"]);
                                if (StrContanier.Length > 16)
                                {
                                    StrContanier = StrContanier.Substring(0, 14) + "...";
                                }
                                string StrSpecimen = dr["SPECIMEN_NAME"].ToString();
                                if (StrSpecimen.Length > 16)
                                {
                                    StrSpecimen = StrSpecimen.Substring(0, 16) + "...";
                                }
                                string strGender = Convert.ToString(dr["GENDER"]);
                                if (strGender.ToUpper() == "MALE")
                                {
                                    strGender = "M";
                                }
                                else if (strGender.ToUpper() == "FEMALE")
                                {
                                    strGender = "F";
                                }
                                string strWithdrawalDate = Convert.ToDateTime(dr["WITHDRAWAL_DATE"]).ToString("dd-MM-yy HH:mm");
                                //if (DateTime.TryParse(,out dtWithdrawalDate))
                                //{                            
                                //    strWithdrawalDate = dtWithdrawalDate.ToString("dd-MM-yyyy HH:mm");
                                //}
                                DateTime dtDOB = DateTime.Now;
                                string strDOB = string.Empty;
                                //if (DateTime.TryParse(dr["DOB"].ToString(), out dtDOB))
                                //{
                                //    strDOB = GetDateInSpecificStringFormat(dtDOB, DateTime.Now);
                                //}
                                strDOB = Convert.ToDateTime(dr["DOB"]).ToString("dd-MM-yyyy");//dr["DOB"].ToString();
                                printData = printData + "B40,100,0,1,2,5,70,B," + DoubleQuotes + dr["LIS_SAMPLE_NO"].ToString() + DoubleQuotes + "\n";

                                if (Convert.ToInt16(dr["ISPATIENT"]) == 1)
                                {
                                    printData = printData + "A10,30,0,2,1,1,N," + DoubleQuotes + "MRN:" + dr["MRNO"].ToString() + DoubleQuotes + "\n";
                                }
                                else
                                {
                                    printData = printData + "A10,30,0,2,1,1,N," + DoubleQuotes + "OUTSIDER" + DoubleQuotes + "\n";
                                }
                                printData = printData + "A190,30,0,2,1,1,N," + DoubleQuotes + "" + strDOB + ": " + strGender.ToUpper() + DoubleQuotes + "\n";
                                printData = printData + "A10,50,0,2,1,1,N," + DoubleQuotes + strName.ToUpper() + DoubleQuotes + "\n";
                                printData = printData + "A10,75,0,2,1,1,N," + DoubleQuotes + "PHY(ID):" + dr["PHYSICIAN_ID"].ToString() + DoubleQuotes + "\n";
                                printData = printData + "A180,75,0,2,1,1,N," + DoubleQuotes + "CID:" + dr["WITHDRAWAL_ID"].ToString()  + DoubleQuotes + "\n";
                                printData = printData + "A305,75,0,3,1,1,N," + DoubleQuotes + Stat.ToUpper() + DoubleQuotes + "\n";
         
                                printData = printData + "A10,200,0,2,1,1,N," + DoubleQuotes + strWithdrawalDate + DoubleQuotes + "\n";
                                printData = printData + "A190,200,0,2,1,1,N," + DoubleQuotes + Convert.ToString(dr["NURSING_STATION"]) + DoubleQuotes + "\n";
                                printData = printData + "A10,220,0,2,1,1,N," + DoubleQuotes + StrSpecimen.ToUpper() + DoubleQuotes + "\n";
                                printData = printData + "A190,220,0,2,1,1,N," + DoubleQuotes + StrContanier.ToUpper() + DoubleQuotes + "\n";
                                //printData = printData + "A275,75,0,2,1,1,N," + DoubleQuotes +  Stat.ToUpper() + DoubleQuotes + "\n";
                                printData = printData + "P1\n"; //Number of copy as 1
                            }
                        }
                    }
                }
                else if (serviceType == ServiceType.Consultation && printType == PrintType.BarCode)
                {
                    strName = dr["PROVIDER_NAME"].ToString();
                    if (strName.Length > 35)
                    {
                        strName = strName.Substring(0, 33);
                        strName = strName + "..";
                    }
                    printData = printData + "N\n";      //START

                    printData = printData + "A30,30,0,2,1,1,N," + DoubleQuotes + "MRNO: " + dr["MRNO"].ToString() + DoubleQuotes + "\n";
                    if (Convert.ToInt16(dr["ISDOCTOR"]) == 1)
                    {
                        printData = printData + "A30,60,0,2,1,1,N," + DoubleQuotes + "Dr." + strName + DoubleQuotes + "\n";
                    }
                    else
                    {
                        printData = printData + "A30,60,0,2,1,1,N," + DoubleQuotes + "Provider: " + strName + DoubleQuotes + "\n";
                    }
                    printData = printData + "A30,90,0,2,1,1,N," + DoubleQuotes + "Room No: " + dr["ROOM_NO"].ToString() + DoubleQuotes + "\n";

                    if (Convert.ToInt16(dr["ISAPPOINTMENT"]) == 1)
                    {
                        printData = printData + "A30,120,0,2,1,1,N," + DoubleQuotes + "App. Date: " + dr["APPOINTMENT_TIME"].ToString() + DoubleQuotes + "\n";
                        printData = printData + "A30,150,0,2,1,1,N," + DoubleQuotes + dr["TOKEN_NO"].ToString() + DoubleQuotes + "\n";
                    }
                    else
                    {
                        printData = printData + "A30,150,0,2,1,1,N," + DoubleQuotes + dr["TOKEN_NO"].ToString() + DoubleQuotes + "\n";
                    }

                    printData = printData + "P1\n"; //Number of copy as 1
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

                    printData = printData + "N\n";      //START
                    printData = printData + "A30,15,0,2,1,1,N," + DoubleQuotes + "NAME : " + strName + DoubleQuotes + "\n";
                    printData = printData + "A30,45,0,2,1,1,N," + DoubleQuotes + "MRNO: " + dr["MRNO"].ToString() + DoubleQuotes + "\n";
                    printData = printData + "B30,75,0,1,2,5,60,N," + DoubleQuotes + dr["MRNO"].ToString() + DoubleQuotes + "\n";  //barcode
                    printData = printData + "A30,155,0,2,1,1,N," + DoubleQuotes + "Gender/Age: " + dr["GENDER"].ToString() + "/" + dr["AGE"].ToString() + DoubleQuotes + "\n";
                    printData = printData + "A200,155,0,2,1,1,N," + DoubleQuotes + "DOB: " + dr["DOB"].ToString() + DoubleQuotes + "\n";
                    printData = printData + "A30,185,0,2,1,1,N," + DoubleQuotes + "Nationality: " + dr["NATIONALITY"].ToString() + DoubleQuotes + "\n";
                    if (dr["IDENTIFYING_DOCUMENT"] != DBNull.Value)
                    {
                        printData = printData + "A30,200,0,2,1,1,N," + DoubleQuotes + description + ": " + dr["DOCUMENT_NO"].ToString() + DoubleQuotes + "\n";
                    }

                    printData = printData + "P1\n"; //Number of copy as 1

                }
                else if (serviceType == ServiceType.Pharmacy && printType == PrintType.Prescription)
                {
                    pos = 170;
                    printData = string.Empty;
                    printData = "N\n";
                    string data = string.Empty;

                    //To generate the Medicine Name.
                    if (Convert.ToString(dr["MEDICINE_TYPE"]).Trim() == "0")
                    {
                        data = "Medicine Name: " + dr["BRAND_GENERIC"].ToString();
                    }
                    else
                    {
                        data = "Medicine Name: " + dr["BRAND_NAME"].ToString();
                    }
                    printData += AlignElements(data, serviceType, printType, CaseType.Normal);
                    printData += CR;
                    data = string.Empty;
                    //To generate the Dosage [eg:2ml 1-0-1]
                    if (Convert.ToString(dr["DOSE_PRN"]).Trim() == "0")
                    {
                        data = "Dosage: ";
                        if (dr["QUANTITY"] != DBNull.Value)
                        {
                            data += dr["QUANTITY"].ToString() + dr["QUANTITY_UNIT"].ToString() + ", ";
                        }
                        if (Convert.ToString(dr["DOSE_TYPE"]).Trim() == "1")
                        {
                            data += Convert.ToString(dr["FREQUENCY_VALUE"]) + Convert.ToString(dr["DOSE_VALUE"] + ", ");
                        }
                        else
                        {
                            data += Convert.ToString(dr["DOSE_VALUE"]);
                            if (dr["ADMIN_TIME"] != DBNull.Value)
                            {
                                data += " at " + Convert.ToString(dr["ADMIN_TIME"]);
                            }
                        }
                        printData += AlignElements(data, serviceType, printType, CaseType.Normal);
                        printData += CR;
                        data = string.Empty;
                    }
                    //To generate the Start Date,Duration & Route
                    if (Convert.ToDateTime(dr["START_DATE"]) > System.DateTime.Now.Date)
                    {
                        data = "Start Date:" + Convert.ToString(dr["START_DATE"]) + " ";

                        //Duration.
                        if (Convert.ToString(dr["ISLIFELONG"]).Trim() == "1")
                        {
                            data += "Duration: Life Long";
                        }
                        else if (dr["DURATION"] != DBNull.Value)
                        {
                            data += "Duration: ";
                            data += Convert.ToString(dr["DURATION"]) + " " + Convert.ToString(dr["DURATION_TYPE_VALUE"]);
                        }
                        printData += AlignElements(data, serviceType, printType, CaseType.Normal);
                        printData += CR;
                        data = string.Empty;
                        //Route.
                        data = "Route: " + Convert.ToString(dr["ROUTE"]) + " " + Convert.ToString(dr["FORM"]);
                        printData += AlignElements(data, serviceType, printType, CaseType.Normal);
                        printData += CR;
                    }
                    else
                    {
                        //Duration.
                        if (Convert.ToString(dr["ISLIFELONG"]).Trim() == "1")
                        {
                            data += "Duration: Life Long";
                        }
                        else if (dr["DURATION"] != DBNull.Value)
                        {
                            data += "Duration: ";
                            data += Convert.ToString(dr["DURATION"]) + " " + Convert.ToString(dr["DURATION_TYPE_VALUE"]);
                        }
                        //Route.
                        data += "Route: " + Convert.ToString(dr["ROUTE"]) + " " + Convert.ToString(dr["FORM"]);
                        printData += AlignElements(data, serviceType, printType, CaseType.Normal);
                        printData += CR;
                    }
                    data = string.Empty;
                    //To generate the instructions.
                    if (dr["SPECIAL_INSTRUCTIONS"] != DBNull.Value)
                    {
                        data = Convert.ToString(dr["SPECIAL_INSTRUCTIONS"]) + ", ";
                    }
                    if (dr["ADMINISTRATION_INSTRUCTION"] != DBNull.Value)
                    {
                        data += Convert.ToString(dr["ADMINISTRATION_INSTRUCTION"]);
                    }
                    if (dr["REMARKS"] != DBNull.Value)
                    {
                        data += ", " + Convert.ToString(dr["REMARKS"]);
                    }
                    printData += AlignElements(data, serviceType, printType, CaseType.Normal);
                    printData += CR;
                    printData += "E" + CR;
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

                    printData = printData + "N\n";      //START
                    printData = printData + "A30,15,0,2,1,1,N," + DoubleQuotes + "NAME : " + strName + DoubleQuotes + "\n";
                    printData = printData + "A30,45,0,2,1,1,N," + DoubleQuotes + "MRNO: " + dr["MRNO"].ToString() + DoubleQuotes + "\n";
                    printData = printData + "A30,75,0,2,1,1,N," + DoubleQuotes + "Age: " + dr["AGE"].ToString() + DoubleQuotes + "\n";
                    printData = printData + "A30,105,0,2,1,1,N," + DoubleQuotes + "Gender: " + dr["GENDER"].ToString() + DoubleQuotes + "\n";

                    printData = printData + "P1\n"; //Number of copy as 1
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


                    while (_name.Length > caseParse)
                    {
                        if (pos >= 170)
                        {
                            print_data += "N\n";

                            //print_data += "E" + CR;
                            //print_data += STX + "L" + CR;
                            //print_data += "D11" + CR;
                            pos = 20;
                        }
                        else
                        {
                            pos -= 20;
                            print_data += "A12," + (pos).ToString() + ",0,2,2,2,N";
                            print_data += DoubleQuotes + _name.Substring(0, caseParse) + "-" + DoubleQuotes + CR;
                            _name = _name.Substring(caseParse, _name.Length - caseParse);
                        }
                    }
                    if (pos >= 170)
                    {
                        //print_data += "E" + CR;
                        //print_data += STX + "L" + CR;
                        //print_data += "D11" + CR;
                        print_data += "N\n";
                        pos = 20;
                    }
                    pos -= 20;
                    print_data += "A12," + (pos).ToString() + ",0,2,2,2,N";
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
            try
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
            catch
            {
                throw;
            }
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
        public string GetDateInSpecificStringFormat(DateTime dateOfBirth, DateTime currentDate)
        {
            try
            {
                int years = 0;
                int months = 0;
                int days = 0;
                // Find years
                years = currentDate.Year - dateOfBirth.Year;
                // Check if the last year was a full year. 
                if (currentDate < dateOfBirth.AddYears(years) && years != 0)
                {
                    --years;
                }
                dateOfBirth = dateOfBirth.AddYears(years);
                // check dateOfBirth <= endDate and the diff between them is < 1 year. 
                if (dateOfBirth.Year == currentDate.Year)
                {
                    months = currentDate.Month - dateOfBirth.Month;
                }
                else
                {
                    months = (12 - dateOfBirth.Month) + currentDate.Month;
                }
                // Check if the last month was a full month.

                if (currentDate < dateOfBirth.AddMonths(months) && months != 0)
                {
                    --months;
                }
                dateOfBirth = dateOfBirth.AddMonths(months);
                //  dateOfBirth < endDate and is within 1 month of each other.
                days = (currentDate - dateOfBirth).Days;
                return GetFormatedAgeInString(years, months, days);
            }
            catch (Exception)
            {
                throw;
            }
        }
        private string GetFormatedAgeInString(int years, int months, int days)
        {
            try
            {
                string strAge = string.Empty;
                if (years > 0)
                {
                    strAge = years + "y";
                    if (months > 0) strAge += " " + months + "m";
                }
                else if (months > 0)
                {
                    strAge = months + "m";
                    if (days > 0) strAge += " " + days + "d";
                }
                else if (days > 0)
                {
                    strAge = days + "d";
                }
                return strAge;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private string FetchNursingStation(DataRow drDtlsSampleCollection,DataRow drMastSampleColection)
        {
            try
            {
                string nursingStationName = string.Empty;
                DataTable dtCriteria = new DataTable();
                dtCriteria.Columns.Add("MODE", typeof(int));
                dtCriteria.Columns.Add("COLLECTION_TIME", typeof(DateTime));
                dtCriteria.Columns.Add("EMR_PAT_DTLS_INV_ORDER_ID", typeof(Int64));
                dtCriteria.Columns.Add("INV_PAT_BILLING_ID", typeof(Int64));
                if (drDtlsSampleCollection.Table.Columns.Contains("EMR_PAT_DTLS_INV_ORDER_ID") && drDtlsSampleCollection["EMR_PAT_DTLS_INV_ORDER_ID"] != DBNull.Value)
                {
                    dtCriteria.Rows.Add(20, drMastSampleColection["COLLECTION_TIME"], drDtlsSampleCollection["EMR_PAT_DTLS_INV_ORDER_ID"], DBNull.Value);
                }
                else if (drDtlsSampleCollection.Table.Columns.Contains("INV_PAT_BILLING_ID") && drDtlsSampleCollection["INV_PAT_BILLING_ID"] != DBNull.Value)
                {
                    dtCriteria.Rows.Add(20, drMastSampleColection["COLLECTION_TIME"], DBNull.Value, drDtlsSampleCollection["INV_PAT_BILLING_ID"]);
                }
                if (dtCriteria.Rows.Count > 0)
                {
                    Assembly asm = Assembly.LoadFile(AppDomain.CurrentDomain.BaseDirectory + "Infologics.Medilogics.CommonShared.PharmacyMain.dll");
                    object obj = new object();
                    obj = asm.CreateInstance("Infologics.Medilogics.CommonShared.PharmacyMain.MainPharmacyShared");
                    MethodInfo mStart = null;
                    mStart = obj.GetType().GetMethod("FetchPharmacyPrintingDetails");                    
                    object[] paramDll = new object[1];
                    paramDll[0] = dtCriteria;
                    DataTable dtNursingStation = (DataTable)mStart.Invoke(obj, paramDll);
                    if (dtNursingStation.Rows.Count > 0 && dtNursingStation.Rows[0]["NURSING_STATION"]!=DBNull.Value)
                    {
                        nursingStationName = Convert.ToString(dtNursingStation.Rows[0]["NURSING_STATION"]);
                    }
                }
                return nursingStationName;
            }
            catch (Exception)
            {
                throw;
            }
        }
        //public void WriteFileLog(string data)
        //{
        //    try
        //    {

        //        string currentDate = string.Empty;
        //        string fileName = string.Empty;

        //        ////Creating the file name for the result file.                    
        //        fileName = "Log";
        //        fileName += ".txt";
        //        ////Create file in the resultant path and write the result.
        //        FileStream fsResult = new FileStream(fileName, FileMode.Append, FileAccess.Write); //File.Create(path + "\\" + fileName);

        //        StreamWriter swResult = new StreamWriter(fsResult);
        //        data = DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss.fff tt") + " : - " + data;
        //        swResult.WriteLine(data);
        //        swResult.Close();
        //        fsResult.Close();
        //    }
        //    catch (Exception)
        //    {
        //        throw new Exception("Exception Occure on file writing");
        //    }
        //}
    }
}
