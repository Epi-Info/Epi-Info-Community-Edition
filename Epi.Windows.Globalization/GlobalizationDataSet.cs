using System.Data;
using System;

namespace Epi.Windows.Globalization
{

	partial class GlobalizationDataSet
	{
		partial class CulturalResourcesDataTable
		{

			/// <summary>
			/// SelectFilter method
			/// </summary>
			/// <param name="expression"></param>
			/// <returns></returns>
			public GlobalizationDataSet.CulturalResourcesDataTable SelectFilter(string expression)
			{
				GlobalizationDataSet.CulturalResourcesDataTable dt = new GlobalizationDataSet.CulturalResourcesDataTable();

				foreach (DataRow dr in this.Select(expression))
				{
					dt.Rows.Add(dr.ItemArray);
				}

				return dt;
			}

			/// <summary>
			/// SelectDistinct() method
			/// </summary>
			/// <param name="FieldName"></param>
			/// <returns></returns>
			public GlobalizationDataSet.CulturalResourcesDataTable SelectDistinct(string FieldName)
			{
				GlobalizationDataSet.CulturalResourcesDataTable dt = new GlobalizationDataSet.CulturalResourcesDataTable();

				object LastValue = null;
				foreach (DataRow dr in this.Select("", FieldName))
				{
					if (LastValue == null || !(ColumnEqual(LastValue, dr[FieldName])))
					{
						LastValue = dr[FieldName];
						dt.Rows.Add(dr.ItemArray);
					}
				}

				return dt;
			}

			private bool ColumnEqual(object A, object B)
			{

				// Compares two values to see if they are equal. Also compares DBNULL.Value.
				// Note: If your DataTable contains object fields, then you must extend this
				// function to handle them in a meaningful way if you intend to group on them.

				if (A == DBNull.Value && B == DBNull.Value) //  both are DBNull.Value
					return true;
				if (A == DBNull.Value || B == DBNull.Value) //  only one is DBNull.Value
					return false;
				return (A.Equals(B));  // value type standard comparison
			}

		}
	}
}
