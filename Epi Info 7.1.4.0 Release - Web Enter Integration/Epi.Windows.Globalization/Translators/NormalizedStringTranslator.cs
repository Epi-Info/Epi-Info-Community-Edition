using System;
using System.Collections.Generic;
using System.Text;

namespace Epi.Windows.Globalization.Translators
{
    /// <summary>
    /// Removes formatting and control characters from strings
    /// </summary>
    public class NormalizedStringTranslator : ITranslator
    {
        char[] noisyCharacters = new char[] { '&' };
        char[] controlCharacters = new char[] { };//{ '!', '@', '#', '$', '%', '^', '*', '/', '\\', ':', '<', '>', '+', '=', '{', '}', '~', '|' };

        /// <summary>
        /// Translate string from source culture to target culture
        /// </summary>
        /// <param name="sourceCultureName"></param>
        /// <param name="targetCultureName"></param>
        /// <param name="sourceText"></param>
        /// <returns></returns>
        public virtual string Translate(string sourceCultureName, string targetCultureName, string sourceText)
        {
            return NormalizeString(sourceText);
        }

        string NormalizeString(string str)
        {
            bool skip = false;
            string normalString = "";


            foreach (char c in str)
            {
                skip = false;
                foreach (char x in controlCharacters)
                {
                    if (c == x)
                    {
                        skip = true;
                        normalString += '_';
                        break;
                    }
                }
                foreach (char x in noisyCharacters)
                {
                    if (c == x)
                    {
                        skip = true;
                        break;
                    }
                }

                if (!skip) normalString += c;
            }

            return normalString.Trim('_').Trim();
        }
    }
}
