using System;
using System.IO;
using System.Linq;

namespace nomsol.core.utilities.converters
{
    public static class strings
    {
        /// <summary>
        /// Capitalizes the first letter of a string.
        /// </summary>
        /// <param name="strg">The string to capitalize.</param>
        /// <returns>The input string with the first letter capitalized.</returns>
        public static string FirstCharToUpper(this string strg)
        {
            if (string.IsNullOrEmpty(strg))
            {
                return strg;
            }

            return strg.First().ToString().ToUpper() + strg.Substring(1);
        }

        /// <summary>
        /// Appends a timestamp to a file name.
        /// </summary>
        /// <param name="strg">The file name to append the timestamp to.</param>
        /// <param name="dateTimeFormat">Optional: The DateTime format to use for the timestamp. Default format: yyyyMMddHHmmssfff</param>
        /// <returns>The file name with an appended timestamp.</returns>
        public static string AppendTimeStamp(this string strg, string dateTimeFormat = "yyyyMMddHHmmssfff")
        {
            return string.Concat(
                Path.GetFileNameWithoutExtension(strg),
                "_",
                DateTime.Now.ToString(dateTimeFormat),
                Path.GetExtension(strg)
                );
        }

        /// <summary>
        /// Removes spaces from a string.
        /// </summary>
        /// <param name="strg">The string to remove spaces from.</param>
        /// <returns>The input string with spaces removed.</returns>
        public static string RemoveWhiteSpace(this string strg)
        {
            if (strg == null) { return strg; }

            return string.Join("", strg.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
