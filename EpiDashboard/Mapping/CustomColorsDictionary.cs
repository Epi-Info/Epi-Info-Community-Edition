using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace EpiDashboard.Mapping
{
    public class CustomColorsDictionary
    {
        private Dictionary<string, Color> dictionary;

        public CustomColorsDictionary()
        {
            dictionary = new Dictionary<string, Color>();
        }

        public Dictionary<string, Color> Dict
        {
            get { return dictionary; }
        }

        /// <summary>
        /// Use the whole leg class desc tex hox to get its text    
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Color GetWithKey(string key)
        {
            if (dictionary.ContainsKey(key) == false)
                dictionary.Add(key, Colors.White);


            return dictionary[key];


        }

        /// <summary>
        ///  Use the numeric part of the leg class desc text hox to get its text    
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public Color GetAt(int pos)
        {

            if (dictionary.ContainsKey(ChoroplethConstants.legendTextControlPrefix + pos))
                return dictionary[ChoroplethConstants.legendTextControlPrefix + pos];
            else
                return Colors.White;

        }

        public void Add(string key, Color value)
        {
            if (dictionary.ContainsKey(key))
                dictionary[key] = value;
            else
                dictionary.Add(key, value);
        }

    }
}
