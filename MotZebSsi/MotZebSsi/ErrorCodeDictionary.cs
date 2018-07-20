namespace MotZebSsi
{
    public static class ErrorCodeDictionary
    {
        public enum DataReadErrors
        {
            ConnectionNotEstabilished,
            ReadTimeout,
            CheckSumError,
            CodeNotCorrect,
        }

        public enum ReadMode
        {
            CodeCompare,
            CodeRead,
            CodeReadAndSend,
        }
    }
}
