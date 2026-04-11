namespace CarX.API.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    using NorthwoodLib.Pools;

    public static class String
    {
        public static int GetDistance(this string firstString, string secondString)
        {
            int n = firstString.Length;
            int m = secondString.Length;
            int[,] d = new int[n + 1, m + 1];

            if (n == 0)
                return m;

            if (m == 0)
                return n;

            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (secondString[j - 1] == firstString[i - 1]) ? 0 : 1;

                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }

            return d[n, m];
        }

        public static (string commandName, string[] arguments) ExtractCommand(this string commandLine)
        {
            string[] extractedArguments = commandLine.Split(' ');

            return (extractedArguments[0].ToLower(), extractedArguments.Skip(1).ToArray());
        }

        public static string ToSnakeCase(this string str, bool shouldReplaceSpecialChars = true)
        {
            string snakeCaseString = string.Concat(str.Select((ch, i) => i > 0 && char.IsUpper(ch) ? "_" + ch.ToString() : ch.ToString())).ToLower();

            return shouldReplaceSpecialChars ? Regex.Replace(snakeCaseString, @"[^0-9a-zA-Z_]+", string.Empty) : snakeCaseString;
        }

        public static string ToString<T>(this IEnumerable<T> enumerable, bool showIndex = true)
        {
            StringBuilder stringBuilder = StringBuilderPool.Shared.Rent();
            int index = 0;

            stringBuilder.AppendLine(string.Empty);

            foreach (var enumerator in enumerable)
            {
                if (showIndex)
                {
                    stringBuilder.Append(index++);
                    stringBuilder.Append(' ');
                }

                stringBuilder.AppendLine(enumerator.ToString());
            }

            string result = stringBuilder.ToString();

            StringBuilderPool.Shared.Return(stringBuilder);

            return result;
        }

        public static string RemoveBracketsOnEndOfName(this string name)
        {
            var bracketStart = name.IndexOf('(') - 1;

            if (bracketStart > 0)
                name = name.Remove(bracketStart, name.Length - bracketStart);

            return name;
        }

        public static string SplitCamelCase(this string input)
        {
            return Regex.Replace(input, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
        }

        public static string RemoveSpaces(this string input)
        {
            return Regex.Replace(input, @"\s+", string.Empty);
        }
    }
}
