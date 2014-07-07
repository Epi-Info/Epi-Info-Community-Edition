using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Epi.Data.Services;

namespace Epi.Data
{
    public class OptionNameProvider
    {
        //Dictionary<int, string> _nameIndexColl;
        Dictionary<string, Dictionary<int, string>> _optionFieldsColl;

        public OptionNameProvider(IMetadataProvider metadata, string identifier)
        {
            View view = metadata.GetViewByFullName(identifier);
            DataTable metaFields = metadata.GetFieldsAsDataTable(view);

            _optionFieldsColl = new Dictionary<string, Dictionary<int, string>>();

            string filter = "FieldTypeId = 12";
            DataRow[] rows = metaFields.Select(filter);

            foreach (DataRow row in rows)
            {
                Dictionary<int, string> nameIndexColl = new Dictionary<int,string>();
                string[] options = row["List"].ToString().Split(new char[] { ',' });

                int index = 0;
                foreach (string option in options)
                {
                    nameIndexColl.Add(index, option);
                    index++;
                }

                
                _optionFieldsColl.Add(row["Name"].ToString(), nameIndexColl);
            }
        }

        public string Name(string optionFieldName, int index)
        {
            string name = null;

            if (_optionFieldsColl.Keys.Contains(optionFieldName))
            {
                Dictionary<int, string> nameIndexColl = _optionFieldsColl[optionFieldName];

                if (nameIndexColl.Keys.Contains(index))
                {
                    if (nameIndexColl[index].IndexOf("||") > -1)
                    {
                        name = nameIndexColl[index].Substring(0, nameIndexColl[index].LastIndexOf("||"));
                    }
                    else
                    {
                        name = nameIndexColl[index];
                    }
                    
                }
            }
            return name;
        }
    }
}
