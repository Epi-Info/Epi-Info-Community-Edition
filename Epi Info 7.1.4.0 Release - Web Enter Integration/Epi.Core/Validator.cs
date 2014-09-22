using System;
using System.Collections.Generic;
using System.Text;

namespace Epi
{
    /// <summary>
    /// Provides all kinds of validations
    /// </summary>
    public static class Validator
    {
        /// <summary>
        /// validates a given database name
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="errorMessages"></param>
        /// <returns></returns>
        public static void ValidateDatabaseName(string dbName, List<string> errorMessages)
        {
            if (Util.IsEmpty(dbName))
            {
                errorMessages.Add(SharedStrings.DATABASE_NAME_REQUIRED);
            }
            else
            {
                if (dbName.Length > 30)
                {
                    errorMessages.Add(SharedStrings.DATABASE_NAME_TOO_LONG);
                }

                else if (dbName.IndexOfAny(new char[] { '#', '@', '!', '%', '^', '&', '*', '.', ' ' }) >= 0)
                {
                    errorMessages.Add(SharedStrings.DATABASE_NAME_CONTAINS_INVALID_CHARACTERS);
                }
                else
                {
                    char firstChar = dbName.ToCharArray()[0];
                    if (!(Char.IsLetter(firstChar) || ((firstChar == '_') && dbName.Length > 1)))
                    {
                        errorMessages.Add(SharedStrings.DATABASE_NAME_MUST_START_WITH_A_LETTE_OR_UNDERSCORE);
                    }
                }
            }
        }
    }
}
