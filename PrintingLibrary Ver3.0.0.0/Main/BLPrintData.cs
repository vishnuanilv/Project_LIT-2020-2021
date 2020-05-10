using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Infologics.Medilogics.Enumerators.General;
using Infologics.Medilogics.General.Control;

namespace Infologics.Medilogics.PrintingLibrary.Main
{
    /// <summary>
    /// To print the details in crystal or dos mode
    /// </summary>
    public class BLPrintData
    {
        private DataTable dtConfigSettings = null;

        /// <summary>
        /// BLs the print data.
        /// </summary>
        public BLPrintData()
        {
        }
        /// <summary>
        /// Accept the configuration settings(crystal or dos mode, crystal and dos namespaces)
        /// </summary>
        /// <param name="dtConfigSettings">The dt config settings.</param>
        public BLPrintData(DataTable dtConfigSettings)
        {
            this.dtConfigSettings = dtConfigSettings;
        }
        /// <summary>
        /// Prints the specified ds data.
        /// </summary>
        /// <param name="dsData">The ds data.</param>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="PrinterName">Name of the printer.</param>
        /// <param name="printType">Type of the print.</param>
        /// <returns></returns>
        public bool Print(DataSet dsData, ServiceType serviceType, string PrinterName, PrintType printType)
        {
            try
            {
                string TypeName = string.Empty;
                TypeName = this.GetTypeName();
                if (TypeName != string.Empty)
                {
                    Common objCommon = new Common();
                    IPrinting objIPrinting=(IPrinting)objCommon.CreateObject(TypeName);
                    objIPrinting.Print(dsData, serviceType, PrinterName, printType);
                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        private string GetTypeName()
        {
            try
            {
                string TypeName = string.Empty;
                if (this.dtConfigSettings != null && this.dtConfigSettings.Rows.Count > 0)
                {
                    DataRow[] drFounds = null;
                    drFounds = dtConfigSettings.Select("KEY='PrintMode'");
                    if (drFounds != null && drFounds.Length > 0)
                    {
                        if (drFounds[0][1].ToString() == "1") //CRYSTAL
                        {
                            drFounds = dtConfigSettings.Select("KEY='CrystalPrint'");
                        }
                        else if (drFounds[0][1].ToString() == "0") //Dos
                        {
                            drFounds = dtConfigSettings.Select("KEY='DosPrint'");
                        }
                        if (drFounds != null && drFounds.Length > 0)
                        {
                            TypeName = drFounds[0][1].ToString();
                        }
                    }
                    else
                    {
                        TypeName = dtConfigSettings.Rows[0][1].ToString();
                    }
                }
                return TypeName;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
