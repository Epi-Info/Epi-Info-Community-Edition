using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Epi;
using Epi.Core;
using Epi.Data;

namespace EpiDashboard
{
    public class MergeConnection
    {
        public MergeConnection(string tableName, IDbDriver db, string parentKey, string childKey)
        {
            this.view = null;
            this.db = db;
            this.TableName = tableName;
            this.ConnectionName = "";
            this.ChildKeyField = childKey;
            this.ParentKeyField = parentKey;
        }

        public MergeConnection(View view, IDbDriver db, string parentKey, string childKey)
        {
            this.view = view;
            this.db = db;
            this.TableName = "";
            this.ConnectionName = "";
            this.ChildKeyField = childKey;
            this.ParentKeyField = parentKey;
        }

        public View view;
        public IDbDriver db;
        public string TableName;
        public string ConnectionName;
        public string ChildKeyField;
        public string ParentKeyField;

        /// <summary>
        /// Gets whether or not this is an Epi Info 7 project
        /// </summary>
        public bool IsEpiInfoProject
        {
            get
            {
                if (this.view == null)
                {
                    return false;
                }
                return true;
            }
        }
    }
}
