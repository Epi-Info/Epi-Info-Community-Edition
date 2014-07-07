#region Namespaces

using System;
using System.Data;
using System.Xml;
using Epi;
using Epi.Fields;
using System.Drawing;
using System.ComponentModel;

#endregion  //Namespaces

namespace Epi.Fields
{
	/// <summary>
	/// Single line text field
	/// </summary>
    public class SingleLineTextField : TextField
    {
        #region Private Data Members

        private XmlElement viewElement;
        private XmlNode fieldNode;
        private System.ComponentModel.BackgroundWorker _updater = null;
        private System.ComponentModel.BackgroundWorker _inserter = null;

        #endregion  //Private Data Members

        #region Constructors


        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="page">The page this field belongs to</param>
        public SingleLineTextField(Page page)
            : base(page)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">The page this field belongs to</param>
        /// <param name="viewElement">Xml view element</param>
        public SingleLineTextField(Page page, XmlElement viewElement)
            : base(page)
        {
            this.viewElement = viewElement;
            this.Page = page;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">View</param>
        public SingleLineTextField(View view)
            : base(view)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">View</param>
        /// <param name="fieldNode">Xml field node</param>
        public SingleLineTextField(View view, XmlNode fieldNode)
            : base(view)
        {
            this.fieldNode = fieldNode;
            this.view.Project.Metadata.GetFieldData(this, fieldNode);           
        }

        #endregion Constructors

        #region Public Methods

        public SingleLineTextField Clone()
        {
            SingleLineTextField clone = (SingleLineTextField) this.MemberwiseClone();
            base.AssignMembers(clone);
            return clone;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Inserts the field to the database
        /// </summary>
        protected override void InsertField()
        {
            this.Id = GetMetadata().CreateField(this);
            base.OnFieldAdded();
        }


        /// <summary>
        /// Update the field to the database
        /// </summary>
        protected override void UpdateField()
        {
            GetMetadata().UpdateField(this);
        }

        #endregion

        #region Event Handlers
        #endregion

        #region Public Properties

        /// <summary>
        /// The view element of the field
        /// </summary>
        public XmlElement ViewElement
        {
            get
            {
                return viewElement;
            }
            set
            {
                viewElement = value;
            }
        }

        #endregion  //Public Properties



    }
}
