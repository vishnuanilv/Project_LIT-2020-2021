using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infologics.Medilogics.PrintingLibrary.Main;
//using Infologics.Medilogics.CommonShared.FOControls;
using Infologics.Medilogics.Enumerators.General;
using System.Data;
using Infologics.Medilogics.CommonClient.Controls.CommonFunctions;
using Infologics.Medilogics.PrintingLibrary.InvoiceCrystal.CrystalReports;
using CrystalDecisions.Shared;
using Infologics.Medilogics.Enumerators.Visit;
using Infologics.Medilogics.PrintingLibrary.InvoiceCrystal.DataSets;
using Infologics.Medilogics.Enumerators.Address;
using Infologics.Medilogics.CommonClient.Controls.StaticData;
using Infologics.Medilogics.PrintingLibrary.Invoice;
using System.Configuration;
namespace Infologics.Medilogics.PrintingLibrary.InvoiceCrystal
{
    public class BLInvoiceCrystal : IPrinting
    {
        internal enum PrintMode
        {
            Print,
            RePrint,
        }
        #region IPrinting Members

        public bool Print(System.Data.DataSet BillPrintDetails, ServiceType serviceType, string PrinterName)
        {
            throw new NotImplementedException();
        }
        public bool Print(DataSet dsData, ServiceType serviceType, string PrinterName, PrintType printType)
        {
            bool isSuccess = false;

            //PrinterName = "\\\\gi23\\PDFCreator";
            //SetBillAmount(BillPrintDetails);
            if (printType == PrintType.Invoice)
            {
                if (Convert.ToString(dsData.Tables["BILL_MASTER"].Rows[0]["BILL_NO"]).ToUpper() != "FREE"
                     && serviceType != ServiceType.Advance && serviceType != ServiceType.Deductible
                     )
                {
                    SetValidGross(dsData);
                }
                dsData.Tables["BILL_MASTER"].Rows[0]["DECIMAL_PLACE"] = CommonData.DecimalPlace;
                if (serviceType == ServiceType.Registration || serviceType == ServiceType.ReRegistration ||
                    serviceType == ServiceType.Consultation || serviceType == ServiceType.Investigation || 
                    serviceType == ServiceType.Surgery || serviceType == ServiceType.FinalBilling
                    || serviceType == ServiceType.Cafeteria
                    || serviceType == ServiceType.IncomingService)
                {
                    rptServiceBill rptRegBill = new rptServiceBill();
                    rptRegBill.SetDataSource(dsData);
                    rptRegBill.PrintOptions.PrinterName = PrinterName;
                    rptRegBill.PrintToPrinter(1, false, 0, 0);

                    isSuccess = true;
                }
                else if (serviceType == ServiceType.Pharmacy)
                {
                   
                   // rptPharmacyBillDetails objPhBill = new rptPharmacyBillDetails();
                    rptServiceBill objPhBill = new rptServiceBill();

                    objPhBill.SetDataSource(dsData);
                    objPhBill.PrintOptions.PrinterName = PrinterName;
                    objPhBill.PrintToPrinter(1, false, 0, 0);
                    isSuccess = true;
                }
                else if (serviceType == ServiceType.Advance || serviceType == ServiceType.Deductible)
                {
                    rptAdvanceBill ObjAdvBill = new rptAdvanceBill();
                    ObjAdvBill.SetDataSource(dsData);
                    ObjAdvBill.PrintOptions.PrinterName = PrinterName;
                    ObjAdvBill.PrintToPrinter(1, false, 0, 0);
                    isSuccess = true;
                }
                else if (serviceType == ServiceType.Cafeteria)
                {
                    rptCafteria objCafteria = new rptCafteria();
                    objCafteria.SetDataSource(dsData);
                    objCafteria.PrintOptions.PrinterName = PrinterName;
                    objCafteria.PrintToPrinter(1, false, 0, 0);
                    
                    //BLInvoice objInvoice = new BLInvoice();
                    //objInvoice.Print(dsData, serviceType, PrinterName);
                    isSuccess = true;
                }
                else if (serviceType == ServiceType.Refund)
                {
                    rptAdvanceRefundBill billRefund = new rptAdvanceRefundBill();
                    billRefund.SetDataSource(dsData);
                    billRefund.PrintOptions.PrinterName = PrinterName;
                    billRefund.PrintToPrinter(1, false, 0, 0);
                    //BLInvoice invoice = new BLInvoice();
                    //isSuccess = invoice.Print(dsData, serviceType, PrinterName);
                    //isSuccess = true;
                }

            }
            else if (printType == PrintType.Prescription)
            {
                DataSet dsTempData = dsData.Copy();
                rptPrintPresc objPrintPres = new rptPrintPresc();

                if (dsData.Tables.Contains("MASTER") && dsData.Tables.Contains("DETAILS"))
                {
                    dsTempData.Tables["MASTER"].TableName = "PRINT_PRESC_MASTER";
                    dsTempData.Tables["DETAILS"].TableName = "PRINT_PRESC_DTLS";
                    objPrintPres.SetDataSource(dsTempData);
                    objPrintPres.PrintOptions.PrinterName = PrinterName;
                    objPrintPres.PrintToPrinter(1, false, 0, 0);
                    isSuccess = true;
                }
                else
                {
                    isSuccess = false;
                }


            }
            else if (printType == PrintType.AppointmentSlip)
            {

                rptApptSlip objApptSlipReport = new rptApptSlip();
                objApptSlipReport.SetDataSource(dsData);
                objApptSlipReport.PrintOptions.PrinterName = PrinterName;
                objApptSlipReport.PrintToPrinter(1, false, 0, 0);

                isSuccess = true;

            }

            return isSuccess;
        }

        #endregion

        /// <summary>
        /// Creates the printing data.
        /// </summary>
        /// <param name="dsPrintDetails">The ds print details.</param>
        /// <param name="servType">Type of the serv.</param>
        /// <param name="PrintOrReprint">The print or reprint.</param>
        /// <returns></returns>
        public DataSet CreatePrintingData(DataSet dsPrintDetails, ServiceType servType, int PrintOrReprint)
        {
            return this.CreatePrintingData(dsPrintDetails, servType, PrintOrReprint, ProfileCategory.Patient);
        }

        //public DataSet CreateGroupPrintingData(DataTable dtPrint, ServiceType serType, int PrintOrReprint)
        //{
        //    DataSet dsPrintData = new DataSet();
        //    InvoicePrint invPrint = new InvoicePrint();
        //    try
        //    {
        //        //if (printType == PrintType.Invoice) //Invoice binding table
        //        //{
        //        DataTable dtBillMaster = null;
        //        DataTable dtBillDetails = null;
        //        dtBillMaster = invPrint.Tables["BILL_MASTER"].Clone();
        //        dsPrintData.Tables.Add(dtBillMaster.Copy());
        //        dtBillDetails = invPrint.Tables["BILL_DETAILS"].Clone();
        //        dsPrintData.Tables.Add(dtBillDetails.Copy());

        //        if (PrintOrReprint == (int)PrintMode.Print)//Printing
        //        {
        //            this.CreateGroupBillPrintData(dtPrint, ref dsPrintData, serType);
        //        }
        //        return dsPrintData;

        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
          
        //}


        /// <summary>
        /// Creates the printing data.(For printing or reprinting)
        /// </summary>
        /// <param name="BillDetails">The bill details.</param>
        /// <param name="servType">Type of the serv.</param>
        /// <param name="printType">Type of the print.</param>
        /// <param name="PrintOrReprint">The print or reprint.</param>
        /// <returns></returns>
        public DataSet CreatePrintingData(DataSet dsPrintDetails, ServiceType servType, int PrintOrReprint, ProfileCategory PatientOrOutsider) //0-print,1-reprint
        {
           // DataSet dsTempPrintDetails = dsPrintDetails.Copy();
            DataSet dsPrintData = new DataSet();
            InvoicePrint invPrint = new InvoicePrint();
            try
            {
                //if (printType == PrintType.Invoice) //Invoice binding table
                //{
                DataTable dtBillMaster = null;
                DataTable dtBillDetails = null;
                dtBillMaster = invPrint.Tables["BILL_MASTER"].Clone();
                dsPrintData.Tables.Add(dtBillMaster.Copy());
                dtBillDetails = invPrint.Tables["BILL_DETAILS"].Clone();
                dsPrintData.Tables.Add(dtBillDetails.Copy());



                if (PrintOrReprint == (int)PrintMode.Print)//Printing
                {
                    if (dsPrintDetails != null && dsPrintDetails.Tables.Contains("PRINT_CREDIT_DATA"))
					{
                        this.CreateGroupBillPrintData(dsPrintDetails, ref dsPrintData, servType);
					}
                    else if (dsPrintDetails != null && dsPrintDetails.Tables.Contains("PRINT_CASH_DATA"))
                    {
                        this.CreateGroupBillPrintData(dsPrintDetails, ref dsPrintData, servType);
                    }
                    else
                    {
                        this.CreatePrintData(dsPrintDetails, ref dsPrintData, servType, PatientOrOutsider);
                    }
                }
                else if (PrintOrReprint == (int)PrintMode.RePrint)//Re-printing
                {
					if (dsPrintDetails!=null&&dsPrintDetails.Tables.Contains("GEN_PAT_BILLING")&&
						dsPrintDetails.Tables["GEN_PAT_BILLING"].Rows[0]["GROUP_BILLING_ID"]!=DBNull.Value)
					{
						//this.CreateGroupBillPrintData(dsPrintDetails, ref dsPrintData, servType, PatientOrOutsider);
					}
					else
					{
						this.CreateRePrintData(dsPrintDetails, ref dsPrintData, servType, PatientOrOutsider);
					}
                }
                //}
                //else if (printType == PrintType.Prescription)//Prescription binding table
                //{
                //    rptPrintPresc objPrintPres = new rptPrintPresc();
                //    if (dsTempPrintDetails.Tables.Contains("MASTER") && dsTempPrintDetails.Tables.Contains("DETAILS"))
                //    {
                //        dsTempPrintDetails.Tables["MASTER"].TableName = "PRINT_PRESC_MASTER";
                //        dsTempPrintDetails.Tables["DETAILS"].TableName = "PRINT_PRESC_DTLS";
                //    }
                //}
                return dsPrintData;
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Creating Data for printing
        /// </summary>
        /// <param name="dsPrintDetails"></param>
        /// <returns></returns>
        public DataSet CreateAppointmentSlipPrintingData(DataTable dtPrintDetails)
        {

            DataSet dsPrintData = new DataSet();
            ApptSlipPrint objApptSlipPrint = new ApptSlipPrint();
            try
            {
                dsPrintData = objApptSlipPrint.Clone();
                this.CreatePrintData(dtPrintDetails, ref dsPrintData);
                return dsPrintData;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //private DataSet CreateBillDetails(DataSet BillDetails, ServiceType servType)
        //{
        //    DataSet dsBillDetails = new DataSet();
        //    DataTable dtBillMaster = null;
        //    DataTable dtBillDetails = null;
        //    InvoicePrint invPrint = new InvoicePrint();
        //    string vmode = string.Empty;
        //    CommonFunctions comFun = new CommonFunctions();
        //    try
        //    {
        //        dtBillMaster = invPrint.Tables["BILL_MASTER"].Clone();
        //        dtBillMaster.Rowc.Add();
        //        dtBillMaster.Rows[0]["BILL_NO"] = BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["BILL_NO"];
        //        dtBillMaster.Rows[0]["PAT_NAME"] = GetPatientName(BillDetails.Tables["PAT_PATIENT_NAME"]);
        //        dtBillMaster.Rows[0]["MRNO"] = BillDetails.Tables["PAT_PATIENT_NAME"].Rows[0]["MRNO"];
        //        dtBillMaster.Rows[0]["BILL_DATE"] = BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["BILL_DATE"];

        //        if (Convert.ToInt16(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["VISIT_MODE"]) == (int)VisitMode.OPCASH)
        //        {
        //            vmode = "OP"; // Enum.GetName(typeof(VisitMode), Convert.ToUInt32(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["VISIT_MODE"]));
        //        }
        //        else//insurace and company
        //        {
        //            vmode = Enum.GetName(typeof(VisitMode), Convert.ToUInt32(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["VISIT_MODE"]));
        //            dtBillMaster.Rows[0]["COMPANY"] = BillDetails.Tables["INCO_PATIENT_SCHEME"].Rows[0]["COMPANY"];
        //            dtBillMaster.Rows[0]["INSURANCE"] = BillDetails.Tables["INCO_PATIENT_SCHEME"].Rows[0]["INSURANCE"];
        //        }
        //        dtBillMaster.Rows[0]["BILL_TYPE"] = vmode;
        //        dtBillMaster.Rows[0]["DOCTOR_NAME"] = BillDetails.Tables["BILL_COMMON_DETAILS"].Rows[0]["PROVIDER_NAME"];
        //        dtBillMaster.Rows[0]["BILLED_BY"] = BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["BILLED_BY"];
        //        dtBillMaster.Rows[0]["SERVICE_TYPE"] = servType;
        //        dtBillMaster.Rows[0]["CLINIC"] = BillDetails.Tables["BILL_COMMON_DETAILS"].Rows[0]["DEPARTMENT_NAME"];
        //        dtBillMaster.Rows[0]["TOTAL_AMOUNT_IN_WORDS"] = comFun.ToWords(Convert.ToDouble(BillDetails.Tables["INV_PAT_BILLING_TOTAL"].Rows[0]["NET_AMOUNT"]));
        //        //Details
        //        dtBillDetails = invPrint.Tables["BILL_DETAILS"].Clone();
        //        DataRow drBilldet = null;
        //        if (servType == ServiceType.Pharmacy)
        //        {
        //            foreach (DataRow dr in BillDetails.Tables["INV_PAT_BILLING"].Rows)
        //            {
        //                drBilldet = dtBillDetails.NewRow();
        //                drBilldet["BILL_NO"] = BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["BILL_NO"];
        //                drBilldet["QTY"] = dr["QTY"];
        //                drBilldet["MEDICINE_NAME"] = dr["NAME"];
        //                drBilldet["GRAND_TOTAL"] = dr["GROSS_AMOUNT"];
        //                drBilldet["COPAY"] = dr["CO_PAY_AMOUNT"];
        //                drBilldet["NET_TOTAL"] = dr["NET_AMOUNT"];
        //                drBilldet["BATCH_NO"] = dr["BATCHNO"];
        //                drBilldet["EXPIRY"] = dr["EXP_DATE"];
        //                drBilldet["IS_RETURN"] = 0;
        //                dtBillDetails.Rows.Add(drBilldet);
        //            }

        //        }
        //        else if (servType == ServiceType.Investigation)
        //        {
        //            foreach (DataRow dr in BillDetails.Tables["INV_PAT_BILLING"].Rows)
        //            {
        //                drBilldet = dtBillDetails.NewRow();
        //                drBilldet["BILL_NO"] = BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["BILL_NO"];
        //                drBilldet["QTY"] = dr["QTY"];
        //                drBilldet["GRAND_TOTAL"] = dr["GROSS_AMOUNT"];
        //                drBilldet["COPAY"] = dr["CO_PAY_AMOUNT"];
        //                drBilldet["NET_TOTAL"] = dr["NET_AMOUNT"];
        //                drBilldet["SERVICE_NAME"] = dr["NAME"];
        //                drBilldet["CPTCODE"] = dr["CPT_CODE"];
        //                dtBillDetails.Rows.Add(drBilldet);
        //            }
        //        }
        //        else if (servType == ServiceType.Advance || servType == ServiceType.Deductible)
        //        {
        //            dtBillDetails.Rows[0]["BILL_NO"] = BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["BILL_NO"];
        //            dtBillDetails.Rows[0]["SERVICE_NAME"] = Enum.GetName(typeof(ServiceType), Convert.ToUInt32(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["VISIT_MODE"]));
        //        }
        //        else if (servType == ServiceType.Consultation)
        //        {
        //            dtBillDetails.Rows.Add();
        //            StringBuilder sbServiceName = new StringBuilder();
        //            sbServiceName.Append(BillDetails.Tables["CON_PAT_BILLING"].Rows[0]["CON_TYPE_NAME"].ToString());
        //            sbServiceName.Append(" Consultation");
        //            dtBillDetails.Rows[0]["BILL_NO"] = BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["BILL_NO"];
        //            dtBillDetails.Rows[0]["SERVICE_NAME"] = sbServiceName.ToString();//BillDetails.Tables["CON_PAT_BILLING"].Rows[0]["CON_TYPE_NAME"];
        //            dtBillDetails.Rows[0]["GRAND_TOTAL"] = BillDetails.Tables["CON_PAT_BILLING"].Rows[0]["AMOUNT"];
        //            dtBillDetails.Rows[0]["NET_TOTAL"] = BillDetails.Tables["CON_PAT_BILLING"].Rows[0]["NET_AMOUNT"];
        //            dtBillDetails.Rows[0]["QTY"] = 1;
        //            dtBillDetails.Rows[0]["COPAY"] = BillDetails.Tables["CON_PAT_BILLING"].Rows[0]["CO_PAY_AMOUNT"];
        //        }
        //        else if (servType == ServiceType.Registration || servType == ServiceType.ReRegistration)
        //        {
        //            dtBillDetails.Rows.Add();
        //            dtBillDetails.Rows[0]["BILL_NO"] = BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["BILL_NO"];
        //            //   dtBillDetails.Rows[0]["SERVICE_NAME"] = BillDetails.Tables["CON_PAT_BILLING"].Rows[0]["CON_TYPE_NAME"];
        //            dtBillDetails.Rows[0]["GRAND_TOTAL"] = BillDetails.Tables["REG_PAT_BILLING"].Rows[0]["AMOUNT"];
        //            dtBillDetails.Rows[0]["NET_TOTAL"] = BillDetails.Tables["REG_PAT_BILLING"].Rows[0]["NET_AMOUNT"];
        //            dtBillDetails.Rows[0]["QTY"] = 1;
        //            dtBillDetails.Rows[0]["COPAY"] = BillDetails.Tables["REG_PAT_BILLING"].Rows[0]["CO_PAY_AMOUNT"];
        //            dtBillDetails.Rows[0]["SERVICE_NAME"] = BillDetails.Tables["BILL_COMMON_DETAILS"].Rows[0]["REG_MAST_TYPE_NAME"];
        //        }
        //        //else if (servType == ServiceType.Registration || servType == ServiceType.ReRegistration)
        //        //{
        //        //    dtBillDetails.Rows[0]["BILL_NO"] = BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["BILL_NO"];
        //        //}
        //        dsBillDetails.Tables.Add(dtBillDetails.Copy());
        //        dsBillDetails.Tables.Add(dtBillMaster.Copy());
        //        return dsBillDetails;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}
        private string GetPatientName(DataTable PatDetls)
        {
            StringBuilder sbPatName = new StringBuilder();
            if (PatDetls.Columns.Contains("PAT_NAME") && Convert.ToString(PatDetls.Rows[0]["PAT_NAME"]).Length > 0)
            {
                sbPatName.Append(PatDetls.Rows[0]["PAT_NAME"]);
            }
            else
            {
                if (PatDetls.Columns.Contains("TITLE"))
                {
                    if (PatDetls.Rows[0]["TITLE"] != DBNull.Value)
                    {
                        sbPatName.Append(PatDetls.Rows[0]["TITLE"]);
                    }
                }
                if (PatDetls.Columns.Contains("FIRST_NAME") && PatDetls.Rows[0]["FIRST_NAME"] != DBNull.Value)
                {

                    sbPatName.Append(" " + PatDetls.Rows[0]["FIRST_NAME"]);

                }
                if (PatDetls.Rows[0]["MIDDLE_NAME"] != DBNull.Value)
                {
                    sbPatName.Append(" " + PatDetls.Rows[0]["MIDDLE_NAME"]);
                }
                if (PatDetls.Rows[0]["LAST_NAME"] != DBNull.Value)
                {

                    sbPatName.Append(" " + PatDetls.Rows[0]["LAST_NAME"]);
                }
            }
            return sbPatName.ToString();
        }
        /// <summary>
        /// over load for appointment slip
        /// </summary>
        /// <param name="Details"></param>
        /// <param name="PrintData"></param>
        /// Modified by Ahamed Fazeel K  --on 11 oct 2011
        /// puropose to check whether the table PrintData contains the column RESOURCE_APPT_TYPE
        private void CreatePrintData(DataTable Details, ref DataSet PrintData)
        {
            try
            {
                DataRow drPrintdata = PrintData.Tables["APPOINTMENT_DETAILS"].NewRow();
                if (PrintData.Tables.Contains("APPOINTMENT_DETAILS") && Details.Rows.Count != 0)
                {
                    DataRow drItem = Details.Rows[0];
                    drPrintdata["PT_NAME"] = drItem["NAME"];
                    drPrintdata["PT_MRNO"] = drItem["MRNO"];
                    drPrintdata["APT_DATE"] = drItem["ALLOCATION_DATE"];
                    if (drItem["CONFIRM_OR_WAIT"].ToString() == "0")
                    {
                        drPrintdata["APT_TIME"] = drItem["ALLOCATION_DATE"];
                    }
                    else
                    {
                        drPrintdata["APT_TIME"] = DBNull.Value;
                    }
                    drPrintdata["DOCTOR_NAME"] = drItem["RESOURCE_NAME"];
                    if (drItem.Table.Columns.Contains("RESOURCE_APPT_TYPE") &&
                       drItem["RESOURCE_APPT_TYPE"] != DBNull.Value && Convert.ToInt32(drItem["RESOURCE_APPT_TYPE"]) == 1)//Procedure Appointment //todo use enum 
                    {
                        drPrintdata["APPT_SERVICE_NAME"] = drItem["APPT_SERVICE_NAME"];
                        drPrintdata["RESOURCE_APPT_TYPE"] = drItem["RESOURCE_APPT_TYPE"];
                        drPrintdata["RESOURCE_APPT_TYPE"] = drItem["RESOURCE_APPT_TYPE"];
                    }                   
                    drPrintdata["SPECIALITY"] = drItem["DEPT_NAME"];
                    drPrintdata["APT_DESK_EXT"] = "";
                    drPrintdata["APT_TOKEN_NO"] = drItem["TOKEN_NO"];
                    PrintData.Tables["APPOINTMENT_DETAILS"].Rows.Add(drPrintdata);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        //private object SetPageMargin(object rptPage)
        //{

        //    return rptPage;
        //}


        //private void SetBillAmount(DataSet BillPrintDetails)
        //{
        //    CommonFunctions comFun = new CommonFunctions();
        //    double netAmount = 0;
        //    foreach (DataRow dr in BillPrintDetails.Tables["BILL_DETAILS"].Rows)
        //    {

        //            if (!IsPharmacyReturn(BillPrintDetails))
        //            {
        //                if (dr["NET_TOTAL"] != DBNull.Value)
        //                {
        //                    netAmount = netAmount + Convert.ToDouble(dr["NET_TOTAL"]);
        //                }
        //            }
        //            else
        //            {
        //                if (dr["IS_RETURN"] != DBNull.Value)
        //                {
        //                    if (Convert.ToInt16(dr["IS_RETURN"]) == 1)
        //                    {
        //                        netAmount = netAmount + Convert.ToDouble(dr["NET_TOTAL"]);
        //                    }
        //                }
        //            }


        //    }
        //    BillPrintDetails.Tables["BILL_MASTER"].Rows[0]["TOTAL_AMOUNT_IN_WORDS"] = comFun.ToWords(netAmount);

        //}
        //private bool IsPharmacyReturn(DataSet BillPrintDetails)
        //{
        //    bool isTrue = false;
        //    int retCount = 0;
        //    if (Convert.ToString(BillPrintDetails.Tables["BILL_MASTER"].Rows[0]["SERVICE_TYPE"]) == Enum.GetName(typeof(ServiceType), ServiceType.Pharmacy))
        //    {
        //        foreach (DataRow dr in BillPrintDetails.Tables["BILL_DETAILS"].Rows)
        //        {
        //            if (dr["IS_RETURN"] != DBNull.Value)
        //            {
        //                retCount = retCount + Convert.ToInt16(dr["IS_RETURN"]);
        //            }
        //        }
        //    }
        //    if (retCount > 0)
        //    {
        //        isTrue = true;
        //    }
        //    return isTrue;
        //}
        /// <summary>
        /// Creates the print data.
        /// </summary>
        /// <param name="BillDetails">The bill details.</param>
        /// <param name="servType">Type of the serv.</param>
        /// <param name="printType">Type of the print.</param>
        /// <returns></returns>
        private void CreatePrintData(DataSet BillDetails, ref DataSet PrintData, ServiceType servType, ProfileCategory PatientOrOutsider)
        {
            try
            {
                string vmode = string.Empty;
                CommonFunctions comFun = new CommonFunctions();
                DataRow drNew = PrintData.Tables["BILL_MASTER"].NewRow();
                DataRow drDtls = null;
                double retTotal = 0;
                double retTotalWord = 0;
                double refundAmount = 0;
                double paidAmount = 0;
                string mrno = string.Empty;
                string contract = string.Empty;
                //double retCopayTotal = 0;
                bool isPharmacyret = false;   
                if (servType == ServiceType.FinalBilling)
                {
                    CreateFinalBillPrintData(BillDetails, ref PrintData);
                }
                else if (servType == ServiceType.Refund)//Advance or deductible refund
                {
                    CreatePrintData(BillDetails, ref PrintData);
                }
                else
                {
                    if (servType == ServiceType.Pharmacy)
                    {//Pharmacy Return
                        if (BillDetails.Tables.Contains("VW_BILL_MASTER") && BillDetails.Tables.Contains("PH_SALESRETURN"))
                        {
                            drNew["BILL_NO"] = BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["BILL_NO"];
                            drNew["PAT_NAME"] = BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["PATIENT_NAME"];
                            drNew["BILL_DATE"] = BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["BILL_DATE"];
                            drNew["DOCTOR_NAME"] = BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["DOCT_NAME"];
                            drNew["CLINIC"] = BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["DOCT_DEPT"];
                            drNew["SERVICE_TYPE"] = servType;
                            drNew["BILLED_BY"] = BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["BILLED_BY"];

                            if (PatientOrOutsider == ProfileCategory.Patient)
                            {
                                drNew["MRNO"] = BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["PATIENT_ID"];
                                if (Convert.ToString(BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["VISIT_MODE"]).ToUpper() == "OP-CASH")
                                {
                                    vmode = "OP"; // Enum.GetName(typeof(VisitMode), Convert.ToUInt32(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["VISIT_MODE"]));
                                    paidAmount = (double)GetPHRetAmount(BillDetails.Tables["VW_BILL_MASTER"]);
                                }
                                else//insurace and company
                                {
                                    vmode = Convert.ToString(BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["VISIT_MODE"]);
                                    // drNew["COMPANY"] = BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["CORP_COMPANY_NAME"];
                                    //  drNew["INSURANCE"] = BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["INS_COMPANY_NAME"];
                                    GetContract(drNew, BillDetails);
                                    paidAmount = (double)GetPHRetAmount(BillDetails.Tables["VW_BILL_MASTER"]);
                                }

                            }
                            else if (PatientOrOutsider == ProfileCategory.OutsidePatient)
                            {
                                drNew["PROFILE_ID"] = BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["PATIENT_ID"];
                                vmode = "OP";
                            }
                            drNew["BILL_TYPE"] = vmode;
                            // PrintData.Tables["BILL_MASTER"].Rows.Add(drNew);
                            foreach (DataRow dr in BillDetails.Tables["PH_SALESRETURN"].Rows)
                            {
                                drDtls = PrintData.Tables["BILL_DETAILS"].NewRow();
                                drDtls["BILL_NO"] = BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["BILL_NO"];
                                drDtls["MEDICINE_NAME"] = dr["SERVICE_NAME"];
                                drDtls["BATCH_NO"] = dr["BATCHNO"];
                                drDtls["EXPIRY"] = dr["EXP_DATE"];
                                if (dr["QTY"] != DBNull.Value)
                                {
                                    drDtls["QTY"] = Convert.ToDecimal(dr["QTY"]) - Convert.ToDecimal(dr["RETURNED_QTY"]);
                                }
                                if (dr["NET_AMOUNT"] != DBNull.Value)
                                {
                                    drDtls["NET_TOTAL"] = Convert.ToDecimal(dr["NET_AMOUNT"]) - Convert.ToDecimal(dr["RETURNED_AMOUNT"]);
                                    retTotalWord = Convert.ToDouble(dr["NET_AMOUNT"]) - Convert.ToDouble(dr["RETURNED_AMOUNT"]);
                                }
                                if (dr["GROSS_AMOUNT"] != DBNull.Value)
                                {
                                    drDtls["GRAND_TOTAL"] = (GetGrossAmount(dr) / Convert.ToDecimal(dr["QTY"])) * (Convert.ToDecimal(drDtls["QTY"]));
                                }
                                if (dr["CO_PAY_AMOUNT"] != DBNull.Value)
                                {
                                    drDtls["COPAY"] = (Convert.ToDecimal(dr["CO_PAY_AMOUNT"]) / Convert.ToDecimal(dr["QTY"])) * (Convert.ToDecimal(drDtls["QTY"]));
                                }
                                else
                                {
                                    drDtls["COPAY"] = 0;
                                }
                                drDtls["IS_RETURN"] = 0;
                                drDtls["MEDICINE_NAME"] = dr["SERVICE_NAME"];
                                if (Convert.ToDecimal(dr["RETURN_QTY"]) > 0)
                                {
                                    PrintData.Tables["BILL_DETAILS"].Rows.Add(drDtls);
                                    drDtls = PrintData.Tables["BILL_DETAILS"].NewRow();
                                    drDtls["BILL_NO"] = BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["BILL_NO"];
                                    drDtls["MEDICINE_NAME"] = dr["SERVICE_NAME"];
                                    drDtls["BATCH_NO"] = dr["BATCHNO"];
                                    drDtls["EXPIRY"] = dr["EXP_DATE"];
                                    drDtls["QTY"] = dr["RETURN_QTY"];
                                    drDtls["NET_TOTAL"] = dr["RETURN_AMOUNT"];
                                    refundAmount += Convert.ToDouble(dr["RETURN_AMOUNT"]);
                                    retTotalWord = Convert.ToDouble(dr["REMAINING_AMOUNT"]);
                                    if (dr["GROSS_AMOUNT"] != DBNull.Value)
                                    {
                                        drDtls["GRAND_TOTAL"] = (GetGrossAmount(dr) / Convert.ToDecimal(dr["QTY"])) * (Convert.ToDecimal(drDtls["QTY"]));
                                    }
                                    else
                                    {
                                        drDtls["GRAND_TOTAL"] = 0;
                                    }
                                    if (dr["CO_PAY_AMOUNT"] != DBNull.Value)
                                    {
                                        drDtls["COPAY"] = (Convert.ToDecimal(dr["CO_PAY_AMOUNT"]) / Convert.ToDecimal(dr["QTY"])) * (Convert.ToDecimal(drDtls["QTY"]));
                                    }
                                    else
                                    {
                                        drDtls["COPAY"] = 0;
                                    }
                                    drDtls["IS_RETURN"] = 1;
                                }
                                    retTotal += retTotalWord;
                                // CheckValidGross(drDtls);

                                PrintData.Tables["BILL_DETAILS"].Rows.Add(drDtls);
                            }

                            drNew["TOTAL_AMOUNT_IN_WORDS"] = comFun.ToWords(retTotal);
                            if (Convert.ToString(drNew["BILL_TYPE"]) == "OP")
                            {
                                drNew["PAID_AMOUNT"] = refundAmount > 0 ? refundAmount : retTotal;
                            }
                            else
                            {
                                drNew["PAID_AMOUNT"] = refundAmount > 0 ? refundAmount : paidAmount;
                            }
                            PrintData.Tables["BILL_MASTER"].Rows.Add(drNew);
                            isPharmacyret = true;
                        }

                    }
                    if (!isPharmacyret)
                    {
                        if (servType == ServiceType.Advance || servType == ServiceType.Deductible)
                        {
                            drNew["BILL_NO"] = BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["BILL_NO"];
                            drNew["PAT_NAME"] = BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["PATIENT_NAME"];
                            drNew["MRNO"] = BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["PATIENT_ID"];
                            drNew["BILL_DATE"] = BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["BILL_DATE"];
                            drNew["BILLED_BY"] = BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["BILLED_BY"];
                            drNew["BILL_DATE"] = BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["BILL_DATE"];
                            drNew["SERVICE_TYPE"] = Enum.GetName(typeof(ServiceType), Convert.ToUInt32(servType));
                            GetContract(drNew, BillDetails);
                            drNew["TOTAL_AMOUNT_IN_WORDS"] = comFun.ToWords(Convert.ToDouble(BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["FINAL_BILL_AMOUNT"]));
                            drNew["ENCOUNTER_NO"] = BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["ENCOUNTER_NO"];
                        }
                        else
                        {


                            if (PatientOrOutsider == ProfileCategory.Patient)
                            {
                                drNew["MRNO"] = BillDetails.Tables["PAT_PATIENT_NAME"].Rows[0]["MRNO"];
                                drNew["CLINIC"] = BillDetails.Tables["BILL_COMMON_DETAILS"].Rows[0]["DEPARTMENT_NAME"];
                                drNew["PAT_NAME"] = GetPatientName(BillDetails.Tables["PAT_PATIENT_NAME"]);
                            }
                            else
                            {
                                drNew["MRNO"] = DBNull.Value;
                                drNew["CLINIC"] = DBNull.Value;
                                drNew["PROFILE_ID"] = BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["PROFILE_ID"];
                                drNew["PAT_NAME"] = GetPatientName(BillDetails.Tables["GEN_PROFILE_CONTACT"]);
                            }

                            //check for free consultation
                            if (BillDetails != null && BillDetails.Tables.Contains("GEN_PAT_BILLING")
                                && BillDetails.Tables["GEN_PAT_BILLING"].Rows.Count > 0)
                            {
                                drNew["BILL_NO"] = BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["BILL_NO"];
                                drNew["BILL_DATE"] = BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["BILL_DATE"];
                                drNew["BILLED_BY"] = BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["BILLED_BY"];
                                if (Convert.ToInt16(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["VISIT_MODE"]) == (int)VisitMode.OPCASH)
                                {
                                    vmode = "OP"; // Enum.GetName(typeof(VisitMode), Convert.ToUInt32(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["VISIT_MODE"]));
                                    if (BillDetails.Tables.Contains("INV_PAT_BILLING") && BillDetails.Tables["INV_PAT_BILLING"].Rows.Count > 0)
                                        drNew["PAID_AMOUNT"] = IfNullReturnZero(BillDetails.Tables["INV_PAT_BILLING"].Rows[0]["ISPACKAGE_SERVICE"]) != 2 ? BillDetails.Tables["INV_PAT_BILLING_TOTAL"].Rows[0]["NET_AMOUNT"] : DBNull.Value;
                                    else if (BillDetails.Tables.Contains("VW_BILL_DTLS") && BillDetails.Tables["VW_BILL_DTLS"].Rows.Count > 0 && Convert.ToInt32(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["VISIT_MODE"]) != 2)
                                        drNew["PAID_AMOUNT"] = IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["ISPACKAGE_SERVICE"]) != 2 ? BillDetails.Tables["INV_PAT_BILLING_TOTAL"].Rows[0]["NET_AMOUNT"] : DBNull.Value;
                                }
                                else//insurace and company
                                {
                                    vmode = Enum.GetName(typeof(VisitMode), Convert.ToUInt32(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["VISIT_MODE"]));
                                    //drNew["COMPANY"] = BillDetails.Tables["INCO_PATIENT_SCHEME"].Rows[0]["COMPANY"];
                                    //drNew["INSURANCE"] = BillDetails.Tables["INCO_PATIENT_SCHEME"].Rows[0]["INSURANCE"];
                                    GetContract(drNew, BillDetails);
                                    if (BillDetails.Tables.Contains("VW_BILL_DTLS") && BillDetails.Tables["VW_BILL_DTLS"].Rows.Count > 0 &&
                                        Convert.ToDecimal(IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["ISPACKAGE_SERVICE"])) != 2
                                        && Convert.ToInt32(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["VISIT_MODE"]) != 2)
                                    {
                                        if (Convert.ToInt16(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["ISPATIENT"]) == 1)
                                        {
                                            drNew["PAID_AMOUNT"] = getSettledAmount(BillDetails, Convert.ToString(BillDetails.Tables["PAT_PATIENT_NAME"].Rows[0]["MRNO"]));
                                        }
                                        else
                                        {
                                            drNew["PAID_AMOUNT"] = 0;
                                        }
                                    }
                                    else if (BillDetails.Tables.Contains("VW_BILL_DTLS") && BillDetails.Tables["VW_BILL_DTLS"].Rows.Count > 0 &&
                                        Convert.ToDecimal(IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["ISPACKAGE_SERVICE"])) == 2
                                        && Convert.ToInt32(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["VISIT_MODE"]) != 2)
                                        drNew["PAID_AMOUNT"] = DBNull.Value;
                                    else
                                    {
                                        if (Convert.ToInt16(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["ISPATIENT"]) == 1)
                                        {
                                            drNew["PAID_AMOUNT"] = getSettledAmount(BillDetails, Convert.ToString(BillDetails.Tables["PAT_PATIENT_NAME"].Rows[0]["MRNO"]));
                                        }
                                        else
                                        {
                                            drNew["PAID_AMOUNT"] = 0;
                                        }
                                    }
                                   // drNew["PAID_AMOUNT"] = getSettledAmount(BillDetails, Convert.ToString(BillDetails.Tables["PAT_PATIENT_NAME"].Rows[0]["MRNO"]));
                                }
                                if (servType == ServiceType.Cafeteria)
                                {
                                    drNew["DISC_N_ADJ"] = -1 * (BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["DISCOUNT"] == DBNull.Value ? 0 : Convert.ToDecimal(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["DISCOUNT"])) +
                                        (BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["DISCOUNT"] == DBNull.Value ? 0 : Convert.ToDecimal(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["MARKUP"]));
                                }
                            }
                            else
                            {
                                drNew["BILL_NO"] = "FREE";
                                drNew["BILL_DATE"] = BillDetails.Tables["CON_PAT_BILLING"].Rows[0]["BILL_DATE"];
                            }

                            drNew["BILL_TYPE"] = vmode;
                            drNew["DOCTOR_NAME"] = BillDetails.Tables["BILL_COMMON_DETAILS"].Rows[0]["PROVIDER_NAME"];
                            drNew["SERVICE_TYPE"] = servType;
                            GetContract(drNew, BillDetails);

                            //for free consulataion
                            if (BillDetails.Tables.Contains("INV_PAT_BILLING") && BillDetails.Tables["INV_PAT_BILLING"].Rows.Count > 0 && IfNullReturnZero(BillDetails.Tables["INV_PAT_BILLING"].Rows[0]["ISPACKAGE_SERVICE"]) != 2)
                            {
                                drNew["TOTAL_AMOUNT_IN_WORDS"] = Convert.ToString(BillDetails.Tables["INV_PAT_BILLING_TOTAL"].Rows[0]["NET_AMOUNT"]) != null &&
                                                                Convert.ToString(BillDetails.Tables["INV_PAT_BILLING_TOTAL"].Rows[0]["NET_AMOUNT"]) != string.Empty &&
                                                                Convert.ToString(BillDetails.Tables["INV_PAT_BILLING_TOTAL"].Rows[0]["NET_AMOUNT"]) != "0"
                                                                ? comFun.ToWords(Convert.ToDouble(BillDetails.Tables["INV_PAT_BILLING_TOTAL"].Rows[0]["NET_AMOUNT"]))
                                                                : "Free";
                            }
                            else if (BillDetails.Tables.Contains("VW_BILL_DTLS") && BillDetails.Tables["VW_BILL_DTLS"].Rows.Count > 0 && IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["ISPACKAGE_SERVICE"]) != 2)
                            {
                                drNew["TOTAL_AMOUNT_IN_WORDS"] = Convert.ToString(BillDetails.Tables["INV_PAT_BILLING_TOTAL"].Rows[0]["NET_AMOUNT"]) != null &&
                                                                Convert.ToString(BillDetails.Tables["INV_PAT_BILLING_TOTAL"].Rows[0]["NET_AMOUNT"]) != string.Empty &&
                                                                Convert.ToString(BillDetails.Tables["INV_PAT_BILLING_TOTAL"].Rows[0]["NET_AMOUNT"]) != "0"
                                                                ? comFun.ToWords(Convert.ToDouble(BillDetails.Tables["INV_PAT_BILLING_TOTAL"].Rows[0]["NET_AMOUNT"]))
                                                                : "Free";
                            }
                            else
                                drNew["TOTAL_AMOUNT_IN_WORDS"] = DBNull.Value;
                        }
                        PrintData.Tables["BILL_MASTER"].Rows.Add(drNew);
                        drNew = null;
                        if (servType == ServiceType.Pharmacy)
                        {
                            foreach (DataRow dr in BillDetails.Tables["INV_PAT_BILLING"].Rows)
                            {
                                drNew = PrintData.Tables["BILL_DETAILS"].NewRow();
                                drNew["BILL_NO"] = BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["BILL_NO"];
                                drNew["QTY"] = dr["QTY"];
                                drNew["MEDICINE_NAME"] = dr["NAME"];
                                if (dr["GROSS_AMOUNT"] != DBNull.Value)
                                {
                                    drNew["GRAND_TOTAL"] = GetGrossAmount(dr);
                                }
                                else
                                {
                                    drNew["GRAND_TOTAL"] = 0;
                                }
                                if (dr["CO_PAY_AMOUNT"] != DBNull.Value)
                                {
                                    drNew["COPAY"] = dr["CO_PAY_AMOUNT"];
                                }
                                else
                                {
                                    drNew["COPAY"] = 0;
                                }
                                drNew["NET_TOTAL"] = dr["NET_AMOUNT"];
                                drNew["BATCH_NO"] = dr["BATCHNO"];
                                drNew["EXPIRY"] = dr["EXP_DATE"];
                                drNew["IS_RETURN"] = 0;
                                //CheckValidGross(drNew);
                                PrintData.Tables["BILL_DETAILS"].Rows.Add(drNew);
                            }

                        }
                        else if (servType == ServiceType.Investigation || servType == ServiceType.Cafeteria || servType == ServiceType.Surgery
                            || servType == ServiceType.IncomingService)
                        {
                            if (servType == ServiceType.Cafeteria)
                            {
                                if (Convert.ToDecimal(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["ISPATIENT"]) == 0
                                    && Convert.ToDecimal(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["PROFILE_ID"]) != -1)
                                {
                                    PrintData.Tables["BILL_MASTER"].Rows[0]["PROFILE_ID"] = 0;//for employee hard coded value
                                }
                                else if (Convert.ToDecimal(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["ISPATIENT"]) == 0
                                    && Convert.ToDecimal(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["PROFILE_ID"]) == -1)
                                {
                                    PrintData.Tables["BILL_MASTER"].Rows[0]["PROFILE_ID"] = 1;//for outsider hard coded value
                                }
                                else
                                {
                                    PrintData.Tables["BILL_MASTER"].Rows[0]["PROFILE_ID"] = 2;//for patient hard coded value
                                }
                            }
                            foreach (DataRow dr in BillDetails.Tables["INV_PAT_BILLING"].Rows)
                            {
                                drNew = PrintData.Tables["BILL_DETAILS"].NewRow();
                                drNew["BILL_NO"] = BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["BILL_NO"];
                                drNew["QTY"] = dr["QTY"];
                                if (BillDetails.Tables.Contains("INV_PAT_BILLING") && IfNullReturnZero(BillDetails.Tables["INV_PAT_BILLING"].Rows[0]["ISPACKAGE_SERVICE"]) != 2)
                                    drNew["GRAND_TOTAL"] = GetGrossAmount(dr);
                                else
                                    drNew["GRAND_TOTAL"] = DBNull.Value;
                                if (BillDetails.Tables.Contains("INV_PAT_BILLING") && IfNullReturnZero(BillDetails.Tables["INV_PAT_BILLING"].Rows[0]["ISPACKAGE_SERVICE"]) != 2)
                                {
                                    if (dr["CO_PAY_AMOUNT"] != DBNull.Value)
                                    {
                                        drNew["COPAY"] = dr["CO_PAY_AMOUNT"];
                                    }
                                    else
                                    {
                                        drNew["COPAY"] = 0;
                                    }
                                }
                                else
                                    drNew["COPAY"] = DBNull.Value;
                                if (servType == ServiceType.Cafeteria)
                                {
                                    drNew["RATE"] = Convert.ToDecimal(dr["NET_AMOUNT"]) / Convert.ToDecimal(dr["QTY"]);
                                }
                                //if (BillDetails.Tables.Contains("INV_PAT_BILLING"))
                                drNew["NET_TOTAL"] = IfNullReturnZero(BillDetails.Tables["INV_PAT_BILLING"].Rows[0]["ISPACKAGE_SERVICE"]) != 2 ? dr["NET_AMOUNT"] : DBNull.Value;
                                //else
                                //    drNew["NET_TOTAL"] = DBNull.Value;
                                drNew["SERVICE_NAME"] = servType == ServiceType.Surgery ? GetSurgeryServiceName(dr) :
                                    dr.Table.Columns.Contains("ALIAS_NAME") && dr["ALIAS_NAME"] != DBNull.Value ?
                                    dr["ALIAS_NAME"] : dr["NAME"];
                                drNew["CPTCODE"] = dr["CPT_CODE"];
                                //CheckValidGross(drNew);
                                PrintData.Tables["BILL_DETAILS"].Rows.Add(drNew);
                            }
                        }
                        else if (servType == ServiceType.Advance || servType == ServiceType.Deductible)
                        {
                            drNew = PrintData.Tables["BILL_DETAILS"].NewRow();
                            drNew["BILL_NO"] = BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["BILL_NO"];
                            if (servType == ServiceType.Advance)
                            {
                                drNew["SERVICE_NAME"] = Enum.GetName(typeof(ServiceType), Convert.ToUInt32(servType)) + " (" + Convert.ToString(BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["ADV_TYPE"]) + ")";
                            }
                            else
                            {
                                drNew["SERVICE_NAME"] = Enum.GetName(typeof(ServiceType), Convert.ToUInt32(servType)) + " (" + Convert.ToString(BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["DEPT_NAME"]) + ")";
                            }
                            drNew["NET_TOTAL"] = BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["FINAL_BILL_AMOUNT"];
                            drNew["ADV_TYPE"] = BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["ADV_TYPE"];
                            PrintData.Tables["BILL_DETAILS"].Rows.Add(drNew);
                        }
                        else if (servType == ServiceType.Consultation)
                        {
                            drNew = PrintData.Tables["BILL_DETAILS"].NewRow();
                            StringBuilder sbServiceName = new StringBuilder();
                            sbServiceName.Append(BillDetails.Tables["CON_PAT_BILLING"].Rows[0]["CON_TYPE_NAME"].ToString());
                            sbServiceName.Append(" Consultation");
                            //for free consulatation
                            drNew["BILL_NO"] = BillDetails != null && BillDetails.Tables.Contains("GEN_PAT_BILLING")
                                                && BillDetails.Tables["GEN_PAT_BILLING"].Rows.Count > 0
                                                ? BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["BILL_NO"]
                                                : "FREE";
                            drNew["SERVICE_NAME"] = sbServiceName.ToString();//BillDetails.Tables["CON_PAT_BILLING"].Rows[0]["CON_TYPE_NAME"];
                            if (IfNullReturnZero(BillDetails.Tables["CON_PAT_BILLING"].Rows[0]["ISPACKAGE_SERVICE"]) != 2)
                                drNew["GRAND_TOTAL"] = GetGrossAmount(BillDetails.Tables["CON_PAT_BILLING"].Rows[0]);
                            else
                                drNew["GRAND_TOTAL"] = DBNull.Value;
                            drNew["NET_TOTAL"] = IfNullReturnZero(BillDetails.Tables["CON_PAT_BILLING"].Rows[0]["ISPACKAGE_SERVICE"]) != 2 ? BillDetails.Tables["CON_PAT_BILLING"].Rows[0]["NET_AMOUNT"] : DBNull.Value;
                            drNew["TOKEN_NO"] = BillDetails.Tables["CON_PAT_BILLING"].Rows[0]["TOKEN_NO"];
                            if (BillDetails.Tables["APPT_ALLOCATION"].Rows.Count > 0)
                            {
                                drNew["APP_TIME"] = BillDetails.Tables["APPT_ALLOCATION"].Rows[0]["ALLOCATION_DATE"];
                                drNew["MODE_OF_APPT"] = BillDetails.Tables["APPT_ALLOCATION"].Rows[0]["MODE_OF_APPT"];
                            }
                            drNew["QTY"] = 1;
                            if (IfNullReturnZero(BillDetails.Tables["CON_PAT_BILLING"].Rows[0]["ISPACKAGE_SERVICE"]) != 2)
                            {
                                if (Convert.ToString(PrintData.Tables["BILL_MASTER"].Rows[0]["BILL_TYPE"]) != "OP")
                                {
                                    if (BillDetails.Tables["INV_PAT_BILLING_TOTAL"].Rows[0]["CO_PAY_AMOUNT"] != DBNull.Value)
                                    {
                                        drNew["COPAY"] = BillDetails.Tables["INV_PAT_BILLING_TOTAL"].Rows[0]["CO_PAY_AMOUNT"];
                                    }
                                }
                                else
                                {
                                    drNew["COPAY"] = 0;
                                }
                            }
                            else
                                drNew["COPAY"] = DBNull.Value;
                            // CheckValidGross(drNew);
                            // drNew["COPAY"] = BillDetails.Tables["CON_PAT_BILLING"].Rows[0]["CO_PAY_AMOUNT"];
                            PrintData.Tables["BILL_DETAILS"].Rows.Add(drNew);
                        }
                        else if (servType == ServiceType.Registration || servType == ServiceType.ReRegistration)
                        {
                            drNew = PrintData.Tables["BILL_DETAILS"].NewRow();
                            drNew["BILL_NO"] = BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["BILL_NO"];
                            //   dtBillDetails.Rows[0]["SERVICE_NAME"] = BillDetails.Tables["CON_PAT_BILLING"].Rows[0]["CON_TYPE_NAME"];
                            drNew["GRAND_TOTAL"] = GetGrossAmount(BillDetails.Tables["REG_PAT_BILLING"].Rows[0]);
                            drNew["NET_TOTAL"] = BillDetails.Tables["REG_PAT_BILLING"].Rows[0]["NET_AMOUNT"];
                            drNew["QTY"] = 1;

                            if (Convert.ToString(PrintData.Tables["BILL_MASTER"].Rows[0]["BILL_TYPE"]) != "OP")
                            {
                                if (BillDetails.Tables["INV_PAT_BILLING_TOTAL"].Rows[0]["CO_PAY_AMOUNT"] != DBNull.Value)
                                {
                                    drNew["COPAY"] = BillDetails.Tables["INV_PAT_BILLING_TOTAL"].Rows[0]["CO_PAY_AMOUNT"];
                                }
                            }
                            else
                            {
                                drNew["COPAY"] = 0;
                            }
                            string strServName = Convert.ToString(BillDetails.Tables["BILL_COMMON_DETAILS"].Rows[0]["REG_MAST_TYPE_NAME"]);
                            string strServNameTmp = strServName.Replace(" ", "").ToUpper();
                            if ((!strServNameTmp.Contains(servType.ToString().ToUpper())))
                            {
                                drNew["SERVICE_NAME"] = strServName + " " + servType.ToString();
                            }
                            else
                            {
                                drNew["SERVICE_NAME"] = BillDetails.Tables["BILL_COMMON_DETAILS"].Rows[0]["REG_MAST_TYPE_NAME"];
                            }
                            //drNew["SERVICE_NAME"] = BillDetails.Tables["BILL_COMMON_DETAILS"].Rows[0]["REG_MAST_TYPE_NAME"]+" "+servType.ToString() ; -- old
                            // CheckValidGross(drNew);
                            PrintData.Tables["BILL_DETAILS"].Rows.Add(drNew);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Creates the re print data.
        /// </summary>
        /// <param name="BillDetails">The bill details.</param>
        /// <param name="servType">Type of the serv.</param>
        /// <param name="printType">Type of the print.</param>
        /// <returns></returns>
        private void CreateRePrintData(DataSet BillDetails, ref DataSet PrintData, ServiceType servType, ProfileCategory PatientOrOutsider)
        {
            try
            {
                string vmode = string.Empty;
                string servName = string.Empty;
                CommonFunctions comFun = new CommonFunctions();
                DataRow drNew = null;
                DataRow drDtls = null;
                double retTotal = 0;
                double salesTotal = 0;
                string contract = string.Empty;
                double tmpVal = 0;
                // double retCopayTotal = 0;
                double salesCopayTotal = 0;
                if (servType == ServiceType.Registration || servType == ServiceType.ReRegistration || servType == ServiceType.Consultation)
                {
                    drNew = PrintData.Tables["BILL_MASTER"].NewRow();
                    drDtls = PrintData.Tables["BILL_DETAILS"].NewRow();
                    //For free consultation 
                    if (BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["BILL_NO"] == DBNull.Value || Convert.ToString(BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["BILL_NO"]) == string.Empty)
                    {
                        BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["BILL_NO"] = "FREE";
                        BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["NAME"] = "FREE";
                    }
                    drNew["BILL_NO"] = BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["BILL_NO"];
                    drNew["MRNO"] = BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["MRNO"];
                    if (BillDetails.Tables["PRINT_INVOICE_DATA"].Columns.Contains("PATIENT_NAME"))
                    {
                        drNew["PAT_NAME"] = BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["PATIENT_NAME"];
                    }
                    else
                    {
                        drNew["PAT_NAME"] = BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["PAT_NAME"];
                    }
                    drNew["BILLED_BY"] = BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["BILLED_BY"];
                    drNew["BILL_DATE"] = BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["BILL_DATE"];
                    drNew["SERVICE_TYPE"] = Enum.GetName(typeof(ServiceType), servType);
                    if (servType == ServiceType.Cafeteria)
                    {
                        drNew["DISC_N_ADJ"] = -1 * (BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["DISCOUNT"] == DBNull.Value ? 0 : Convert.ToDecimal(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["DISCOUNT"])) +
                            (BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["DISCOUNT"] == DBNull.Value ? 0 : Convert.ToDecimal(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["MARKUP"]));
                    }
                    if (servType == ServiceType.Consultation)
                    {
                        drNew["DOCTOR_NAME"] = BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["H_EMP_FNAME"];
                        if (BillDetails.Tables["PRINT_INVOICE_DATA"].Columns.Contains("H_ADMIN_DEPT_DNAME"))
                        {
                            drNew["CLINIC"] = BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["H_ADMIN_DEPT_DNAME"];
                        }
                        else
                        {
                            drNew["CLINIC"] = BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["DEPARTMENT_NAME"];
                        }


                    }//for free consulatation donot enter the condition
                    if (Convert.ToString(BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["VISIT_MODE"]) != null &&
                        Convert.ToString(BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["VISIT_MODE"]) != string.Empty &&
                        Convert.ToInt16(BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["VISIT_MODE"]) == (int)VisitMode.OPCASH)
                    {
                        vmode = "OP"; // Enum.GetName(typeof(VisitMode), Convert.ToUInt32(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["VISIT_MODE"]));
                        drNew["PAID_AMOUNT"] = BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["NET_AMOUNT"];
                    }
                    else//insurace and company
                    {
                        //for free consulatation does not have visit_mode
                        vmode = Convert.ToString(BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["VISIT_MODE"]) != null &&
                                Convert.ToString(BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["VISIT_MODE"]) != string.Empty
                                ? Enum.GetName(typeof(VisitMode), Convert.ToUInt32(BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["VISIT_MODE"]))
                                : string.Empty;

                        if (BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["CORP_COMPANY_NAME"] != DBNull.Value)
                        {
                            contract = Convert.ToString(BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["CORP_COMPANY_NAME"]);

                        }
                        if (BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["INS_COMPANY_NAME"] != DBNull.Value)
                        {
                            if (contract != string.Empty)
                            {
                                contract = contract + "/ ";
                            }
                            contract = contract + BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["INS_COMPANY_NAME"];
                        }
                        if (BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["SCHEME_NAME"] != DBNull.Value)
                        {
                            if (contract != string.Empty)
                            {
                                contract = contract + "/ ";
                            }
                            contract = contract + BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["SCHEME_NAME"];
                            if (BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["PATIENT_SETTLED_AMOUNT"] != DBNull.Value)
                            {
                                drNew["PAID_AMOUNT"] = BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["PATIENT_SETTLED_AMOUNT"];
                            }
                            else
                            {
                                drNew["PAID_AMOUNT"] = 0;
                            }
                        }
                    }
                    drNew["CONTRACT_NAME"] = contract;
                    drNew["BILL_TYPE"] = vmode;
                    if (IfNullReturnZero(BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["ISPACKAGE_SERVICE"]) != 2)
                        drNew["TOTAL_AMOUNT_IN_WORDS"] = Convert.ToString(BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["NET_AMOUNT"]) != null &&
                                                        Convert.ToString(BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["NET_AMOUNT"]) != string.Empty &&
                                                        Convert.ToInt32(BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["NET_AMOUNT"]) != 0
                                                        ? comFun.ToWords(Convert.ToDouble(BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["NET_AMOUNT"]))
                                                        : "Free";
                    else
                        drNew["TOTAL_AMOUNT_IN_WORDS"] = DBNull.Value;
                    PrintData.Tables["BILL_MASTER"].Rows.Add(drNew);
                    drNew = null;
                    drNew = PrintData.Tables["BILL_DETAILS"].NewRow();

                    drNew["NET_TOTAL"] = IfNullReturnZero(BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["ISPACKAGE_SERVICE"]) != 2 ? BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["NET_AMOUNT"] : DBNull.Value;
                    drNew["BILL_NO"] = BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["BILL_NO"];
                    drNew["COPAY"] = IfNullReturnZero(BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["ISPACKAGE_SERVICE"]) != 2 ? BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["CO_PAY_AMOUNT"] : DBNull.Value;
                    if (BillDetails.Tables["PRINT_INVOICE_DATA"].Columns.Contains("GROSS_AMOUNT"))
                    {
                        if (BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["GROSS_AMOUNT"] != DBNull.Value)
                        {
                            if (IfNullReturnZero(BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["ISPACKAGE_SERVICE"]) != 2)
                                drNew["GRAND_TOTAL"] = GetGrossAmount(BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]);
                            else
                                drNew["GRAND_TOTAL"] = DBNull.Value;
                        }
                    }
                    else if (BillDetails.Tables["PRINT_INVOICE_DATA"].Columns.Contains("AMOUNT"))
                    {
                        if (BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["AMOUNT"] != DBNull.Value)
                        {
                            if (IfNullReturnZero(BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["ISPACKAGE_SERVICE"]) != 2)
                                drNew["GRAND_TOTAL"] = GetGrossAmount(BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]);
                            else
                                drNew["GRAND_TOTAL"] = DBNull.Value;
                        }
                    }

                    if (servType == ServiceType.Registration)
                    {
                        if (Convert.ToString(BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["NAME"]).ToUpper() != "REGISTRATION")
                        {
                            servName = Convert.ToString(BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["NAME"]) + " Registration";
                        }
                        else
                        {
                            servName = Convert.ToString(BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["NAME"]);
                        }
                    }
                    else if (servType == ServiceType.ReRegistration)
                    {
                        if (Convert.ToString(BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["NAME"]).ToUpper() != "REGISTRATION" || Convert.ToString(BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["NAME"]).ToUpper() != "REREGISTRATION")
                        {
                            servName = Convert.ToString(BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["NAME"]) + " Re Registration";
                        }
                    }
                    else if (servType == ServiceType.Consultation)
                    {
                        if (Convert.ToString(BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["NAME"]).ToUpper() != "CONSULTATION")
                        {
                            servName = Convert.ToString(BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["NAME"]) + " Consultation";
                        }
                        else
                        {
                            servName = Convert.ToString(BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["SERVICE_NAME"]) + " Consultation";
                        }
                        drNew["TOKEN_NO"] = BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["TOKEN_NO"];
                        drNew["APP_TIME"] = BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["ALLOCATION_DATE"];
                        drNew["MODE_OF_APPT"] = BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["MODE_OF_APPT"];
                    }
                    drNew["SERVICE_NAME"] = servName;
                    drNew["QTY"] = 1;
                    //drNew["CPTCODE"]=;
                    //drNew["COPAY"]=;
                    //drNew["GRAND_TOTAL"]=BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["GROSS_AMOUNT"];
                    //CheckValidGross(drNew);
                    PrintData.Tables["BILL_DETAILS"].Rows.Add(drNew);
                }
                else if (servType == ServiceType.Advance || servType == ServiceType.Deductible)
                {
                    drNew = PrintData.Tables["BILL_MASTER"].NewRow();
                    drNew["BILL_NO"] = BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["BILL_NO"];
                    drNew["MRNO"] = BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["MRNO"];
                    drNew["PAT_NAME"] = GetPatientName(BillDetails.Tables["PRINT_INVOICE_DATA"]);
                    drNew["BILLED_BY"] = BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["BILLED_BY"];
                    drNew["BILL_DATE"] = BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["BILL_DATE"];
                    drNew["SERVICE_TYPE"] = Enum.GetName(typeof(ServiceType), servType);
                    drNew["TOTAL_AMOUNT_IN_WORDS"] = comFun.ToWords(Convert.ToDouble(BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["NET_AMOUNT"]));
                    drNew["PAID_AMOUNT"] = BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["NET_AMOUNT"];
                    drNew["ENCOUNTER_NO"] = BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["ENCOUNTER_NO"];
                    GetContract(drNew, BillDetails);
                    PrintData.Tables["BILL_MASTER"].Rows.Add(drNew);
                    drNew = PrintData.Tables["BILL_DETAILS"].NewRow();
                    drNew["BILL_NO"] = BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["BILL_NO"];
                   
                   // drNew["SERVICE_NAME"] = Enum.GetName(typeof(ServiceType), Convert.ToUInt32(servType));
                    if (servType == ServiceType.Advance)
                    {
                        drNew["SERVICE_NAME"] = Enum.GetName(typeof(ServiceType), Convert.ToUInt32(servType)) + " (" + Convert.ToString(BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["ADV_TYPE"]) + ")";
                    }
                    else
                    {
                        drNew["SERVICE_NAME"] = Enum.GetName(typeof(ServiceType), Convert.ToUInt32(servType)) + " (" + Convert.ToString(BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["DEPT_NAME"]) + ")";
                    }
                    drNew["NET_TOTAL"] = BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["NET_AMOUNT"];
                    PrintData.Tables["BILL_DETAILS"].Rows.Add(drNew);
                }
                else if (servType == ServiceType.Pharmacy || servType == ServiceType.Investigation || servType == ServiceType.Cafeteria || servType == ServiceType.Surgery)
                {
                    drNew = PrintData.Tables["BILL_MASTER"].NewRow();
                    drNew["BILL_NO"] = BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["BILL_NO"];
                    if (PatientOrOutsider == ProfileCategory.Patient)
                    {
                        drNew["MRNO"] = BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["MRNO"];
                    }
                    else
                    {
                        drNew["PROFILE_ID"] = BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["PROFILE_ID"];
                    }
                    if (servType == ServiceType.Cafeteria)
                    {
                        if (BillDetails.Tables["VW_BILL_MASTER"].Columns.Contains("ADJUSTMENTS"))
                        {
                            drNew["DISC_N_ADJ"] = BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["ADJUSTMENTS"];
                        }
                        else if (BillDetails.Tables["VW_BILL_MASTER"].Columns.Contains("DISCOUNT") && BillDetails.Tables["VW_BILL_MASTER"].Columns.Contains("MARKUP"))
                        {
                            drNew["DISC_N_ADJ"] = -1 * (BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["DISCOUNT"] == DBNull.Value ? 0 : Convert.ToDecimal(BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["DISCOUNT"])) +
                                (BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["DISCOUNT"] == DBNull.Value ? 0 : Convert.ToDecimal(BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["MARKUP"]));
                        }
                    }
                    drNew["PAT_NAME"] = BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["PATIENT_NAME"];
                    drNew["BILLED_BY"] = BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["BILLED_BY"];
                    drNew["BILL_DATE"] = BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["BILL_DATE"];
                    drNew["DOCTOR_NAME"] = BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["H_EMP_FNAME"];
                    drNew["CLINIC"] = BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["DEPARTMENT_NAME"];
                    //if (Convert.ToString(BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["VISIT_MODE"]).Length < 2)
                    //{
                    //    BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["VISIT_MODE"] = 
                    //}
                    if (Convert.ToInt16(BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["VISIT_MODE"]) == (int)VisitMode.OPCASH)//|| PatientOrOutsider == ProfileCategory.OutsidePatient
                    {
                        vmode = "OP"; // Enum.GetName(typeof(VisitMode), Convert.ToUInt32(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["VISIT_MODE"]));
                        drNew["PAID_AMOUNT"] = BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["NET_AMOUNT"];
                    }
                    else//insurace and company
                    {
                        vmode = Enum.GetName(typeof(VisitMode), Convert.ToUInt32(BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["VISIT_MODE"]));
                        //drNew["COMPANY"] = BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["COMPANY_NAME"];
                        // drNew["INSURANCE"] = BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["INSURANCE_NAME"];
                        GetContract(drNew, BillDetails);
                        if (PatientOrOutsider == ProfileCategory.OutsidePatient)
                        {
                            drNew["PAID_AMOUNT"] = 0;
                        }
                        else
                        {
                            if (BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["PATIENT_SETTLED_AMOUNT"] != DBNull.Value)
                            {
                                drNew["PAID_AMOUNT"] = BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["PATIENT_SETTLED_AMOUNT"];
                            }
                            else
                            {
                                drNew["PAID_AMOUNT"] = 0;
                            }
                        }
                    }
                    drNew["BILL_TYPE"] = vmode;

                    if (servType == ServiceType.Pharmacy)
                    {

                        foreach (DataRow dr in BillDetails.Tables["VW_BILL_DTLS"].Rows)
                        {
                            drDtls = PrintData.Tables["BILL_DETAILS"].NewRow();
                            drDtls["BILL_NO"] = BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["BILL_NO"];
                            drDtls["CPTCODE"] = dr["CPT_CODE"];
                            if (Convert.ToDecimal(dr["QTY"]) < 0)
                            {
                                if (dr["FINAL_BILL_AMOUNT"] != DBNull.Value)
                                {
                                    drDtls["NET_TOTAL"] = (-1 * (Convert.ToDecimal(dr["FINAL_BILL_AMOUNT"])));
                                    retTotal = retTotal + Convert.ToDouble(drDtls["NET_TOTAL"]);
                                }
                                drDtls["QTY"] = -1 * (Convert.ToDouble(dr["QTY"]));
                                drDtls["GRAND_TOTAL"] = -1 * (GetGrossAmount(dr));
                                if (dr["CO_PAY_AMOUNT"] != DBNull.Value)
                                {
                                    drDtls["COPAY"] = -1 * (Convert.ToDecimal(dr["CO_PAY_AMOUNT"]));
                                }
                                else
                                {
                                    drDtls["COPAY"] = 0;
                                }
                                drDtls["IS_RETURN"] = 1;

                            }
                            else
                            {
                                drDtls["QTY"] = dr["QTY"];
                                drDtls["GRAND_TOTAL"] = GetGrossAmount(dr);
                                drDtls["NET_TOTAL"] = dr["FINAL_BILL_AMOUNT"];
                                drDtls["IS_RETURN"] = 0;
                                if (dr["CO_PAY_AMOUNT"] != DBNull.Value)
                                {
                                    drDtls["COPAY"] = dr["CO_PAY_AMOUNT"];
                                    salesCopayTotal = salesCopayTotal + Convert.ToDouble(drDtls["COPAY"]);
                                }
                                else
                                {
                                    drDtls["COPAY"] = 0;
                                }
                                salesTotal = salesTotal + Convert.ToDouble(drDtls["NET_TOTAL"]);
                            }
                            drDtls["MEDICINE_NAME"] = dr["SERVICE_NAME"];
                            drDtls["BATCH_NO"] = dr["BATCHNO"];
                            drDtls["EXPIRY"] = dr["EXP_DATE"];
                            //CheckValidGross(drDtls);
                            PrintData.Tables["BILL_DETAILS"].Rows.Add(drDtls);
                        }
                        if (Convert.ToString(drNew["BILL_TYPE"]) == "OP")
                        {

                            if (retTotal > 0)
                            {
                                drNew["TOTAL_AMOUNT_IN_WORDS"] = comFun.ToWords(retTotal);
                                drNew["PAID_AMOUNT"] = retTotal;
                            }
                            else
                            {
                                drNew["TOTAL_AMOUNT_IN_WORDS"] = comFun.ToWords(salesTotal);
                                drNew["PAID_AMOUNT"] = salesTotal;
                            }

                        }
                        else
                        {

                            if (retTotal > 0)
                            {
                                if (BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["PATIENT_REFUND_AMOUNT"] != DBNull.Value)
                                {
                                    tmpVal = Convert.ToDouble(BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["PATIENT_REFUND_AMOUNT"]);
                                }
                                else
                                {
                                    tmpVal = 0;
                                }
                                drNew["PAID_AMOUNT"] = tmpVal;
                                drNew["TOTAL_AMOUNT_IN_WORDS"] = comFun.ToWords(tmpVal);
                            }
                            else
                            {
                                if (BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["PATIENT_SETTLED_AMOUNT"] != DBNull.Value)
                                {
                                    tmpVal = Convert.ToDouble(BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["PATIENT_SETTLED_AMOUNT"]);
                                }
                                else
                                {
                                    tmpVal = 0;
                                }
                                drNew["PAID_AMOUNT"] = tmpVal;
                                drNew["TOTAL_AMOUNT_IN_WORDS"] = comFun.ToWords(tmpVal);
                            }
                        }
                    }
                    else if (servType == ServiceType.Investigation || servType == ServiceType.Cafeteria || servType == ServiceType.Surgery)
                    {
                        //if (servType == ServiceType.Cafeteria)
                        //{
                        //    if (Convert.ToDecimal(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["ISPATIENT"]) == 0
                        //        && Convert.ToDecimal(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["PROFILE_ID"]) != -1)
                        //    {
                        //        PrintData.Tables["BILL_MASTER"].Rows[0]["PROFILE_ID"] = 0;//for employee hard coded value
                        //    }
                        //    else if (Convert.ToDecimal(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["ISPATIENT"]) == 0
                        //        && Convert.ToDecimal(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["PROFILE_ID"]) == -1)
                        //    {
                        //        PrintData.Tables["BILL_MASTER"].Rows[0]["PROFILE_ID"] = 1;//for outsider hard coded value
                        //    }
                        //    else
                        //    {
                        //        PrintData.Tables["BILL_MASTER"].Rows[0]["PROFILE_ID"] = 2;//for patient hard coded value
                        //    }
                        //}
                        foreach (DataRow dr in BillDetails.Tables["VW_BILL_DTLS"].Rows)
                        {
                            drDtls = PrintData.Tables["BILL_DETAILS"].NewRow();
                            drDtls["BILL_NO"] = BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["BILL_NO"];
                            drDtls["QTY"] = dr["QTY"];
                            if (IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["ISPACKAGE_SERVICE"]) != 2)
                                drDtls["GRAND_TOTAL"] = GetGrossAmount(dr);
                            else if (IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["ISPACKAGE_SERVICE"]) == 2
                                && BillDetails.Tables["VW_BILL_DTLS"].Columns.Contains("VISIT_MODE")
                                && IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["VISIT_MODE"]) == 2)
                                drDtls["GRAND_TOTAL"] = GetGrossAmount(dr);
                            else if (IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["ISPACKAGE_SERVICE"]) == 2
                                && BillDetails.Tables.Contains("GEN_PAT_BILLING") && BillDetails.Tables["GEN_PAT_BILLING"].Rows.Count > 0
                                && IfNullReturnZero(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["VISIT_MODE"]) == 2)
                                drDtls["GRAND_TOTAL"] = GetGrossAmount(dr);
                            else if (IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["ISPACKAGE_SERVICE"]) != 2
                              && IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["SERVICE_TYPE_ID"]) == 11)
                                drDtls["GRAND_TOTAL"] = GetGrossAmount(dr);
                            else
                                drDtls["GRAND_TOTAL"] = DBNull.Value;
                            if (dr["CO_PAY_AMOUNT"] != DBNull.Value)
                            {
                                if (BillDetails.Tables["VW_BILL_DTLS"].Columns.Contains("BILL_CATEGORY"))
                                    drDtls["COPAY"] = (IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["ISPACKAGE_SERVICE"]) == 2
                                        && IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["BILL_CATEGORY"]) != 1) ? DBNull.Value : dr["CO_PAY_AMOUNT"];
                                else
                                    drDtls["COPAY"] = (IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["ISPACKAGE_SERVICE"]) == 2) ? DBNull.Value : dr["CO_PAY_AMOUNT"];
                                if (IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["ISPACKAGE_SERVICE"]) == 2
                                    && BillDetails.Tables["VW_BILL_DTLS"].Columns.Contains("VISIT_MODE")
                                   && IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["VISIT_MODE"]) == 2)
                                    drDtls["COPAY"] = dr["CO_PAY_AMOUNT"];
                                else if (IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["ISPACKAGE_SERVICE"]) == 2
                                    && BillDetails.Tables.Contains("GEN_PAT_BILLING") && BillDetails.Tables["GEN_PAT_BILLING"].Rows.Count > 0
                                   && IfNullReturnZero(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["VISIT_MODE"]) == 2)
                                    drDtls["COPAY"] = dr["CO_PAY_AMOUNT"];
                                //else if (IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["ISPACKAGE_SERVICE"]) == 2
                                //    && IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["VISIT_MODE"]) == 0)
                                //    drDtls["COPAY"] = DBNull.Value;
                            }
                            else
                            {
                                drDtls["COPAY"] = 0;
                            }
                            if (servType == ServiceType.Cafeteria)
                            {
                                drDtls["RATE"] = Convert.ToDecimal(dr["FINAL_BILL_AMOUNT"]) / Convert.ToDecimal(dr["QTY"]);
                            }
                            //decimal final_bill_amount = (IfNullReturnZero(dr["FINAL_BILL_AMOUNT"]) + IfNullReturnZero(dr["PKG_AMOUNT"]));
                            if (BillDetails.Tables["VW_BILL_DTLS"].Columns.Contains("BILL_CATEGORY") &&
                                (IfNullReturnZero(dr["ISPACKAGE_SERVICE"]) == 2 && IfNullReturnZero(dr["BILL_CATEGORY"]) != 1))
                                drDtls["NET_TOTAL"] = DBNull.Value;
                            else if ((IfNullReturnZero(dr["ISPACKAGE_SERVICE"]) == 2))
                                drDtls["NET_TOTAL"] = DBNull.Value;
                            else if (BillDetails.Tables["VW_BILL_DTLS"].Columns.Contains("PKG_DISCOUNT"))
                                drDtls["NET_TOTAL"] = (IfNullReturnZero(dr["FINAL_BILL_AMOUNT"]) + IfNullReturnZero(dr["PKG_DISCOUNT"]));
                            else
                                drDtls["NET_TOTAL"] = (IfNullReturnZero(dr["FINAL_BILL_AMOUNT"]));
                            //if (BillDetails.Tables["VW_BILL_DTLS"].Columns.Contains("BILL_CATEGORY"))
                            //    drDtls["NET_TOTAL"] = (IfNullReturnZero(dr["ISPACKAGE_SERVICE"]) == 2
                            //        && IfNullReturnZero(dr["BILL_CATEGORY"]) != 1) ? DBNull.Value : final_bill_amount;
                            //else
                            //    drDtls["NET_TOTAL"] = (IfNullReturnZero(dr["ISPACKAGE_SERVICE"]) == 2) ? DBNull.Value : dr["FINAL_BILL_AMOUNT"];
                            drDtls["SERVICE_NAME"] = servType == ServiceType.Surgery ? GetSurgeryServiceName(dr) :
                                 dr.Table.Columns.Contains("ALIAS_NAME") && dr["ALIAS_NAME"] != DBNull.Value ?
                                 dr["ALIAS_NAME"] : dr["SERVICE_NAME"];
                            drDtls["CPTCODE"] = dr["CPT_CODE"];
                            if (IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["ISPACKAGE_SERVICE"]) == 2
                                 && BillDetails.Tables["VW_BILL_DTLS"].Columns.Contains("VISIT_MODE")
                                 && BillDetails.Tables["VW_BILL_DTLS"].Columns.Contains("PKG_DISCOUNT")
                                    && IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["VISIT_MODE"]) == 2)
                                drDtls["NET_TOTAL"] = (IfNullReturnZero(dr["FINAL_BILL_AMOUNT"]) + IfNullReturnZero(dr["PKG_DISCOUNT"]));
                            else if (IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["ISPACKAGE_SERVICE"]) == 2
                                && BillDetails.Tables.Contains("GEN_PAT_BILLING") && BillDetails.Tables["GEN_PAT_BILLING"].Rows.Count > 0
                                   && IfNullReturnZero(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["VISIT_MODE"]) == 2
                                && BillDetails.Tables["VW_BILL_DTLS"].Columns.Contains("PKG_DISCOUNT"))
                                drDtls["NET_TOTAL"] = (IfNullReturnZero(dr["FINAL_BILL_AMOUNT"]) + IfNullReturnZero(dr["PKG_DISCOUNT"]));
                            else if (IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["ISPACKAGE_SERVICE"]) != 2
                                && BillDetails.Tables["VW_BILL_DTLS"].Columns.Contains("SERVICE_TYPE_ID")
                              && IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["SERVICE_TYPE_ID"]) == 11)
                                drDtls["NET_TOTAL"] = (IfNullReturnZero(dr["FINAL_BILL_AMOUNT"]) + IfNullReturnZero(dr["PKG_DISCOUNT"]));
                            else if (IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["ISPACKAGE_SERVICE"]) == 2
                                && BillDetails.Tables.Contains("GEN_PAT_BILLING") && BillDetails.Tables["GEN_PAT_BILLING"].Rows.Count > 0
                                   && IfNullReturnZero(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["VISIT_MODE"]) == 2)
                                drDtls["NET_TOTAL"] = (IfNullReturnZero(dr["FINAL_BILL_AMOUNT"]));
                            else if (IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["ISPACKAGE_SERVICE"]) != 2
                                 && BillDetails.Tables["VW_BILL_DTLS"].Columns.Contains("SERVICE_TYPE_ID")
                                && IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["SERVICE_TYPE_ID"]) == 11)
                                drDtls["NET_TOTAL"] = (IfNullReturnZero(dr["FINAL_BILL_AMOUNT"]));
                            //CheckValidGross(drDtls);                            
                            PrintData.Tables["BILL_DETAILS"].Rows.Add(drDtls);
                        }
                        if (BillDetails.Tables["VW_BILL_MASTER"].Columns.Contains("TOT_AMOUNT"))
                        {
                            if (BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["TOT_AMOUNT"] != DBNull.Value)
                                if (IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["ISPACKAGE_SERVICE"]) == 2 && BillDetails.Tables["VW_BILL_DTLS"].Columns.Contains("BILL_CATEGORY")
                                    && IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["BILL_CATEGORY"]) == 1)
                                    drNew["TOTAL_AMOUNT_IN_WORDS"] = comFun.ToWords(Convert.ToDouble(BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["TOT_AMOUNT"]));
                                else if (IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["ISPACKAGE_SERVICE"]) != 2)
                                    drNew["TOTAL_AMOUNT_IN_WORDS"] = comFun.ToWords(Convert.ToDouble(BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["TOT_AMOUNT"]));
                                else if (IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["ISPACKAGE_SERVICE"]) == 2
                                    && BillDetails.Tables["VW_BILL_DTLS"].Columns.Contains("VISIT_MODE")
                                    && IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["VISIT_MODE"]) == 2)
                                    drNew["TOTAL_AMOUNT_IN_WORDS"] = comFun.ToWords(Convert.ToDouble(BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["TOT_AMOUNT"]));
                                else if (IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["ISPACKAGE_SERVICE"]) == 2
                                    && IfNullReturnZero(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["VISIT_MODE"]) == 2)
                                    drNew["TOTAL_AMOUNT_IN_WORDS"] = comFun.ToWords(Convert.ToDouble(BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["TOT_AMOUNT"]));
                                else
                                    drNew["TOTAL_AMOUNT_IN_WORDS"] = DBNull.Value;
                            else
                                if (IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["ISPACKAGE_SERVICE"]) == 2 && BillDetails.Tables["VW_BILL_DTLS"].Columns.Contains("BILL_CATEGORY")
                                    && IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["BILL_CATEGORY"]) == 1)
                                    drNew["TOTAL_AMOUNT_IN_WORDS"] = comFun.ToWords(Convert.ToDouble(BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["GROSS_AMOUNT"]));
                                else if (IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["ISPACKAGE_SERVICE"]) != 2)
                                    drNew["TOTAL_AMOUNT_IN_WORDS"] = comFun.ToWords(Convert.ToDouble(BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["GROSS_AMOUNT"]));
                                else if (IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["ISPACKAGE_SERVICE"]) == 2
                                    && BillDetails.Tables["VW_BILL_DTLS"].Columns.Contains("VISIT_MODE")
                                    && IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["VISIT_MODE"]) == 2)
                                    drNew["TOTAL_AMOUNT_IN_WORDS"] = comFun.ToWords(Convert.ToDouble(BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["GROSS_AMOUNT"]));
                                else if (IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["ISPACKAGE_SERVICE"]) == 2
                                    && BillDetails.Tables.Contains("GEN_PAT_BILLING") && BillDetails.Tables["GEN_PAT_BILLING"].Rows.Count > 0
                                    && IfNullReturnZero(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["VISIT_MODE"]) == 2)
                                    drNew["TOTAL_AMOUNT_IN_WORDS"] = comFun.ToWords(Convert.ToDouble(BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["GROSS_AMOUNT"]));
                                else if (IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["ISPACKAGE_SERVICE"]) != 2
                                    && IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["SERVICE_TYPE_ID"]) == 11)
                                    drNew["TOTAL_AMOUNT_IN_WORDS"] = comFun.ToWords(Convert.ToDouble(BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["GROSS_AMOUNT"]));
                                else
                                    drNew["TOTAL_AMOUNT_IN_WORDS"] = DBNull.Value;
                        }
                        else
                        {
                            if (BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["NET_AMOUNT"] != DBNull.Value)
                                if (IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["ISPACKAGE_SERVICE"]) == 2 && BillDetails.Tables["VW_BILL_DTLS"].Columns.Contains("BILL_CATEGORY")
                                    && IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["BILL_CATEGORY"]) == 1)
                                    drNew["TOTAL_AMOUNT_IN_WORDS"] = comFun.ToWords(Convert.ToDouble(BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["NET_AMOUNT"]));
                                else if (IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["ISPACKAGE_SERVICE"]) != 2)
                                    drNew["TOTAL_AMOUNT_IN_WORDS"] = comFun.ToWords(Convert.ToDouble(BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["NET_AMOUNT"]));
                                else if (IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["ISPACKAGE_SERVICE"]) == 2
                                    && BillDetails.Tables["VW_BILL_DTLS"].Columns.Contains("VISIT_MODE")
                                    && IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["VISIT_MODE"]) == 2)
                                    drNew["TOTAL_AMOUNT_IN_WORDS"] = comFun.ToWords(Convert.ToDouble(BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["NET_AMOUNT"]));
                                else if (IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["ISPACKAGE_SERVICE"]) == 2
                                    && IfNullReturnZero(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["VISIT_MODE"]) == 2)
                                    drNew["TOTAL_AMOUNT_IN_WORDS"] = comFun.ToWords(Convert.ToDouble(BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["NET_AMOUNT"]));
                                else
                                    drNew["TOTAL_AMOUNT_IN_WORDS"] = DBNull.Value;
                            else
                                if (IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["ISPACKAGE_SERVICE"]) != 2 && BillDetails.Tables["VW_BILL_DTLS"].Columns.Contains("BILL_CATEGORY")
                                    && IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["BILL_CATEGORY"]) != 1)
                                    drNew["TOTAL_AMOUNT_IN_WORDS"] = comFun.ToWords(Convert.ToDouble(BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["GROSS_AMOUNT"]));
                                else if (IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["ISPACKAGE_SERVICE"]) != 2)
                                    drNew["TOTAL_AMOUNT_IN_WORDS"] = comFun.ToWords(Convert.ToDouble(BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["GROSS_AMOUNT"]));
                                else if (IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["ISPACKAGE_SERVICE"]) == 2
                                    && BillDetails.Tables["VW_BILL_DTLS"].Columns.Contains("VISIT_MODE")
                                    && IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["VISIT_MODE"]) == 2)
                                    drNew["TOTAL_AMOUNT_IN_WORDS"] = comFun.ToWords(Convert.ToDouble(BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["GROSS_AMOUNT"]));
                                else if (IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["ISPACKAGE_SERVICE"]) == 2
                                    && IfNullReturnZero(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["VISIT_MODE"]) == 2)
                                    drNew["TOTAL_AMOUNT_IN_WORDS"] = comFun.ToWords(Convert.ToDouble(BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["GROSS_AMOUNT"]));
                                else
                                    drNew["TOTAL_AMOUNT_IN_WORDS"] = DBNull.Value;
                        }
                        GetContract(drNew, BillDetails);
                    }                   
                    PrintData.Tables["BILL_MASTER"].Rows.Add(drNew);
                    if (servType == ServiceType.Cafeteria)
                    {
                        if (BillDetails.Tables.Contains("GEN_PAT_BILLING") && BillDetails.Tables["GEN_PAT_BILLING"].Rows.Count > 0)
                        {
                            if (Convert.ToDecimal(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["ISPATIENT"]) == 0
                                && Convert.ToDecimal(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["PROFILE_ID"]) != -1)
                            {
                                PrintData.Tables["BILL_MASTER"].Rows[0]["PROFILE_ID"] = 0;//for employee hard coded value
                            }
                            else if (Convert.ToDecimal(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["ISPATIENT"]) == 0
                                && Convert.ToDecimal(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["PROFILE_ID"]) == -1)
                            {
                                PrintData.Tables["BILL_MASTER"].Rows[0]["PROFILE_ID"] = 1;//for outsider hard coded value
                            }
                            else
                            {
                                PrintData.Tables["BILL_MASTER"].Rows[0]["PROFILE_ID"] = 2;//for patient hard coded value
                            }
                        }
                        else if (BillDetails.Tables.Contains("VW_BILL_MASTER") && BillDetails.Tables["VW_BILL_MASTER"].Rows.Count > 0
                            && Convert.ToDecimal(BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["VISIT_MODE"]) == 2)
                        {
                            PrintData.Tables["BILL_MASTER"].Rows[0]["PROFILE_ID"] = 2;//for patient hard coded value
                        }
                    }
                }
                else if (servType == ServiceType.Refund)
                {
                    if (BillDetails.Tables.Contains("ADV_USAGE_DTLS") && BillDetails.Tables["ADV_USAGE_DTLS"] != null)
                    {
                        BillDetails.Tables["ADV_USAGE_DTLS"].TableName = "ADVRET_PAT_BILLING";
                    }
                    if (BillDetails != null && BillDetails.Tables.Contains("ADVRET_PAT_BILLING")
                        && BillDetails.Tables["ADVRET_PAT_BILLING"] != null && BillDetails.Tables["ADVRET_PAT_BILLING"].Rows.Count > 0)
                    {

                        DataRow drBillMaster = PrintData.Tables["BILL_MASTER"].NewRow();
                        drBillMaster["PAT_NAME"] = BillDetails.Tables["ADVRET_PAT_BILLING"].Rows[0]["PAT_NAME"];
                        drBillMaster["MRNO"] = BillDetails.Tables["ADVRET_PAT_BILLING"].Rows[0]["MRNO"];
                        drBillMaster["ENCOUNTER_NO"] = BillDetails.Tables["ADVRET_PAT_BILLING"].Rows[0]["MRNO"];
                        drBillMaster["BILL_DATE"] = BillDetails.Tables["ADVRET_PAT_BILLING"].Columns.Contains("PAYMENT_TIME")
                            && BillDetails.Tables["ADVRET_PAT_BILLING"].Rows[0]["PAYMENT_TIME"] != DBNull.Value ?
                            BillDetails.Tables["ADVRET_PAT_BILLING"].Rows[0]["PAYMENT_TIME"] :
                            BillDetails.Tables["ADVRET_PAT_BILLING"].Rows[0]["PAYMENT_DATE"];
                        drBillMaster["BILL_TYPE"] = ServiceType.Refund.ToString();
                        drBillMaster["BILL_NO"] = BillDetails.Tables["ADVRET_PAT_BILLING"].Rows[0]["REFERENCE"];
                        drBillMaster["SERVICE_TYPE"] = ServiceType.Refund.ToString();
                        drBillMaster["BILLED_BY"] = BillDetails.Tables["ADVRET_PAT_BILLING"].Rows[0]["BILLED_BY"];
                        drBillMaster["DECIMAL_PLACE"] = CommonData.DecimalPlace;
                        drBillMaster["PAID_AMOUNT"] = BillDetails.Tables["ADVRET_PAT_BILLING"].Rows[0]["WITHDRAW"]; ;
                        drBillMaster["ENCOUNTER_STARTDATE"] = BillDetails.Tables["ADVRET_PAT_BILLING"].Rows[0]["START_DATE"];
                        drBillMaster["TOTAL_AMOUNT_IN_WORDS"] = comFun.ToWords(Convert.ToDouble(BillDetails.Tables["ADVRET_PAT_BILLING"].Rows[0]["WITHDRAW"]));
                        PrintData.Tables["BILL_MASTER"].Rows.Add(drBillMaster);
                        DataRow drBillDtls = PrintData.Tables["BILL_DETAILS"].NewRow();
                        drBillDtls["BILL_NO"] = BillDetails.Tables["ADVRET_PAT_BILLING"].Rows[0]["BILL_NO"];
                        drBillDtls["MRNO"] = BillDetails.Tables["ADVRET_PAT_BILLING"].Rows[0]["MRNO"];
                        drBillDtls["NET_TOTAL"] = BillDetails.Tables["ADVRET_PAT_BILLING"].Rows[0]["WITHDRAW"];
                        drBillDtls["SERVICE_NAME"] = BillDetails.Tables["ADVRET_PAT_BILLING"].Rows[0]["REFERENCE"];
                        PrintData.Tables["BILL_DETAILS"].Rows.Add(drBillDtls);

                    }
                }
                //if (servType == ServiceType.Registration || servType == ServiceType.ReRegistration || servType == ServiceType.Consultation)
                //{
                //    drNew = PrintData.Tables["BILL_DETAILS"].NewRow();
                //    drNew["NET_TOTAL"] = BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["NET_AMOUNT"];
                //    drNew["BILL_NO"] = BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["BILL_NO"];
                //    if (servType == ServiceType.Registration)
                //    {
                //        servName = Convert.ToString(BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["NAME"]) + " Registration";
                //    }
                //    else if (servType == ServiceType.ReRegistration)
                //    {
                //        servName = Convert.ToString(BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["NAME"]);
                //    }
                //    else if (servType == ServiceType.Consultation)
                //    {
                //        servName = Convert.ToString(BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["NAME"]) + " Consultation";
                //    }
                //    drNew["SERVICE_NAME"] = servName;
                //    drNew["QTY"] = 1;
                //    //drNew["CPTCODE"]=;
                //    //drNew["COPAY"]=;
                //    //drNew["GRAND_TOTAL"]=BillDetails.Tables["PRINT_INVOICE_DATA"].Rows[0]["GROSS_AMOUNT"];
                //    PrintData.Tables["BILL_DETAILS"].Rows.Add(drNew);
                //}
            }
            catch (Exception)
            {

                throw;
            }
        }
        /// <summary>
        /// Gets the contract.
        /// </summary>
        /// <param name="drMaster">The dr master.</param>
        /// <param name="BillDtls">The bill DTLS.</param>
        private void GetContract(DataRow drMaster, DataSet BillDtls)
        {
            string contract = string.Empty;
            // DataTable dtTmp = null;
            if (BillDtls.Tables.Contains("VW_BILL_MASTER"))
            {
                GetContract(drMaster, BillDtls.Tables["VW_BILL_MASTER"]);
            }
            else if (BillDtls.Tables.Contains("PRINT_INVOICE_DATA"))
            {
                GetContract(drMaster, BillDtls.Tables["PRINT_INVOICE_DATA"]);
            }
            else if (BillDtls.Tables.Contains("INCO_PATIENT_SCHEME") && BillDtls.Tables.Contains("GEN_PAT_BILLING"))
            {
                foreach (DataRow dr in BillDtls.Tables["INCO_PATIENT_SCHEME"].Rows)
                {
                    if (BillDtls.Tables["GEN_PAT_BILLING"].Rows.Count > 0)
                    {
                        if (Convert.ToInt32(BillDtls.Tables["GEN_PAT_BILLING"].Rows[0]["INCO_DTLS_SCHEME_ID"]) == Convert.ToInt32(dr["INCO_DTLS_SCHEME_ID"]))
                        {
                            if (dr["CORPORATE"] != DBNull.Value)
                            {
                                contract = Convert.ToString(dr["CORPORATE"]);

                            }
                            if (dr["INSURANCE"] != DBNull.Value)
                            {
                                if (contract != string.Empty)
                                {
                                    contract = contract + "/ ";
                                }
                                contract = contract + Convert.ToString(dr["INSURANCE"]);
                            }
                            if (dr["SCHEME"] != DBNull.Value)
                            {
                                if (contract != string.Empty)
                                {
                                    contract = contract + "/ ";
                                }
                                contract = contract + Convert.ToString(dr["SCHEME"]);
                            }
                        }
                    }
                }
                drMaster["CONTRACT_NAME"] = contract;

            }

        }

        /// <summary>
        /// Gets the contract.
        /// </summary>
        /// <param name="drMaster">The dr master.</param>
        /// <param name="BillDtls">The bill DTLS.</param>
        private void GetContract(DataRow drMaster, DataTable BillDtls)
        {
            string contract = string.Empty;
            if (BillDtls.Columns.Contains("CORP_COMPANY_NAME") && BillDtls.Rows[0]["CORP_COMPANY_NAME"] != DBNull.Value)
            {
                contract = Convert.ToString(BillDtls.Rows[0]["CORP_COMPANY_NAME"]);

            }
            if (BillDtls.Columns.Contains("INS_COMPANY_NAME") && BillDtls.Rows[0]["INS_COMPANY_NAME"] != DBNull.Value)
            {
                if (contract != string.Empty)
                {
                    contract = contract + "/ ";
                }
                contract = contract + Convert.ToString(BillDtls.Rows[0]["INS_COMPANY_NAME"]);
            }
            if (BillDtls.Columns.Contains("SCHEME_NAME") && BillDtls.Rows[0]["SCHEME_NAME"] != DBNull.Value)
            {
                if (contract != string.Empty)
                {
                    contract = contract + "/ ";
                }
                contract = contract + Convert.ToString(BillDtls.Rows[0]["SCHEME_NAME"]);
            }
            drMaster["CONTRACT_NAME"] = contract;
        }
        /// <summary>
        /// Gets the name of the service.
        /// </summary>
        /// <param name="drService">The dr service.</param>
        /// <returns></returns>
        private string GetSurgeryServiceName(DataRow drService)
        {
            StringBuilder sbServiceName = new StringBuilder();
            if (drService.Table.Columns.Contains("OT_PARAMETER_NAME"))
            {
                sbServiceName.Append(drService["OT_PARAMETER_NAME"]);
            }
            if (drService.Table.Columns.Contains("ANESTHESIA_TYPE_NAME") && Convert.ToString(drService["ANESTHESIA_TYPE_NAME"]).Length > 1)
            {
                sbServiceName.Append(" - ");
                sbServiceName.Append(drService["ANESTHESIA_TYPE_NAME"]);
            }
            if (drService.Table.Columns.Contains("PROVIDER_NAME") && Convert.ToString(drService["PROVIDER_NAME"]).Length > 1)
            {
                sbServiceName.Append(" @ ");
                sbServiceName.Append(drService["PROVIDER_NAME"]);
            }
            return sbServiceName.Length > 0 ? sbServiceName.ToString() : string.Empty;
        }
        /// <summary>
        /// Gets the settled amount.
        /// </summary>
        /// <param name="dsBillDet">The ds bill det.</param>
        /// <param name="mrno">The mrno.</param>
        /// <returns> settled Amount</returns>
        private double getSettledAmount(DataSet dsBillDet, string mrno)
        {

            double settledAmount = 0;
            if (dsBillDet.Tables.Contains("GEN_PAT_BILL_ALLOCATION"))
            {
                foreach (DataRow dr in dsBillDet.Tables["GEN_PAT_BILL_ALLOCATION"].Rows)
                {
                    if ((dr["SETTLEMENT_AMOUNT"] != DBNull.Value))//&& (dr["ISCOPAY"] != DBNull.Value) && (dr["BILL_TO_PAY_BY"] != DBNull.Value) && (dr["BILL_TO_PAY_BY_ID"] != DBNull.Value)
                    {
                        if (Convert.ToDecimal(dr["SETTLEMENT_AMOUNT"]) > 0) //&& (Convert.ToInt32(dr["ISCOPAY"]) == 1) &&( Convert.ToInt32(dr["BILL_TO_PAY_BY"]) == 0) && (Convert.ToString(dr["BILL_TO_PAY_BY_ID"]) == mrno))
                        {
                            settledAmount = settledAmount + Convert.ToDouble(dr["SETTLEMENT_AMOUNT"]);
                        }
                    }
                }
            }
            return settledAmount;
        }
        /// <summary>
        /// Gets the Pharmacy Return Amount.
        /// </summary>
        /// <param name="PhDtls">The ph DTLS.</param>
        /// <returns></returns>
        private double GetPHRetAmount(DataTable PhDtls)
        {
            double retTotal = 0;
            foreach (DataRow dr in PhDtls.Rows)
            {
                if (dr["SETTLEMENT_AMOUNT"] != DBNull.Value)
                {
                    if (Convert.ToDouble(dr["SETTLEMENT_AMOUNT"]) > 0)
                    {
                        retTotal = retTotal + Convert.ToDouble(dr["REFUND_AMOUNT"]);
                    }
                }
            }
            return retTotal;
        }
        /// <summary>
        /// Gets the gross amount.
        /// </summary>
        /// <param name="drServiceDtls">The dr service DTLS.</param>
        /// <returns></returns>
        private decimal GetGrossAmount(DataRow drServiceDtls)
        {
            decimal grossAmount = 0;
            if (drServiceDtls != null)
            {
                if (drServiceDtls.Table.Columns.Contains("GROSS_AMOUNT") && drServiceDtls["GROSS_AMOUNT"] != DBNull.Value)
                {
                    grossAmount = Convert.ToDecimal(drServiceDtls["GROSS_AMOUNT"]);
                }
                else if (drServiceDtls.Table.Columns.Contains("AMOUNT") && drServiceDtls["AMOUNT"] != DBNull.Value)
                {
                    grossAmount = Convert.ToDecimal(drServiceDtls["AMOUNT"]);
                }
                if (drServiceDtls.Table.Columns.Contains("SERVICE_TYPE"))
                {
                    if (Convert.ToInt32(drServiceDtls["SERVICE_TYPE"]) != 0 && Convert.ToInt32(drServiceDtls["SERVICE_TYPE"]) != 2)
                    {
                        if (drServiceDtls.Table.Columns.Contains("SCHEME_MARKUP") && drServiceDtls["SCHEME_MARKUP"] != DBNull.Value)
                        {
                            grossAmount = grossAmount + Convert.ToDecimal(drServiceDtls["SCHEME_MARKUP"]);
                        }
                    }
                }
                else
                {
                    if (drServiceDtls.Table.Columns.Contains("SCHEME_MARKUP") && drServiceDtls["SCHEME_MARKUP"] != DBNull.Value)
                    {
                        grossAmount = grossAmount + Convert.ToDecimal(drServiceDtls["SCHEME_MARKUP"]);
                    }
                }
                if (drServiceDtls.Table.Columns.Contains("MARKUP") && drServiceDtls["MARKUP"] != DBNull.Value)
                {
                    grossAmount = grossAmount + Convert.ToDecimal(drServiceDtls["MARKUP"]);
                }
                if (drServiceDtls.Table.Columns.Contains("OTHER_CHARGE") && drServiceDtls["OTHER_CHARGE"] != DBNull.Value)
                {
                    grossAmount = grossAmount + Convert.ToDecimal(drServiceDtls["OTHER_CHARGE"]);
                }

            }
            return grossAmount;
        }
        /// <summary>
        /// Function to replace gross amount with net amount if gross amount lessthan net amount
        /// </summary>
        /// <param name="BillData">The bill data.</param>
        private void SetValidGross(DataSet BillData)
        {
            if (BillData.Tables.Contains("BILL_DETAILS") && BillData.Tables["BILL_DETAILS"] != null)
            {
                foreach (DataRow drBillDtls in BillData.Tables["BILL_DETAILS"].Rows)
                {
                    if (drBillDtls["GRAND_TOTAL"] != DBNull.Value && drBillDtls["NET_TOTAL"] != DBNull.Value && Convert.ToDecimal(drBillDtls["GRAND_TOTAL"]) < Convert.ToDecimal(drBillDtls["NET_TOTAL"]))
                    {
                        drBillDtls["GRAND_TOTAL"] = drBillDtls["NET_TOTAL"];
                    }
                }
            }

        }
        /// <summary>
        /// Creates the print data
        /// overloaded method to create print data for advance refund
        /// </summary>
        /// <param name="dsAdvDtls">The ds adv DTLS.</param>
        /// <returns></returns>
        public DataSet CreatePrintData(DataSet dsAdvDtls)
        {
            DataSet dsPrintData = new DataSet();
            InvoicePrint dsInvoice = new InvoicePrint();
            try
            {
                DataTable dtBillMaster = dsInvoice.BILL_MASTER.Clone();
                DataTable dtBillDtls = dsInvoice.BILL_DETAILS.Clone();

                if (dsAdvDtls.Tables.Contains("ADVRET_PAT_BILLING") && dsAdvDtls.Tables["ADVRET_PAT_BILLING"].Rows.Count > 0)
                {
                    foreach (DataRow dr in dsAdvDtls.Tables["ADVRET_PAT_BILLING"].Rows)
                    {
                        DataRow drBillMaster = dtBillMaster.NewRow();
                        drBillMaster["PAT_NAME"] = dsAdvDtls.Tables["ADVRET_PAT_BILLING"].Rows[0]["PAID_AGAINST_NAME"];
                        drBillMaster["MRNO"] = dsAdvDtls.Tables["ADVRET_PAT_BILLING"].Rows[0]["PAID_AGAINST_ID"];
                        drBillMaster["BILL_DATE"] = dsAdvDtls.Tables["GEN_AUDIT"] != null && dsAdvDtls.Tables["GEN_AUDIT"].Rows.Count > 0 &&
                        dsAdvDtls.Tables["GEN_AUDIT"].Rows[0]["AUDIT_DATE"] != DBNull.Value ? dsAdvDtls.Tables["GEN_AUDIT"].Rows[0]["AUDIT_DATE"] : DBNull.Value;
                        drBillMaster["BILL_TYPE"] = "OP";
                        drBillMaster["BILL_NO"] = dr["BILL_NO"];
                        drBillMaster["SERVICE_TYPE"] = "ADVANCE";
                        drBillMaster["BILLED_BY"] = dr["BILLED_BY"];
                        drBillMaster["DECIMAL_PLACE"] = CommonData.DecimalPlace;
                        dtBillMaster.Rows.Add(drBillMaster);
                        DataRow drBillDtls = dtBillDtls.NewRow();
                        drBillDtls["BILL_NO"] = dr["BILL_NO"];
                        drBillDtls["SERVICE_NAME"] = "Advance: " + dr["BILL_NO"];
                        drBillDtls["NET_TOTAL"] = dr["REFUND_AMOUNT"] != DBNull.Value ? Convert.ToDecimal(dr["NET_AMOUNT"]) - Convert.ToDecimal(dr["REFUND_AMOUNT"]) : dr["NET_AMOUNT"];
                        drBillDtls["IS_RETURN"] = 0;
                        dtBillDtls.Rows.Add(drBillDtls);
                        DataRow drRefundDtls = dtBillDtls.NewRow();
                        drRefundDtls["BILL_NO"] = dr["BILL_NO"];
                        drRefundDtls["SERVICE_NAME"] = "Refunded From:" + dr["BILL_NO"];
                        drRefundDtls["IS_RETURN"] = 1;
                        dtBillDtls.Rows.Add(drRefundDtls);
                    }

                }
                dsPrintData.Tables.Add(dtBillMaster.Copy());
                dsPrintData.Tables.Add(dtBillDtls.Copy());
                return dsPrintData;
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }
        /// <summary>
        /// Creates the print data
        /// overloaded method to create print data for advance refund
        /// </summary>
        /// <param name="dsAdvDtls">The ds adv DTLS.</param>
        /// <returns></returns>
        public void CreatePrintData(DataSet dsAdvDtls, ref DataSet PrintData)
        {
            DataSet dsPrintData = new DataSet();
            InvoicePrint dsInvoice = new InvoicePrint();
            CommonFunctions comFun = new CommonFunctions();
            try
            {
                if (dsAdvDtls.Tables.Contains("ADVRET_PAT_BILLING") && dsAdvDtls.Tables["ADVRET_PAT_BILLING"].Rows.Count > 0)
                {
                    DataRow drBillMaster = PrintData.Tables["BILL_MASTER"].NewRow();
                    drBillMaster["PAT_NAME"] = dsAdvDtls.Tables["ADVRET_PAT_BILLING"].Rows[0]["FIRST_NAME"] + " " + dsAdvDtls.Tables["ADVRET_PAT_BILLING"].Rows[0]["MIDDLE_NAME"] + " " + dsAdvDtls.Tables["ADVRET_PAT_BILLING"].Rows[0]["LAST_NAME"];
                    // drBillMaster["PAT_NAME"] = dsAdvDtls.Tables["ADVRET_PAT_BILLING"].Rows[0]["PAID_AGAINST_NAME"];
                    //drBillMaster["MRNO"] = dsAdvDtls.Tables["ADVRET_PAT_BILLING"].Rows[0]["PAID_AGAINST_ID"];
                    drBillMaster["MRNO"] = dsAdvDtls.Tables["ADVRET_PAT_BILLING"].Rows[0]["MRNO"];
                    drBillMaster["BILL_DATE"] = dsAdvDtls.Tables["GEN_AUDIT"] != null && dsAdvDtls.Tables["GEN_AUDIT"].Rows.Count > 0 &&
                    dsAdvDtls.Tables["GEN_AUDIT"].Rows[0]["AUDIT_DATE"] != DBNull.Value ? dsAdvDtls.Tables["GEN_AUDIT"].Rows[0]["AUDIT_DATE"] : DBNull.Value;
                    drBillMaster["BILL_TYPE"] = ServiceType.Refund.ToString();
                    drBillMaster["BILL_NO"] = dsAdvDtls.Tables["ADVRET_PAT_BILLING"].Rows[0]["BILL_NO"];
                    drBillMaster["SERVICE_TYPE"] = "ADVANCE";
                    drBillMaster["BILLED_BY"] = dsAdvDtls.Tables["ADVRET_PAT_BILLING"].Rows[0]["BILLED_BY"];
                    drBillMaster["DECIMAL_PLACE"] = CommonData.DecimalPlace;
                    drBillMaster["ENCOUNTER_STARTDATE"] = dsAdvDtls.Tables["ADVRET_PAT_BILLING"].Rows[0]["ENCOUNTER_START_DATE"];
                    decimal refundAmount = (from dr in dsAdvDtls.Tables["ADVRET_PAT_BILLING"].AsEnumerable()
                                            where dr["RETURN_AMOUNT"] != DBNull.Value
                                            select Convert.ToDecimal(dr["RETURN_AMOUNT"])).Sum();
                    string[] billNos = (from dr in dsAdvDtls.Tables["ADVRET_PAT_BILLING"].AsEnumerable()
                                        where dr["BILL_NO"] != DBNull.Value
                                        select Convert.ToString(dr["BILL_NO"])).ToArray();
                    foreach (DataRow dr in dsAdvDtls.Tables["ADVRET_PAT_BILLING"].Rows)
                    {
                        DataRow drBillDtls = PrintData.Tables["BILL_DETAILS"].NewRow();
                        drBillDtls["BILL_NO"] = dr["REFUND_BILL_NO"];
                        drBillDtls["MRNO"] = dr["MRNO"];
                        drBillDtls["NET_TOTAL"] = dr["RETURN_AMOUNT"];
                        drBillDtls["SERVICE_NAME"] = "Advance (" + Convert.ToString(dsAdvDtls.Tables["ADVRET_PAT_BILLING"].Rows[0]["ADV_TYPE_NAME"]) + ") Refunded From :" + dr["BILL_NO"];
                        PrintData.Tables["BILL_DETAILS"].Rows.Add(drBillDtls);
                    }
                    drBillMaster["PAID_AMOUNT"] = refundAmount;
                    //drBillDtls["SERVICE_NAME"] = "Advance Refunded From  :" + dr["BILL_NO"];
                    //drBillDtls["NET_TOTAL"] = dr["RETURN_AMOUNT"];                
                    //PrintData.Tables["BILL_DETAILS"].Rows.Add(drBillDtls);
                    drBillMaster["TOTAL_AMOUNT_IN_WORDS"] = comFun.ToWords(Convert.ToDouble(drBillMaster["PAID_AMOUNT"]));
                    PrintData.Tables["BILL_MASTER"].Rows.Add(drBillMaster);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }


        }
        private void CreateFinalBillPrintData(DataSet dsBillDtls, ref DataSet dsPrintData)
        {
            // DataSet  = null;
            //InvoicePrint xsdInvoice = new InvoicePrint();
            //DataTable dtBillMaster = xsdInvoice.BILL_MASTER;
            //DataTable dtBillDtls = xsdInvoice.BILL_DETAILS;
            string servText = "AMOUNT COLLECTED: ";
            CommonFunctions comFun = new CommonFunctions();
            try
            {
                if (dsBillDtls.Tables.Contains("STATEMENT_DETAILS") &&
                    dsBillDtls.Tables["STATEMENT_DETAILS"] != null &&
                    dsBillDtls.Tables["STATEMENT_DETAILS"].Rows.Count > 0)
                {
                    DataRow drBillMaster = dsPrintData.Tables["BILL_MASTER"].NewRow();
                    DataRow drBillDtls = dsPrintData.Tables["BILL_DETAILS"].NewRow();
                    drBillMaster["BILL_NO"] = dsBillDtls.Tables["STATEMENT_DETAILS"].Rows[0]["BILL_NO"];
                    drBillMaster["MRNO"] = dsBillDtls.Tables["STATEMENT_DETAILS"].Rows[0]["MRNO"];
                    if (dsBillDtls.Tables.Contains("VW_BILL_MASTER") &&
                    dsBillDtls.Tables["VW_BILL_MASTER"] != null &&
                    dsBillDtls.Tables["VW_BILL_MASTER"].Rows.Count > 0)
                    {
                        drBillMaster["PAT_NAME"] = dsBillDtls.Tables["VW_BILL_MASTER"].Columns.Contains("PATIENT_NAME") ?
                            dsBillDtls.Tables["VW_BILL_MASTER"].Rows[0]["PATIENT_NAME"] : DBNull.Value;

                    }
                    if (dsBillDtls.Tables.Contains("GEN_TRANSACTION") &&
                        dsBillDtls.Tables["GEN_TRANSACTION"] != null &&
                        dsBillDtls.Tables["GEN_TRANSACTION"].Rows.Count > 0)
                    {
                        drBillMaster["BILLED_BY"] = dsBillDtls.Tables["GEN_TRANSACTION"].Columns.Contains("TRANSACTION_BY") ?
                            dsBillDtls.Tables["GEN_TRANSACTION"].Rows[0]["TRANSACTION_BY"] : DBNull.Value;
                        drBillMaster["BILL_DATE"] = dsBillDtls.Tables["GEN_TRANSACTION"].Columns.Contains("PAYMENT_DATE") ?
                       dsBillDtls.Tables["GEN_TRANSACTION"].Rows[0]["PAYMENT_DATE"] : DBNull.Value;
                        var bankAmount = (from dr in dsBillDtls.Tables["GEN_TRANSACTION"].AsEnumerable()
                                          where dr["AMOUNT"] != DBNull.Value && dr["PAY_METHOD"] != DBNull.Value && Convert.ToInt16(dr["PAY_METHOD"]) != 5 &&
                                          Convert.ToInt16(dr["PAY_METHOD"]) == Convert.ToInt16(Infologics.Medilogics.Enumerators.Billing.PaymentOption.Bank)
                                          select Convert.ToDecimal(dr["AMOUNT"])).Sum();
                        if (bankAmount > 0)
                        {
                            drBillMaster["CARD"] = bankAmount;
                        }
                        var CardAmount = (from dr in dsBillDtls.Tables["GEN_TRANSACTION"].AsEnumerable()
                                          where dr["AMOUNT"] != DBNull.Value && dr["PAY_METHOD"] != DBNull.Value && Convert.ToInt16(dr["PAY_METHOD"]) != 5 &&
                                          Convert.ToInt16(dr["PAY_METHOD"]) == Convert.ToInt16(Infologics.Medilogics.Enumerators.Billing.PaymentOption.Card)
                                          select Convert.ToDecimal(dr["AMOUNT"])).Sum();
                        if (CardAmount > 0)
                        {
                            drBillMaster["BANK"] = CardAmount;
                        }
                        var CashAmount = (from dr in dsBillDtls.Tables["GEN_TRANSACTION"].AsEnumerable()
                                          where dr["AMOUNT"] != DBNull.Value && dr["PAY_METHOD"] != DBNull.Value && Convert.ToInt16(dr["PAY_METHOD"]) != 5 &&
                                          Convert.ToInt16(dr["PAY_METHOD"]) == Convert.ToInt16(Infologics.Medilogics.Enumerators.Billing.PaymentOption.Cash)
                                          select Convert.ToDecimal(dr["AMOUNT"])).Sum();
                        if (CashAmount > 0)
                        {
                            drBillMaster["CASH"] = CashAmount;
                        }
                    }
                    drBillDtls["BILL_NO"] = dsBillDtls.Tables["STATEMENT_DETAILS"].Rows[0]["BILL_NO"];
                    decimal netAmount = (from dr in dsBillDtls.Tables["GEN_TRANSACTION"].AsEnumerable()
                                         where dr["AMOUNT"] != DBNull.Value && dr["PAY_METHOD"] != DBNull.Value && Convert.ToInt16(dr["PAY_METHOD"]) != 5
                                         select Convert.ToDecimal(dr["AMOUNT"])).Sum();
                    drBillDtls["NET_TOTAL"] = netAmount;// dsBillDtls.Tables["STATEMENT_DETAILS"].Rows[0]["SETTLEMENT_AMOUNT"];
                    drBillMaster["PAID_AMOUNT"] = netAmount;
                    drBillMaster["BILL_TYPE"] = "SETTLEMENT";
                    drBillMaster["PAYINGPARTY_NAME"] = dsBillDtls.Tables["STATEMENT_DETAILS"].Rows[0]["PAYING_PARTY_NAME"];
                    drBillMaster["TOTAL_AMOUNT_IN_WORDS"] = comFun.ToWords(Convert.ToDouble(drBillMaster["PAID_AMOUNT"]));
                    drBillDtls["SERVICE_NAME"] = servText;
                    // GetContract(drBillMaster, dsBillDtls.Tables["VW_BILL_MASTER"]);
                    dsPrintData.Tables["BILL_MASTER"].Rows.Add(drBillMaster);
                    dsPrintData.Tables["BILL_DETAILS"].Rows.Add(drBillDtls);
                    //dsPrintData = new DataSet();
                    //dsPrintData.Tables.Add(dtBillMaster.Copy());
                    //dsPrintData.Tables.Add(dtBillDtls.Copy());
                }
                // return dsPrintData;
            }
            catch
            {
                throw;
            }
        }
        /// <summary>
        /// Checks the valid gross.
        /// </summary>
        /// <param name="drBillDtls">The dr bill DTLS.</param>
        //private void CheckValidGross(DataRow drBillDtls)
        //{
        //    if (drBillDtls != null && drBillDtls.Table.Columns.Contains("NET_TOTAL")
        //        && drBillDtls.Table.Columns.Contains("GRAND_TOTAL"))
        //    {
        //        if(Convert.ToDecimal(drBillDtls["NET_TOTAL"])< Convert.ToDecimal(drBillDtls["GRAND_TOTAL"]))
        //        {
        //            decimal tmpVal = Convert.ToDecimal(drBillDtls["NET_TOTAL"]);
        //            drBillDtls["NET_TOTAL"] = drBillDtls["GRAND_TOTAL"];
        //            drBillDtls["GRAND_TOTAL"] = tmpVal;
        //        }
        //    }
        //}

        public void PrintStayPass(DataTable dt)
        {
            try
            {
                string PrinterName = string.Empty;
                //PrinterName=
                string InvoicePrinterName = ConfigurationSettings.AppSettings["InvoicePrinter"];
                //string CardPrinterName = ConfigurationSettings.AppSettings["CardPrinter"];
                CrptStayPass objCrptStayPass = new CrptStayPass();
                objCrptStayPass.SetDataSource(dt);
                objCrptStayPass.PrintOptions.PrinterName = InvoicePrinterName;
                objCrptStayPass.PrintToPrinter(1, false, 0, 0);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void PrintPatientTag(DataTable dt)
        {
            try
            {
                string PrinterName = string.Empty;
                string InvoicePrinterName = ConfigurationSettings.AppSettings["InvoicePrinter"];
                CrptPatientTag objCrptPatientTag = new CrptPatientTag();
                objCrptPatientTag.SetDataSource(dt);
                objCrptPatientTag.PrintOptions.PrinterName = InvoicePrinterName;
                objCrptPatientTag.PrintToPrinter(1, false, 0, 0);
            }
            catch (Exception)
            {
                throw;
            }
        }
        decimal IfNullReturnZero(object RowToConvert)
        {
            try
            {
                decimal convert = string.IsNullOrEmpty(RowToConvert.ToString()) ? 0 : Convert.ToDecimal(RowToConvert);
                return convert;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //private void 
        //    CreateGroupBillPrintData ( DataSet BillDetails, ref DataSet PrintData, ServiceType servType, ProfileCategory PatientOrOutsider )
        //{
        //    string vmode=string.Empty;
        //    CommonFunctions comFun=new CommonFunctions();
        //    DataRow drNew=PrintData.Tables["BILL_MASTER"].NewRow();
        //    DataRow drDtls=null;
        //    double retTotal=0;
        //    double retTotalWord=0;
        //    double refundAmount=0;
        //    double paidAmount=0;
        //    string mrno=string.Empty;
        //    string contract=string.Empty;
        //    decimal Netamout=0;
        //    //double retCopayTotal = 0;
        //    bool isPharmacyret=false;
        //    if (PatientOrOutsider==ProfileCategory.Patient)
        //    {
        //        drNew["MRNO"]=BillDetails.Tables["PAT_PATIENT_NAME"].Rows[0]["MRNO"];
        //        drNew["CLINIC"]=BillDetails.Tables["BILL_COMMON_DETAILS"].Rows[0]["DEPARTMENT_NAME"];
        //        drNew["PAT_NAME"]=GetPatientName(BillDetails.Tables["PAT_PATIENT_NAME"]);
        //    }
        //    else
        //    {
        //        drNew["MRNO"]=DBNull.Value;
        //        drNew["CLINIC"]=DBNull.Value;
        //        drNew["PROFILE_ID"]=BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["PROFILE_ID"];
        //        drNew["PAT_NAME"]=GetPatientName(BillDetails.Tables["GEN_PROFILE_CONTACT"]);
        //    }
        //    if (BillDetails!=null&&BillDetails.Tables.Contains("GEN_PAT_BILLING")
        //                       &&BillDetails.Tables["GEN_PAT_BILLING"].Rows.Count>0)
        //    {
        //        drNew["BILL_NO"]=BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["BILL_NO"];
        //        drNew["BILL_DATE"]=BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["BILL_DATE"];
        //        drNew["BILLED_BY"]=BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["BILLED_BY"];
        //        if (Convert.ToInt16(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["VISIT_MODE"])==(int)VisitMode.OPCASH)
        //        {
        //            vmode="OP"; // Enum.GetName(typeof(VisitMode), Convert.ToUInt32(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["VISIT_MODE"]));
        //            if (BillDetails.Tables.Contains("INV_PAT_BILLING")&&BillDetails.Tables.Contains("INV_PAT_BILLING_TOTAL")
        //                &&BillDetails.Tables["INV_PAT_BILLING"].Rows.Count>0)
        //            {
        //                drNew["PAID_AMOUNT"]=IfNullReturnZero(BillDetails.Tables["INV_PAT_BILLING"].Rows[0]["ISPACKAGE_SERVICE"])!=2?BillDetails.Tables["INV_PAT_BILLING_TOTAL"].Rows[0]["NET_AMOUNT"]:DBNull.Value;
        //            }
        //            else if (BillDetails.Tables.Contains("VW_BILL_DTLS")&&BillDetails.Tables["VW_BILL_DTLS"].Rows.Count>0&&BillDetails.Tables.Contains("INV_PAT_BILLING_TOTAL")
        //                &&Convert.ToInt32(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["VISIT_MODE"])!=2)
        //                drNew["PAID_AMOUNT"]=IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["ISPACKAGE_SERVICE"])!=2?BillDetails.Tables["INV_PAT_BILLING_TOTAL"].Rows[0]["NET_AMOUNT"]:DBNull.Value;
        //            else if (BillDetails.Tables.Contains("GEN_PAT_BILLING"))
        //            {
						
        //                Netamout=BillDetails.Tables["GEN_PAT_BILLING"].AsEnumerable()
        //                    .Where(x => x["NET_AMOUNT"]!=DBNull.Value)
        //                    .Select(x => Convert.ToDecimal(x["NET_AMOUNT"])).Sum();
        //                drNew["PAID_AMOUNT"]=Netamout;
        //            }
        //        }
        //        else//insurace and company
        //        {
        //            vmode=Enum.GetName(typeof(VisitMode), Convert.ToUInt32(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["VISIT_MODE"]));
        //            //drNew["COMPANY"] = BillDetails.Tables["INCO_PATIENT_SCHEME"].Rows[0]["COMPANY"];
        //            //drNew["INSURANCE"] = BillDetails.Tables["INCO_PATIENT_SCHEME"].Rows[0]["INSURANCE"];
        //            GetContract(drNew, BillDetails);
        //            if (BillDetails.Tables.Contains("VW_BILL_DTLS")&&BillDetails.Tables["VW_BILL_DTLS"].Rows.Count>0&&
        //                Convert.ToDecimal(IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["ISPACKAGE_SERVICE"]))!=2
        //                &&Convert.ToInt32(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["VISIT_MODE"])!=2)
        //            {
        //                if (Convert.ToInt16(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["ISPATIENT"])==1)
        //                {
        //                    drNew["PAID_AMOUNT"]=getSettledAmount(BillDetails, Convert.ToString(BillDetails.Tables["PAT_PATIENT_NAME"].Rows[0]["MRNO"]));
        //                }
        //                else
        //                {
        //                    drNew["PAID_AMOUNT"]=0;
        //                }
        //            }
        //            else if (BillDetails.Tables.Contains("VW_BILL_DTLS")&&BillDetails.Tables["VW_BILL_DTLS"].Rows.Count>0&&
        //                Convert.ToDecimal(IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["ISPACKAGE_SERVICE"]))==2
        //                &&Convert.ToInt32(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["VISIT_MODE"])!=2)
        //                drNew["PAID_AMOUNT"]=DBNull.Value;
        //            else
        //            {
        //                if (Convert.ToInt16(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["ISPATIENT"])==1)
        //                {
        //                    drNew["PAID_AMOUNT"]=getSettledAmount(BillDetails, Convert.ToString(BillDetails.Tables["PAT_PATIENT_NAME"].Rows[0]["MRNO"]));
        //                }
        //                else
        //                {
        //                    drNew["PAID_AMOUNT"]=0;
        //                }
        //            }
        //            // drNew["PAID_AMOUNT"] = getSettledAmount(BillDetails, Convert.ToString(BillDetails.Tables["PAT_PATIENT_NAME"].Rows[0]["MRNO"]));
        //        }
        //        if (servType==ServiceType.Cafeteria)
        //        {
        //            drNew["DISC_N_ADJ"]=-1*(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["DISCOUNT"]==DBNull.Value?0:Convert.ToDecimal(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["DISCOUNT"]))+
        //                (BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["DISCOUNT"]==DBNull.Value?0:Convert.ToDecimal(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["MARKUP"]));
        //        }
        //    }
        //    else
        //    {
        //        drNew["BILL_NO"]="FREE";
        //        drNew["BILL_DATE"]=BillDetails.Tables["CON_PAT_BILLING"].Rows[0]["BILL_DATE"];
        //    }

        //    drNew["BILL_TYPE"]=vmode;
        //    drNew["DOCTOR_NAME"]=BillDetails.Tables["BILL_COMMON_DETAILS"].Rows[0]["PROVIDER_NAME"];
        //    drNew["SERVICE_TYPE"]=servType;
        //    GetContract(drNew, BillDetails);

        //    //for free consulataion
        //    if (BillDetails.Tables.Contains("INV_PAT_BILLING")&&BillDetails.Tables["INV_PAT_BILLING"].Rows.Count>0&&IfNullReturnZero(BillDetails.Tables["INV_PAT_BILLING"].Rows[0]["ISPACKAGE_SERVICE"])!=2)
        //    {
        //        if (BillDetails.Tables.Contains("INV_PAT_BILLING_TOTAL"))
        //        {
        //            drNew["TOTAL_AMOUNT_IN_WORDS"]=Convert.ToString(BillDetails.Tables["INV_PAT_BILLING_TOTAL"].Rows[0]["NET_AMOUNT"])!=null&&
        //                                            Convert.ToString(BillDetails.Tables["INV_PAT_BILLING_TOTAL"].Rows[0]["NET_AMOUNT"])!=string.Empty&&
        //                                            Convert.ToString(BillDetails.Tables["INV_PAT_BILLING_TOTAL"].Rows[0]["NET_AMOUNT"])!="0"
        //                                            ?comFun.ToWords(Convert.ToDouble(BillDetails.Tables["INV_PAT_BILLING_TOTAL"].Rows[0]["NET_AMOUNT"]))
        //                                            :"Free";
        //        }
        //        else
        //        {
        //            drNew["TOTAL_AMOUNT_IN_WORDS"]=comFun.ToWords(Convert.ToDouble(Netamout));
        //        }
        //    }
        //    else if (BillDetails.Tables.Contains("VW_BILL_DTLS")&&BillDetails.Tables["VW_BILL_DTLS"].Rows.Count>0&&IfNullReturnZero(BillDetails.Tables["VW_BILL_DTLS"].Rows[0]["ISPACKAGE_SERVICE"])!=2)
        //    {
        //        drNew["TOTAL_AMOUNT_IN_WORDS"]=Convert.ToString(BillDetails.Tables["INV_PAT_BILLING_TOTAL"].Rows[0]["NET_AMOUNT"])!=null&&
        //                                        Convert.ToString(BillDetails.Tables["INV_PAT_BILLING_TOTAL"].Rows[0]["NET_AMOUNT"])!=string.Empty&&
        //                                        Convert.ToString(BillDetails.Tables["INV_PAT_BILLING_TOTAL"].Rows[0]["NET_AMOUNT"])!="0"
        //                                        ?comFun.ToWords(Convert.ToDouble(BillDetails.Tables["INV_PAT_BILLING_TOTAL"].Rows[0]["NET_AMOUNT"]))
        //                                        :"Free";
        //    }
        //    else
        //        drNew["TOTAL_AMOUNT_IN_WORDS"]=DBNull.Value;

        //    PrintData.Tables["BILL_MASTER"].Rows.Add(drNew);
        //    drNew=null;

        //    //Details Table.

        //    foreach (DataRow dr in BillDetails.Tables["INV_PAT_BILLING"].Rows)
        //    {
        //        drNew=PrintData.Tables["BILL_DETAILS"].NewRow();
        //        drNew["BILL_NO"]=BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["BILL_NO"];
        //        drNew["QTY"]=dr["QTY"];
        //        if (BillDetails.Tables.Contains("INV_PAT_BILLING")&&IfNullReturnZero(BillDetails.Tables["INV_PAT_BILLING"].Rows[0]["ISPACKAGE_SERVICE"])!=2)
        //            drNew["GRAND_TOTAL"]=GetGrossAmount(dr);
        //        else
        //            drNew["GRAND_TOTAL"]=DBNull.Value;
        //        if (BillDetails.Tables.Contains("INV_PAT_BILLING")&&IfNullReturnZero(BillDetails.Tables["INV_PAT_BILLING"].Rows[0]["ISPACKAGE_SERVICE"])!=2)
        //        {
        //            if (dr["CO_PAY_AMOUNT"]!=DBNull.Value)
        //            {
        //                drNew["COPAY"]=dr["CO_PAY_AMOUNT"];
        //            }
        //            else
        //            {
        //                drNew["COPAY"]=0;
        //            }
        //        }
        //        else
        //            drNew["COPAY"]=DBNull.Value;
        //        if (servType==ServiceType.Cafeteria)
        //        {
        //            drNew["RATE"]=Convert.ToDecimal(dr["NET_AMOUNT"])/Convert.ToDecimal(dr["QTY"]);
        //        }
        //        //if (BillDetails.Tables.Contains("INV_PAT_BILLING"))
        //        drNew["NET_TOTAL"]=IfNullReturnZero(BillDetails.Tables["INV_PAT_BILLING"].Rows[0]["ISPACKAGE_SERVICE"])!=2?dr["NET_AMOUNT"]:DBNull.Value;
        //        //else
        //        //    drNew["NET_TOTAL"] = DBNull.Value;
        //        drNew["SERVICE_NAME"]=servType==ServiceType.Surgery?GetSurgeryServiceName(dr):
        //            dr.Table.Columns.Contains("ALIAS_NAME")&&dr["ALIAS_NAME"]!=DBNull.Value?
        //            dr["ALIAS_NAME"]:dr["NAME"];
        //        drNew["CPTCODE"]=dr["CPT_CODE"];
        //        //CheckValidGross(drNew);
        //        PrintData.Tables["BILL_DETAILS"].Rows.Add(drNew);
        //    }


        //    foreach (DataRow dr in BillDetails.Tables["CON_PAT_BILLING"].Rows)
        //    {
        //        drNew=PrintData.Tables["BILL_DETAILS"].NewRow();
        //        StringBuilder sbServiceName=new StringBuilder();
        //        if(BillDetails.Tables["CON_PAT_BILLING"].Columns.Contains("CON_TYPE_NAME") )  
        //        {
        //        sbServiceName.Append(BillDetails.Tables["CON_PAT_BILLING"].Rows[0]["CON_TYPE_NAME"].ToString());
        //        }
        //        sbServiceName.Append(" Consultation");
        //        //for free consulatation
        //        drNew["BILL_NO"]=BillDetails!=null&&BillDetails.Tables.Contains("GEN_PAT_BILLING")
        //                            &&BillDetails.Tables["GEN_PAT_BILLING"].Rows.Count>0
        //                            ?BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["BILL_NO"]
        //                            :"FREE";
        //        drNew["SERVICE_NAME"]=sbServiceName.ToString();//BillDetails.Tables["CON_PAT_BILLING"].Rows[0]["CON_TYPE_NAME"];
        //        if (IfNullReturnZero(BillDetails.Tables["CON_PAT_BILLING"].Rows[0]["ISPACKAGE_SERVICE"])!=2)
        //            drNew["GRAND_TOTAL"]=GetGrossAmount(BillDetails.Tables["CON_PAT_BILLING"].Rows[0]);
        //        else
        //            drNew["GRAND_TOTAL"]=DBNull.Value;
        //        drNew["NET_TOTAL"]=IfNullReturnZero(BillDetails.Tables["CON_PAT_BILLING"].Rows[0]["ISPACKAGE_SERVICE"])!=2?BillDetails.Tables["CON_PAT_BILLING"].Rows[0]["NET_AMOUNT"]:DBNull.Value;
        //        drNew["TOKEN_NO"]=BillDetails.Tables["CON_PAT_BILLING"].Rows[0]["TOKEN_NO"];
        //        if (BillDetails.Tables.Contains("APPT_ALLOCATION")&&  BillDetails.Tables["APPT_ALLOCATION"].Rows.Count>0)
        //        {
        //            drNew["APP_TIME"]=BillDetails.Tables["APPT_ALLOCATION"].Rows[0]["ALLOCATION_DATE"];
        //            drNew["MODE_OF_APPT"]=BillDetails.Tables["APPT_ALLOCATION"].Rows[0]["MODE_OF_APPT"];
        //        }
        //        drNew["QTY"]=1;
        //        if (IfNullReturnZero(BillDetails.Tables["CON_PAT_BILLING"].Rows[0]["ISPACKAGE_SERVICE"])!=2)
        //        {
        //            if (Convert.ToString(PrintData.Tables["BILL_MASTER"].Rows[0]["BILL_TYPE"])!="OP")
        //            {
        //                if (BillDetails.Tables["INV_PAT_BILLING_TOTAL"].Rows[0]["CO_PAY_AMOUNT"]!=DBNull.Value)
        //                {
        //                    drNew["COPAY"]=BillDetails.Tables["INV_PAT_BILLING_TOTAL"].Rows[0]["CO_PAY_AMOUNT"];
        //                }
        //            }
        //            else
        //            {
        //                drNew["COPAY"]=0;
        //            }
        //        }
        //        else
        //            drNew["COPAY"]=DBNull.Value;
        //        // CheckValidGross(drNew);
        //        // drNew["COPAY"] = BillDetails.Tables["CON_PAT_BILLING"].Rows[0]["CO_PAY_AMOUNT"];
        //        PrintData.Tables["BILL_DETAILS"].Rows.Add(drNew);

        //    }
        //    if (servType == ServiceType.Pharmacy)
        //    {
        //        foreach (DataRow dr in BillDetails.Tables["PH_PAT_BILLING"].Rows)
        //        {
        //            drDtls = PrintData.Tables["BILL_DETAILS"].NewRow();
        //            drDtls["BILL_NO"] = BillDetails != null && BillDetails.Tables.Contains("GEN_PAT_BILLING")
        //                            && BillDetails.Tables["GEN_PAT_BILLING"].Rows.Count > 0
        //                            ? BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["BILL_NO"]
        //                            : "FREE";
        //            drDtls["QTY"] = Convert.ToDecimal(dr["QTY"]) - Convert.ToDecimal(dr["RETURNED_QTY"]); ;
        //            drDtls["SERVICE_NAME"] = dr["SERVICE_NAME"]; //gives medicine name
        //            drDtls["BATCH_NO"] = dr["BATCHNO"];
        //            if (dr["GROSS_AMOUNT"] != DBNull.Value)
        //            {
        //                drDtls["GRAND_TOTAL"] = (GetGrossAmount(dr) / Convert.ToDecimal(dr["QTY"])) * (Convert.ToDecimal(drDtls["QTY"]));
        //            }
        //           if (dr["CO_PAY_AMOUNT"] != DBNull.Value)
        //           {
        //               drDtls["COPAY"] = (Convert.ToDecimal(dr["CO_PAY_AMOUNT"]) / Convert.ToDecimal(dr["QTY"])) * (Convert.ToDecimal(drDtls["QTY"]));
        //           }
        //           else
        //           {
        //               drDtls["COPAY"] = 0;
        //           }
        //           drDtls["IS_RETURN"] = 0;
        //           PrintData.Tables["BILL_DETAILS"].Rows.Add(drDtls);

        //        }


        //        foreach (DataRow dr in BillDetails.Tables["PH_SALESRETURN"].Rows)
        //        {
        //            drDtls = PrintData.Tables["BILL_DETAILS"].NewRow();
        //            drDtls["BILL_NO"] = BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["BILL_NO"];
        //            drDtls["MEDICINE_NAME"] = dr["SERVICE_NAME"];
        //            drDtls["BATCH_NO"] = dr["BATCHNO"];
        //            drDtls["EXPIRY"] = dr["EXP_DATE"];
        //            if (dr["QTY"] != DBNull.Value)
        //            {
        //                drDtls["QTY"] = Convert.ToDecimal(dr["QTY"]) - Convert.ToDecimal(dr["RETURNED_QTY"]);
        //            }
        //            if (dr["NET_AMOUNT"] != DBNull.Value)
        //            {
        //                drDtls["NET_TOTAL"] = Convert.ToDecimal(dr["NET_AMOUNT"]) - Convert.ToDecimal(dr["RETURNED_AMOUNT"]);
        //                retTotalWord = Convert.ToDouble(dr["NET_AMOUNT"]) - Convert.ToDouble(dr["RETURNED_AMOUNT"]);
        //            }
        //            if (dr["GROSS_AMOUNT"] != DBNull.Value)
        //            {
        //                drDtls["GRAND_TOTAL"] = (GetGrossAmount(dr) / Convert.ToDecimal(dr["QTY"])) * (Convert.ToDecimal(drDtls["QTY"]));
        //            }
        //            if (dr["CO_PAY_AMOUNT"] != DBNull.Value)
        //            {
        //                drDtls["COPAY"] = (Convert.ToDecimal(dr["CO_PAY_AMOUNT"]) / Convert.ToDecimal(dr["QTY"])) * (Convert.ToDecimal(drDtls["QTY"]));
        //            }
        //            else
        //            {
        //                drDtls["COPAY"] = 0;
        //            }
        //            drDtls["IS_RETURN"] = 0;
        //            drDtls["MEDICINE_NAME"] = dr["SERVICE_NAME"];
        //            if (Convert.ToDecimal(dr["RETURN_QTY"]) > 0)
        //            {
        //                PrintData.Tables["BILL_DETAILS"].Rows.Add(drDtls);
        //                drDtls = PrintData.Tables["BILL_DETAILS"].NewRow();
        //                drDtls["BILL_NO"] = BillDetails.Tables["VW_BILL_MASTER"].Rows[0]["BILL_NO"];
        //                drDtls["MEDICINE_NAME"] = dr["SERVICE_NAME"];
        //                drDtls["BATCH_NO"] = dr["BATCHNO"];
        //                drDtls["EXPIRY"] = dr["EXP_DATE"];
        //                drDtls["QTY"] = dr["RETURN_QTY"];
        //                drDtls["NET_TOTAL"] = dr["RETURN_AMOUNT"];
        //                refundAmount += Convert.ToDouble(dr["RETURN_AMOUNT"]);
        //                retTotalWord = Convert.ToDouble(dr["REMAINING_AMOUNT"]);
        //                if (dr["GROSS_AMOUNT"] != DBNull.Value)
        //                {
        //                    drDtls["GRAND_TOTAL"] = (GetGrossAmount(dr) / Convert.ToDecimal(dr["QTY"])) * (Convert.ToDecimal(drDtls["QTY"]));
        //                }
        //                else
        //                {
        //                    drDtls["GRAND_TOTAL"] = 0;
        //                }
        //                if (dr["CO_PAY_AMOUNT"] != DBNull.Value)
        //                {
        //                    drDtls["COPAY"] = (Convert.ToDecimal(dr["CO_PAY_AMOUNT"]) / Convert.ToDecimal(dr["QTY"])) * (Convert.ToDecimal(drDtls["QTY"]));
        //                }
        //                else
        //                {
        //                    drDtls["COPAY"] = 0;
        //                }
        //                drDtls["IS_RETURN"] = 1;
        //            }
        //            retTotal += retTotalWord;
        //            // CheckValidGross(drDtls);

        //            PrintData.Tables["BILL_DETAILS"].Rows.Add(drDtls);
        //        }

        //    }
                   
        //}


        private void
           CreateGroupBillPrintData(DataSet BillDetails, ref DataSet PrintData, ServiceType servType)
        {
            CommonFunctions comFun = new CommonFunctions();
            decimal Netamout = 0;
            decimal BalanceAmount = 0;
            DataRow drNew = PrintData.Tables["BILL_MASTER"].NewRow();
            if (BillDetails != null)
            {
                if (BillDetails.Tables.Contains("PRINT_CREDIT_DATA"))
                {
                    drNew["MRNO"] = BillDetails.Tables["PRINT_CREDIT_DATA"].Rows[0]["MRNO"];
                    drNew["PROFILE_ID"] = BillDetails.Tables["PRINT_CREDIT_DATA"].Rows[0]["MRNO"];
                    drNew["CLINIC"] = BillDetails.Tables["PRINT_CREDIT_DATA"].Rows[0]["DEPARTMENT"];
                    drNew["PAT_NAME"] = BillDetails.Tables["PRINT_CREDIT_DATA"].Rows[0]["PATIENT_NAME"];
                    drNew["BILL_NO"] = BillDetails.Tables["PRINT_CREDIT_DATA"].Rows[0]["BILL_NO"];
                    drNew["BILL_DATE"] = BillDetails.Tables["PRINT_CREDIT_DATA"].Rows[0]["BILL_DATE"];
                    drNew["BILLED_BY"] = BillDetails.Tables["PRINT_CREDIT_DATA"].Rows[0]["BILLED_BY"];
                    drNew["DOCTOR_NAME"] = BillDetails.Tables["PRINT_CREDIT_DATA"].Rows[0]["EMP_NAME"];
                    drNew["ISEMERGENCY"] = BillDetails.Tables["PRINT_CREDIT_DATA"].Rows[0]["ISEMERGENCY"];
                    drNew["ARABIC_NAME"] = BillDetails.Tables["PRINT_CREDIT_DATA"].Rows[0]["ARABIC_NAME"];
                    drNew["TRNO"] = BillDetails.Tables["PRINT_CREDIT_DATA"].Rows[0]["TRNO"];
                    drNew["DECIMAL_PLACE"] = CommonData.DecimalPlace;
                    drNew["HOSPITAL_TRNO"] = CommonData.GetDefaultSettings("HOSPITAL_TAX_REG_NO");
                    drNew["COMPANY_ADDRESS"] = BillDetails.Tables["PRINT_CREDIT_DATA"].Rows[0]["COMPANY_ADDRESS"];
                    drNew["ENCOUNTER_NO"] = BillDetails.Tables["PRINT_CREDIT_DATA"].Rows[0]["ENCOUNTER_NO"];
                    drNew["BILL_TYPE"] = "OPCREDIT";
                    //Netamout = BillDetails.Tables["PRINT_CREDIT_DATA"].AsEnumerable()
                    //      .Where(x => x["GEN_PATIENT_SHARE"] != DBNull.Value)
                    //      .Select(x => Convert.ToDecimal(x["GEN_PATIENT_SHARE"])).Sum();





                    Netamout = BillDetails.Tables["PRINT_CREDIT_DATA"].AsEnumerable()
                          .Where(x => x["FINAL_AMOUNT"] != DBNull.Value)
                          .Select(x => Convert.ToDecimal(x["FINAL_AMOUNT"])).Sum();

                    drNew["PAID_AMOUNT"] = Netamout;
                    drNew["TOTAL_AMOUNT_IN_WORDS"] = comFun.ToWords(Convert.ToDouble(drNew["PAID_AMOUNT"]));
                    drNew["CONTRACT_NAME"] = BillDetails.Tables["PRINT_CREDIT_DATA"].Rows[0]["CONTRACT"];
                    drNew["COMPANY_NAME"] = BillDetails.Tables["PRINT_CREDIT_DATA"].Rows[0]["COMPANY"];
                    drNew["VISIT_NO"] = BillDetails.Tables["PRINT_CREDIT_DATA"].Rows[0]["VISIT_NO"];
                    drNew["TPA"] = BillDetails.Tables["PRINT_CREDIT_DATA"].Rows[0]["TPA"];
                    if (BillDetails.Tables["PRINT_CREDIT_DATA"].Rows[0]["INSURANCE_COMPANY"] != DBNull.Value)
                    {
                        drNew["INSURANCE_CORPORATE"] = BillDetails.Tables["PRINT_CREDIT_DATA"].Rows[0]["INSURANCE_COMPANY"];
                    }
                    else
                    {
                        drNew["INSURANCE_CORPORATE"] = BillDetails.Tables["PRINT_CREDIT_DATA"].Rows[0]["CORPORATE_NAME"];
                    }
                    drNew["DOCTOR_NAME"] = BillDetails.Tables["PRINT_CREDIT_DATA"].Rows[0]["DOCTOR"];
                    drNew["GENDER"] = BillDetails.Tables["PRINT_CREDIT_DATA"].Rows[0]["GENDER"];
                    drNew["POLICY_NO"] = BillDetails.Tables["PRINT_CREDIT_DATA"].Rows[0]["POLICY_NO"];
                    drNew["MEMBERSHIP_NO"] = BillDetails.Tables["PRINT_CREDIT_DATA"].Rows[0]["MEMBERSHIP_NO"];
                    drNew["CLAIM_FORM_NO"] = BillDetails.Tables["PRINT_CREDIT_DATA"].Rows[0]["CLAIM_FORM_NO"];
                    drNew["SCHEME_EXPIRY_DATE"] = BillDetails.Tables["PRINT_CREDIT_DATA"].Rows[0]["SCHEME_EXP_DATE"];
                    drNew["VISIT_MODE"] = BillDetails.Tables["PRINT_CREDIT_DATA"].Rows[0]["VISIT_MODE"];
                    drNew["SERVICE_TYPE"] = BillDetails.Tables["PRINT_CREDIT_DATA"].Rows[0]["SERVICE_TYPE"];
                    drNew["CONTACT_NO"] = BillDetails.Tables["PRINT_CREDIT_DATA"].Rows[0]["CONTACT_NO"];
                    //drNew["MEMBERSHIP_NO"] = BillDetails.Tables["PRINT_CREDIT_DATA"].Rows[0]["MEMBERSHIP_NO"];

                    PrintData.Tables["BILL_MASTER"].Rows.Add(drNew);
                    foreach (DataRow dr in BillDetails.Tables["PRINT_CREDIT_DATA"].Rows)
                    {
                        drNew = PrintData.Tables["BILL_DETAILS"].NewRow();
                        if (dr["QTY"] != DBNull.Value)
                        {
                            drNew["QTY"] = dr["QTY"];
                        }
                        else
                            drNew["QTY"] = 1;

                        if (dr["GRANULAR_QTY"] != DBNull.Value)
                        {
                            drNew["GRANULAR_QTY"] = dr["GRANULAR_QTY"];
                        }
                        else
                            drNew["GRANULAR_QTY"] = 1;

                        drNew["UNIT_NAME"] = dr["UNIT_NAME"];


                        //if (BillDetails.Tables.Contains("INV_PAT_BILLING"))
                        // drNew["NET_TOTAL"] = dr["BILL_AMOUNT"];
                        //else
                        //    drNew["NET_TOTAL"] = DBNull.Value;
                        drNew["GROSS_AMOUNT"] = Convert.ToDecimal(dr["GROSS_AMOUNT"]) + Convert.ToDecimal(dr["MARKUP"]);
                        drNew["SERVICE_NAME"] = dr["NAME"];
                        drNew["CPTCODE"] = dr["CPT_CODE"];
                        drNew["BILL_NO"] = dr["BILL_NO"];
                        drNew["SPONSOR"] = dr["FINAL_AMOUNT"];
                        if (dr["GEN_PATIENT_SHARE"] != DBNull.Value)
                        {
                            drNew["PATIENT"] = dr["GEN_PATIENT_SHARE"];
                        }
                        else
                            drNew["PATIENT"] = 0;
                        drNew["DISCOUNT"] = dr["DISCOUNT"];
                        if (dr["FINAL_AMOUNT"] != DBNull.Value && dr["GEN_PATIENT_SHARE"] != DBNull.Value)
                        {
                            drNew["NET_TOTAL"] = Convert.ToDecimal(dr["FINAL_AMOUNT"]) + Convert.ToDecimal(dr["GEN_PATIENT_SHARE"]);
                            //  drNew["NET_TOTAL"] = Convert.ToDecimal(dr["FINAL_AMOUNT"]);
                        }
                        else if (dr["FINAL_AMOUNT"] != DBNull.Value)
                        {
                            drNew["NET_TOTAL"] = Convert.ToDecimal(dr["FINAL_AMOUNT"]);
                        }
                        else if (dr["GEN_PATIENT_SHARE"] != DBNull.Value)
                        {
                            drNew["NET_TOTAL"] = Convert.ToDecimal(dr["GEN_PATIENT_SHARE"]);
                        }
                        else
                        {
                            drNew["NET_TOTAL"] = 0;
                        }
                        if (Convert.ToInt32(dr["SERVICE_TYPE"]) == Convert.ToInt32(ServiceType.Pharmacy))
                        {
                            drNew["MEDICINE_NAME"] = dr["NAME"];
                            drNew["BATCH_NO"] = dr["BATCHNO"];
                            drNew["EXPIRY"] = dr["EXP_DATE"];
                        }
                        drNew["SERVICE_TYPE_NAME"] = dr["SERVICE_TYPE_NAME"];
                        drNew["SERVICE_DATE"] = dr["SERVICE_DATE"];
                        if (dr["SERVICE_TAX"] != DBNull.Value)
                        {
                            drNew["SERVICE_TAX"] = Infologics.Medilogics.General.Control.Common.MathRound(Convert.ToDecimal(dr["SERVICE_TAX"]), 2);
                        }
                        drNew["SERVICE_TAX_PERC"] = dr["SERVICE_TAX_PERC"];

                        //CheckValidGross(drNew);
                        PrintData.Tables["BILL_DETAILS"].Rows.Add(drNew);

                    }


                }
                else if (BillDetails.Tables.Contains("PRINT_CASH_DATA"))//Cash
                {
                    drNew["MRNO"] = BillDetails.Tables["PRINT_CASH_DATA"].Rows[0]["MRNO"];
                    drNew["PROFILE_ID"] = BillDetails.Tables["PRINT_CASH_DATA"].Rows[0]["MRNO"];
                    drNew["CLINIC"] = BillDetails.Tables["PRINT_CASH_DATA"].Rows[0]["DEPARTMENT"];
                    drNew["PAT_NAME"] = BillDetails.Tables["PRINT_CASH_DATA"].Rows[0]["PATIENT_NAME"];
                    drNew["BILL_NO"] = BillDetails.Tables["PRINT_CASH_DATA"].Rows[0]["BILL_NO"];
                    drNew["BILL_DATE"] = BillDetails.Tables["PRINT_CASH_DATA"].Rows[0]["BILL_DATE"];
                    drNew["BILLED_BY"] = BillDetails.Tables["PRINT_CASH_DATA"].Rows[0]["BILLED_BY"];
                    drNew["DOCTOR_NAME"] = BillDetails.Tables["PRINT_CASH_DATA"].Rows[0]["EMP_NAME"];
                    drNew["DECIMAL_PLACE"] = CommonData.DecimalPlace;
                    drNew["BILL_TYPE"] = "OP";
                    drNew["HOSPITAL_TRNO"] = CommonData.GetDefaultSettings("HOSPITAL_TAX_REG_NO");
                    drNew["COMPANY_ADDRESS"] = BillDetails.Tables["PRINT_CASH_DATA"].Rows[0]["COMP_ADDRESS"];
                    drNew["ARABIC_NAME"] = BillDetails.Tables["PRINT_CASH_DATA"].Rows[0]["COMP_ARABIC_NAME"];
                    drNew["ENCOUNTER_NO"] = BillDetails.Tables["PRINT_CASH_DATA"].Rows[0]["ENCOUNTER_NO"];
                    drNew["TRNO"] = BillDetails.Tables["PRINT_CASH_DATA"].Rows[0]["COMP_TRNO"];





                    Netamout = BillDetails.Tables["PRINT_CASH_DATA"].AsEnumerable()
                        .Where(x => x["FINAL_AMOUNT"] != DBNull.Value)
                        .Select(x => Convert.ToDecimal(x["FINAL_AMOUNT"])).Sum();

                    // DataRow[] BalanceAmount = dsToSave.Tables["PRINT_CASH_DATA"].AsEnumerable()
                    //   .Where(x => x["SERVICE_TYPE"] != DBNull.Value).Distinct()
                    //   .Select(x => x).Distinct().ToArray();
                    // var sum = dsToSave.Tables["PRINT_CASH_DATA"].AsEnumerable()
                    //     .Where(x => x["BANALCE_AMOUNT"] != DBNull.Value)
                    //.Where(x => x["SERVICE_TYPE"] != DBNull.Value).Distinct()
                    //.Select(x => Convert.ToDecimal(x["BANALCE_AMOUNT"])).Sum();

                    DataTable dtBalanceAmountTemp = BillDetails.Tables["PRINT_CASH_DATA"].DefaultView.ToTable(true, "SERVICE_TYPE", "BALANCE_AMOUNT");

                    // BalanceAmount = dtBalanceAmountTemp.AsEnumerable()
                    //    .Where(x => x["BALANCE_AMOUNT"] != DBNull.Value)
                    //.Where(x => x["SERVICE_TYPE"] != DBNull.Value).Distinct()
                    //.Select(x => Convert.ToDecimal(x["BALANCE_AMOUNT"])).Sum();

                    BalanceAmount = dtBalanceAmountTemp.AsEnumerable()
                       .Where(x => x["BALANCE_AMOUNT"] != DBNull.Value)
                   .Select(x => Convert.ToDecimal(x["BALANCE_AMOUNT"])).Sum();

                    if (BalanceAmount > Netamout)
                    {
                        BalanceAmount = Netamout;
                    }

                    drNew["BALANCE_AMOUNT"] = BalanceAmount;
                    drNew["PAID_AMOUNT"] = Netamout - BalanceAmount;

                    if (drNew["PAID_AMOUNT"] != DBNull.Value && Convert.ToDouble(drNew["PAID_AMOUNT"]) > 0)
                    {
                        drNew["PAID_AMOUNT"] = Math.Round(Convert.ToDouble(drNew["PAID_AMOUNT"]), CommonData.DecimalPlace);
                    }

                    drNew["TOTAL_AMOUNT_IN_WORDS"] = comFun.ToWords(Convert.ToDouble(drNew["PAID_AMOUNT"]));
                    if (Convert.ToString(drNew["TOTAL_AMOUNT_IN_WORDS"]) != string.Empty)
                    {
                        drNew["TOTAL_AMOUNT_IN_WORDS"] = Convert.ToString(drNew["TOTAL_AMOUNT_IN_WORDS"]);
                        //"Received with thanks" + " " + Convert.ToString(drNew["TOTAL_AMOUNT_IN_WORDS"]);
                    }
                    drNew["CONTRACT_NAME"] = BillDetails.Tables["PRINT_CASH_DATA"].Rows[0]["CONTRACT"];
                    drNew["COMPANY_NAME"] = BillDetails.Tables["PRINT_CASH_DATA"].Rows[0]["COMPANY"];
                    drNew["ISEMERGENCY"] = BillDetails.Tables["PRINT_CASH_DATA"].Rows[0]["ISEMERGENCY"];
                    drNew["CONTACT_NO"] = BillDetails.Tables["PRINT_CASH_DATA"].Rows[0]["CONTACT_NO"];
                    if (BillDetails.Tables.Contains("PAY_MODE") && BillDetails.Tables["PAY_MODE"] != null && BillDetails.Tables["PAY_MODE"].Rows.Count > 0)
                    {
                        foreach (DataRow dr in BillDetails.Tables["PAY_MODE"].Rows)
                        {
                            if (dr["TRANSACTION_TYPE"] != DBNull.Value && Convert.ToInt16(dr["TRANSACTION_TYPE"]) != 6 && dr["PAY_METHOD"] != DBNull.Value)//6 means return
                            {
                                //if (dr["PAY_METHOD"] != DBNull.Value)
                                //{
                                if (Convert.ToInt32(dr["PAY_METHOD"]) == Convert.ToInt32(Infologics.Medilogics.Enumerators.Billing.PaymentOption.Bank))
                                {
                                    if (drNew["CARD"] == DBNull.Value)
                                    {
                                        drNew["CARD"] = dr["AMOUNT"];
                                    }
                                    else
                                        drNew["CARD"] = Convert.ToDecimal(drNew["CARD"]) + Convert.ToDecimal(dr["AMOUNT"]);

                                }
                                else if (Convert.ToInt32(dr["PAY_METHOD"]) == Convert.ToInt32(Infologics.Medilogics.Enumerators.Billing.PaymentOption.Card))
                                {
                                    if (drNew["BANK"] == DBNull.Value)
                                    {
                                        drNew["BANK"] = dr["AMOUNT"];
                                    }
                                    else
                                        drNew["BANK"] = Convert.ToDecimal(drNew["BANK"]) + Convert.ToDecimal(dr["AMOUNT"]);
                                }
                                else if (Convert.ToInt32(dr["PAY_METHOD"]) == Convert.ToInt32(Infologics.Medilogics.Enumerators.Billing.PaymentOption.Cash))
                                {
                                    if (drNew["CASH"] == DBNull.Value)
                                    {
                                        drNew["CASH"] = dr["AMOUNT"];
                                    }
                                    else
                                        drNew["CASH"] = Convert.ToDecimal(drNew["CASH"]) + Convert.ToDecimal(dr["AMOUNT"]);
                                }
                                else if (Convert.ToInt32(dr["PAY_METHOD"]) == 5)
                                {
                                    if (drNew["ADVANCE"] == DBNull.Value)
                                    {
                                        drNew["ADVANCE"] = dr["AMOUNT"];
                                    }
                                    else
                                        drNew["ADVANCE"] = Convert.ToDecimal(drNew["ADVANCE"]) + Convert.ToDecimal(dr["AMOUNT"]);
                                }
                                //else if (Convert.ToDecimal(dr["PAY_METHOD"]) == 6)
                                //{
                                //    if (drNew["CASH"] == DBNull.Value)
                                //    {
                                //        drNew["CASH"] = dr["AMOUNT"];
                                //    }
                                //    else
                                //        drNew["CASH"] = Convert.ToDecimal(drNew["CASH"]) + Convert.ToDecimal(dr["AMOUNT"]);

                                //}
                            }


                            //else
                            //{                                   
                            //    drNew["CASH"] =  Netamout;
                            //}
                            //}
                            //else
                            //{
                            //    drNew["CASH"] =  Netamout;
                            //}
                        }





                    }
                    decimal netReturnAmount = 0;
                    if (BillDetails.Tables.Contains("PAY_MODE"))
                    {

                        netReturnAmount = BillDetails.Tables["PAY_MODE"].AsEnumerable()
                           .Where(x => x["TRANSACTION_TYPE"] != DBNull.Value && Convert.ToInt16(x["TRANSACTION_TYPE"]) == 6)
                           .Select(x => Convert.ToDecimal(x["AMOUNT"])).Sum();
                    }
                    drNew["VISIT_NO"] = BillDetails.Tables["PRINT_CASH_DATA"].Rows[0]["VISIT_NO"];
                    drNew["TPA"] = BillDetails.Tables["PRINT_CASH_DATA"].Rows[0]["TPA"];
                    if (BillDetails.Tables["PRINT_CASH_DATA"].Rows[0]["COMP_NAME"] != DBNull.Value)
                    {
                        drNew["INSURANCE_CORPORATE"] = BillDetails.Tables["PRINT_CASH_DATA"].Rows[0]["COMP_NAME"];
                    }
                    else if (BillDetails.Tables["PRINT_CASH_DATA"].Rows[0]["INSURANCE_COMPANY"] != DBNull.Value)
                    {
                        drNew["INSURANCE_CORPORATE"] = BillDetails.Tables["PRINT_CASH_DATA"].Rows[0]["INSURANCE_COMPANY"];
                    }
                    else
                    {
                        drNew["INSURANCE_CORPORATE"] = BillDetails.Tables["PRINT_CASH_DATA"].Rows[0]["CORPORATE_NAME"];
                    }
                    drNew["DOCTOR_NAME"] = BillDetails.Tables["PRINT_CASH_DATA"].Rows[0]["DOCTOR"];
                    drNew["GENDER"] = BillDetails.Tables["PRINT_CASH_DATA"].Rows[0]["GENDER"];
                    drNew["POLICY_NO"] = BillDetails.Tables["PRINT_CASH_DATA"].Rows[0]["POLICY_NO"];
                    drNew["MEMBERSHIP_NO"] = BillDetails.Tables["PRINT_CASH_DATA"].Rows[0]["MEMBERSHIP_NO"];
                    drNew["CLAIM_FORM_NO"] = BillDetails.Tables["PRINT_CASH_DATA"].Rows[0]["CLAIM_FORM_NO"];
                    drNew["SCHEME_EXPIRY_DATE"] = BillDetails.Tables["PRINT_CASH_DATA"].Rows[0]["SCHEME_EXP_DATE"];
                    drNew["VISIT_MODE"] = BillDetails.Tables["PRINT_CASH_DATA"].Rows[0]["VISIT_MODE"];
                    drNew["SERVICE_TYPE"] = BillDetails.Tables["PRINT_CASH_DATA"].Rows[0]["SERVICE_TYPE"];
                    PrintData.Tables["BILL_MASTER"].Rows.Add(drNew);
                    if (netReturnAmount > 0)
                    {
                        foreach (DataRow drreturn in PrintData.Tables["BILL_MASTER"].Rows)
                        {
                            if (netReturnAmount > 0 && drreturn["CASH"] != DBNull.Value)
                            {
                                if (Convert.ToDecimal(drreturn["CASH"]) > netReturnAmount)
                                {
                                    drreturn["CASH"] = Convert.ToDecimal(drreturn["CASH"]) - netReturnAmount;
                                    netReturnAmount = 0;
                                }
                                else
                                {
                                    drreturn["CASH"] = 0;
                                    netReturnAmount = netReturnAmount - Convert.ToDecimal(drreturn["CASH"]);
                                }
                            }
                            if (netReturnAmount > 0 && drreturn["BANK"] != DBNull.Value)
                            {
                                if (Convert.ToDecimal(drreturn["BANK"]) > netReturnAmount)
                                {
                                    drreturn["BANK"] = Convert.ToDecimal(drreturn["BANK"]) - netReturnAmount;
                                    netReturnAmount = 0;
                                }
                                else
                                {
                                    drreturn["BANK"] = 0;
                                    netReturnAmount = netReturnAmount - Convert.ToDecimal(drreturn["BANK"]);
                                }
                            }

                            if (netReturnAmount > 0 && drreturn["CARD"] != DBNull.Value)
                            {
                                if (Convert.ToDecimal(drreturn["CARD"]) > netReturnAmount)
                                {
                                    drreturn["CARD"] = Convert.ToDecimal(drreturn["CARD"]) - netReturnAmount;
                                    netReturnAmount = 0;
                                }
                                else
                                {
                                    drreturn["CARD"] = 0;
                                    netReturnAmount = netReturnAmount - Convert.ToDecimal(drreturn["CARD"]);
                                }
                            }
                            if (netReturnAmount > 0 && drreturn["ADVANCE"] != DBNull.Value)
                            {
                                if (Convert.ToDecimal(drreturn["ADVANCE"]) > netReturnAmount)
                                {
                                    drreturn["ADVANCE"] = Convert.ToDecimal(drreturn["ADVANCE"]) - netReturnAmount;
                                    netReturnAmount = 0;
                                }
                                else
                                {
                                    drreturn["ADVANCE"] = 0;
                                    netReturnAmount = netReturnAmount - Convert.ToDecimal(drreturn["ADVANCE"]);
                                }
                            }
                        }
                    }


                    foreach (DataRow dr in BillDetails.Tables["PRINT_CASH_DATA"].Rows)
                    {
                        drNew = PrintData.Tables["BILL_DETAILS"].NewRow();

                        if (dr["QTY"] != DBNull.Value)
                        {
                            drNew["QTY"] = dr["QTY"];
                        }
                        else
                            drNew["QTY"] = 1;

                        if (dr["GRANULAR_QTY"] != DBNull.Value)
                        {
                            drNew["GRANULAR_QTY"] = dr["GRANULAR_QTY"];
                        }
                        else
                            drNew["GRANULAR_QTY"] = 1;


                        drNew["UNIT_NAME"] = dr["UNIT_NAME"];
                        //if (BillDetails.Tables.Contains("INV_PAT_BILLING"))
                        // drNew["NET_TOTAL"] = dr["BILL_AMOUNT"];
                        //else
                        //    drNew["NET_TOTAL"] = DBNull.Value;
                        drNew["SERVICE_NAME"] = dr["NAME"];
                        drNew["CPTCODE"] = dr["CPT_CODE"];
                        drNew["BILL_NO"] = dr["BILL_NO"];
                        drNew["SPONSOR"] = dr["FINAL_AMOUNT"];
                        if (dr["GEN_PATIENT_SHARE"] != DBNull.Value)
                        {
                            drNew["PATIENT"] = dr["GEN_PATIENT_SHARE"];
                        }
                        else
                            drNew["PATIENT"] = 0;

                        drNew["DISCOUNT"] = dr["DISCOUNT"];
                        drNew["GROSS_AMOUNT"] = Convert.ToDecimal(dr["GROSS_AMOUNT"]) + Convert.ToDecimal(dr["MARKUP"]);
                        if (dr["FINAL_AMOUNT"] != DBNull.Value && dr["GEN_PATIENT_SHARE"] != DBNull.Value)
                        {
                            // drNew["NET_TOTAL"] = Convert.ToDecimal(dr["FINAL_AMOUNT"]) + Convert.ToDecimal(dr["GEN_PATIENT_SHARE"]);
                            drNew["NET_TOTAL"] = Convert.ToDecimal(dr["FINAL_AMOUNT"]);
                        }
                        else if (dr["FINAL_AMOUNT"] != DBNull.Value)
                        {
                            drNew["NET_TOTAL"] = Convert.ToDecimal(dr["FINAL_AMOUNT"]);
                        }
                        else if (dr["GEN_PATIENT_SHARE"] != DBNull.Value)
                        {
                            drNew["NET_TOTAL"] = Convert.ToDecimal(dr["GEN_PATIENT_SHARE"]);
                        }
                        else
                        {
                            drNew["NET_TOTAL"] = 0;
                        }
                        if (Convert.ToInt32(dr["SERVICE_TYPE"]) == Convert.ToInt32(ServiceType.Pharmacy))
                        {
                            drNew["MEDICINE_NAME"] = dr["NAME"];
                            drNew["BATCH_NO"] = dr["BATCHNO"];
                            drNew["EXPIRY"] = dr["EXP_DATE"];
                        }
                        if (dr["GROSS_AMOUNT"] != DBNull.Value && dr["COPAY_DISCOUNT"] != DBNull.Value)
                        {
                            drNew["NETAMOUNT"] = Convert.ToDecimal(dr["GROSS_AMOUNT"]) - Convert.ToDecimal(dr["COPAY_DISCOUNT"]);

                        }
                        else
                        {
                            drNew["NETAMOUNT"] = dr["GROSS_AMOUNT"];
                        }

                        if (dr["GROSS_AMOUNT"] != DBNull.Value && dr["COPAY_DISCOUNT"] != DBNull.Value && dr["FINAL_AMOUNT"] != DBNull.Value)
                        {
                            drNew["NETAMOUNT"] = Convert.ToDecimal(dr["GROSS_AMOUNT"]) - (Convert.ToDecimal(dr["COPAY_DISCOUNT"]) + Convert.ToDecimal(dr["FINAL_AMOUNT"]));

                        }
                        else if (dr["GROSS_AMOUNT"] != DBNull.Value && dr["FINAL_AMOUNT"] != DBNull.Value)
                        {
                            drNew["NETAMOUNT"] = Convert.ToDecimal(dr["GROSS_AMOUNT"]) - Convert.ToDecimal(dr["FINAL_AMOUNT"]);
                        }
                        else
                        {
                            drNew["NETAMOUNT"] = 0;
                        }

                        if (dr["SCHEME_DISCOUNT"] != DBNull.Value && Convert.ToInt32(dr["SERVICE_TYPE"]) == Convert.ToInt32(ServiceType.Pharmacy))
                        {
                            if (dr["QTY"] != DBNull.Value && dr["ACTUAL_QTY"] != DBNull.Value && Convert.ToDecimal(dr["QTY"]) < Convert.ToDecimal(dr["ACTUAL_QTY"]))
                            {
                                dr["SCHEME_DISCOUNT"] = (Convert.ToDecimal(dr["SCHEME_DISCOUNT"]) / Convert.ToDecimal(dr["ACTUAL_QTY"])) * Convert.ToDecimal(dr["QTY"]);
                                drNew["NETAMOUNT"] = Convert.ToDecimal(drNew["NETAMOUNT"]) - (Convert.ToDecimal(dr["SCHEME_DISCOUNT"])) + Convert.ToDecimal(dr["MARKUP"]);

                            }
                            else
                            {
                                drNew["NETAMOUNT"] = Convert.ToDecimal(drNew["NETAMOUNT"]) - (Convert.ToDecimal(dr["SCHEME_DISCOUNT"])) + Convert.ToDecimal(dr["MARKUP"]);
                            }
                        }
                        else
                        {
                            drNew["NETAMOUNT"] = Convert.ToDecimal(drNew["NETAMOUNT"]) + Convert.ToDecimal(dr["MARKUP"]);
                        }

                        if (drNew["NETAMOUNT"] != DBNull.Value && Convert.ToDecimal(drNew["NETAMOUNT"]) < 0)
                        {
                            drNew["NETAMOUNT"] = 0;
                        }
                        if (dr["SERVICE_TAX"] != DBNull.Value)
                        {
                            drNew["SERVICE_TAX"] = Infologics.Medilogics.General.Control.Common.MathRound(Convert.ToDecimal(dr["SERVICE_TAX"]), 2);
                        }
                        drNew["SERVICE_TAX_PERC"] = dr["SERVICE_TAX_PERC"];
                        if (drNew["NETAMOUNT"] != DBNull.Value && Convert.ToDecimal(drNew["NETAMOUNT"]) > 0 && dr["SERVICE_TAX"] != DBNull.Value)
                        {
                            drNew["NETAMOUNT"] = Convert.ToDecimal(drNew["NETAMOUNT"]) + Convert.ToDecimal(dr["SERVICE_TAX"]);
                        }

                        drNew["SERVICE_TYPE_NAME"] = dr["SERVICE_TYPE_NAME"];
                        drNew["SERVICE_DATE"] = dr["SERVICE_DATE"];

                        //CheckValidGross(drNew);
                        PrintData.Tables["BILL_DETAILS"].Rows.Add(drNew);
                    }

                }

            }
        }

 

	}
	
}

 











//////using System;
//////using System.Collections.Generic;
//////using System.Linq;
//////using System.Text;
//////using Infologics.Medilogics.PrintingLibrary.Main;
//////using Infologics.Medilogics.CommonShared.FOControls;
//////using Infologics.Medilogics.Enumerators.General;
//////using System.Data; 
//////using Infologics.Medilogics.CommonClient.Controls.CommonFunctions;
//////using Infologics.Medilogics.PrintingLibrary.InvoiceCrystal.CrystalReports;
//////using CrystalDecisions.Shared;
//////using Infologics.Medilogics.Enumerators.Visit;
//////using Infologics.Medilogics.PrintingLibrary.InvoiceCrystal.DataSets;
//////namespace Infologics.Medilogics.PrintingLibrary.InvoiceCrystal
//////{
//////    public class BLInvoiceCrystal:IPrinting
//////    { 
//////        #region IPrinting Members

//////        public bool Print(System.Data.DataSet BillPrintDetails, ServiceType serviceType, string PrinterName)
//////        {
//////            throw new NotImplementedException();

//////        }
//////        #endregion
//////        private DataSet CreateBillDetails(DataSet BillDetails, ServiceType servType)
//////        {
//////            DataSet dsBillDetails = new DataSet();
//////            DataTable dtBillMaster = null;
//////            DataTable dtBillDetails = null;
//////            InvoicePrint invPrint = new InvoicePrint();
//////            string vmode = string.Empty;
//////            CommonFunctions comFun = new CommonFunctions();
//////            try
//////            {
//////                dtBillMaster = invPrint.Tables["BILL_MASTER"].Clone();
//////                dtBillMaster.Rows.Add();
//////                dtBillMaster.Rows[0]["BILL_NO"] = BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["BILL_NO"];
//////                dtBillMaster.Rows[0]["PAT_NAME"] = GetPatientName(BillDetails.Tables["PAT_PATIENT_NAME"]);
//////                dtBillMaster.Rows[0]["MRNO"] = BillDetails.Tables["PAT_PATIENT_NAME"].Rows[0]["MRNO"];
//////                dtBillMaster.Rows[0]["BILL_DATE"] = BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["BILL_DATE"];

//////                if (Convert.ToInt16(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["VISIT_MODE"]) == (int)VisitMode.OPCASH)
//////                {
//////                    vmode = "OP"; // Enum.GetName(typeof(VisitMode), Convert.ToUInt32(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["VISIT_MODE"]));
//////                }
//////                else//insurace and company
//////                {
//////                    vmode = Enum.GetName(typeof(VisitMode), Convert.ToUInt32(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["VISIT_MODE"]));
//////                    dtBillMaster.Rows[0]["COMPANY"] = BillDetails.Tables["INCO_PATIENT_SCHEME"].Rows[0]["COMPANY"];
//////                    dtBillMaster.Rows[0]["INSURANCE"] = BillDetails.Tables["INCO_PATIENT_SCHEME"].Rows[0]["INSURANCE"];
//////                }
//////                dtBillMaster.Rows[0]["BILL_TYPE"] = vmode;
//////                dtBillMaster.Rows[0]["DOCTOR_NAME"] = BillDetails.Tables["BILL_COMMON_DETAILS"].Rows[0]["PROVIDER_NAME"];
//////                dtBillMaster.Rows[0]["BILLED_BY"] = BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["BILLED_BY"];
//////                dtBillMaster.Rows[0]["SERVICE_TYPE"] = servType;
//////                dtBillMaster.Rows[0]["CLINIC"] = BillDetails.Tables["BILL_COMMON_DETAILS"].Rows[0]["DEPARTMENT_NAME"];
//////                dtBillMaster.Rows[0]["TOTAL_AMOUNT_IN_WORDS"] = comFun.ToWords(Convert.ToDouble(BillDetails.Tables["INV_PAT_BILLING_TOTAL"].Rows[0]["NET_AMOUNT"]));
//////                //Details
//////                dtBillDetails = invPrint.Tables["BILL_DETAILS"].Clone();
//////                DataRow drBilldet = null;
//////                if (servType == ServiceType.Pharmacy)
//////                {
//////                    foreach (DataRow dr in BillDetails.Tables["INV_PAT_BILLING"].Rows)
//////                    {
//////                        drBilldet = dtBillDetails.NewRow();
//////                        drBilldet["BILL_NO"] = BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["BILL_NO"];
//////                        drBilldet["QTY"] = dr["QTY"];
//////                        drBilldet["MEDICINE_NAME"] = dr["NAME"];
//////                        drBilldet["GRAND_TOTAL"] = dr["GROSS_AMOUNT"];
//////                        drBilldet["COPAY"] = dr["CO_PAY_AMOUNT"];
//////                        drBilldet["NET_TOTAL"] = dr["NET_AMOUNT"];
//////                        drBilldet["BATCH_NO"] = dr["BATCHNO"];
//////                        drBilldet["EXPIRY"] = dr["EXP_DATE"];
//////                        drBilldet["IS_RETURN"] = 0;
//////                        dtBillDetails.Rows.Add(drBilldet);
//////                    }

//////                }
//////                else if (servType == ServiceType.Investigation)
//////                {
//////                    foreach (DataRow dr in BillDetails.Tables["INV_PAT_BILLING"].Rows)
//////                    {
//////                        drBilldet = dtBillDetails.NewRow();
//////                        drBilldet["BILL_NO"] = BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["BILL_NO"];
//////                        drBilldet["QTY"] = dr["QTY"];
//////                        drBilldet["GRAND_TOTAL"] = dr["GROSS_AMOUNT"];
//////                        drBilldet["COPAY"] = dr["CO_PAY_AMOUNT"];
//////                        drBilldet["NET_TOTAL"] = dr["NET_AMOUNT"];
//////                        drBilldet["SERVICE_NAME"] = dr["NAME"];
//////                        drBilldet["CPTCODE"] = dr["CPT_CODE"];
//////                        dtBillDetails.Rows.Add(drBilldet);
//////                    }
//////                }
//////                else if (servType == ServiceType.Advance || servType == ServiceType.Deductible)
//////                {
//////                    dtBillDetails.Rows[0]["BILL_NO"] = BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["BILL_NO"];
//////                    dtBillDetails.Rows[0]["SERVICE_NAME"] = Enum.GetName(typeof(ServiceType), Convert.ToUInt32(BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["VISIT_MODE"]));
//////                }
//////                else if (servType == ServiceType.Consultation)
//////                {
//////                    dtBillDetails.Rows.Add();
//////                    StringBuilder sbServiceName = new StringBuilder();
//////                    sbServiceName.Append(BillDetails.Tables["CON_PAT_BILLING"].Rows[0]["CON_TYPE_NAME"].ToString());
//////                    sbServiceName.Append(" Consultation");
//////                    dtBillDetails.Rows[0]["BILL_NO"] = BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["BILL_NO"];
//////                    dtBillDetails.Rows[0]["SERVICE_NAME"] = sbServiceName.ToString();//BillDetails.Tables["CON_PAT_BILLING"].Rows[0]["CON_TYPE_NAME"];
//////                    dtBillDetails.Rows[0]["GRAND_TOTAL"] = BillDetails.Tables["CON_PAT_BILLING"].Rows[0]["AMOUNT"];
//////                    dtBillDetails.Rows[0]["NET_TOTAL"] = BillDetails.Tables["CON_PAT_BILLING"].Rows[0]["NET_AMOUNT"];
//////                    dtBillDetails.Rows[0]["QTY"] = 1;
//////                    dtBillDetails.Rows[0]["COPAY"] = BillDetails.Tables["CON_PAT_BILLING"].Rows[0]["CO_PAY_AMOUNT"];
//////                }
//////                else if (servType == ServiceType.Registration || servType == ServiceType.ReRegistration)
//////                {
//////                    dtBillDetails.Rows.Add();
//////                    dtBillDetails.Rows[0]["BILL_NO"] = BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["BILL_NO"];
//////                    //   dtBillDetails.Rows[0]["SERVICE_NAME"] = BillDetails.Tables["CON_PAT_BILLING"].Rows[0]["CON_TYPE_NAME"];
//////                    dtBillDetails.Rows[0]["GRAND_TOTAL"] = BillDetails.Tables["REG_PAT_BILLING"].Rows[0]["AMOUNT"];
//////                    dtBillDetails.Rows[0]["NET_TOTAL"] = BillDetails.Tables["REG_PAT_BILLING"].Rows[0]["NET_AMOUNT"];
//////                    dtBillDetails.Rows[0]["QTY"] = 1;
//////                    dtBillDetails.Rows[0]["COPAY"] = BillDetails.Tables["REG_PAT_BILLING"].Rows[0]["CO_PAY_AMOUNT"];
//////                    dtBillDetails.Rows[0]["SERVICE_NAME"] = BillDetails.Tables["BILL_COMMON_DETAILS"].Rows[0]["REG_MAST_TYPE_NAME"];
//////                }
//////                //else if (servType == ServiceType.Registration || servType == ServiceType.ReRegistration)
//////                //{
//////                //    dtBillDetails.Rows[0]["BILL_NO"] = BillDetails.Tables["GEN_PAT_BILLING"].Rows[0]["BILL_NO"];
//////                //}
//////                dsBillDetails.Tables.Add(dtBillDetails.Copy());
//////                dsBillDetails.Tables.Add(dtBillMaster.Copy());
//////                return dsBillDetails;
//////            }
//////            catch (Exception)
//////            {
//////                throw;
//////            }
//////        }
//////        private string GetPatientName(DataTable PatDetls)
//////        {
//////            StringBuilder sbPatName = new StringBuilder();
//////            if (PatDetls.Rows[0]["TITLE"] != DBNull.Value)
//////            {
//////                sbPatName.Append(PatDetls.Rows[0]["TITLE"]);
//////                sbPatName.Append(".");
//////            }
//////            if (PatDetls.Rows[0]["FIRST_NAME"] != DBNull.Value)
//////            {

//////                sbPatName.Append(" "+PatDetls.Rows[0]["FIRST_NAME"]);

//////            }
//////            if (PatDetls.Rows[0]["MIDDLE_NAME"] != DBNull.Value)
//////            {
//////                sbPatName.Append(" "+PatDetls.Rows[0]["MIDDLE_NAME"]);
//////            }
//////            if (PatDetls.Rows[0]["LAST_NAME"] != DBNull.Value)
//////            {

//////                sbPatName.Append(" "+PatDetls.Rows[0]["LAST_NAME"]);
//////            }
//////            return sbPatName.ToString();
//////        }
//////        //private object SetPageMargin(object rptPage)
//////        //{

//////        //    return rptPage;
//////        //}


//////        //private void SetBillAmount(DataSet BillPrintDetails)
//////        //{
//////        //    CommonFunctions comFun = new CommonFunctions();
//////        //    double netAmount = 0;
//////        //    foreach (DataRow dr in BillPrintDetails.Tables["BILL_DETAILS"].Rows)
//////        //    {

//////        //            if (!IsPharmacyReturn(BillPrintDetails))
//////        //            {
//////        //                if (dr["NET_TOTAL"] != DBNull.Value)
//////        //                {
//////        //                    netAmount = netAmount + Convert.ToDouble(dr["NET_TOTAL"]);
//////        //                }
//////        //            }
//////        //            else
//////        //            {
//////        //                if (dr["IS_RETURN"] != DBNull.Value)
//////        //                {
//////        //                    if (Convert.ToInt16(dr["IS_RETURN"]) == 1)
//////        //                    {
//////        //                        netAmount = netAmount + Convert.ToDouble(dr["NET_TOTAL"]);
//////        //                    }
//////        //                }
//////        //            }


//////        //    }
//////        //    BillPrintDetails.Tables["BILL_MASTER"].Rows[0]["TOTAL_AMOUNT_IN_WORDS"] = comFun.ToWords(netAmount);

//////        //}
//////        //private bool IsPharmacyReturn(DataSet BillPrintDetails)
//////        //{
//////        //    bool isTrue = false;
//////        //    int retCount = 0;
//////        //    if (Convert.ToString(BillPrintDetails.Tables["BILL_MASTER"].Rows[0]["SERVICE_TYPE"]) == Enum.GetName(typeof(ServiceType), ServiceType.Pharmacy))
//////        //    {
//////        //        foreach (DataRow dr in BillPrintDetails.Tables["BILL_DETAILS"].Rows)
//////        //        {
//////        //            if (dr["IS_RETURN"] != DBNull.Value)
//////        //            {
//////        //                retCount = retCount + Convert.ToInt16(dr["IS_RETURN"]);
//////        //            }
//////        //        }
//////        //    }
//////        //    if (retCount > 0)
//////        //    {
//////        //        isTrue = true;
//////        //    }
//////        //    return isTrue;
//////        //}


//////        #region IPrinting Members

//////        public bool Print(DataSet dsData, ServiceType serviceType, string PrinterName, PrintType printType)
//////        {
//////            bool isSuccess = false;
//////            DataSet dsBillDet = null;
//////            //PrinterName = "\\\\gi12\\EpsonLX";
//////            //SetBillAmount(BillPrintDetails);
//////            if (printType == PrintType.Invoice)
//////            {
//////                if (serviceType == ServiceType.Registration)
//////                {
//////                    rptServiceBill rptRegBill = new rptServiceBill();
//////                    dsBillDet = CreateBillDetails(dsData, serviceType);
//////                    rptRegBill.SetDataSource(dsBillDet);
//////                    rptRegBill.PrintOptions.PrinterName = PrinterName;
//////                    rptRegBill.PrintToPrinter(1, false, 0, 0);
//////                    isSuccess = true;
//////                }
//////                else if (serviceType == ServiceType.Consultation)
//////                {
//////                    rptServiceBill rptConsBill = new rptServiceBill();
//////                    dsBillDet = CreateBillDetails(dsData, serviceType);
//////                    rptConsBill.SetDataSource(dsBillDet);
//////                    rptConsBill.PrintOptions.PrinterName = PrinterName;
//////                    rptConsBill.PrintToPrinter(1, false, 0, 0);
//////                    isSuccess = true;
//////                }
//////                else if (serviceType == ServiceType.Pharmacy)
//////                {
//////                    rptPharmacyBillDetails objPhBill = new rptPharmacyBillDetails();
//////                    dsBillDet = CreateBillDetails(dsData, serviceType);
//////                    objPhBill.SetDataSource(dsBillDet);
//////                    objPhBill.PrintOptions.PrinterName = PrinterName;
//////                    objPhBill.PrintToPrinter(1, false, 0, 0);
//////                    isSuccess = true;
//////                }
//////                else if (serviceType == ServiceType.Investigation)
//////                {
//////                    rptServiceBill rptInvBill = new rptServiceBill();
//////                    dsBillDet = CreateBillDetails(dsData, serviceType);
//////                    rptInvBill.SetDataSource(dsBillDet);
//////                    rptInvBill.PrintOptions.PrinterName = PrinterName;
//////                    rptInvBill.PrintToPrinter(1, false, 0, 0);
//////                    isSuccess = true;
//////                }
//////                else if (serviceType == ServiceType.Advance || serviceType == ServiceType.Deductible)
//////                {
//////                    rptAdvanceBill ObjAdvBill = new rptAdvanceBill();
//////                    ObjAdvBill.SetDataSource(dsData);
//////                    ObjAdvBill.PrintOptions.PrinterName = PrinterName;
//////                    ObjAdvBill.PrintToPrinter(1, false, 0, 0);
//////                    isSuccess = true;
//////                }
//////            }
//////            else if (printType == PrintType.Prescription)
//////            {
//////                rptPrintPresc objPrintPres = new rptPrintPresc();
//////                DataTable dtMast = null;
//////                DataTable dtDtls = null;
//////                DataSet dsPresc = null;
//////                try
//////                {
//////                    if (dsData.Tables.Contains("MASTER") && dsData.Tables.Contains("DETAILS"))
//////                    {
//////                        dtMast = dsData.Tables["MASTER"];
//////                        dtMast.TableName = "PRINT_PRESC_MASTER";
//////                        dtDtls = dsData.Tables["DETAILS"];
//////                        dtDtls.TableName = "PRINT_PRESC_DTLS";
//////                        dsPresc = new DataSet();
//////                        dsPresc.Tables.Add(dtMast.Copy());
//////                        dsPresc.Tables.Add(dtDtls.Copy());
//////                        objPrintPres.SetDataSource(dsPresc);
//////                        objPrintPres.PrintOptions.PrinterName = PrinterName;
//////                        objPrintPres.PrintToPrinter(1, false, 0, 0);
//////                        isSuccess = true;
//////                    }
//////                    else
//////                    {
//////                        isSuccess = false;
//////                    }

//////                }
//////                catch (Exception Ex)
//////                {
//////                    throw Ex;
//////                }
//////        }

//////            return isSuccess;
//////        }

//////        #endregion
//////    }
//////}
