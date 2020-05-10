//-----------------------------------------------------------------------
// <copyright file="RawPrinterHelper.cs" company="GI Infologics PVT Ltd">
//     Copyright (c) GI Infologics Pvt Ltd. All rights reserved.
// </copyright>
// <author>Biju S J</author>
//<Date>22-Jan-2010<Date>
//-----------------------------------------------------------------------

namespace Infologics.Medilogics.PrintingLibrary.Main
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Drawing;
    using System.Drawing.Printing;
    using System.Runtime.InteropServices;
    using System.IO;
    //using System.Windows.Forms;

    /// <summary>
    /// ///Raw Printer Helper to support document print in Raw Printers (DOS mode printing)
    /// </summary>
    public class RawPrinterHelper
    {
        // Structure and API declarions:
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDataType;
        }
        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool StartDocPrinter(IntPtr hPrinter, Int32 level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, Int32 dwCount, out Int32 dwWritten);

        
        /// <summary>
        // SendBytesToPrinter()
        // When the function is given a printer name and an unmanaged array
        // of bytes, the function sends those bytes to the print queue.
        // Returns true on success, false on failure.
        /// </summary>
        /// <param name="szPrinterName">Printer Name</param>
        /// <param name="pBytes">System formated Print Bytes</param>
        /// <param name="dwCount">Size</param>
        /// <param name="docName">Name of the Document,Default is "Y A S A S I I"</param>
        /// <returns>bool</returns>
        public static bool SendBytesToPrinter(string szPrinterName, IntPtr pBytes, Int32 dwCount, string docName)
        {
            bool bSuccess = false; // Assume failure unless you specifically succeed.
            try
            {
                Int32 dwError = 0, dwWritten = 0;
                IntPtr hPrinter = new IntPtr(0);
                DOCINFOA di = new DOCINFOA();
                if (docName == string.Empty)
                {
                    docName = "Y A S A S I I";
                }

                di.pDocName = docName;
                di.pDataType = "RAW";

                // Open the printer.
                if (OpenPrinter(szPrinterName.Normalize(), out hPrinter, IntPtr.Zero))
                {
                    // Start a document.
                    if (StartDocPrinter(hPrinter, 1, di))
                    {
                        // Start a page.
                        if (StartPagePrinter(hPrinter))
                        {
                            // Write your bytes.
                            bSuccess = WritePrinter(hPrinter, pBytes, dwCount, out dwWritten);
                            EndPagePrinter(hPrinter);
                        }
                        EndDocPrinter(hPrinter);
                    }
                    ClosePrinter(hPrinter);
                }
                // If you did not succeed, GetLastError may give more information
                // about why not.
                if (bSuccess == false)
                {
                    dwError = Marshal.GetLastWin32Error();
                }
            }
            catch (Exception)
            {
                bSuccess = false; 
                throw;
            }
           
            return bSuccess;
        }

        /// <summary>
        /// Send FileName to Raw Printer
        /// </summary>
        /// <param name="szPrinterName">Printer Name</param>
        /// <param name="szString">Printing Data</param>
        /// <param name="docName">Name of the Document</param>
        /// <returns>bool</returns>
        public static bool SendFileToPrinter(string szPrinterName, string szFileName, string docName)
        {
            bool bSuccess = false;
            try
            {
                // Open the file.
                FileStream fs = new FileStream(szFileName, FileMode.Open);
                // Create a BinaryReader on the file.
                BinaryReader br = new BinaryReader(fs);
                // Dim an array of bytes big enough to hold the file's contents.
                Byte[] bytes = new Byte[fs.Length];
                // Your unmanaged pointer.
                IntPtr pUnmanagedBytes = new IntPtr(0);
                int nLength;

                nLength = Convert.ToInt32(fs.Length);
                // Read the contents of the file into the array.
                bytes = br.ReadBytes(nLength);
                // Allocate some unmanaged memory for those bytes.
                pUnmanagedBytes = Marshal.AllocCoTaskMem(nLength);
                // Copy the managed byte array into the unmanaged array.
                Marshal.Copy(bytes, 0, pUnmanagedBytes, nLength);
                // Send the unmanaged bytes to the printer.
                bSuccess = SendBytesToPrinter(szPrinterName, pUnmanagedBytes, nLength, docName);
                // Free the unmanaged memory that you allocated earlier.
                Marshal.FreeCoTaskMem(pUnmanagedBytes);
            }
            catch (FileNotFoundException)
            {
          //      MessageBox.Show("File not Found in Path", "Y A S A S I I", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                bSuccess = false;
            }
            catch (Exception)
            {
                throw;
            }

            return bSuccess;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="szPrinterName">Printer Name</param>
        /// <param name="szString">Printing Data</param>
        /// <param name="docName">Name of the Document</param>
        /// <returns>bool</returns>
        public static bool SendStringToPrinter(string szPrinterName, string szString, string docName)
        {
            bool blnStatus = false;
            try
            {
                IntPtr pBytes;
                Int32 dwCount;
                // How many characters are in the string?
                dwCount = szString.Length;
                // Assume that the printer is expecting ANSI text, and then convert
                // the string to ANSI text.
                pBytes = Marshal.StringToCoTaskMemAnsi(szString);
                // Send the converted ANSI string to the printer.
                blnStatus = SendBytesToPrinter(szPrinterName, pBytes, dwCount, docName);
                Marshal.FreeCoTaskMem(pBytes);
            }
            catch (Exception)
            {
                blnStatus = false;
                throw;
            }
            return blnStatus;
        }
    }
}
