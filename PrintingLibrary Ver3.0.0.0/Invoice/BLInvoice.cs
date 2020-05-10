//-----------------------------------------------------------------------
// <copyright file="BLInvoice.cs" company="Kameda Infologics PVT Ltd">
//     Copyright (c) Kameda Infologics Pvt Ltd. All rights reserved.
// </copyright>
// <author>Biju S J</author>
//<Date>22-Mar-2010<Date>
//-----------------------------------------------------------------------

namespace Infologics.Medilogics.PrintingLibrary.Invoice
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Data;
    using Infologics.Medilogics.PrintingLibrary.Main;
    using Infologics.Medilogics.Enumerators.General;
    using System.Windows.Forms;
    using System.Drawing.Printing;
    using Infologics.Medilogics.CommonClient.Controls.StaticData;

    public class BLInvoice : IPrinting
    {
        
        #region IPrinting Members
        /// <summary>
        /// Function to print data in Datamax Barcode printer
        /// </summary>
        /// <param name="dsData">Data set which contains the data to print</param>
        /// <param name="serviceType">Type of service</param>
        /// <param name="PrinterName">Name of the Datamax printer</param>
        /// <returns></returns>
        public bool Print(DataSet dsData, ServiceType serviceType, string PrinterName)
        {
            bool printStatus = false;
            printStatus = RawPrinting(dsData, serviceType, PrinterName);
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
        private bool RawPrinting(DataSet dsData, ServiceType serviceType, string rawPrinterName)
        {
            bool printStatus = false;
            string printData = String.Empty;
            string docName = String.Empty;
            DataTable dtPrintData = new DataTable();
            if (dsData != null)
            {
                dtPrintData = SelectDataToPrint(dsData, serviceType);
                PrintDialog objPd = new PrintDialog();
                objPd.PrinterSettings = new PrinterSettings();
                if (rawPrinterName == String.Empty)
                {
                    objPd.ShowDialog();
                }
                else
                {
                    objPd.PrinterSettings.PrinterName = rawPrinterName;
                }
                if (dtPrintData != null && dtPrintData.Rows.Count > 0)
                {
                    
                    if (serviceType == ServiceType.Registration)
                    {
                        docName = "Registration Invoice";
                        foreach (DataRow dr in dtPrintData.Rows)
                        {
                            printStatus = false;
                            printData = AlignRegistration(dr);
                            if (printData.Length > 0)
                            {
                                printStatus = RawPrinterHelper.SendStringToPrinter(objPd.PrinterSettings.PrinterName, printData, docName);
                            }
                            if (printStatus == false)
                            {
                                break;
                            }
                        }
                    }
                    else if (serviceType == ServiceType.ReRegistration)
                    {
                        docName = "Re-Registration Invoice";
                        foreach (DataRow dr in dtPrintData.Rows)
                        {
                            printStatus = false;
                            printData = AlignReRegistration(dr);
                            if (printData.Length > 0)
                            {
                                printStatus = RawPrinterHelper.SendStringToPrinter(objPd.PrinterSettings.PrinterName, printData, docName);
                            }
                            if (printStatus == false)
                            {
                                break;
                            }
                        }
                    }
                    else if (serviceType == ServiceType.Consultation)
                    {
                        docName = "Consultation Invoice";
                        foreach (DataRow dr in dtPrintData.Rows)
                        {
                            printStatus = false;
                            printData = AlignConsultation(dr);
                            if (printData.Length > 0)
                            {
                                printStatus = RawPrinterHelper.SendStringToPrinter(objPd.PrinterSettings.PrinterName, printData, docName);
                            }
                            if (printStatus == false)
                            {
                                break;
                            }
                        }
                    }
                    else if (serviceType == ServiceType.Cafeteria)
                    {
                        docName = "Cafeteria Invoice";
                        //foreach (DataRow dr in dtPrintData.Rows)
                        //{
                        printStatus = false;
                        printData = AlignCafeteria(dsData);
                        if (printData.Length > 0)
                        {
                            printStatus = RawPrinterHelper.SendStringToPrinter(objPd.PrinterSettings.PrinterName, printData, docName);
                        }
                        //if (printStatus == false)
                        //{
                        //    break;
                        //}
                        //}
                    }
                }
                else
                {
                    if (serviceType == ServiceType.Cafeteria)
                    {
                        docName = "Cafeteria Invoice";
                        //foreach (DataRow dr in dtPrintData.Rows)
                        //{
                        printStatus = false;
                        printData = AlignCafeteria(dsData);
                        if (printData.Length > 0)
                        {
                            printStatus = RawPrinterHelper.SendStringToPrinter(objPd.PrinterSettings.PrinterName, printData, docName);
                        }
                        //if (printStatus == false)
                        //{
                        //    break;
                        //}
                        //}
                    }
                }
            }
            return printStatus;
        }

        /// <summary>
        /// Select necessary data to print from dataset and copy to datatable
        /// </summary>
        /// <param name="dsData">Dataset from which data required to print is to select</param>
        /// <param name="service">Type of Service</param>
        /// <returns>Datatable which contains only necessary data to print</returns>
        private DataTable SelectDataToPrint(DataSet dsData,ServiceType service)
        {
            DataTable dtPrintData = new DataTable("PrintData");
            int i = 0;
            if (service == ServiceType.Registration)
            {
                if (dsData.Tables["PRINT_INVOICE_DATA"] != null && dsData.Tables["PRINT_INVOICE_DATA"].Rows.Count > 0)
                {
                    //Create table to copy data to print                
                    dtPrintData.Columns.Add("FIRST_NAME", System.Type.GetType("System.String"));
                    dtPrintData.Columns.Add("MIDDLE_NAME", System.Type.GetType("System.String"));
                    dtPrintData.Columns.Add("LAST_NAME", System.Type.GetType("System.String"));
                    dtPrintData.Columns.Add("MRNO", System.Type.GetType("System.String"));
                    dtPrintData.Columns.Add("ISDUPLICATE", System.Type.GetType("System.Int32"));
                    dtPrintData.Columns.Add("REG_TYPE", System.Type.GetType("System.String"));
                    dtPrintData.Columns.Add("REG_CHARGE", System.Type.GetType("System.String"));
                    dtPrintData.Columns.Add("REG_FEE", System.Type.GetType("System.String"));
                    dtPrintData.Columns.Add("REG_DATE", System.Type.GetType("System.String"));
                    dtPrintData.Columns.Add("BILL_NO", System.Type.GetType("System.String"));
                    dtPrintData.Columns.Add("BILL_TYPE", System.Type.GetType("System.String"));
                    dtPrintData.Columns.Add("PRINT_MSG", System.Type.GetType("System.String"));
                    dtPrintData.Columns.Add("USER_ID", System.Type.GetType("System.String"));
                    ////
                    //Add necessary data from dataset to datatable to print
                    foreach (DataRow dr in dsData.Tables["PRINT_INVOICE_DATA"].Rows)                    
                    {
                        dtPrintData.Rows.Add();
                        dtPrintData.Rows[i]["FIRST_NAME"] = dr["FIRST_NAME"].ToString();
                        dtPrintData.Rows[i]["MIDDLE_NAME"] = dr["MIDDLE_NAME"].ToString();
                        dtPrintData.Rows[i]["LAST_NAME"] = dr["LAST_NAME"].ToString();
                        dtPrintData.Rows[i]["MRNO"] = dr["MRNO"].ToString();
                        dtPrintData.Rows[i]["ISDUPLICATE"] = Convert.ToInt32(dr["ISDUPLICATE"].ToString());
                        dtPrintData.Rows[i]["REG_TYPE"] = dr["REG_TYPE"].ToString();
                        dtPrintData.Rows[i]["REG_CHARGE"] = dr["REG_CHARGE"].ToString();
                        dtPrintData.Rows[i]["REG_FEE"] = Convert.ToDouble(dr["REG_FEE"].ToString());
                        dtPrintData.Rows[i]["REG_DATE"] = dr["REG_DATE"].ToString();
                        dtPrintData.Rows[i]["BILL_NO"] = dr["BILL_NO"].ToString();
                        dtPrintData.Rows[i]["BILL_TYPE"] = dr["BILL_TYPE"].ToString();
                        dtPrintData.Rows[i]["PRINT_MSG"] = dr["PRINT_MSG"].ToString();
                        dtPrintData.Rows[i]["LAST_NAME"] = dr["LAST_NAME"].ToString();
                        dtPrintData.Rows[i]["USER_ID"] = dr["USER_ID"].ToString();
                        i++;
                    }
                    ////
                }
            }
            else if (service == ServiceType.ReRegistration)
            {
                if (dsData.Tables["PRINT_INVOICE_DATA"] != null && dsData.Tables["PRINT_INVOICE_DATA"].Rows.Count > 0)
                {
                    //Create table to print                
                    dtPrintData.Columns.Add("FIRST_NAME", System.Type.GetType("System.String"));
                    dtPrintData.Columns.Add("MIDDLE_NAME", System.Type.GetType("System.String"));
                    dtPrintData.Columns.Add("LAST_NAME", System.Type.GetType("System.String"));
                    dtPrintData.Columns.Add("MRNO", System.Type.GetType("System.String"));
                    dtPrintData.Columns.Add("ISDUPLICATE", System.Type.GetType("System.Int32"));
                    dtPrintData.Columns.Add("REG_TYPE", System.Type.GetType("System.String"));
                    dtPrintData.Columns.Add("REG_CHARGE", System.Type.GetType("System.String"));
                    dtPrintData.Columns.Add("REG_FEE", System.Type.GetType("System.String"));
                    dtPrintData.Columns.Add("REG_DATE", System.Type.GetType("System.String"));
                    dtPrintData.Columns.Add("BILL_NO", System.Type.GetType("System.String"));
                    dtPrintData.Columns.Add("BILL_TYPE", System.Type.GetType("System.String"));
                    dtPrintData.Columns.Add("PRINT_MSG", System.Type.GetType("System.String"));
                    dtPrintData.Columns.Add("USER_ID", System.Type.GetType("System.String"));
                    ////
                    //Add necessary data from dataset to datatable to print
                    foreach (DataRow dr in dsData.Tables["PRINT_INVOICE_DATA"].Rows)                    
                    {
                        dtPrintData.Rows.Add();
                        dtPrintData.Rows[i]["FIRST_NAME"] = dr["FIRST_NAME"].ToString();
                        dtPrintData.Rows[i]["MIDDLE_NAME"] = dr["MIDDLE_NAME"].ToString();
                        dtPrintData.Rows[i]["LAST_NAME"] = dr["LAST_NAME"].ToString();
                        dtPrintData.Rows[i]["MRNO"] = dr["MRNO"].ToString();
                        dtPrintData.Rows[i]["ISDUPLICATE"] = Convert.ToInt32(dr["ISDUPLICATE"].ToString());
                        dtPrintData.Rows[i]["REG_TYPE"] = dr["REG_TYPE"].ToString();
                        dtPrintData.Rows[i]["REG_CHARGE"] = dr["REG_CHARGE"].ToString();
                        dtPrintData.Rows[i]["REG_FEE"] = dr["REG_FEE"].ToString();
                        dtPrintData.Rows[i]["REG_DATE"] = dr["REG_DATE"].ToString();
                        dtPrintData.Rows[i]["BILL_NO"] = dr["BILL_NO"].ToString();
                        dtPrintData.Rows[i]["BILL_TYPE"] = dr["BILL_TYPE"].ToString();
                        dtPrintData.Rows[i]["PRINT_MSG"] = dr["PRINT_MSG"].ToString();
                        dtPrintData.Rows[i]["LAST_NAME"] = dr["LAST_NAME"].ToString();
                        dtPrintData.Rows[i]["USER_ID"] = dr["USER_ID"].ToString();
                        i++;
                    }
                    ////
                }
            }
            else if (service == ServiceType.Consultation)
            {
                if (dsData.Tables["PRINT_INVOICE_DATA"] != null && dsData.Tables["PRINT_INVOICE_DATA"].Rows.Count > 0)
                {
                    //Create table to print                
                    dtPrintData.Columns.Add("FIRST_NAME", System.Type.GetType("System.String"));
                    dtPrintData.Columns.Add("MIDDLE_NAME", System.Type.GetType("System.String"));
                    dtPrintData.Columns.Add("LAST_NAME", System.Type.GetType("System.String"));
                    dtPrintData.Columns.Add("MRNO", System.Type.GetType("System.String"));
                    dtPrintData.Columns.Add("ISDUPLICATE", System.Type.GetType("System.Int32"));
                    dtPrintData.Columns.Add("CONSULT_DATE", System.Type.GetType("System.String"));
                    dtPrintData.Columns.Add("CONSULT_TYPE", System.Type.GetType("System.String"));
                    dtPrintData.Columns.Add("CONSULT_CHARGE", System.Type.GetType("System.String"));
                    dtPrintData.Columns.Add("BILL_NO", System.Type.GetType("System.String"));
                    dtPrintData.Columns.Add("BILL_TYPE", System.Type.GetType("System.String"));
                    dtPrintData.Columns.Add("PROVIDER_NAME", System.Type.GetType("System.String"));
                    dtPrintData.Columns.Add("DEPARTMENT_NAME", System.Type.GetType("System.String"));
                    dtPrintData.Columns.Add("ISAPPOINTMENT", System.Type.GetType("System.String"));
                    dtPrintData.Columns.Add("TOKEN_NO", System.Type.GetType("System.String"));
                    dtPrintData.Columns.Add("APPOINTMENT_TIME", System.Type.GetType("System.String"));
                    dtPrintData.Columns.Add("PRINT_MSG", System.Type.GetType("System.String"));
                    dtPrintData.Columns.Add("USER_ID", System.Type.GetType("System.String"));
                    ////
                    //Add necessary data from dataset to datatable to print
                    foreach (DataRow dr in dsData.Tables["PRINT_INVOICE_DATA"].Rows)                    
                    {
                        dtPrintData.Rows.Add();
                        dtPrintData.Rows[i]["FIRST_NAME"] = dr["FIRST_NAME"].ToString();
                        dtPrintData.Rows[i]["MIDDLE_NAME"] = dr["MIDDLE_NAME"].ToString();
                        dtPrintData.Rows[i]["LAST_NAME"] = dr["LAST_NAME"].ToString();
                        dtPrintData.Rows[i]["MRNO"] = dr["MRNO"].ToString();
                        dtPrintData.Rows[i]["ISDUPLICATE"] = Convert.ToInt32(dr["ISDUPLICATE"].ToString());
                        dtPrintData.Rows[i]["CONSULT_DATE"] = dr["CONSULT_DATE"].ToString();
                        dtPrintData.Rows[i]["CONSULT_TYPE"] = dr["CONSULT_TYPE"].ToString();
                        dtPrintData.Rows[i]["CONSULT_CHARGE"] = dr["CONSULT_CHARGE"].ToString();
                        dtPrintData.Rows[i]["BILL_NO"] = dr["BILL_NO"].ToString();
                        dtPrintData.Rows[i]["BILL_TYPE"] = dr["BILL_TYPE"].ToString();
                        dtPrintData.Rows[i]["PROVIDER_NAME"] = dr["PROVIDER_NAME"].ToString();
                        dtPrintData.Rows[i]["DEPARTMENT_NAME"] = dr["DEPARTMENT_NAME"].ToString();
                        dtPrintData.Rows[i]["ISAPPOINTMENT"] = dr["ISAPPOINTMENT"].ToString();
                        dtPrintData.Rows[i]["TOKEN_NO"] = dr["TOKEN_NO"].ToString();
                        dtPrintData.Rows[i]["APPOINTMENT_TIME"] = dr["APPOINTMENT_TIME"].ToString();
                        dtPrintData.Rows[i]["PRINT_MSG"] = dr["PRINT_MSG"].ToString();
                        dtPrintData.Rows[i]["USER_ID"] = dr["USER_ID"].ToString();
                        i++;
                    }                               
                    ////
                }
            }
            return dtPrintData;
        }

        /// <summary>
        /// Function to allign the Registration data to print
        /// </summary>
        /// <param name="dt">Registration Information</param>
        /// <returns>formated string to be send to printer</returns>
        private string AlignRegistration(DataRow dr)
        {
            string printData = string.Empty;
            string patientName = string.Empty;            

            try
            {
                int iSpace = 0;
                int sp = 0;
                
                int count = 0;
                string billprinter = String.Empty;
                count = 1;
                patientName = dr["FIRST_NAME"].ToString() + " " + dr["MIDDLE_NAME"].ToString() + " " + dr["LAST_NAME"].ToString();


                for (int i = 0; i <= 6; i++)
                {
                    printData += Convert.ToString((char)27) + "j" + Convert.ToString((char)72) + "\n";
                }
                printData += "\n";

                if (dr["ISDUPLICATE"].ToString().Equals("1"))
                {
                    if (patientName.Length > 19)
                    {
                        sp = 3;
                        printData += new String(' ', 6) + patientName.Substring(0, 19) + new String(' ', sp - 2) + "[Duplicate]" + new String(' ', 4) + Convert.ToDateTime(dr["REG_DATE"]).ToString("dd-MMM-yy") + "\n";
                    }
                    else
                    {
                        sp = 19 - (patientName.Length) + 3;
                        printData += new String(' ', 6) + patientName + new String(' ', sp - 2) + "[Duplicate]" + new String(' ', 4) + Convert.ToDateTime(dr["REG_DATE"]).ToString("dd-MMM-yy") + "\n";
                    }
                }
                else
                {
                    if (patientName.Length > 32)
                    {
                        sp = 3;
                        printData += new String(' ', 6) + patientName.Substring(0, 32) + new String(' ', sp - 1) + new String(' ', 1) + Convert.ToDateTime(dr["REG_DATE"]).ToString("dd-MMM-yy") + "\n";
                    }
                    else
                    {
                        sp = 32 - (patientName.Length) + 3;
                        printData += new String(' ', 6) + patientName + new String(' ', sp - 1) + new String(' ', 1) + Convert.ToDateTime(dr["REG_DATE"]).ToString("dd-MMM-yy") + "\n";
                    }
                }
                printData += new String(' ', 6) + dr["MRNO"].ToString() + new String(' ', 15 - dr["MRNO"].ToString().Length) + "[" + dr["BILL_TYPE"].ToString() + "]" + new String(' ', 4) + "Bill No: " + dr["BILL_NO"].ToString() + "\n";

                for (int j = 1; j <= 5; j++)
                {
                    printData += "\n";
                }
                if (dr["REG_TYPE"].ToString().Length <= 12)
                {
                    printData += new String(' ', 1) + dr["PRINT_MSG"].ToString() + "(" + Convert.ToString(dr["REG_TYPE"]) + ")" + new String(' ', 41 - dr["PRINT_MSG"].ToString().Length - dr["REG_TYPE"].ToString().Length - dr["REG_CHARGE"].ToString().Length) + dr["REG_FEE"].ToString() + "\n";
                }
                else
                {
                    iSpace = dr["REG_TYPE"].ToString().Length - 12;

                    printData += new String(' ', 1) + dr["PRINT_MSG"].ToString() + "(" + Convert.ToString(dr["REG_TYPE"]).Substring(0, 12) + new String(' ', 42 - dr["PRINT_MSG"].ToString().Length - 12 - dr["REG_CHARGE"].ToString().Length) + dr["REG_FEE"].ToString() + "\n";
                    count++;
                    printData += new String(' ', 1) + dr["REG_TYPE"].ToString().Substring(12, iSpace) + ")" + "\n";
                }

                for (int j = 1; j <= 9 - count; j++)
                {
                    printData += "\n";
                }
                printData += new String(' ', 5) + "Billed By :" + dr["USER_ID"].ToString() + new String(' ', 18 - dr["USER_ID"].ToString().Length) + new String(' ', 10 - dr["REG_CHARGE"].ToString().Length) + dr["REG_FEE"].ToString() + "\n";

                for (int j = 1; j <= 13; j++)
                {
                    printData += "\n";
                }
                return printData;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Function to allign the Re-Registration data to print
        /// </summary>
        /// <param name="dt">ReRegistration Information</param>
        /// <returns>formated string</returns>
        private string AlignReRegistration(DataRow dr)
        {
            string printData = string.Empty;
            string patientName = string.Empty;            

            try
            {
                int iSpace = 0;
                int sp = 0;                
                int count = 0;
                string billprinter = String.Empty;
                count = 1;
                patientName = dr["FIRST_NAME"].ToString() + " " + dr["MIDDLE_NAME"].ToString() + " " + dr["LAST_NAME"].ToString();

                for (int i = 0; i <= 6; i++)
                {
                    printData += Convert.ToString((char)27) + "j" + Convert.ToString((char)72) + "\n";
                }
                printData += "\n";
                if (dr["ISDUPLICATE"].ToString().Equals("1"))
                {
                    if (patientName.Length > 19)
                    {
                        sp = 3;
                        printData += new String(' ', 6) + patientName.Substring(0, 19) + new String(' ', sp - 2) + "[Duplicate]" + new String(' ', 4) + Convert.ToDateTime(dr["REG_DATE"]).ToString("dd-MMM-yy") + "\n";
                    }
                    else
                    {
                        sp = 19 - (patientName.Length) + 3;
                        printData += new String(' ', 6) + patientName + new String(' ', sp - 2) + "[Duplicate]" + new String(' ', 4) + Convert.ToDateTime(dr["REG_DATE"]).ToString("dd-MMM-yy") + "\n";
                    }
                }
                else
                {
                    if (patientName.Length > 32)
                    {
                        sp = 3;
                        printData += new String(' ', 6) + patientName.Substring(0, 32) + new String(' ', sp - 1) + new String(' ', 1) + Convert.ToDateTime(dr["REG_DATE"]).ToString("dd-MMM-yy") + "\n";
                    }
                    else
                    {
                        sp = 32 - (patientName.Length) + 3;
                        printData += new String(' ', 6) + patientName + new String(' ', sp - 1) + new String(' ', 1) + Convert.ToDateTime(dr["REG_DATE"]).ToString("dd-MMM-yy") + "\n";
                    }
                }
                printData += new String(' ', 6) + dr["MRNO"].ToString() + new String(' ', 15 - dr["MRNO"].ToString().Length) + "[" + dr["BILL_TYPE"].ToString() + "]" + new String(' ', 4) + "Bill No: " + dr["BILL_NO"].ToString() + "\n";

                for (int j = 1; j <= 5; j++)
                {
                    printData += "\n";
                }
                if (dr["REG_TYPE"].ToString().Length <= 12)
                {
                    printData += new String(' ', 1) + dr["PRINT_MSG"].ToString() + "(" + Convert.ToString(dr["REG_TYPE"]) + ")" + new String(' ', 41 - dr["PRINT_MSG"].ToString().Length - dr["REG_TYPE"].ToString().Length - dr["REG_CHARGE"].ToString().Length) + dr["REG_FEE"].ToString() + "\n";
                }
                else
                {
                    iSpace = dr["REG_TYPE"].ToString().Length - 12;

                    printData += new String(' ', 1) + dr["PRINT_MSG"].ToString() + "(" + Convert.ToString(dr["REG_TYPE"]).Substring(0, 12) + new String(' ', 42 - dr["PRINT_MSG"].ToString().Length - 12 - dr["REG_CHARGE"].ToString().Length) + dr["REG_FEE"].ToString() + "\n";
                    count++;
                    printData += new String(' ', 1) + dr["REG_TYPE"].ToString().Substring(12, iSpace) + ")" + "\n";
                }

                for (int j = 1; j <= 9 - count; j++)
                {
                    printData += "\n";
                }
                printData += new String(' ', 5) + "Billed By :" + dr["USER_ID"].ToString() + new String(' ', 18 - dr["USER_ID"].ToString().Length) + new String(' ', 10 - dr["REG_CHARGE"].ToString().Length) + dr["REG_FEE"].ToString() + "\n";

                for (int j = 1; j <= 13; j++)
                {
                    printData += "\n";
                }
                return printData;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Function to allign the Consultation data to print
        /// </summary>
        /// <param name="dt">Consultation Information</param>
        /// <returns>formated string</returns>
        private string AlignConsultation(DataRow dr)
        {
            string printData = string.Empty;
            string patientName = string.Empty;

            try
            {
                int iSpace = 0;
                int sp = 0;
                int count = 0;
                string billprinter = String.Empty;
                count = 1;
                patientName = dr["FIRST_NAME"].ToString() + " " + dr["MIDDLE_NAME"].ToString() + " " + dr["LAST_NAME"].ToString();

                for (int i = 0; i <= 6; i++)
                {
                    printData += Convert.ToString((char)27) + "j" + Convert.ToString((char)72) + "\n";
                }
                printData += "\n";

                if (dr["ISDUPLICATE"].ToString().Equals("1"))
                {
                    if (patientName.Length > 19)
                    {
                        sp = 3;
                        printData += new String(' ', 6) + patientName.Substring(0, 19) + new String(' ', sp - 2) + "[Duplicate]" + new String(' ', 4) + Convert.ToDateTime(dr["CONSULT_DATE"]).ToString("dd-MMM-yy") + "\n";
                    }
                    else
                    {
                        sp = 19 - (patientName.Length) + 3;
                        printData += new String(' ', 6) + patientName + new String(' ', sp - 2) + "[Duplicate]" + new String(' ', 4) + Convert.ToDateTime(dr["CONSULT_DATE"]).ToString("dd-MMM-yy") + "\n";
                    }
                }
                else
                {
                    if (patientName.Length > 32)
                    {
                        sp = 3;
                        printData += new String(' ', 6) + patientName.Substring(0, 32) + new String(' ', sp - 1) + new String(' ', 1) + Convert.ToDateTime(dr["CONSULT_DATE"]).ToString("dd-MMM-yy") + "\n";
                    }
                    else
                    {
                        sp = 32 - (patientName.Length) + 3;
                        printData += new String(' ', 6) + patientName + new String(' ', sp - 1) + new String(' ', 1) + Convert.ToDateTime(dr["CONSULT_DATE"]).ToString("dd-MMM-yy") + "\n";
                    }
                }
                if (dr["BILL_NO"] == DBNull.Value || dr["BILL_NO"] ==string.Empty)
                {
                    printData += new String(' ', 6) + dr["MRNO"].ToString() + new String(' ', 15 - dr["MRNO"].ToString().Length) + "\n";
                    if (dr["CONSULT_TYPE"] == DBNull.Value || dr["CONSULT_TYPE"].ToString().Length == 0)
                    {
                        dr["CONSULT_TYPE"] = "FREE";
                    }
                }
                else
                {
                    printData += new String(' ', 6) + dr["MRNO"].ToString() + new String(' ', 15 - dr["MRNO"].ToString().Length) + "[" + dr["BILL_TYPE"].ToString() + "]" + new String(' ', 4) + "Bill No: " + dr["BILL_NO"].ToString() + "\n";
                }

                for (int j = 1; j <= 5; j++)
                {
                    printData += "\n";
                }
                if (dr["CONSULT_TYPE"].ToString().Length <= 12)
                {
                    printData += new String(' ', 1) + dr["PRINT_MSG"].ToString() + "(" + Convert.ToString(dr["CONSULT_TYPE"]) + ")" + new String(' ', 41 - dr["PRINT_MSG"].ToString().Length - dr["CONSULT_TYPE"].ToString().Length - dr["CONSULT_CHARGE"].ToString().Length) + dr["CONSULT_CHARGE"].ToString() + "\n";
                }
                else
                {
                    iSpace = dr["CONSULT_TYPE"].ToString().Length - 12;

                    printData += new String(' ', 1) + dr["PRINT_MSG"].ToString() + "(" + Convert.ToString(dr["CONSULT_TYPE"]).Substring(0, 12) + new String(' ', 42 - dr["PRINT_MSG"].ToString().Length - 12 - dr["CONSULT_CHARGE"].ToString().Length) + dr["CONSULT_CHARGE"].ToString() + "\n";
                    count++;
                    printData += new String(' ', 1) + dr["CONSULT_TYPE"].ToString().Substring(12, iSpace) + ")" + "\n";
                }
                printData += new String(' ', 4) + "of Dr." + dr["PROVIDER_NAME"].ToString() + "\n";
                if (dr["DEPARTMENT_NAME"].ToString().Length <= 32)
                {
                    printData += new String(' ', 4) + dr["DEPARTMENT_NAME"].ToString() + "\n";
                }
                else
                {
                    iSpace = dr["DEPARTMENT_NAME"].ToString().Length - 32;
                    printData += new String(' ', 4) + dr["DEPARTMENT_NAME"].ToString().Substring(0, 32) + "\n";
                    if (iSpace > 32)
                    {
                        printData += new String(' ', 4) + dr["DEPARTMENT_NAME"].ToString().Substring(32, 32) + "\n";
                        count++;
                        iSpace = iSpace - 32;
                        if (iSpace > 32)
                        {
                            iSpace = 32;
                        }
                        printData += new String(' ', 4) + dr["DEPARTMENT_NAME"].ToString().Substring(32, iSpace) + "\n";
                        count++;
                    }
                    else
                    {
                        printData += new String(' ', 4) + dr["DEPARTMENT_NAME"].ToString().Substring(32, iSpace) + "\n";
                        count++;
                    }
                }
                if (dr["ISAPPOINTMENT"].ToString() == "1")
                {
                    printData += new String(' ', 4) + "Appointment Time: " + dr["APPOINTMENT_TIME"].ToString() + " (" + dr["TOKEN_NO"].ToString() + ")" + "\n";
                }
                else
                {
                    printData += new String(' ', 4) + "Token No : " + dr["TOKEN_NO"].ToString() + "\n";
                }

                for (int j = 1; j <= 6 - count; j++)
                {
                    printData += "\n";
                }
                printData += new String(' ', 5) + "Billed By :" + dr["USER_ID"].ToString() + new String(' ', 18 - dr["USER_ID"].ToString().Length) + new String(' ', 10 - dr["CONSULT_CHARGE"].ToString().Length) + dr["CONSULT_CHARGE"].ToString() + "\n";

                for (int j = 1; j <= 13; j++)
                {
                    printData += "\n";
                }

                return printData;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string AlignCafeteria(DataSet dsPrintData)
        {
            string printData = string.Empty;
            string patientName = string.Empty;

            try
            {
                int iSpace = 0;
                int sp = 0;
                int count = 0;
                int SLNO = 0;
                string billprinter = String.Empty;
                count = 1;
                decimal itemAmount;
                decimal totalBillAmount = 0;
                string Qty = string.Empty;
                string Rate = string.Empty;
                string strCount = string.Empty;
                string serviceName = string.Empty;
                decimal billAmount = dsPrintData.Tables["BILL_DETAILS"].Rows[0]["NET_TOTAL"] != DBNull.Value ?
                    Convert.ToDecimal(dsPrintData.Tables["BILL_DETAILS"].Rows[0]["NET_TOTAL"]) : 0;
                string billType = string.Empty;
                // string serviceCharge = Math.Round(billAmount, CommonData.DecimalPlace).ToString();
                //serviceName = Convert.ToString(dsPrintData.Tables["BILL_DETAILS"].Rows[0]["SERVICE_NAME"]).PadRight(12, ' '); ;
                patientName = Convert.ToString(dsPrintData.Tables["BILL_MASTER"].Rows[0]["PAT_NAME"]);
                billType = "CASH BILL";
                //serviceCharge = Convert.ToString(dsPrintData.Tables["BILL_DETAILS"].Rows[0]["NET_TOTAL"]).PadLeft(10, ' ');
                for (int i = 0; i <= 6; i++)
                {
                    printData += Convert.ToString((char)27) + "j" + Convert.ToString((char)72) + "\n";
                }
                printData += "\n";
                printData += new String(' ', 28);
                printData += "K I M S";
                printData += "\n";
                printData += new String(' ', 25);
                printData += "*---------*";
                printData += "\n";
                printData += new String(' ', 24);
                printData += "Cafeteria Bill";
                printData += "\n";
                printData += new String(' ', 1);
                printData += "VAT:32071370318";
                printData += "\n";
                printData += new String(' ', 1);
                printData += "CST:3207127038C";
                printData += "\n";
                printData += new String(' ', 1);
                printData += "Bill No:" + dsPrintData.Tables["BILL_MASTER"].Rows[0]["BILL_NO"].ToString();
                printData += "\n";
                printData += new String(' ', 1);
                printData += "Date:" + Convert.ToDateTime(dsPrintData.Tables["BILL_MASTER"].Rows[0]["BILL_DATE"]).ToString("dd-MMM-yy HH:mm");
                printData += "\n Patient Name :" + patientName;
                printData += "\n MRNO :";
                printData += dsPrintData.Tables["BILL_MASTER"].Rows[0]["MRNO"] != DBNull.Value ? dsPrintData.Tables["BILL_MASTER"].Rows[0]["MRNO"].ToString()
                    : "Outsider";
                printData += "\n Pay Mode :";
                if (Convert.ToString(dsPrintData.Tables["BILL_MASTER"].Rows[0]["BILL_TYPE"]) == "OP")
                {
                    printData += "Cash";
                }
                else
                {
                    printData += "Credit";

                }

                printData += "\n";
                //Service Setting //
                printData += "\n";
                printData += new String(' ', 1);
                printData += "No.";
                printData += new String(' ', 1);
                printData += "Item";
                printData += new String(' ', 20);
                printData += new String(' ', 8);
                printData += "Rate";
                printData += new String(' ', 4);
                printData += "Qty";
                printData += new String(' ', 7);
                printData += "Amount";
                printData += "\n";
                printData += "--------------------------------------------------------------";
                foreach (DataRow drBillDtls in dsPrintData.Tables["BILL_DETAILS"].Rows)
                {
                    count = dsPrintData.Tables["BILL_DETAILS"].Rows.IndexOf(drBillDtls) + 1;
                    SLNO = count;
                    strCount = count.ToString() + ".";
                    strCount = strCount.Length >= 3 ? strCount.Substring(0, 3) : strCount.PadRight(3, ' ');
                    serviceName = Convert.ToString(drBillDtls["SERVICE_NAME"]);
                    serviceName = serviceName.Length >= 25 ? serviceName.Substring(0, 25) : serviceName.PadRight(25, ' ');
                    Rate = drBillDtls["RATE"].ToString();
                    Rate = KIFormatDecimalPlace(Rate, CommonData.DecimalPlace);
                    Rate = Rate.PadLeft(12, ' ');
                    Qty = drBillDtls["QTY"].ToString();
                    Qty = KIFormatDecimalPlace(Qty, CommonData.DecimalPlace);
                    Qty = Qty.PadLeft(7, ' ');
                    itemAmount = drBillDtls["NET_TOTAL"] != DBNull.Value ? Convert.ToDecimal(drBillDtls["NET_TOTAL"]) : 0;

                    totalBillAmount += itemAmount;

                    string itemAmountTmp = KIFormatDecimalPlace(itemAmount.ToString(), CommonData.DecimalPlace);
                    itemAmountTmp = itemAmountTmp.PadLeft(13, ' ');
                    printData += "\n";
                    printData += new String(' ', 1) + strCount + serviceName + Rate + Qty + itemAmountTmp;



                }
                printData += "\n";
                printData += "---------------------------------------------------------------";


                //Grant Total 
                printData += "\n";
                printData += new String(' ', 1);
                printData += "Discount/Additional Charges : " + Convert.ToString(dsPrintData.Tables["BILL_MASTER"].Rows[0]["DISC_N_ADJ"]);

              
                //Total Amount 
                printData += "\n";
                printData += "\n";
                printData += new String(' ', 1);
                string strTotalAmt = KIFormatDecimalPlace(totalBillAmount.ToString(), CommonData.DecimalPlace);
                printData += ("Total Amount :" + new String(' ', 10) + strTotalAmt);

                
                //Billed By
                printData += "\n";
                printData += new String(' ', 1);
                printData += "Billed By : " + Convert.ToString(dsPrintData.Tables["BILL_MASTER"].Rows[0]["BILLED_BY"]);

                //if (patientName.Length > 32)
                //{
                //    sp = 3;
                //    printData += new String(' ', 8) + patientName.Substring(0, 32) + new String(' ', sp - 1) + new String(' ', 1) + Convert.ToDateTime(dsPrintData.Tables["BILL_MASTER"].Rows[0]["BILL_DATE"]).ToString("dd-MMM-yy") + "\n";
                //}
                //else
                //{
                //    sp = 32 - (patientName.Length) + 3;
                //    printData += new String(' ', 8) + patientName + new String(' ', sp - 1) + new String(' ', 1) + Convert.ToDateTime(dsPrintData.Tables["BILL_MASTER"].Rows[0]["BILL_DATE"]).ToString("dd-MMM-yy") + "\n";
                //}

                //printData += new String(' ', 8) + dsPrintData.Tables["BILL_MASTER"].Rows[0]["MRNO"].ToString() + new String(' ', 12 - dsPrintData.Tables["BILL_MASTER"].Rows[0]["MRNO"].ToString().Length) + "[" + billType + "]" + new String(' ', 3) + "Bill No: " + dsPrintData.Tables["BILL_MASTER"].Rows[0]["BILL_NO"].ToString() + "\n";

                //for (int j = 1; j <= 5; j++)
                //{
                //    printData += "\n";
                //}

                //printData += new String(' ', 5) + serviceName + new String(' ', 25) + serviceCharge + "\n";
                //for (int j = 1; j <= 9 - count; j++)
                //{
                //    printData += "\n";
                //}
                //printData += new String(' ', 5) + "Billed By :" + Convert.ToString(dsPrintData.Tables["BILL_MASTER"].Rows[0]["BILLED_BY"]) + new String(' ', 26 - Convert.ToString(dsPrintData.Tables["BILL_MASTER"].Rows[0]["BILLED_BY"]).Length) + serviceCharge + "\n";
                //for (int j = 1; j <= 13; j++)
                //{
                //    printData += "\n";
                //}
                for (int i = 0; i < 15; i++)
                {
                    printData += "\n";
                }
                return printData;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private  string KIFormatDecimalPlace(string value, int DecimalPlace)
        {
            System.Globalization.NumberFormatInfo numFormat = new System.Globalization.CultureInfo(System.Globalization.CultureInfo.CurrentCulture.Name, false).NumberFormat;
            numFormat.NumberDecimalDigits = DecimalPlace;
            return string.IsNullOrEmpty(System.Convert.ToString(value)) ? Math.Round(0.00, DecimalPlace).ToString("F", numFormat) :
                Math.Round(System.Convert.ToDouble(value), DecimalPlace).ToString("F", numFormat);
        }
        #region IPrinting Members

        public bool Print(DataSet dsData, ServiceType serviceType, string PrinterName, PrintType printType)
        {
           return this.Print(dsData, serviceType, PrinterName);
        }

        #endregion
    }
}
