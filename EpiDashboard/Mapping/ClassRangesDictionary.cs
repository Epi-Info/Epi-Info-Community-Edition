using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace EpiDashboard.Mapping
{
    public class ClassRangesDictionary
    {

        private Dictionary<string, string> dictionary;

        public ClassRangesDictionary()
        {
            dictionary = new Dictionary<string, string>();
        }

        public Dictionary<string, string> Dict
        {
            get { return dictionary; }
        }

        /// <summary>
        /// Use the whole leg class desc tex hox to get its text    
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetWithKey(string key)
        {
            if (dictionary.ContainsKey(key) == false)
                dictionary.Add(key, "");


            return dictionary[key];


        }

        /// <summary>
        ///  Use the numeric part of the leg class desc text hox to get its text    
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public string GetAt(int pos)
        {

            if (dictionary.ContainsKey(ChoroplethConstants.legendTextControlPrefix + pos))
                return dictionary[ChoroplethConstants.legendTextControlPrefix + pos];
            else
                return "";

        }

        public void Add(string key, string value)
        {
            if (dictionary.ContainsKey(key))
                dictionary[key] = value;
            else
                dictionary.Add(key, value);
        }


    }
}
