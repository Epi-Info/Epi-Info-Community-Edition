using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace EpiDashboard.Mapping
{
    public class ClassRangeDictionary
    {
        private Dictionary<string, string> _dictionary;
        private List<ClassLimits> _rangeValues;

        public ClassRangeDictionary()
        {
            _dictionary = new Dictionary<string, string>();
        }

        public Dictionary<string, string> RangeDictionary
        {
            get { return _dictionary; }
        }

        public List<double> RangeStarts
        {
            get 
            {
                List<double> starts = new List<double>();

                for (int classOrdinal = 1; classOrdinal < 11; classOrdinal++)
                {
                    float start;
                    string start_str, key_Start;

                    key_Start = "rampStart" + string.Format("{0:D2}", classOrdinal);
                    start_str = GetAt(key_Start);

                    if (float.TryParse(start_str, out start))
                    {
                        starts.Add(start);
                    }
                    else
                    {
                        return starts;
                    }
                }

                return starts; 
            }
        }

        internal int PropertyPanelMaxClassCount
        {
            get { return _dictionary.Count / 2; }
        }

        public List<ClassLimits> GetLimitValues()
        {
            _rangeValues = new List<ClassLimits>();
            
            for (int classOrdinal = 1; classOrdinal < 11; classOrdinal++)
            {
                float start, end;
                string start_str, end_str, key_Start, key_End;

                key_Start = "rampStart" + string.Format("{0:D2}", classOrdinal);
                key_End = "rampEnd" + string.Format("{0:D2}", classOrdinal);

                start_str = GetAt(key_Start);
                end_str = GetAt(key_End);

                if (float.TryParse(start_str, out start) && float.TryParse(end_str, out end))
                {
                    _rangeValues.Add(new ClassLimits(start, end, key_Start, key_End));
                }
                else
                {
                    return _rangeValues;
                }
            }

            return _rangeValues;
        }

        public string GetAt(string key)
        {
            if (_dictionary.ContainsKey(key) == false)
            {
                _dictionary.Add(key, "");
            }

            return _dictionary[key];
        }

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
            int result = -1;
            
            if (_dictionary.ContainsKey(name) == false)
            {
                return result;
            }

            string parse = name.Replace("rampStart", "").Replace("rampEnd", "");

            int.TryParse(parse, out result);

            return result - 1;
        }

        internal ClassLimitType GetLimitTypeWithKey(string name)
        {
            ClassLimitType limitType = ClassLimitType.Indeterminate;

            if (_dictionary.ContainsKey(name) == false)
            {
                return limitType;
            }

            if (name.Contains("Start"))
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

        internal void SetRangesDictionary(string[,] rangeValues)
        {
            int classOrdinal = 1;
            string limitType = "rampStart";
            string compound = string.Empty;
            _dictionary.Clear();

            foreach (string value in rangeValues)
            {
                if (value == null) break;
                
                compound = limitType + string.Format("{0:D2}", classOrdinal);
                _dictionary.Add(compound, value);

                if (limitType == "rampStart")
                {
                    limitType = "rampEnd";
                }
                else
                {
                    limitType = "rampStart";
                    classOrdinal++;
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
