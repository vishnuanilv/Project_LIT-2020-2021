using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Infologics.Medilogics.PrintingLibrary.ServicePrinting.CrystalReports;
using Infologics.Medilogics.PrintingLibrary.Main;
using System.IO;

namespace Infologics.Medilogics.PrintingLibrary.ServicePrinting
{
    public class ServiceReport:IPrinting
    {
        public object GetTestResultReport(DataTable Result ,Dictionary<string ,object> ReportParameter ,int ReportType)
        {
            try
            {
                DataTable dtEmployeeDtls = ReportParameter["authorizedEmployees"] as DataTable;
                DataTable dtAuthData = new Infologics.Medilogics.PrintingLibrary.ServicePrinting.DataSets.Investigation.EMPLOYEEDataTable().Clone();
                if(dtEmployeeDtls != null)
                {
                    foreach(DataRow item in dtEmployeeDtls.Rows)
                    {
                        DataRow dr = dtAuthData.NewRow();
                        dr["EMPLOYEE_NAME"] = item["EMP_NAME"];
                        if(item["DIGITAL_SIGNATURE"] != DBNull.Value && File.Exists(item["DIGITAL_SIGNATURE"].ToString()) == true)
                        {
                            FileStream fs = new FileStream(item["DIGITAL_SIGNATURE"].ToString() ,
                            System.IO.FileMode.Open ,System.IO.FileAccess.Read);
                            byte[] Image = new byte[fs.Length];
                            fs.Read(Image ,0 ,Convert.ToInt32(fs.Length));
                            fs.Close();
                            dr["SIGNATURE"] = Image;
                            dr["ISEXISTS_SIGNATURE"] = 1;
                        }
                        dtAuthData.Rows.Add(dr);
                    }

                }
                ReportParameter.Remove("authorizedEmployees");
                if(dtAuthData.Rows.Count > 0)
                {
                    ReportParameter.Add("authorizedEmployees" ,"Authorized By :");
                }
                else
                {
                    ReportParameter.Add("authorizedEmployees" ,"");
                }
                if(ReportType == 0)
                {

                    rptTestResults objrptTestResults = new rptTestResults();
                    objrptTestResults.Subreports["rptAuthorizedEmployeeDetails.rpt"].SetDataSource(dtAuthData);
                    objrptTestResults.SetDataSource(Result);
                    var QueryReportParameters = from para in ReportParameter.AsEnumerable()
                                                join crpt in objrptTestResults.ParameterFields.ToArray().AsEnumerable()
                                                on para.Key.ToUpper() equals (crpt as CrystalDecisions.Shared.ParameterField).Name.ToUpper()
                                                select para;
                    foreach(KeyValuePair<string ,object> Parameter in QueryReportParameters)
                    {
                        objrptTestResults.SetParameterValue(Parameter.Key ,Parameter.Value);
                    }
                    return objrptTestResults;
                }
                else
                {
                    rptTestResultsServiceOrder objrptTestResults = new rptTestResultsServiceOrder();
                    objrptTestResults.Subreports["rptAuthorizedEmployeeDetails.rpt"].SetDataSource(dtAuthData);
                    objrptTestResults.SetDataSource(Result);
                    var QueryReportParameters = from para in ReportParameter.AsEnumerable()
                                                join crpt in objrptTestResults.ParameterFields.ToArray().AsEnumerable()
                                                on para.Key.ToUpper() equals (crpt as CrystalDecisions.Shared.ParameterField).Name.ToUpper()
                                                select para;
                    foreach(KeyValuePair<string ,object> Parameter in QueryReportParameters)
                    {
                        objrptTestResults.SetParameterValue(Parameter.Key ,Parameter.Value);
                    }
                    return objrptTestResults;
                }

            }
            catch(Exception)
            {

                throw;
            }
        }
        public object PrintReciept(DataTable Result)
        {
            try
            {
                rptLabSampleOrderDetails objrptTestResults = new rptLabSampleOrderDetails();
                objrptTestResults.SetDataSource(Result);
                return objrptTestResults;

            }
            catch(Exception)
            {

                throw;
            }
        }
        public object PrintReciept(DataTable Result ,string PrinterName)
        {
            try
            {
                rptLabSampleOrderDetails objrptTestResults = new rptLabSampleOrderDetails();
                objrptTestResults.SetDataSource(Result);
                objrptTestResults.PrintOptions.PrinterName = PrinterName;
                // objrptTestResults.PrintOptions.PaperSize = CrystalDecisions.Shared.PaperSize.PaperA4;   
                objrptTestResults.PrintToPrinter(1 ,false ,0 ,0);
                return objrptTestResults;

            }
            catch(Exception)
            {

                throw;
            }
        }

        #region IPrinting Members

        public bool Print(DataSet dsData ,Infologics.Medilogics.Enumerators.General.ServiceType serviceType ,string PrinterName ,Infologics.Medilogics.Enumerators.General.PrintType printType)
        {
            throw new NotImplementedException();
        }

        public bool Print(DataSet dsData ,Infologics.Medilogics.Enumerators.General.ServiceType serviceType ,string PrinterName)
        {
            throw new NotImplementedException();
        }
        public bool Print(DataSet dsData ,int PrintType ,string PrinterName)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
