namespace MotZebSsi
{
    public class ScannerErrorEventArgs
    {
        public ErrorCodeDictionary.DataReadErrors ErrorType { get; set; }
        public string ErrorDescription { get; set; }

        public ScannerErrorEventArgs(ErrorCodeDictionary.DataReadErrors errorType, string errorDescription)
        {
            ErrorType = errorType;
            ErrorDescription = errorDescription;
        }
    }
}
