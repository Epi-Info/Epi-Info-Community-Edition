using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.OleDb;
using Epi.Data;


namespace Epi.Windows.Globalization.Translators
{
    /// <summary>
    /// Translates strings using legacy Epi Info 3.x translation method
    /// </summary>
    public class LegacyLanguageDbTranslator : NormalizedStringTranslator
    {

        Dictionary<string, string> translationTable = new Dictionary<string, string>(3000);

        /// <summary>
        /// Populate internal translation table using Epi 3.x db
        /// </summary>
        /// <param name="legacyLanguageDatabasePath"></param>
        public void ReadDatabase(string legacyLanguageDatabasePath)
        {
           
            //IDbDriver db = DatabaseFactory.CreateDatabaseInstanceByFileExtension(legacyLanguageDatabasePath);
            IDbDriverFactory dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(Epi.Configuration.AccessDriver);
            OleDbConnectionStringBuilder dbCnnStringBuilder = new OleDbConnectionStringBuilder();
            dbCnnStringBuilder.FileName = legacyLanguageDatabasePath;
            IDbDriver db = dbFactory.CreateDatabaseObject(dbCnnStringBuilder);
            
            List<string> tableNames = db.GetTableNames();

            foreach (string tableName in tableNames)
            {
                List<string> columnNames = db.GetTableColumnNames(tableName);
                if (columnNames.Contains("English") && columnNames.Count == 2)
                {

                    DataTable span = db.Select(db.CreateQuery ("select * from " + tableName));


                    int sourceColumnOrdinal, translationColumnOrdinal;

                    if (string.Compare(span.Columns[0].ColumnName, "English", true) == 0)
                    {
                        sourceColumnOrdinal = 0;
                        translationColumnOrdinal = 1;
                    }
                    else
                    {
                        sourceColumnOrdinal = 1;
                        translationColumnOrdinal = 0;
                    }

                    foreach (DataRow row in span.Rows)
                    {
                        AddTranslationTableEntry(sourceColumnOrdinal, translationColumnOrdinal, row);
                    }
                }
            }
        }

        void AddTranslationTableEntry(int sourceColumnOrdinal, int translationColumnOrdinal, DataRow row)
        {
            string sourceText = row[sourceColumnOrdinal].ToString();
            string translatedText = row[translationColumnOrdinal].ToString();

            if (!string.IsNullOrEmpty(translatedText.Trim()))
            {
                if (translationTable.ContainsKey(sourceText))
                {
                    // could make a value comparison here to decide which one to keep
                    // for now - first come, only served
                }
                else
                {
                    translationTable.Add(sourceText, translatedText);
                }
            }
        }

        /// <summary>
        /// Translate string from source culture to target culture. Must call ReadDatabase to populate translation table
        /// </summary>
        /// <param name="sourceCultureName"></param>
        /// <param name="targetCultureName"></param>
        /// <param name="sourceText"></param>
        /// <returns></returns>
        public override string Translate(string sourceCultureName, string targetCultureName, string sourceText)
        {
            string translatedText;

            // cache translated strings, weak implementation
            if (translationTable.ContainsKey(sourceText))
            {
                translatedText = translationTable[sourceText];
            }
            else
            {
                translatedText = base.Translate(sourceCultureName, targetCultureName, sourceText);
            }

            return translatedText;
        }

    }

  
}
