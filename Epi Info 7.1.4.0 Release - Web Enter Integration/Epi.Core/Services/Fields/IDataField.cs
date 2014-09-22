
using System.Data;
using Epi.Data;
using EpiInfo.Plugin;

namespace Epi.Fields
{
	/// <summary>
	/// IDataField Interface
	/// </summary>
	public interface IDataField : IField, IDataSourceVariable, EpiInfo.Plugin.IVariable
	{
        /// <summary>
        /// Returns a QueryParameter ready for use in an insert / update statement.
        /// </summary>
        /// <returns></returns>
        QueryParameter CurrentRecordValueAsQueryParameter{ get;}
        
        /// <summary>
        /// 
        /// </summary>
        string CurrentRecordValueString { get;set;}

        void SetNewRecordValue();
	}
}