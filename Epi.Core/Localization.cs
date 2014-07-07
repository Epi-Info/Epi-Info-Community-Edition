//using System;
//using System.Data;
//using Epi.Data;
//using System.Collections;
//using System.Collections.Generic;

//namespace Epi
//{
//    /// <summary>
//    /// Summary description for Localization.
//    /// </summary>
//    public class Localization
//    {

//        #region Class Fields
//        private static DataTable translations = null;
//        private static IDictionary languageDictionary = null;
//        #endregion

       
//        #region Protected Properties

//        /// <summary>
//        /// Gets/sets the translation data table
//        /// </summary>
//        private static DataTable Translations
//        {
//            get
//            {
//                if (translations == null)
//                {
//                    translations = GetTranslationsFor(DatabaseFactory.GetConfiguredDatabase(DatabaseFactory.KnownDatabaseNames.Translation), System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
//                }
//                return translations;
//            }
//            set
//            {
//                translations = value;
//            }
//        }

//        /// <summary>
//        /// Resets the translations table
//        /// </summary>
//        private static void Reset()
//        {
//            translations = null;
//            languageDictionary = new Hashtable();
//        }

//        #endregion Protected Properties

//        #region Protected Methods
//        protected Localization()
//        {
//        }

//        static Localization()
//        {
//            languageDictionary = new System.Collections.Specialized.HybridDictionary();
//        }

//        /// <summary>
//        /// Returns a dictionary suitable for english language translation. 
//        /// </summary>
//        /// <param name="cultureName">Culture Name</param>
//        /// <returns>Newly created or cached english-to-language dictionary. 
//        /// Lower-case english term is the key.</returns>
//        private static IDictionary GetLanguageDictionary(string cultureName)
//        {
//            // lock language dictionary to prevent threading problems
//            lock (languageDictionary)
//            {
//                // if language has not be initalized, pull data and build the index
//                if (!languageDictionary.Contains(cultureName))
//                {

//                    string columnNameLanguage = GetColumnNameFor(cultureName);
//                    DataTable translations = Translations;

//                    Hashtable newLanguageDictionary = new Hashtable(translations.Rows.Count);

//                    foreach (DataRow row in translations.Rows)
//                    {
//                        string englishString = row[Localization.COLUMN_NAME_ENGLISH].ToString();
//                        string translatedString = row[columnNameLanguage].ToString();

//                        // add english string if no translation is provided
//                        string effectiveString = string.IsNullOrEmpty(translatedString) ? englishString : translatedString;

//                        // make lower cased for case insenitive indexing
//                        string indexKey = englishString.ToLowerInvariant();

//                        if (newLanguageDictionary.Contains(indexKey))
//                        { 
//                            // last definition prevails
//                            newLanguageDictionary[indexKey] = effectiveString;
//                        }
//                        else
//                        {
//                            newLanguageDictionary.Add(indexKey, effectiveString);
//                        }
//                    }
//                    languageDictionary.Add(cultureName, newLanguageDictionary);

//                    return newLanguageDictionary;
//                }
//                else
//                {
//                    return (IDictionary)languageDictionary[cultureName];
//                }
//            }
//        }

//        public static string T(string englishString)
//        {
//            return Translate(englishString);
//        }

//        /// <summary>
//        /// Translates a string based on the currently selected language.
//        /// It is OK to pass an empty string
//        /// </summary>
//        /// <param name="englishString">The string (in English) to be translated.</param>
//        /// <returns>Translated String</returns>
//        protected static string Translate(string englishString)
//        {
//            if (!string.IsNullOrEmpty(englishString))
//            {
//                string targetLanguage = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
//                if (targetLanguage != Defaults.LANGUAGE)
//                {
//                    string key = englishString.ToLowerInvariant();

//                    IDictionary dictionary = GetLanguageDictionary(targetLanguage);
//                    if (dictionary.Contains(key))
//                    {
//                        return (string)dictionary[key];
//                    }
//                }
//            }
//            // Didn't find a translation. return the English  string.
//            return englishString;
//        }

       
//        protected const string COLUMN_NAME_ENGLISH = "lang_en-US";

       
//        protected const string TRANSLATION_TABLE_NAME = "Translations";


//        /// <summary>
//        /// Gets a data table containing all the translation strings for a culture
//        /// </summary>
//        /// <param name="cultureName">Culture Name</param>
//        /// <returns>Data table containing all the translations for the module</returns>
//        protected static DataTable GetTranslationsFor(IDbDriver db, string cultureName)
//        {
//            string query = "select [" + COLUMN_NAME_ENGLISH + "] , [" + GetColumnNameFor(cultureName) + "] from " + TRANSLATION_TABLE_NAME;
//            Query translationQuery = db.CreateQuery(query);
//            DataTable dt = db.Select(translationQuery);
//            return dt;
//        }

//        ///// <summary>
//        ///// Saves the initial English values into the language database
//        ///// </summary>
//        ///// <param name="keys">Datatable containing the English keys</param>
//        //public void SaveTranslationKeys(DataTable keys)
//        //{
//        //    try
//        //    {

//        //        foreach (DataRow row in keys.Rows)
//        //        {
//        //            Query insertQuery = Db.CreateQuery("insert into Translations(" + COLUMN_NAME_ENGLISH + ") VALUES (@Value)");
//        //            insertQuery.Parameters.Add(new QueryParameter("@Value", DbType.String, row[COLUMN_NAME_ENGLISH].ToString()));
//        //            Db.ExecuteNonQuery(insertQuery);
//        //        }
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        throw new System.ApplicationException("Could not save translations", ex);
//        //    }
//        //    finally
//        //    {

//        //    }
//        //}

//        /// <summary>
//        /// Gets a DataTable containing the list of installed languages.
//        /// </summary>
//        protected static DataTable GetSupportedLanguages(IDbDriver db)
//        {
//            DataTable supportedLanguages = new DataTable();
//            supportedLanguages.Columns.Add(ColumnNames.NAME, typeof(string));
//            supportedLanguages.Columns.Add(ColumnNames.CULTURE_NAME, typeof(string));
//            DataTable schemaTable = db.GetTableColumnSchema(TRANSLATION_TABLE_NAME);
//            DataRow[] schemaRows = schemaTable.Select("COLUMN_NAME like 'lang_*'");
//            foreach (DataRow schemaRow in schemaRows)
//            {
//                string cultureName = schemaRow["COLUMN_NAME"].ToString().Remove(0, 5);
//                DataRow languageRow = supportedLanguages.NewRow();
//                languageRow[ColumnNames.CULTURE_NAME] = cultureName;
//                languageRow[ColumnNames.NAME] = new System.Globalization.CultureInfo(cultureName).NativeName;
//                supportedLanguages.Rows.Add(languageRow);
//            }
//            return (supportedLanguages);
//        }

//        /// <summary>
//        /// Returns column name for the given language.
//        /// </summary>
//        /// <param name="lang"></param>
//        /// <returns></returns>
//        protected static string GetColumnNameFor(string cultureName)
//        {
//            return "lang_" + cultureName;
//        }
//        #endregion Protected Methods

//        #region Public Methods


//        /// <summary>
//        /// Translates a given string
//        /// </summary>
//        /// <param name="str">String to translate</param>
//        /// <returns>The translated string</returns>
//        [Obsolete] 
//        public static string LocalizeString(string str)
//        {
//            throw new NotSupportedException("LocalizeString method is no longer supported.");
//        }

//        ///// <summary>
//        ///// Translates a given string
//        ///// </summary>
//        ///// <param name="str">String to translate</param>
//        ///// <returns>The translated string</returns>
//        //public static string LocalizeString(string str)
//        //{
//        //    if (!string.IsNullOrEmpty(str))
//        //    {
//        //        if (!Configuration.Settings.Language.Equals(Defaults.LANGUAGE))
//        //        {				
//        //            return Translate(str);
//        //        }
//        //    }
//        //    return str;
//        //}

//        ///// <summary>
//        ///// Returns a formated message by translating the localizable part, leaving the non-localizable part alone.
//        ///// </summary>
//        ///// <param name="LocalizablePart">The localizable part of the message</param>
//        ///// <param name="nonLocalizablePart">The non-localizable part of the message</param>
//        ///// <param name="separator">Separator to be used between the two parts of the message</param>
//        ///// <returns></returns>
//        //public static string LocalizeMessage(string localizablePart, string nonLocalizablePart, string separator)
//        //{
//        //    if (!string.IsNullOrEmpty(nonLocalizablePart))
//        //    {
//        //        return(LocalizeString(localizablePart) + separator + nonLocalizablePart);
//        //    }
//        //    else
//        //    {
//        //        return(LocalizeString(localizablePart));
//        //    }
//        //}

//        ///// <summary>
//        ///// Returns a formated message by translating the localizable part, leaving the non-localizable part alone.
//        ///// </summary>
//        ///// <param name="LocalizablePart">The localizable part of the message</param>
//        ///// <param name="nonLocalizablePart">The non-localizable part of the message</param>
//        ///// <returns></returns>
//        //public static string LocalizeMessage(string localizablePart, string nonLocalizablePart)
//        //{
//        //    const string separator = ": \n";
//        //    if (Configuration.Settings.Language != "en-US")
//        //    {
//        //        return (LocalizeMessage(localizablePart, nonLocalizablePart, separator));
//        //    }
//        //    else
//        //    {
//        //        return localizablePart + separator + nonLocalizablePart;
//        //    }
//        //}

    
//        #endregion Public Methods

//    }
//}
