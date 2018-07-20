using System;
using System.Collections.Generic;

namespace MotZebSsi
{
    class SsiMessages
    {
        private static readonly Dictionary<string, byte> SsiMessage = new Dictionary<string, byte>
        {
            {"EnableTrigger", 0x8A},
            {"AimOff", 0xC4},
            {"AimOn", 0xC5},
            {"Beep", 0xE6},
            {"Ack", 0xD0},
            {"NAck", 0xD1},
            {"Image",0xB1},
            {"Disable", 0xEA},
            {"Enable", 0xE9},
            {"TriggerOn", 0xE4},
            {"TriggerOff", 0xE5}
        };

        private const byte Length = 4;
        private const byte Retries = 0;
        private const byte MessageSource = 4;

        public static byte[] PrepareMessage(string message)
        {
            var checkSum = CalculateChecksum(SsiMessage[message]);
            byte[] messageArray = { Length, SsiMessage[message], MessageSource, Retries, checkSum[0], checkSum[1] };
            return messageArray;
        }


        private static byte[] CalculateChecksum(byte opcode)
        {
            var sum = Convert.ToInt16(-(Length + opcode + MessageSource + Retries));
            var b = BitConverter.GetBytes(sum);
            Array.Reverse(b);
            return b;
        }

        public static byte GetCheckSumErrorVal()
        {
            return SsiMessage["NAck"];
        }
    }
}
