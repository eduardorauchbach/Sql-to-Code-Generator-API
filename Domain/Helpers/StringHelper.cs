using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WorkUtilities.Helpers
{
	public static class StringHelper
	{
		public const string Unidentified = "????";

		private static List<string> ReservedWords = new List<string>
		{
			"ID"
		};

		public static string Clear(this string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return name;
			}

			return Regex.Replace(name, @"[\[\]\(\)\n\t\r ]", "");
		}

		public static string ToLetters(this int index)
		{
			const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

			var value = "";

			if (index >= letters.Length)
				value += letters[index / letters.Length - 1];

			value += letters[index % letters.Length];

			return value;
		}

		public static string ToCamelCase(this string name, bool firstLower = false)
		{
			string newName;

			if (string.IsNullOrEmpty(name))
			{
				newName = name;
			}
			else if ((name.Contains('_') || name == name.ToLower() || name == name.ToUpper()) && !ReservedWords.Contains(name))
			{
				newName = name.ToLower();

				string[] array = newName.Split('_');
				for (int i = 0; i < array.Length; i++)
				{
					string s = array[i];
					string first = string.Empty;
					string rest = string.Empty;
					if (s.Length > 0)
					{
						first = Char.ToUpperInvariant(s[0]).ToString();
					}
					if (s.Length > 1)
					{
						rest = s.Substring(1).ToLowerInvariant();
					}
					array[i] = first + rest;
				}

				newName = string.Join("", array);
				if (newName.Length == 0)
				{
					newName = name;
				}
			}
			else
			{
				newName = name;
			}

			if (firstLower)
			{
				newName = newName[..1].ToLower() + newName[1..];
			}

			return newName;
		}


		public static StringBuilder AppendCode(this StringBuilder builder, int identationLevel, string text, int lines = 0)
		{
			return builder.Append(new String('\t', identationLevel) + text + new String('\n', lines));
		}
	}
}
