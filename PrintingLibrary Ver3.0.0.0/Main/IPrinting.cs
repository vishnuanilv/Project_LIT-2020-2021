using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Infologics.Medilogics.Enumerators.General;


namespace Infologics.Medilogics.PrintingLibrary.Main
{
    public interface IPrinting
    {        
       bool Print(DataSet dsData, ServiceType serviceType, string PrinterName);
       bool Print(DataSet dsData, ServiceType serviceType, string PrinterName,PrintType printType);
    }
}
