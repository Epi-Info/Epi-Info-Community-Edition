namespace Epi.DataSets
{
	/// <summary>
	/// Class AppDataSet
	/// </summary>
	partial class AppDataSet
	{
		partial class DataPatternsDataTable
		{
			#region Public Methods

			/// <summary>
			/// Returns mask for a given patterm
			/// </summary>
			/// <param name="pattern">Specified pattern for a datetime, date, time or number</param>
			/// <returns>Returns mask for pattern</returns>
			public string GetMaskByPattern(string pattern)
			{
				foreach (DataPatternsRow row in this.Rows)
				{
					if (row.Expression.Equals(pattern))
					{
						return row["Mask"].ToString();
					}
				}
				return pattern;
			}

			public string GetFormat(string pattern)
			{
				foreach (DataPatternsRow row in this.Rows)
				{
					if (row.Expression.Equals(pattern))
					{
						return row["FormattedExpression"].ToString();
					}
				}
				return pattern;
			}

			/// <summary>
			/// Returns the expression of a given mask
			/// </summary>
			/// <param name="mask">Specified mask for a datetime, date, time or number</param>
			/// <param name="pattern">Specified pattern</param>
			/// <returns>Returns expression for item with specified mask and pattern</returns>
			public string GetExpressionByMask(string mask, string pattern)
			{
				foreach (DataPatternsRow row in this.Rows)
				{
					if (row.Mask.Equals(mask) && row.Expression.Equals(pattern))
					{
						return row["FormattedExpression"].ToString();
					}
				}
				return pattern;
			}

			/// <summary>
			/// Retrieves the default pattern's expression
			/// </summary>
			/// <param name="patternId">The pattern's id</param>
			/// <returns>Returns pattern's expression</returns>
			public string GetDefaultPattern(int patternId)
			{
				#region Input Validation
				if (patternId < 1)
				{
					throw new GeneralException("Invalid Pattern Id");
				}
				#endregion  //Input Validation

				foreach (DataPatternsRow row in this.Rows)
				{
					if (row.PatternId == patternId)
					{
						return row["Expression"].ToString();
					}
				}
				throw new GeneralException("Invalid Pattern Id");
			}

			#endregion  //Public Methods
		}

		/// <summary>
		/// Class ReservedWordsDataTable (nested)
		/// </summary>
		partial class ReservedWordsDataTable
		{
			#region Public Methods

			/// <summary>
			/// IsReservedWord()
			/// </summary>
			/// <param name="name"></param>
			/// <returns></returns>
			public bool IsReservedWord(string name)
			{
				ReservedWordsRow row = (ReservedWordsRow)this.Rows.Find(new object[] { name });
				return (row != null);
			}
			#endregion Public Methods
		}

		/// <summary>
		/// Class CommandsDataTable (nested)
		/// </summary>
		public partial class CommandsDataTable
		{
		}

		/// <summary>
		/// Class VariableScopesDataTable (nested)
		/// </summary>
		public partial class VariableScopesDataTable : global::System.Data.TypedTableBase<VariableScopesRow>
		{

			#region public methods

			/// <summary>
			/// Given the Scope ID returns the Scope Name
			/// </summary>
			/// <param name="scopeId">The enumerated scope identifier</param>
			/// <returns>A string with the name of the sope</returns>
			/// 
			public string GetVariableScopeNameById(int scopeId)
			{
				VariableScopesRow row = this.FindById((short)scopeId);
				return (row == null) ? string.Empty : row.Name;
			}
			#endregion

			#region IEnumerable Members

			public System.Collections.IEnumerator GetEnumerator()
			{
				throw new System.NotImplementedException();
			}

			#endregion
		}

		/// <summary>
		/// Class DataTypesDataTable (nested)
		/// </summary>
		public partial class DataTypesDataTable : global::System.Data.TypedTableBase<DataTypesRow>
		{
			#region public methods
			/// <summary>
			/// Given the DataType ID returns the SDataType Name
			/// </summary>
			/// <param name="id">The enumerated DataType identifier</param>
			/// <returns>A string with the name of the DataType</returns>
			/// 
			public string GetDataTypeNameById(int id)
			{
				return this.FindByDataTypeId(id).Name;
			}
			#endregion

			#region IEnumerable Members

			public System.Collections.IEnumerator GetEnumerator()
			{
				throw new System.NotImplementedException();
			}

			#endregion
		}

		/// <summary>
		/// Class FieldTypesDataTable (nested)
		/// </summary>
		public partial class FieldTypesDataTable : global::System.Data.TypedTableBase<FieldTypesRow>
		{

			#region public methods
			/// <summary>
			/// Given the DataType ID returns the SDataType Name
			/// </summary>
			/// <param name="id">The enumerated DataType identifier</param>
			/// <returns>A string with the name of the DataType</returns>
			/// 
			public string GetFieldTypeNameById(int id)
			{
				return this.FindByFieldTypeId(id).Name.ToString();
			}

			/// <summary>
			/// GetDataTypeByFieldTypeId()
			/// </summary>
			/// <param name="id"></param>
			/// <returns>int</returns>
			public int GetDataTypeByFieldTypeId(int id)
			{
				return this.FindByFieldTypeId(id).DataTypeId;
			}

			/// <summary>
			/// GetFieldTypeByDataTypeId()
			/// </summary>
			/// <param name="id"></param>
			/// <returns></returns>
			public int GetFieldTypeByDataTypeId(int id)
			{
				int fieldType = 99;
				foreach (FieldTypesRow row in this.Rows)
				{
					fieldType = (int)row[ColumnNames.DATA_TYPE];
					if (fieldType == id)
					{
						return fieldType;
					}
				}
				return fieldType;
			}

			/// <summary>
			/// Getss the default pattern id of a field type
			/// </summary>
			/// <param name="id">The field id</param>
			/// <returns>Returns default patter id</returns>
			public int GetPatternIdByFieldId(int id)
			{
				#region Input Validation
				if (id < 1)
				{
					throw new GeneralException("Invalid Id");
				}
				#endregion  //Input Validation


				foreach (FieldTypesRow row in this.Rows)
				{
					if (row.FieldTypeId == id)
					{
						return int.Parse(row["DefaultPatternId"].ToString());
					}
				}
				throw new GeneralException("Invalid Field Id");

			}

			#endregion  //Public Methods

			#region IEnumerable Members

			public System.Collections.IEnumerator GetEnumerator()
			{
				throw new System.NotImplementedException();
			}

			#endregion
		}


	}
}
