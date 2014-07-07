using System;
using System.Drawing;

namespace Epi.Web
{
    /// <summary>
    /// Hypter Text Markup Language static helper class
    /// </summary>
    public static class HTML
    {
        #region Public Interface
        #region Constructors
        //
        #endregion Constructors

        #region Public Enums and Constants
        //
        #endregion Public Enums and Constants

        #region Public Properties
        //
        #endregion Public Properties

        #region Public Methods
        /// <summary>
        /// Returns an HTML Hyperlink.
        /// </summary>
        /// <param name="text">Text to display/markup.</param>
        /// <param name="href">Hyperlink reference.</param>
        /// <returns>Well-formed anchor hyperlink reference tag.</returns>
        public static string HyperLink(string text, string href)
        {
            //<a href='href'>text</a>
            return Tag(text, "a", "href", href);
        }

        /// <summary>
        /// Returns an HTML anchor with name.
        /// </summary>
        /// <remarks>
        /// this function prepends the required '#'
        /// </remarks>
        /// <param name="name">Name of anchor.</param>
        /// <returns>Well-formed anchor tag.</returns>
        public static string Anchor(string name)
        {
            //<a name="#name"></a>
            if (name.StartsWith("#"))
                return Tag(string.Empty, "a", "name", name);
            else
                return Tag(string.Empty, "a", "name", "#" + name);
        }

        /// <summary>
        /// Returns an HTML Font tag with optional color and size attributes.
        /// </summary>
        /// <param name="text">Text to display/markup.</param>
        /// <param name="color">Color name/number.</param>
        /// <param name="size">Size of font.</param>
        /// <returns>Well-formed Font tag.</returns>
        public static string Font(string text, string color, int size)
        {
            //<font color="Black" size="-1">text</font>
            return Tag(text, "font", "color", color, "size", size.ToString());
        }

        ///// <summary>
        ///// Returns an HTML Font tag with optional color and size attributes.
        ///// </summary>
        ///// <param name="text">Text to display/markup.</param>
        ///// <param name="color">Color name/number.</param>
        ///// <param name="size">Size of font.</param>
        ///// <returns>Well-formed Font tag.</returns>
        //public static string Font(string text, Color color, int size)
        //{
        //    //<font color="Azure" size="+3">text</font>
        //    return Font(text, color.Name, size);
        //}

        /// <summary>
        /// Returns an H1 heading.
        /// </summary>
        /// <param name="text">Text to display.</param>
        /// <returns>Well-formed Header #1 tag.</returns>
        public static string H1(string text)
        {
            return Tag(text, "h1");
        }

        /// <summary>
        /// Returns an H3-type heading with a horizontal line.
        /// </summary>
        /// <param name="text">Text to display/markup.</param>
        /// <returns>Well-formed Header #3 tag.</returns>
        public static string H3(string text)
        {
            return Tag(text, "h3") + Tag("hr");
        }

        /// <summary>
        /// Returns an H4-type heading with a horizontal line.
        /// </summary>
        /// <param name="text">Text to display/markup.</param>
        /// <returns>Well-formed Header #4 tag.</returns>
        public static string H4(string text)
        {
            return Tag(text, "h4") + Tag("hr");
        }

        /// <summary>
        /// Returns bold face text.
        /// </summary>
        /// <param name="text">Text to display/markup.</param>
        /// <returns>Well-formed Bold tag.</returns>
        public static string Bold(string text)
        {
            string tag = Tag(text, "b");
            return tag;
        }

        /// <summary>
        /// Returns italicized face text.
        /// </summary>
        /// <param name="text">Text to display/markup.</param>
        /// <returns>Well-formed Italics tag.</returns>
        public static string Italics(string text)
        {
            return Tag(text, "i");
        }

        /// <summary>
        /// Returns underlined text.
        /// </summary>
        /// <param name="text">Text to display/markup.</param>
        /// <returns>Well-formed Underline tag.</returns>
        public static string Underline(string text)
        {
            return Tag(text, "u");
        }

        /// <summary>
        /// Returns HTML table text.
        /// </summary>
        /// <param name="text">Text to display/markup.</param>
        /// <param name="attributeName">Name of tag property.</param>
        /// <param name="attributeValue">Value of tag property name.</param>
        /// <returns>Well-formed HTML Table Tag with attribute.</returns>
        public static string Table(string text, string attributeName, string attributeValue)
        {
            return Tag(text, "table", attributeName, attributeValue);
        }

        /// <summary>
        /// Returns HTML table text.
        /// </summary>
        /// <param name="text">Text to display/markup.</param>
        /// <returns>Well-formed HTML Table Tag with attribute.</returns>
        public static string Table(string text)
        {
            return Tag(text, "table");
        }

        /// <summary>
        /// Returns HTML TableHeader text.
        /// </summary>
        /// <param name="text">Text to display/markup.</param>
        /// <param name="attributeName">Name of tag property.</param>
        /// <param name="attributeValue">Value of tag property name.</param>
        /// <returns>Well-formed HTML TableHeader Tag with attribute.</returns>
        public static string TableHeader(string text, string attributeName, string attributeValue)
        {
            return Tag(text, "th", attributeName, attributeValue);
        }

        /// <summary>
        /// Returns HTML TableHeader text.
        /// </summary>
        /// <param name="text">Text to display/markup.</param>
        /// <returns>Well-formed HTML TableHeader Tag with attribute.</returns>
        public static string TableHeader(string text)
        {
            return Tag(text, "th");
        }

        /// <summary>
        /// Returns HTML TableRow text.
        /// </summary>
        /// <param name="text">TableCells to display/markup.</param>
        /// <param name="attributeName">Name of tag property.</param>
        /// <param name="attributeValue">Value of tag property name.</param>
        /// <returns>Well-formed HTML TableRow Tag with attribute.</returns>
        public static string TableRow(string text, string attributeName, string attributeValue)
        {
            return Tag(text, "tr", attributeName, attributeValue);
        }

        /// <summary>
        /// Returns HTML TableRow text.
        /// </summary>
        /// <param name="text">TableCells to display/markup.</param>
        /// <returns>Well-formed HTML TableRow Tag with attribute.</returns>
        public static string TableRow(string text)
        {
            //<tr>text</tr>
            return Tag(text, "tr");
        }

        /// <summary>
        /// Returns HTML TableCell text.
        /// </summary>
        /// <param name="text">Text to display/markup.</param>
        /// <param name="attributeName">Name of tag property.</param>
        /// <param name="attributeValue">Value of tag property name.</param>
        /// <returns>Well-formed HTML TableCell Tag with attribute.</returns>
        public static string TableCell(string text, string attributeName, string attributeValue)
        {
            //<td attributeName=attributeVale>text</td>
            return Tag(text, "td", attributeName, attributeValue);
        }

        /// <summary>
        /// Returns HTML TableCell text.
        /// </summary>
        /// <param name="text">Text to display/markup.</param>
        /// <returns>Well-formed HTML TableCell Tag with attribute.</returns>
        public static string TableCell(string text)
        {
            //<td>text</td>
            return Tag(text, "td");
        }

        /// <summary>
        /// Creates a basic html page with well-formed html, head, title, and body tags
        /// </summary>
        /// <param name="title">Page title.</param>
        /// <param name="bgColor">Body background color.</param>
        /// <param name="bodyContents">Inner elements of body.</param>
        /// <returns>Basic HTML Page.</returns>
        public static string HTMLPage(string title, string bgColor, string bodyContents)
        {
            string titleTag = Tag(title, "title");
            string headerTag = Tag(titleTag, "header");
            string bodyTag = Tag(bodyContents, "body", "bgColor", bgColor);

            return Tag(headerTag + bodyTag, "html");
        }

        /// <summary>
        /// Create a well-formed HTML Tag with an attribute.
        /// <example>&lt;b id="myBoldText"&gt;Bold Text&lt;/b&gt;</example>
        /// </summary>
        /// <param name="text">Text to display.</param>
        /// <param name="HTMLTag">Code of HTML Tag to create</param>
        /// <param name="attributeName">Name of tag property.</param>
        /// <param name="attributeValue">Value of tag property name.</param>
        /// <returns>Well-formed HTML Tag with attribute.</returns>
        public static string Tag(string text, string HTMLTag, string attributeName, string attributeValue)
        {
            return StringLiterals.LESS_THAN + HTMLTag.ToLower() + Attribute(attributeName, attributeValue) + StringLiterals.GREATER_THAN
                + text
                + StringLiterals.LESS_THAN + StringLiterals.FORWARD_SLASH + HTMLTag.ToLower() + StringLiterals.GREATER_THAN;
        }

        /// <summary>
        /// Create a well-formed HTML Tag with an attribute.
        /// <example>&lt;b id="myBoldText"&gt;Bold Text&lt;/b&gt;</example>
        /// </summary>
        /// <param name="text">Text to display.</param>
        /// <param name="HTMLTag">Code of HTML Tag to create</param>
        /// <param name="attributeName1">Name of first tag property.</param>
        /// <param name="attributeValue1">Value of first tag property name.</param>
        /// <param name="attributeName2">Name of second tag property.</param>
        /// <param name="attributeValue2">Value of second tag property name.</param>
        /// <returns>Well-formed HTML Tag with attribute.</returns>
        public static string Tag(string text, string HTMLTag, string attributeName1, string attributeValue1, string attributeName2, string attributeValue2)
        {
            return StringLiterals.LESS_THAN + HTMLTag.ToLower()
                + Attribute(attributeName1, attributeValue1) + StringLiterals.SPACE
                + Attribute(attributeName2, attributeValue2) + StringLiterals.SPACE
                + StringLiterals.GREATER_THAN + text
                + StringLiterals.LESS_THAN + StringLiterals.FORWARD_SLASH + HTMLTag.ToLower() + StringLiterals.GREATER_THAN;
        }
        /// <summary>
        /// Create a well-formed HTML Tag without an attribute.
        /// </summary>
        /// <example>&lt;b&gt;Bold Text&lt;/b&gt;</example>
        /// <param name="text">Text to display.</param>
        /// <param name="HTMLTag">Code of HTML Tag to create</param>
        /// <returns>Well-formed HTML Tag.</returns>
        public static string Tag(string text, string HTMLTag)
        {
            string tag = Tag(text, HTMLTag, string.Empty, string.Empty);
            return tag;
        }

        /// <summary>
        /// Create a well-formed self-enclosed HTML Tag.
        /// </summary>
        /// <example>&lt;hr/&gt;</example>
        /// <param name="HTMLTag">Code of HTML Tag to create</param>
        /// <returns>Well-formed HTML Tag.</returns>
        public static string Tag(string HTMLTag)
        {
            return StringLiterals.LESS_THAN + HTMLTag.ToLower() + StringLiterals.FORWARD_SLASH + StringLiterals.GREATER_THAN;
        }
        #endregion Public Methods
        #endregion Public Interface

        #region Protected Interface
        //
        #region Protected Properties
        //
        #endregion Protected Properties

        #region Protected Methods
        //
        #endregion Protected Methods

        #region Protected Events
        //
        #endregion Protected Events
        #endregion Protected Interface

        #region Private Members

        #region Private Enums and Constants
        //
        #endregion Private Enums and Constants

        #region Private Attributes
        //
        #endregion Private Attributes

        #region Private Properties
        //
        #endregion Private Properties

        #region Private Methods
        //
        private static string Attribute(string attributeName, string attributeValue)
        {
            string attribute = string.Empty;
            if ((attributeName.Length != 0) && (attributeValue.Length != 0))
            {
                attribute = StringLiterals.SPACE + attributeName.ToLower() + StringLiterals.EQUAL + Util.InsertInDoubleQuotes(attributeValue);
            }
            return attribute;
        }
        #endregion Private Methods

        #region Private Events
        //
        #endregion Private Events
        #endregion Private Members
    }
}