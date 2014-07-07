using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Epi.Core.AnalysisInterpreter
{
    public class EpiDataReader : System.Data.IDataReader
    {
        private System.Data.IDataReader mReader;
        private Rule_Context mContext;

        public EpiDataReader(Rule_Context Context, System.Data.IDataReader Reader)
        {
            this.mContext = Context;
            this.mReader = Reader;
        }
        
        void System.Data.IDataReader.Close()
        {
            this.mReader.Close();
        }

        int System.Data.IDataReader.Depth
        {
            get { return this.mReader.Depth; }
        }

        System.Data.DataTable System.Data.IDataReader.GetSchemaTable()
        {
            return this.mReader.GetSchemaTable();
        }

        bool System.Data.IDataReader.IsClosed
        {
            get { return this.mReader.IsClosed; }
        }

        bool System.Data.IDataReader.NextResult()
        {
            return this.mReader.NextResult();
        }

        bool System.Data.IDataReader.Read()
        {
            return this.mReader.Read();
        }

        int System.Data.IDataReader.RecordsAffected
        {
            get { return this.mReader.RecordsAffected; }
        }

        void IDisposable.Dispose()
        {
            this.mReader.Dispose();;
        }


        int System.Data.IDataRecord.FieldCount
        {
            get { return this.mReader.FieldCount; }
        }

        bool System.Data.IDataRecord.GetBoolean(int i)
        {
            return this.mReader.GetBoolean(i);
        }

        byte System.Data.IDataRecord.GetByte(int i)
        {
            return this.mReader.GetByte(i);
        }

        long System.Data.IDataRecord.GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            return this.mReader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
        }

        char System.Data.IDataRecord.GetChar(int i)
        {
            return this.mReader.GetChar(i);
        }

        long System.Data.IDataRecord.GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            return this.mReader.GetChars(i, fieldoffset, buffer, bufferoffset, length);
        }

        System.Data.IDataReader System.Data.IDataRecord.GetData(int i)
        {
            return this.mReader.GetData(i);
        }

        string System.Data.IDataRecord.GetDataTypeName(int i)
        {
            return this.mReader.GetDataTypeName(i);
        }

        DateTime System.Data.IDataRecord.GetDateTime(int i)
        {
            return this.mReader.GetDateTime(i);
        }

        decimal System.Data.IDataRecord.GetDecimal(int i)
        {
            return this.mReader.GetDecimal(i);
        }

        double System.Data.IDataRecord.GetDouble(int i)
        {
            return this.mReader.GetDouble(i);
        }

        Type System.Data.IDataRecord.GetFieldType(int i)
        {
            return this.mReader.GetFieldType(i);
        }

        float System.Data.IDataRecord.GetFloat(int i)
        {
            return this.mReader.GetFloat(i);
        }

        Guid System.Data.IDataRecord.GetGuid(int i)
        {
            return this.mReader.GetGuid(i);
        }

        short System.Data.IDataRecord.GetInt16(int i)
        {
            return this.mReader.GetInt16(i);
        }

        int System.Data.IDataRecord.GetInt32(int i)
        {
            return this.mReader.GetInt32(i);
        }

        long System.Data.IDataRecord.GetInt64(int i)
        {
            return this.mReader.GetInt64(i);
        }

        string System.Data.IDataRecord.GetName(int i)
        {
            return this.mReader.GetName(i);
        }

        int System.Data.IDataRecord.GetOrdinal(string name)
        {
            return this.mReader.GetOrdinal(name);
        }

        string System.Data.IDataRecord.GetString(int i)
        {
            return this.mReader.GetString(i);
        }

        object System.Data.IDataRecord.GetValue(int i)
        {
            return this.mReader.GetValue(i);
        }

        int System.Data.IDataRecord.GetValues(object[] values)
        {
            return this.mReader.GetValues(values);
        }

        bool System.Data.IDataRecord.IsDBNull(int i)
        {
            return this.mReader.IsDBNull(i);
        }

        object System.Data.IDataRecord.this[string name]
        {
            get {

                if (this.mContext != null)
                {
                    if (this.mContext.VariableExpressionList.ContainsKey(name))
                    {
                        return this.mContext.VariableExpressionList[name].Execute();
                    }
                    else
                        if (this.mContext.VariableValueList.ContainsKey(name))
                        {

                            return this.mContext.VariableValueList[name];

                            /*
                            IVariable result = null;
                            result = (IVariable) this.mContext.GetVariable(name);
                            return result.Expression; */

                        }
                        else
                        {
                            return this.mReader[name];
                        }
                }
                else
                {
                    return this.mReader[name];
                }
            }
            /*
            set
            {

            }*/
        }

        object System.Data.IDataRecord.this[int i]
        {
            get { return this.mReader[i]; }
        }

        
    }
}
