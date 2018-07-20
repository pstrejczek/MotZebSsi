using MotZebSsi;
using System;
using System.Windows.Forms;

namespace MotZebSsiTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var portName = tComPort.Text;

            var ssi = new MotZebSsi.MotZebSsi(portName, 9600, ErrorCodeDictionary.ReadMode.CodeReadAndSend, "", 5000);

            ssi.ReadedCodeEvent += _barcodeScanner_ReadedCodeEvent;
            ssi.ScannerErrorEvent += _barcodeScanner_ScannerErrorEvent;
            ssi.Start();
        }

        private void _barcodeScanner_ReadedCodeEvent(object sender, ReadedCodeEventArgs oEventArgs)
        {
            var scannedCode = oEventArgs.ReadedCode;

            MessageBox.Show(scannedCode, "CODE", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void _barcodeScanner_ScannerErrorEvent(object sender, ScannerErrorEventArgs oEventArgs)
        {
            var scannerErrorString = string.Empty;

            switch (oEventArgs.ErrorType)
            {
                case ErrorCodeDictionary.DataReadErrors.CheckSumError:
                    scannerErrorString = "CHECKSUM ERROR";
                    break;
                case ErrorCodeDictionary.DataReadErrors.CodeNotCorrect:
                    scannerErrorString = "SCANNED CODE IS INCORRECT";
                    break;
                case ErrorCodeDictionary.DataReadErrors.ConnectionNotEstabilished:
                    scannerErrorString = "NO CONNECTION TO BARCODE SCANNER";
                    break;
                case ErrorCodeDictionary.DataReadErrors.ReadTimeout:
                    scannerErrorString = "CODE SCAN TIMEOUT";
                    break;
            }

            MessageBox.Show(scannerErrorString, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
