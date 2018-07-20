using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Windows.Threading;

namespace MotZebSsi
{
    public class MotZebSsi
    {
        private SerialPort _sPort;
        private readonly string _codeToCompare;
        private readonly int _readTimeout;
        private DispatcherTimer _dtimer;
        private readonly ErrorCodeDictionary.ReadMode _mode;

        public delegate void ScannerError(object sender, ScannerErrorEventArgs oEventArgs);
        public event ScannerError ScannerErrorEvent;
        public delegate void ReadedCode(object sender, ReadedCodeEventArgs oEventArgs);
        public event ReadedCode ReadedCodeEvent;

        public MotZebSsi(string portName, int baudRate, ErrorCodeDictionary.ReadMode mode, string codeToCompare, int timeout)
        {
            _codeToCompare = codeToCompare;
            _mode = mode;
            _readTimeout = timeout;

            SetSerialConnectionPort(portName, baudRate, timeout);
        }

        private void SetSerialConnectionPort(string portName, int baudRate, int timeout)
        {
            _sPort = new SerialPort
            {
                PortName = portName,
                BaudRate = baudRate,
                DataBits = 8,
                Parity = Parity.None,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
                ReadTimeout = timeout,
                WriteTimeout = timeout
            };

            // Add data recieved event handler
            _sPort.DataReceived += _sPort_DataReceived;
        }

        private void _sPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var sPort = (SerialPort)sender;
            var lData = new List<byte>();

            Thread.Sleep(10); //Wait for data to arrive in the recieve buffer

            try
            {
                while (sPort.BytesToRead > 0)
                {
                    lData.Add((byte)sPort.ReadByte());
                    Thread.Sleep(1);
                }

                InterpreteFrame(lData.ToArray(), lData.Count);
            }
            catch (Exception ex)
            {
                ScannerErrorEvent?.Invoke(this, new ScannerErrorEventArgs(ErrorCodeDictionary.DataReadErrors.ReadTimeout, ""));
            }

        }

        private void StartTimer()
        {
            _dtimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, _readTimeout) };
            _dtimer.Tick += ScanTimeout;
            _dtimer.Start();
        }


        public void Start()
        {
            StartTimer();
            PullTrigger();
        }

        public void Break()
        {
            if (_sPort != null)
                if (_sPort.IsOpen)
                    TriggerDown();

            _dtimer.Stop();
            CloseConnection();

            ScannerErrorEvent?.Invoke(this, new ScannerErrorEventArgs(ErrorCodeDictionary.DataReadErrors.ReadTimeout, ""));
        }

        private void PullTrigger()
        {
            var emessage = SsiMessages.PrepareMessage("EnableTrigger");
            SendMessage(emessage);

            Thread.Sleep(500);

            var message = SsiMessages.PrepareMessage("TriggerOn");
            SendMessage(message);
        }

        private void TriggerDown()
        {
            var message = SsiMessages.PrepareMessage("TriggerOff");
            SendMessage(message);
        }

        private void InterpreteFrame(byte[] bytes, int length)
        {
            var mLength = bytes[0];
            if (length == mLength) //Configuration frame
            {
                if (bytes[1] == SsiMessages.GetCheckSumErrorVal())
                {
                    ScannerErrorEvent?.Invoke(this, new ScannerErrorEventArgs(ErrorCodeDictionary.DataReadErrors.CheckSumError, "Błąd sumy kontrolnej w komunikacji SSI"));
                }
            }
            else // Barcode
            {
                var codeBytes = new byte[length];
                Array.Copy(bytes, 0, codeBytes, 0, length);
                var barcode = Encoding.ASCII.GetString(codeBytes);
                _dtimer.Stop();
                CloseConnection();
                CompareCode(barcode);
            }
        }

        private void SendMessage(byte[] message)
        {
            try
            {
                if (_sPort == null) throw new Exception("Błąd podczas wysyłania komunikatu do skanera");
                if (!_sPort.IsOpen) OpenConnection();
                _sPort.BaseStream.Flush();
                _sPort.Write(message, 0, message.Length);
            }
            catch (Exception ex)
            {
                ScannerErrorEvent?.Invoke(this, new ScannerErrorEventArgs(ErrorCodeDictionary.DataReadErrors.ConnectionNotEstabilished, ex.Message));
                _dtimer?.Stop();
                CloseConnection();
            }
        }

        private void OpenConnection()
        {
            _sPort?.Open();

            if (_sPort != null && !_sPort.IsOpen)
            {
                ScannerErrorEvent?.Invoke(this, new ScannerErrorEventArgs(ErrorCodeDictionary.DataReadErrors.ConnectionNotEstabilished, "Błąd nawiązywania połączenia ze skanerem \nSkaner nie podłączony lub błędna konfiguracja"));
            }
        }

        private void CloseConnection()
        {
            if (_sPort == null) return;
            if (_sPort.IsOpen) _sPort.Close();
        }

        private void ScanTimeout(object sender, EventArgs e)
        {
            TriggerDown();
            _dtimer.Stop();
            ScannerErrorEvent?.Invoke(this, new ScannerErrorEventArgs(ErrorCodeDictionary.DataReadErrors.ReadTimeout, "Timeout odczytu danych ze skanera"));
            CloseConnection();
        }

        private void CompareCode(string barcode)
        {
            if (_mode == ErrorCodeDictionary.ReadMode.CodeCompare)
            {
                if (string.Compare(barcode, _codeToCompare, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    ReadedCodeEvent?.Invoke(this, new ReadedCodeEventArgs(barcode)); // RaiseEvent ReadedCode (barcode);
                }
                else
                {
                    ScannerErrorEvent?.Invoke(this, new ScannerErrorEventArgs(ErrorCodeDictionary.DataReadErrors.CodeNotCorrect, ""));
                    _dtimer?.Stop();
                }
            }
            else
            {
                ReadedCodeEvent?.Invoke(this, new ReadedCodeEventArgs(barcode)); // RaiseEvent ReadedCode (barcode);
            }

            _dtimer?.Stop();
            CloseConnection();
        }
    }
}
