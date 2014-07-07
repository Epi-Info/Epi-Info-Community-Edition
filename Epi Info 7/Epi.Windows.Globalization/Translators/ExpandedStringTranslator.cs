using System;
using System.Collections.Generic;
using System.Text;

namespace Epi.Windows.Globalization.Translators
{
    /// <summary>
    /// Doubles incoming strings. For demonstration purposes only.
    /// </summary>
    public class ExpandedStringTranslator : NormalizedStringTranslator
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
            return ExpandString(normalizedText);
        }

        string ExpandString(string str)
        {
            string returnString = str;
            for (int i = 0; i < str.Length / 4; i++)
            {
                returnString = "_" + returnString;
            }
            for (int i = 0; i < str.Length / 4; i++)
            {
                returnString = returnString + "_";
            }
            return returnString;
        }
    }
}
