using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Epi.Data;

namespace Epi.ImportExport.Filters
{
    /// <summary>
    /// A class used to manage row filtering, primary for use in creating data packages or otherwise
    /// exporting data from Epi Info 7 projects.
    /// </summary>
    public class RowFilters : IEnumerable<IRowFilterCondition>
    {
        #region Private Members
        private List<IRowFilterCondition> rowFilterConditions;
        #endregion // Private Members

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataDriver">The data driver to attach to this object</param>
        public RowFilters(IDbDriver dataDriver)
        {
            DataDriver = dataDriver;
            rowFilterConditions = new List<IRowFilterCondition>();
        }
        #endregion // Constructors

        #region Public Properties
        /// <summary>
        /// Gets/sets the data driver attached to this row filter object.
        /// </summary>
        private IDbDriver DataDriver { get; set; }        

        public string WhereClause
        {
            get
            {
                WordBuilder wb = new WordBuilder(" AND ");
                foreach (IRowFilterCondition rfc in this)
                {
                    wb.Add(rfc.Sql);
                }
                return wb.ToString();
            }
        }
        #endregion // Public Properties

        #region Public Methods

        /// <summary>
        /// Generates a Query object that can be executed to return only the GlobalRecordId values matching the filter conditions. This method can only be used for Epi Info 7 forms.
        /// </summary>
        /// <param name="form">The form to process</param>
        /// <returns>Query</returns>
        public Query GetGuidSelectQuery(View form)
        {
            if (DataDriver == null)
            {
                throw new NullReferenceException();
            }
            if (form == null)
            {
                throw new ArgumentNullException("form");
            }

            string baseTableName = "t";

            string fromClause = form.FromViewSQL;
            WordBuilder filterSql = new WordBuilder(" AND ");
            WordBuilder columns = new WordBuilder(", ");

            columns.Append("[" + baseTableName + "].[GlobalRecordId]");
            columns.Append("[" + baseTableName + "].[FKEY]");
            columns.Append("[" + baseTableName + "].[RECSTATUS]");

            if (DataDriver.ColumnExists(baseTableName, "FirstSaveLogonName"))
            {
                columns.Append("[" + baseTableName + "].[FirstSaveLogonName]");
                columns.Append("[" + baseTableName + "].[FirstSaveTime]");
                columns.Append("[" + baseTableName + "].[LastSaveLogonName]");
                columns.Append("[" + baseTableName + "].[LastSaveTime]");
            }

            string fullSql = "SELECT " + columns.ToString() + " " + fromClause + " WHERE [" + baseTableName + "].[RECSTATUS] = 1 ";
            filterSql.Append(fullSql);

            foreach (IRowFilterCondition rowFc in rowFilterConditions)
            {
                filterSql.Append(rowFc.Sql);
            }

            Query selectQuery = DataDriver.CreateQuery(filterSql.ToString());

            foreach (IRowFilterCondition rowFc in rowFilterConditions)
            {
                selectQuery.Parameters.Add(rowFc.Parameter);
            }

            return selectQuery;            
        }

        /// <summary>
        /// Adds a new condition to the filter
        /// </summary>
        /// <param name="newCondition">The condition to be added</param>
        public void Add(IRowFilterCondition newCondition)
        {
            if (this.Contains(newCondition.Description))
            {
                throw new InvalidOperationException("This item has already been added.");
            }
            else
            {
                rowFilterConditions.Add(newCondition);
            }
        }

        /// <summary>
        /// Removes a condition from the filter
        /// </summary>
        /// <param name="condition">The condition to be removed</param>
        public void Remove(IRowFilterCondition condition)
        {
            if (this.Contains(condition.Description))
            {
                rowFilterConditions.Remove(condition);
            }
            else
            {
                throw new KeyNotFoundException("The condition '" + condition.Description + "' could not be found.");
            }
        }        

        /// <summary>
        /// Clears all conditions from the filter
        /// </summary>
        public void Clear()
        {
            this.rowFilterConditions.Clear();
        }

        /// <summary>
        /// Determines whether an IRowFilterCondition is in the list of conditions, by its description
        /// </summary>
        /// <param name="description">The description of the condition to be searched for</param>
        /// <returns>bool; represents whether the condition is contained in the filter</returns>
        public bool Contains(string description)
        {
            foreach (IRowFilterCondition rowFc in this.rowFilterConditions)
            {
                if (rowFc.Description.Equals(description))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        #endregion // Public Methods

        #region Private Methods
        #endregion // Private Methods

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<IRowFilterCondition> GetEnumerator()
        {
            return new RowFiltersEnumerator(rowFilterConditions);
        }

        public void Dispose() { }

        #endregion // IEnumerable Members

        #region Classes
        /// <summary>
        /// Enumerator class for the data filter class
        /// </summary>
        protected class RowFiltersEnumerator : IEnumerator<IRowFilterCondition>
        {
            #region Private Members
            private List<IRowFilterCondition> rowFilterConditions;
            private int currentIndex;
            #endregion Private Members

            public RowFiltersEnumerator(List<IRowFilterCondition> rowFilterConditions)
            {
                this.rowFilterConditions = rowFilterConditions;
                Reset();
            }


            public void Reset()
            {
                currentIndex = -1;
            }

            public IRowFilterCondition Current
            {
                get
                {
                    return rowFilterConditions[currentIndex];
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            public bool MoveNext()
            {
                currentIndex++;
                return currentIndex < rowFilterConditions.Count;
            }

            public void Dispose() { }
        }

        #endregion // Classes
    }
}
