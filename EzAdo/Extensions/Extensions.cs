namespace EzAdo.Extensions
{
    /// <summary>   Extension methods for converting string values. </summary>
    public static class Extensions
    {
        /// <summary>   A string extension method that converts a string to underscore case. </summary>
        /// <param name="value">    The value to act on. </param>
        /// <returns>   value as a string. </returns>
        public static string ToUnderscore(this string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            int len = value.Length;
            int outLen = len * 2;
            int outPos = 0;
            char[] input = value.ToCharArray();
            char[] output = new char[outLen];
            bool previousCharWasLower = false;
            bool previousCharWasNumber = false;

            for (int idx = 0; idx != len; idx++)
            {
                char current = input[idx];
                if (previousCharWasLower || previousCharWasNumber)
                {
                    if (char.IsUpper(current))
                    {
                        output[outPos++] = '_';
                    }
                }
                output[outPos++] = char.ToUpper(input[idx]);
                previousCharWasLower = char.IsLower(current);
                previousCharWasNumber = char.IsDigit(current);
            }
            return new string(output, 0, outPos);
        }

        /// <summary>   A string extension method that converts a value string to ProperCase. </summary>
        /// <param name="value">    The value to act on. </param>
        /// <returns>   value as a string. </returns>
        public static string ToProperCase(this string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            int len = value.Length;
            int outLen = len;
            int outPos = 0;
            char[] input = value.ToCharArray();
            char[] output = new char[outLen];
            bool previousCharUnderscore = true;

            for (int idx = 0; idx != len; idx++)
            {
                char current = input[idx];
                if (current == '_')
                {
                    previousCharUnderscore = true;
                    continue;
                }
                if (previousCharUnderscore)
                {
                    output[outPos++] = char.ToUpper(input[idx]);
                }
                else
                {
                    output[outPos++] = char.ToLower(input[idx]);
                }
                previousCharUnderscore = false;
            }
            return new string(output, 0, outPos);

        }

        /// <summary>   A string extension method that converts a string to a cameCase. </summary>
        /// <param name="value">    The value to act on. </param>
        /// <returns>   value as a string. </returns>
        public static string ToCamel(this string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            int len = value.Length;
            int outLen = len;
            int outPos = 0;
            char[] input = value.ToCharArray();
            char[] output = new char[outLen];
            bool previousCharUnderscore = true;

            for (int idx = 0; idx != len; idx++)
            {
                char current = input[idx];
                if (current == '_')
                {
                    previousCharUnderscore = true;
                    continue;
                }
                if (previousCharUnderscore)
                {
                    if (idx == 0)
                    {
                        output[outPos++] = char.ToLower(input[idx]);
                    }
                    else
                    {
                        output[outPos++] = char.ToUpper(input[idx]);
                    }
                }
                else
                {
                    output[outPos++] = char.ToLower(input[idx]);
                }
                previousCharUnderscore = false;
            }
            return new string(output, 0, outPos);
        }
    }
}
