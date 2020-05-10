//-----------------------------------------------------------------------
// <copyright file="DatamaxMarkIII.cs" company="Kameda Infologics PVT Ltd">
//     Copyright (c) Kameda Infologics Pvt Ltd. All rights reserved.
// </copyright>
// <author>Anish C.G </author>
//<Date> 26-Aug-2011 <Date>
//-----------------------------------------------------------------------

namespace Infologics.Medilogics.PrintingLibrary.DatamaxMarkIII
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Data;
    using System.Windows.Forms;
    using System.Drawing.Printing;

    using Infologics.Medilogics.PrintingLibrary.Main;
    using Infologics.Medilogics.Enumerators.General;
    using Infologics.Medilogics.General.Control;
    using Infologics.Medilogics.CommonClient.Controls.CommonFunctions;


    public class BLDatamaxMarkIII : IPrinting
    {
        private static string CR = Convert.ToString((char)0x0D);
        private static string STX = Convert.ToString((char)0x02);
        private static string DoubleQuotes = Convert.ToString((char)0x22);
        private long pos = 0;

        #region IPrinting Members

        public bool Print(DataSet dsData, ServiceType serviceType, string PrinterName, PrintType printType)
        {
            bool printStatus = false;
            printStatus = RawPrinting(dsData, serviceType, PrinterName, printType);
            return printStatus;
        }

        public bool Print(DataSet dsData, ServiceType serviceType, string PrinterName)
        {
            bool printStatus = false;
            printStatus = RawPrinting(dsData, serviceType, PrinterName, PrintType.BarCode);
            return printStatus;
            //throw new NotImplementedException();
        }

        #endregion

        private bool RawPrinting(DataSet dsData, ServiceType serviceType, string rawPrinterName, PrintType printType)
        {
            bool printStatus = false;
            string printData = string.Empty;
            string docName = string.Empty;
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
            if (serviceType == ServiceType.Pharmacy && printType == PrintType.Prescription)
            {
                docName = "Prescription";
                if (dsData != null)
                {
                    dtPrintData = SelectDataToPrint(dsData, serviceType, printType);
                    if (dtPrintData != null)
                    {
                        DataColumn dcBarCode=new DataColumn("BARCODE_COUNT",typeof(Int32));
                        dcBarCode.DefaultValue = 1;
                        dtPrintData.Columns.Add(dcBarCode);
                        foreach (DataRow dr in dtPrintData.Rows)
                        {
                            printData = AlignSampleToPrint(dr, serviceType, printType);
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
            else if (serviceType == ServiceType.Investigation && printType == PrintType.BarCode)
            {
                docName = "Investigation";
                if (dsData != null)
                {
                    dtPrintData = SelectDataToPrint(dsData, serviceType, printType);
                    if (dtPrintData != null)
                    {
                        foreach (DataRow dr in dtPrintData.Rows)
                        {
                            printData = AlignSampleToPrint(dr, serviceType, printType);
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

        private DataTable SelectDataToPrint(DataSet dsData, ServiceType serviceType, PrintType printType)
        {
            Common objComman = new Common();
            DataTable dtPrintData = new DataTable("PrintData");
            try
            {
                if (serviceType == ServiceType.Pharmacy && printType == PrintType.Prescription)
                {
                    if (dsData.Tables["INV_PAT_BILLING"] != null && dsData.Tables["Detail"] != null)
                    {
                        var BrandQuery = from emr in dsData.Tables["Detail"].AsEnumerable().Where(r => r.RowState != DataRowState.Deleted)
                                         join inv in dsData.Tables["INV_PAT_BILLING"].AsEnumerable().Where(r => r.RowState != DataRowState.Deleted)
                                         on emr.Field<Decimal>("EMR_PAT_DTLS_INV_ORDER_ID") equals
                                         inv.Field<Decimal>("EMR_PAT_DTLS_MEDICATION_ID")
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
                                         on Mast.Field<Decimal>("FREQUENCY") equals
                                         emr.Field<Decimal>("EMR_LOOKUP_ID")
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
                else if (serviceType == ServiceType.Investigation && printType == PrintType.BarCode)
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
                            drNew["ISPATIENT"] = Convert.ToDecimal(dr["ISPATIENT"].ToString());
                            dtPrintData.Rows.Add(drNew);
                        }
                        ////
                    }
                }
               
                return dtPrintData;
            }
            catch
            {
                throw;
            }
        }

        private string AlignSampleToPrint(DataRow drData, ServiceType serviceType, PrintType printType)
        {
            string data = string.Empty;
            string DataPrint = string.Empty;
            string strName = string.Empty;
            try
            {
                if (serviceType == ServiceType.Pharmacy && printType == PrintType.Prescription)
                {
                    pos = 191100301700012;
                    DataPrint = string.Empty;
                    DataPrint = STX + "L" + CR;
                    DataPrint += "D11" + CR;

                    //To generate the Medicine Name.
                    if (Convert.ToString(drData["MEDICINE_TYPE"]).Trim() == "0")
                    {
                        data = "Medicine Name: " + drData["BRAND_GENERIC"].ToString();
                    }
                    else
                    {
                        data = "Medicine Name: " + drData["BRAND_NAME"].ToString();
                    }
                    DataPrint += AlignElements(data, serviceType, printType,CaseType.Normal);
                    DataPrint += CR;
                    data = string.Empty;
                    //To generate the Dosage [eg:2ml 1-0-1]
                    if (Convert.ToString(drData["DOSE_PRN"]).Trim() == "0")
                    {
                        data = "Dosage: ";
                        if (drData["QUANTITY"] != DBNull.Value)
                        {
                            data +=drData["QUANTITY"].ToString() + drData["QUANTITY_UNIT"].ToString()+", ";
                        }
                        if (Convert.ToString(drData["DOSE_TYPE"]).Trim() == "1")
                        {
                            data += Convert.ToString(drData["FREQUENCY_VALUE"]) + Convert.ToString(drData["DOSE_VALUE"]+", ");
                        }
                        else
                        {
                            data +=Convert.ToString(drData["DOSE_VALUE"]);
                            if (drData["ADMIN_TIME"] != DBNull.Value)
                            {
                                data += " at " + Convert.ToString(drData["ADMIN_TIME"]);
                            }
                        }                       
                        DataPrint += AlignElements(data, serviceType, printType,CaseType.Normal);
                        DataPrint += CR;
                        data = string.Empty;
                    }
                    //To generate the Start Date,Duration & Route
                    if (Convert.ToDateTime(drData["START_DATE"]) > System.DateTime.Now.Date)
                    {
                        data = "Start Date:" + Convert.ToString(drData["START_DATE"])+" ";

                        //Duration.
                        if(Convert.ToString(drData["ISLIFELONG"]).Trim()=="1")
                        {
                            data += "Duration: Life Long";
                        }
                        else if (drData["DURATION"] != DBNull.Value)
                        {
                            data += "Duration: ";
                            data += Convert.ToString(drData["DURATION"]) + " " + Convert.ToString(drData["DURATION_TYPE_VALUE"]);
                        }
                        DataPrint += AlignElements(data, serviceType, printType, CaseType.Normal);
                        DataPrint += CR;
                        data = string.Empty;
                        //Route.
                        data = "Route: " + Convert.ToString(drData["ROUTE"]) + " " + Convert.ToString(drData["FORM"]);
                        DataPrint += AlignElements(data, serviceType, printType, CaseType.Normal);
                        DataPrint += CR;
                    }
                    else
                    {
                        //Duration.
                        if (Convert.ToString(drData["ISLIFELONG"]).Trim() == "1")
                        {
                            data += "Duration: Life Long";
                        }
                        else if (drData["DURATION"] != DBNull.Value)
                        {
                            data += "Duration: ";
                            data += Convert.ToString(drData["DURATION"]) + " " + Convert.ToString(drData["DURATION_TYPE_VALUE"]);
                        }                   
                        //Route.
                        data += "Route: " + Convert.ToString(drData["ROUTE"]) + " " + Convert.ToString(drData["FORM"]);
                        DataPrint += AlignElements(data, serviceType, printType, CaseType.Normal);
                        DataPrint += CR;
                    }
                    data = string.Empty;
                    //To generate the instructions.
                    if (drData["SPECIAL_INSTRUCTIONS"] != DBNull.Value)
                    {
                        data = Convert.ToString(drData["SPECIAL_INSTRUCTIONS"]) + ", ";
                    }
                    if (drData["ADMINISTRATION_INSTRUCTION"] != DBNull.Value)
                    {
                        data += Convert.ToString(drData["ADMINISTRATION_INSTRUCTION"]);
                    }
                    if (drData["REMARKS"] != DBNull.Value)
                    {
                        data += ", " + Convert.ToString(drData["REMARKS"]);
                    }
                    DataPrint += AlignElements(data, serviceType, printType,CaseType.Normal);
                    DataPrint += CR;
                    DataPrint += "E" + CR;
                }
                    /////////////////////////////////////////////////////////////////////////////////////////
                else if (serviceType == ServiceType.Investigation && printType == PrintType.BarCode)
                {
                    pos = 191100301700012;
                    DataPrint = string.Empty;
                    DataPrint = STX + "L" + CR;
                    DataPrint += "D11" + CR;
                    strName = drData["PATIENT_NAME"].ToString();

                    if (strName.Length > 30)
                    {
                        strName = strName.Substring(0, 28);
                        strName = strName + "..";
                    }
                    //DataPrint +
                    //printData = printData + "N\n";      //START

                    if (Convert.ToInt16(drData["ISSTICKER"]) == 1)
                    {
                        if (drData["BILL_NO"] != DBNull.Value || drData["BILL_NO"].ToString() != String.Empty)
                        {
                            DataPrint += "191100101800050" +"Bill No:" + drData["BILL_NO"].ToString()+"\n";
                        }
                        else
                        {
                            DataPrint += "191100101800050" + "Not Billed"  + "\n";
                        }
                        if (Convert.ToInt16(drData["ISPATIENT"]) == 1)
                        {
                            DataPrint += "191100101500050" + "MRNO:" + drData["MRNO"].ToString() + "\n";
                        }
                        else
                        {
                            DataPrint += "191100101500050" + "OUTSIDER" + "\n";
                        }
                        DataPrint += "191100101100050" + "Name:" + strName + "\n";
                        DataPrint += "191100100800050" + "Specimen:" + drData["SPECIMEN_NAME"].ToString() + "\n";
                    }
                    else
                    {
                        DataPrint += "1e2206000100050" + drData["LIS_SAMPLE_NO"].ToString() + "\n";

                        if (Convert.ToInt16(drData["ISPATIENT"]) == 1)
                        {
                            DataPrint += "191100101800050" + "MRNO: " + drData["MRNO"].ToString() + "\n";
                        }
                        else
                        {
                            DataPrint += "191100101800050" + "OUTSIDER" + "\n";
                        }
                        DataPrint += "191100101500050" + "Name: " + strName + "\n";
                        DataPrint += "191100101200050" + "Specimen: " + drData["SPECIMEN_NAME"].ToString() + "\n";
                        DataPrint += "191100100900050" + "Sample ID: " + drData["LIS_SAMPLE_NO"].ToString() + "\n";
                    }
                    DataPrint += "E" + CR;
                }
                return DataPrint;
            }
            catch
            {
                return null;
            }
        }

        private string AlignElements(string name, ServiceType serviceType, PrintType printType,CaseType _caseType)
        {
            try
            {
                string _name = name;
                string print_data = string.Empty;
                int caseParse=0;
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
                                caseParse=42;
                                break;
                            }
                        case CaseType.Lower:
                            {
                                break;
                            }
                    }


                    while (_name.Length > caseParse)
                    {
                        if (pos <= 191100300100012)
                        {
                            print_data += "E" + CR;
                            print_data += STX + "L" + CR;
                            print_data += "D11" + CR;
                            pos = 191100301700012;
                        }
                        else
                        {
                            pos -= 200000;
                            print_data += (pos).ToString();
                            print_data += _name.Substring(0, caseParse) + "-" + CR;
                            _name = _name.Substring(caseParse, _name.Length - caseParse);
                        }
                    }
                    if (pos <= 191100300100012)
                    {
                        print_data += "E" + CR;
                        print_data += STX + "L" + CR;
                        print_data += "D11" + CR;
                        pos = 191100301700012;
                    }
                    pos -= 200000;
                    print_data += (pos).ToString();
                    print_data += _name.Substring(0, _name.Length);
                    //print_data += CR;
                }
                return print_data;
            }
            catch
            {
                throw;
            }
        }

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
      
    }
}
