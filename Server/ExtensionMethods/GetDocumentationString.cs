using Microsoft.Extensions.FileSystemGlobbing.Internal;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GERMAG.Server.ExtensionMethods;

public static class GetDocumentationString
{
    public static String ConvertDocumentationString (this List<string> dokumentationString)
    {
        if (dokumentationString.Count() == 0)
        {
            return "";
        }

        var IncreasedValue = "";
        var DecreasedValues = "";
        List<double> numbers = new List<double>();

        foreach (string str in dokumentationString)
        {
            string TrimmedString = Regex.Replace(Regex.Replace(str, "bis", "-"), ",", ".");


            Regex IsSmallerRegex = new Regex(@"^[^<>-]*<[^<>-]*$");
            Regex IsLargerRegex = new Regex(@"^[^<>-]*>[^<>-]*$");

            if (IsLargerRegex.IsMatch(TrimmedString)){
                IncreasedValue = ">";
            }

            if (IsSmallerRegex.IsMatch(TrimmedString))
            {
                DecreasedValues = "<";
            }

            MatchCollection matches = Regex.Matches(TrimmedString, @"[+-]?\s*\d+(\.\d*)?(?:\s*-\s*[+-]?\s*\d+(\.\d*)?)?");

            foreach (Match match in matches)
            {
                string[] rangeValues = match.Value.Split('-');
                foreach (string value in rangeValues)
                {
                    if (double.TryParse(value.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double number))
                    {
                        numbers.Add(number);
                    }
                }
            }

        }

        if (numbers.Min() == numbers.Max())
        {
            return DecreasedValues + IncreasedValue + numbers.Max();
        }
        else
        {
            return DecreasedValues + numbers.Min() + " - " + IncreasedValue + numbers.Max();
        }
    }
}
