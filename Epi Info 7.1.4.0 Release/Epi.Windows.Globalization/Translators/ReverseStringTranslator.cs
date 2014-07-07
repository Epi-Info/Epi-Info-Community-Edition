using System;
using System.Collections.Generic;
using System.Text;

namespace Epi.Windows.Globalization.Translators
{
    /// <summary>
    /// Reverses all characters in a string
    /// </summary>
    public class ReverseStringTranslator : NormalizedStringTranslator
    {
        /// <summary>
        /// Translate string from source culture to target culture
        /// </summary>
        /// <param name="sourceCultureName"></param>
        /// <param name="targetCultureName"></param>
        /// <param name="sourceText"></param>
        /// <returns></returns>
        public override string Translate(string sourceCultureName, string targetCultureName, string sourceText)
        {
            string normalizedText = base.Translate(sourceCultureName, targetCultureName, sourceText);
            return ReverseString(normalizedText);
        }

        string ReverseString(string str)
        {
            string returnString = "";
            foreach (char c in str.ToCharArray())
            {
                returnString = c + returnString;
            }
            for (int i = 0; i < 10; i++)
            {
                returnString = returnString.Replace("}" + i.ToString() + "{", "{" + i.ToString() + "}");
            }
            return returnString;
        }

    }
}
