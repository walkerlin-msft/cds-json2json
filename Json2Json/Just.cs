using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Json2Json
{
    /* 
     * JUST Stands for JSON Under Simple Transformation
     * https://github.com/WorkMaze/JUST.net
     * */

    public class Just
    {
        public static string CDS_MESSAGE_TRANSFORM_PROPERTY_NOT_FOUND = "CDS_MESSAGE_TRANSFORM_PROPERTY_NOT_FOUND";

        public static string GetValue(string key)
        {
            return "#valueof($." + key + ")";
        }

        public static string GetValueIfExists(string key)
        {
            return IfCondition(Exists(key),
                "true",
                GetValue(key),
                CDS_MESSAGE_TRANSFORM_PROPERTY_NOT_FOUND);
        }

        public static string GetValueIfExistsAndNotEmpty(string key)
        {
            return IfCondition(ExistsAndNotEmpty(key),
                "true",
                GetValue(key),
                CDS_MESSAGE_TRANSFORM_PROPERTY_NOT_FOUND);
        }

        public static string IfCondition(string condition, string evaluation, string trueResult, string falseResult)
        {
            return "#ifcondition(" + condition + "," + evaluation + "," + trueResult + "," + falseResult + ")";
        }

        public static string GetLastIndexOf(string input, string search)
        {
            return "#lastindexof(" + input + "," + search + ")";
        }
        public static string GetFastIndexOf(string input, string search)
        {
            return "#firstindexof(" + input + "," + search + ")";
        }

        public static string Add(string valueA, string valueB)
        {
            return "#add(" + valueA + "," + valueB + ")";
        }

        public static string Subtract(string valueA, string valueB)
        {
            return "#subtract(" + valueA + "," + valueB + ")";
        }

        public static string Concat(string s1, string s2)
        {
            return "#concat(" + s1 + "," + s2 + ")";
        }

        public static string SubString(string input, string startIndex, string length)
        {
            return "#substring(" + input + "," + startIndex + "," + length + ")";
        }

        public static string ContainsOfString(string input, string pattern)
        {
            // Return "true" or "false"
            return IfCondition(GetFastIndexOf(input, pattern),
                "-1",
                "false",
                "true");
        }

        public static string GetStringLength(string input)
        {
            return GetLastIndexOf(Concat(input, ":"), ":");
        }

        public static string Exists(string key)
        {
            // Return "true" or "false"
            return "#exists($." + key + ")";
        }

        public static string ExistsAndNotEmpty(string key)
        {
            // Return "true" or "false"
            return "#existsandnotempty($." + key + ")";
        }

        public static string SubStringFromLastColon(string input)
        {
            string startIndex = Add(GetLastIndexOf(input, ":"), "1");

            string totalLength = GetStringLength(input);

            string length = Subtract(totalLength, startIndex);

            return SubString(input, startIndex, length);
        }
    }
}
