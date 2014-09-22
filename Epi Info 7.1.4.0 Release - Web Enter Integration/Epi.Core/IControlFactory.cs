using System;
namespace Epi.Core
{
    /// <summary>
    /// IControl Factory
    /// </summary>
    public interface IControlFactory
    {
        /// <summary>
        /// GetAssociatedControls
        /// </summary>
        /// <param name="field">field</param>
        /// <returns>List of controls</returns>
        System.Collections.Generic.List<System.Windows.Forms.Control> GetAssociatedControls(Epi.Fields.Field field);

        /// <summary>
        /// GetAssociatedField
        /// </summary>
        /// <param name="control">control</param>
        /// <returns>Field</returns>
        Epi.Fields.Field GetAssociatedField(System.Windows.Forms.Control control);

        /// <summary>
        /// GetFieldControls
        /// </summary>
        /// <param name="field">field</param>
        /// <param name="canvasSize">canvas size</param>
        /// <returns>List of controls</returns>
        System.Collections.Generic.List<System.Windows.Forms.Control> GetFieldControls(Epi.Fields.Field field, System.Drawing.Size canvasSize);

        /// <summary>
        /// GetFieldGroupControl
        /// </summary>
        /// <param name="group">group</param>
        /// <param name="canvasSize">canvas size</param>
        /// <returns>Control</returns>
  //    System.Windows.Forms.Control GetFieldGroupControl(Epi.Fields.GroupField group, System.Drawing.Size canvasSize);

        /// <summary>
        /// GetPageControls
        /// </summary>
        /// <param name="page">page</param>
        /// <param name="canvasSize">canvas size</param>
        /// <returns>List of controls</returns>
        System.Collections.Generic.List<System.Windows.Forms.Control> GetPageControls(Epi.Page page, System.Drawing.Size canvasSize);


    }
}
