using System;
namespace Epi.Data
{
    interface ITableColumn
    {
        bool AllowNull { get; set; }
        bool CascadeDelete { get; set; }
        GenericDbColumnType DataType { get; set; }
        string ForeignKeyColumnName { get; set; }
        string ForeignKeyTableName { get; set; }
        bool IsIdentity { get; set; }
        bool IsPrimaryKey { get; set; }
        int? Length { get; set; }
        string Name { get; set; }
        int? Precision { get; set; }
        int? Scale { get; set; }
    }
}
