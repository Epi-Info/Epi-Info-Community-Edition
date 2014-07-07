using System;
using System.Data;

using Epi;
using Epi.Data.Services;

namespace Epi.Fields
{
    /// <summary>
    /// Represents the interface for a field in a view.
    /// </summary>
	public interface IField : INamedObject
	{
        /// <summary>
        /// Gets the type of the field.
        /// </summary>
        MetaFieldType FieldType { get;}
        /// <summary>
        /// Loads a field object from the database row.
        /// </summary>
        /// <param name="row"></param>
        void LoadFromRow(DataRow row);

        /// <summary>
        /// Returns the parent view
        /// </summary>
        /// <returns></returns>
        View GetView();

        /// <summary>
        /// Returns the project this fields belongs to.
        /// </summary>
        /// <returns></returns>
        Project GetProject();

        /// <summary>
        /// Returns metadata provider this field belongs to.
        /// </summary>
        /// <returns></returns>
        IMetadataProvider GetMetadata();
	}
}