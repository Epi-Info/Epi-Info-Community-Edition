using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace EpiDashboard.Mapping
{
    public class ClassRangesDictionary
    {
        private Dictionary<string, string> _dictionary;
        private List<ClassLimits> _rangeValues;

        public ClassRangesDictionary()
        {
            _dictionary = new Dictionary<string, string>();
        }

        public Dictionary<string, string> Dict
        {
            get { return _dictionary; }
        }

        internal int PropertyPanelMaxClassCount
        {
            get { return _dictionary.Count / 2; }
        }

        public List<ClassLimits> GetLimitValues()
        {
            _rangeValues = new List<ClassLimits>();

            for (int i = 0; i < PropertyPanelMaxClassCount; i++)
            {
                float start, end;
                string start_str, end_str, key_Start, key_End;

                start_str = _dictionary.Values.ToList()[i];
                end_str = _dictionary.Values.ToList()[i + PropertyPanelMaxClassCount];
                key_Start = _dictionary.Keys.ToList()[i];
                key_End = _dictionary.Keys.ToList()[i + PropertyPanelMaxClassCount];

                if (float.TryParse(start_str, out start) && float.TryParse(end_str, out end))
                {
                    _rangeValues.Add(new ClassLimits(start, end, key_Start, key_End));
                }
            }

            return _rangeValues;
        }

        /// <summary>
        /// Use the whole leg class desc tex hox to get its text    
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetWithKey(string key)
        {
            if (_dictionary.ContainsKey(key) == false)
            {
                _dictionary.Add(key, "");
            }

            return _dictionary[key];
        }

        /// <summary>
        ///  Use the numeric part of the leg class desc text hox to get its text    
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public string GetAt(int pos)
        {

            if (_dictionary.ContainsKey(ChoroplethConstants.legendTextControlPrefix + pos))
            {
                return _dictionary[ChoroplethConstants.legendTextControlPrefix + pos];
            }
            else
            {
                return "";
            }
        }

        public void Add(string key, string value)
        {
            if (_dictionary.ContainsKey(key))
            {
                _dictionary[key] = value;
            }
            else
            {
                _dictionary.Add(key, value);
            }
        }

        internal int GetClassLevelWithKey(string name)
        {
            int index = 0;

            if (_dictionary.ContainsKey(name) == false)
            {
                return -1;
            }

            foreach(string key in _dictionary.Keys)
            {
                if (key == name)
                {
                    break;
                }
                index++;
            }

            int indexOfFirstEnd = _dictionary.Count / 2;

            return index % indexOfFirstEnd;
        }

        internal ClassLimitType GetLimitTypeWithKey(string name)
        {
            int index = 0;
            ClassLimitType limitType = ClassLimitType.Indeterminate;

            if (_dictionary.ContainsKey(name) == false)
            {
                return limitType;
            }

            foreach (string key in _dictionary.Keys)
            {
                if (key == name)
                {
                    break;
                }
                index++;
            }

            if (index < PropertyPanelMaxClassCount)
            {
                return ClassLimitType.Start;
            }
            else
            {
                return ClassLimitType.End;
            }
        }

        internal void SetRangesDictionary(List<ClassLimits> limits)
        {
            foreach (ClassLimits classLimits in limits)
            {
                if (classLimits.Start != float.NaN)
                {
                    _dictionary[classLimits.Key_Start] = string.Format("{0:N2}", classLimits.Start); 
                }
                else
                {
                    _dictionary[classLimits.Key_Start] = string.Empty;
                }

                if (classLimits.End != float.NaN)
                {
                    _dictionary[classLimits.Key_End] = string.Format("{0:N2}", classLimits.End); 
                }
                else
                {
                    _dictionary[classLimits.Key_End] = string.Empty;
                }
            }
        }
    }

    enum ClassLimitType
    {
        Start,
        End,
        Indeterminate
    }

    public class ClassLimits
    {
        public float Start { get; set; }
        public float End { get; set; }
        public string Key_Start { get; set; }
        public string Key_End { get; set; }

        public ClassLimits(float start, float end, string key_Start, string key_end)
        {
            Start = start;
            End = end;
            Key_Start = key_Start;
            Key_End = key_end;
        }

        public ClassLimits(float start, float end)
        {
            Start = start;
            End = end;
        }
    }
}
