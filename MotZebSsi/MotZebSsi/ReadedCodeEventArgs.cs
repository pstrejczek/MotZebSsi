namespace MotZebSsi
{
    public class ReadedCodeEventArgs
    {
        public string ReadedCode { get; set; }

        public ReadedCodeEventArgs(string readedCode)
        {
            ReadedCode = readedCode;
        }
    }
}
