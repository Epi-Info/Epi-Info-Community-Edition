using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;

namespace Epi.Windows.Globalization.Translators
{
    /// <summary>
    /// Translate text items using the BableFish web service 
    /// </summary>
    public class WebServiceTranslator : NormalizedStringTranslator
    {

        //Dictionary<string, string> translationTable = new Dictionary<string, string>(ds.CulturalResources.Rows.Count);
        Dictionary<string, string> translationTable = new Dictionary<string, string>(3000);

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
            string translatedText;
            // cache translated strings, weak implementation
            if (translationTable.ContainsKey(normalizedText))
            {
                translatedText = translationTable[normalizedText];
            }
            else
            {
                translatedText = TranslateResourceText(sourceCultureName, targetCultureName, sourceText, normalizedText);
                translationTable.Add(normalizedText, translatedText);
            }

            return translatedText;
        }


        string TranslateResourceText(string sourceCultureName, string targetCultureName, string resourceValue, string resourceText)
        {
            string translatedString = BabelFish(sourceCultureName, targetCultureName, resourceText);
            return translatedString;
        }

        static readonly string[] VALIDTRANSLATIONMODES = new string[] { 
        "zh_en", "zt_en", "en_zh", "en_zt", "en_nl", "en_fr", "en_de", "en_el", "en_it", "en_ja", 
        "en_ko", "en_pt", "en_ru", "en_es", "nl_en", "nl_fr", "fr_nl", "fr_en", "fr_de", "fr_el", 
        "fr_it", "fr_pt", "fr_es", "de_en", "de_fr", "el_en", "el_fr", "it_en", "it_fr", "ja_en", 
        "ko_en", "pt_en", "pt_fr", "ru_en", "es_en", "es_fr" };

        const string BABELFISHURL = "http://babelfish.altavista.com/babelfish/tr";
        const string BABELFISHREFERER = "http://babelfish.altavista.com/";
        const string ERRORSTRINGSTART = "<font color=red>";
        const string ERRORSTRINGEND = "</font>";

        /// <summary>
        /// GetSupportedTargetLanguages() method
        /// </summary>
        /// <param name="sourceCultureName"></param>
        /// <returns></returns>
        public static List<string> GetSupportedTargetLanguages(string sourceCultureName)
        {
            List<string> result = new List<string>();

            string sourceLanguage;

            int seperatorPosition = sourceCultureName.IndexOf('-');
            if (seperatorPosition > -1)
                sourceLanguage = sourceCultureName.Substring(0, seperatorPosition);
            else
                sourceLanguage = sourceCultureName;

            foreach (string language in VALIDTRANSLATIONMODES)
            {
                if (language.StartsWith(sourceLanguage))
                {
                    result.Add(language.Substring(3, 2));
                }
            }
            return result;
        }

        /// <summary>
        /// Translate using AltaVista's BabelFish Translation service
        /// </summary>
        /// <param name="sourceCultureName"></param>
        /// <param name="targetCultureName"></param>
        /// <param name="sourcedata"></param>
        /// <returns></returns>
        public static string BabelFish(string sourceCultureName, string targetCultureName, string sourcedata)
        {
            string translation;

            try
            {
                // validate and remove trailing spaces
                if (string.IsNullOrEmpty(sourceCultureName)) throw new ArgumentNullException("sourceCultureName");
                if (string.IsNullOrEmpty(targetCultureName)) throw new ArgumentNullException("targetCultureName");
                if (string.IsNullOrEmpty(sourcedata)) throw new ArgumentNullException("sourcedata");

                int hyphenPosition;

                hyphenPosition = targetCultureName.IndexOf('-');
                if (hyphenPosition > -1)
                {
                    targetCultureName = targetCultureName.Substring(0, hyphenPosition);
                }

                hyphenPosition = sourceCultureName.IndexOf('-');
                if (hyphenPosition > -1)
                {
                    sourceCultureName = sourceCultureName.Substring(0, hyphenPosition);
                }

                string translationmode = sourceCultureName + "_" + targetCultureName;


                Uri uri = new Uri(BABELFISHURL);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

                request.Referer = BABELFISHREFERER;

                // Encode all the sourcedata 
                string postsourcedata;
                postsourcedata = "lp=" + translationmode + "&tt=urltext&intl=1&doit=done&urltext=" + System.Web.HttpUtility.UrlEncode(sourcedata);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = postsourcedata.Length;
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1)";

                // new code 
                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
                //request.Proxy = System.Net.WebProxy.GetDefaultProxy();
                request.AllowAutoRedirect = true;
                request.MaximumAutomaticRedirections = 10;
                request.Timeout = (int)new TimeSpan(0, 0, 300).TotalMilliseconds;
                request.ServicePoint.ConnectionLimit = 25;

                using (Stream writeStream = request.GetRequestStream())
                {
                    UTF8Encoding encoding = new UTF8Encoding();
                    byte[] bytes = encoding.GetBytes(postsourcedata);
                    writeStream.Write(bytes, 0, bytes.Length);
                    writeStream.Close();
                }


                StringBuilder pageBuilder = new StringBuilder();

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        StreamReader readStream = new StreamReader(responseStream, Encoding.UTF8);

                        bool record = false;
                        while (true)
                        {
                            string nextline = readStream.ReadLine();
                            if (nextline == null) break;

                            if (nextline.Contains("form ")
                                && nextline.Contains("http://www.altavista.com/web/results"))
                                record = true;

                            if (record) pageBuilder.AppendLine(nextline);

                            if (nextline.Contains("</html>")) break;
                        }
                    }
                }

                Regex reg = new Regex(@"<div style=padding:10px;>((?:.|\n)*?)</div>", RegexOptions.IgnoreCase);
                MatchCollection matches = reg.Matches(pageBuilder.ToString());
                if (matches.Count != 1 || matches[0].Groups.Count != 2)
                {
                    translation = sourcedata.ToUpper();
                }
                else
                {
                    translation = matches[0].Groups[1].Value;
                }

                translation.Replace("\n", "");
                translation.Replace("\r", "");
            }

            catch (WebException wex)
            {
                System.Diagnostics.Debug.WriteLine(wex.Message);
                translation = sourcedata;
            }

            return translation;
        }
    }

}
