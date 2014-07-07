using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Epi;
using Epi.Core;
using Epi.Data;

namespace EpiDashboard
{
    public class RelatedConnection
    {
        public RelatedConnection(string tableName, IDbDriver db, string parentKey, string childKey, bool useUnmatched, bool sameDataSource)
        {
            this.view = null;
            this.db = db;
            this.TableName = tableName;
            this.ConnectionName = "";
            this.ChildKeyField = childKey;
            this.ParentKeyField = parentKey;
            this.UseUnmatched = useUnmatched;
            this.SameDataSource = sameDataSource;
        }

        public RelatedConnection(View view, IDbDriver db, string parentKey, string childKey, bool useUnmatched, bool sameDataSource)
        {
            this.view = view;
            this.db = db;
            this.TableName = "";
            this.ConnectionName = "";
            this.ChildKeyField = childKey;
            this.ParentKeyField = parentKey;
            this.UseUnmatched = useUnmatched;
            this.SameDataSource = sameDataSource;
        }

        public View view;
        public IDbDriver db;
        public string TableName;
        public string ConnectionName;
        public string ChildKeyField;
        public string ParentKeyField;
        public bool UseUnmatched = true;
        public bool SameDataSource = false;

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
