using System;
using System.Collections.Generic;
using System.Text;

namespace Epi.Collections
{
    /// <summary>
    /// A List class that can store tags for each object in the list
    /// </summary>
    public class TaggedList<ElementType, TagType> : List<ElementType>
    {
        #region Protected Attributes
        /// <summary>
        /// List of tag types
        /// </summary>
        protected List<TagType> tags = null;
        #endregion Protected Attributes

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        public TaggedList()
        {
            tags = new List<TagType>();
        }
        #endregion Constructors

        #region Public Methods
        ///// <summary>
        ///// Add an object to the end of the list with a null tag
        ///// </summary>
        ///// <param name="obj"></param>
        ///// <returns></returns>
        //public new void Add(ElementType element)
        //{
        //    Add(element, (TagType) null);
        //}
        /// <summary>
        /// Add an item to the end of the list with the specified tag.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public void Add(ElementType element, TagType tag)
        {
            base.Add(element);
            tags.Add(tag);
        }

        /// <summary>
        /// Clears the list as well as tags
        /// </summary>
        public new void Clear()
        {
            base.Clear();
            tags.Clear();
        }

        /// <summary>
        /// Returns tag associated with the object.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public TagType GetTag(ElementType element)
        {
            int index = IndexOf(element);
            return tags[index];
        }
        
        #endregion Public Methods

    }
}