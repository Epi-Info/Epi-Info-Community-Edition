using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using Epi.Data;
using Epi.ImportExport;

namespace Epi.ImportExport.Filters
{
    /// <summary>
    /// A class used to manage row filtering in the context of import/export operations, and
    /// for which filters are applied directly to the underlying SQL. This class was designed
    /// primarily for use in supporting the creation of XML data packages.
    /// </summary>
    public class RowFilters : IEnumerable<IRowFilterCondition>
    {
        #region Private Members
        private List<IRowFilterCondition> _rowFilterConditions;
        private readonly ConditionJoinTypes _conditionJoinType = ConditionJoinTypes.And;
        #endregion // Private Members

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataDriver">The data driver to attach to this object</param>
        /// <param name="joinType">Whether to use a global AND or OR to join all the conditions</param>
        public RowFilters(IDbDriver dataDriver, ConditionJoinTypes joinType = ConditionJoinTypes.And)
        {
            // pre
            Contract.Requires(dataDriver != null);

            // post
            Contract.Ensures(DataDriver != null);
            Contract.Ensures(_rowFilterConditions != null);

            _conditionJoinType = joinType;
            DataDriver = dataDriver;
            _rowFilterConditions = new List<IRowFilterCondition>();
        }
        #endregion // Constructors

        #region Public Properties
        /// <summary>
        /// Gets/sets the data driver attached to this row filter object.
        /// </summary>
        private IDbDriver DataDriver { get; set; }

        #endregion // Public Properties

        #region Public Methods

        /// <summary>
        /// Generates a Query object that can be executed to return only the GlobalRecordId values matching the filter conditions. This method can only be used for Epi Info 7 forms.
        /// </summary>
        /// <param name="form">The form to process</param>
        /// <returns>Query</returns>
        public virtual Query GetGuidSelectQuery(View form)
        {
            // pre
            Contract.Requires(form != null);

            // post
            Contract.Ensures(Contract.Result<Query>() != null);
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<Query>().SqlStatement));

            // assumes
            Contract.Assume(DataDriver != null);

            if (DataDriver == null)
            {
                throw new InvalidOperationException();
            }
            if (form == null)
            {
                throw new ArgumentNullException("form");
            }

            string baseTableName = "t";

            string fromClause = form.FromViewSQL;

            Contract.Assert(!String.IsNullOrEmpty(fromClause));

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

            string fullSql = "SELECT " + columns.ToString() + " " + fromClause + " WHERE [" + baseTableName + "].[RECSTATUS] = 1 AND (";
            //filterSql.Append(fullSql);

            string logicalOperatorString = " AND ";
            if (_conditionJoinType == ConditionJoinTypes.Or)
            {
                logicalOperatorString = " OR ";
            }

            WordBuilder filterSql = new WordBuilder(logicalOperatorString);
            foreach (IRowFilterCondition rowFc in _rowFilterConditions)
            {
                filterSql.Append(rowFc.Sql);
            }

            Query selectQuery = DataDriver.CreateQuery(fullSql + " " + filterSql.ToString() + ")");

            Contract.Assert(selectQuery != null);
            Contract.Assert(!String.IsNullOrEmpty(selectQuery.SqlStatement));

            foreach (IRowFilterCondition rowFc in _rowFilterConditions)
            {
                selectQuery.Parameters.Add(rowFc.Parameter);
            }

            return selectQuery;
        }

        /// <summary>
        /// Adds a new condition to the filter
        /// </summary>
        /// <param name="newCondition">The condition to be added</param>
        public virtual void Add(IRowFilterCondition newCondition)
        {
            // pre
            Contract.Requires(newCondition != null);

            if (this.Contains(newCondition.Description))
            {
                throw new InvalidOperationException("This item has already been added.");
            }
            else
            {
                _rowFilterConditions.Add(newCondition);
            }
        }

        /// <summary>
        /// Removes a condition from the filter
        /// </summary>
        /// <param name="condition">The condition to be removed</param>
        public virtual void Remove(IRowFilterCondition condition)
        {
            // pre
            Contract.Requires(condition != null);

            if (this.Contains(condition.Description))
            {
                _rowFilterConditions.Remove(condition);
            }
            else
            {
                throw new KeyNotFoundException("The condition '" + condition.Description + "' could not be found.");
            }
        }

        /// <summary>
        /// Clears all conditions from the filter
        /// </summary>
        public virtual void Clear()
        {
            this._rowFilterConditions.Clear();
        }

        /// <summary>
        /// Determines whether an IRowFilterCondition is in the list of conditions, by its description
        /// </summary>
        /// <param name="description">The description of the condition to be searched for</param>
        /// <returns>bool; represents whether the condition is contained in the filter</returns>
        public virtual bool Contains(string description)
        {
            // pre
            Contract.Requires(!String.IsNullOrEmpty(description));

            foreach (IRowFilterCondition rowFc in this._rowFilterConditions)
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
            return new RowFiltersEnumerator(_rowFilterConditions);
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
            private List<IRowFilterCondition> _rowFilterConditions;
            private int currentIndex;
            #endregion Private Members

            public RowFiltersEnumerator(List<IRowFilterCondition> _rowFilterConditions)
            {
                this._rowFilterConditions = _rowFilterConditions;
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
                    return _rowFilterConditions[currentIndex];
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
                return currentIndex < _rowFilterConditions.Count;
            }

            public void Dispose() { }
        }

        #endregion // Classes
    }
}
