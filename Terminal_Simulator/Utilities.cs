using System;

namespace Terminal_Simulator {
    static class Utilities {

        public static void log(string message = "") {
            Console.WriteLine(message);
        }
        public static string CalculateHeader(byte[] buffer) {
            //The Header contains the data length in hexadecimal format on two digits
            var hex = buffer.Length.ToString("X4");
            hex = hex.PadLeft(4, '0');

            // Get total value per two char.
            var fDigit = hex[0].ToString() + hex[1];
            var sDigit = hex[2].ToString() + hex[3];

            return string.Format("{0}{1}", Convert.ToChar(Convert.ToUInt32(fDigit, 16)),
               Convert.ToChar(Convert.ToUInt32(sDigit, 16)));
        }
        public static int HeaderLength(byte[] buffer) {
            // Conversion from decimal to hex value
            var fHex = Convert.ToInt64(buffer[0]).ToString("X2");
            var sHex = Convert.ToInt64(buffer[1]).ToString("X2");

            // Concat two hex value
            var _hex = fHex + sHex;

            // Get decimal value of concatenated hex
            return int.Parse(_hex, System.Globalization.NumberStyles.HexNumber);
        }

        public static string ByteArrayToString(byte[] buffer) {
            return System.Text.Encoding.ASCII.GetString(buffer);
        }

        public static byte[] StringToByteArray(string data) {
            if (data.Length > 128) {
                return System.Text.Encoding.Default.GetBytes(data);
            }

            return System.Text.Encoding.ASCII.GetBytes(data);
        }
    }
}
