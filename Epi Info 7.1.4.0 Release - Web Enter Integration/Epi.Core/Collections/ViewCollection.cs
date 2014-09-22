using System;
using Epi;
using System.Collections.Generic;

namespace Epi.Collections
{
    /// <summary>
    /// Named View collection class
    /// </summary>
    public class ViewCollection : NamedObjectCollection<View>
    {
        #region Public Methods
        /// <summary>
        /// Gets a view by referencing it's Id
        /// </summary>
        /// <param name="viewId"></param>
        /// <returns></returns>
        public virtual View GetViewById(int viewId)
        {
            foreach (View view in this)
            {
                if (view.Id == viewId)
                {
                    return (view);
                }
            }
            return (null);
        }
        #endregion Public Methods
    }
}