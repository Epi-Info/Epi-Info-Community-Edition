using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EpiDashboard.Mapping
{
    public class ChoroDictionary<TK, TV>
        where TV : new()
        where TK : new()
    {

        private Dictionary<TK, TV> tdictionary;

        public ChoroDictionary(Dictionary<TK, TV> tdict)
        {
          
              tdictionary = new Dictionary< TK  , TV>();


        }


        public Dictionary<TK, TV> Dict
        {
            get { return tdictionary; }
        }

        /// <summary>
        /// Use the whole leg class desc tex hox to get its text    
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TV GetWithKey(TK key)
        {
            if (tdictionary.ContainsKey(key) == false)
            {
                TV val = new TV();

                tdictionary.Add(key, val);
            }
            return tdictionary[key];


        }

        /// <summary>
        ///  Use the numeric part of the leg class desc text hox to get its text    
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public  TV GetAt(int pos)
        {

            string x = ChoroplethConstants.legendTextControlPrefix + pos;

            TK key = (TK) x;

            if ( tdictionary.ContainsKey(  new TK( ChoroplethConstants.legendTextControlPrefix + pos))
                return  tdictionary[ChoroplethConstants.legendTextControlPrefix + pos];
            else
                return ">" + pos;

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
