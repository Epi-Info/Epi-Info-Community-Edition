using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Epi.Analysis
{
    /// <summary>
    /// Header Level Class
    /// </summary>
    public class HeaderLevel
    {
        private int levelNumber;
        private string text;
        private string color;
        private string size;
        private bool shouldAppend;
        private bool shouldBold;
        private bool shouldItalicize;
        private bool shouldUnderline;
        private string cssClass = String.Empty;
        private string cssStyle = String.Empty;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="levelNumber">Integer Level Number</param>
        public HeaderLevel(int levelNumber)
        {
            this.LevelNumber = levelNumber;
            this.Reset();
        }

        /// <summary>
        /// Level Number
        /// </summary>
        public int LevelNumber
        {
            get { return levelNumber; }
            set { levelNumber = value; }
        }

        /// <summary>
        /// Text
        /// </summary>
        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        /// <summary>
        /// Color
        /// </summary>
        public string Color
        {
            get { return color; }
            set { color = value; }
        }

        /// <summary>
        /// Size
        /// </summary>
        public string Size
        {
            get { return size; }
            set { size = value; }
        }

        /// <summary>
        /// Should Append ?
        /// </summary>
        public bool ShouldAppend
        {
            get { return shouldAppend; }
            set { shouldAppend = value; }
        }

        /// <summary>
        /// Should Bold ?
        /// </summary>
        public bool ShouldBold
        {
            get { return shouldBold; }
            set { shouldBold = value; }
        }

        /// <summary>
        /// Should Italicize ?
        /// </summary>
        public bool ShouldItalicize
        {
            get { return shouldItalicize; }
            set { shouldItalicize = value; }
        }

        /// <summary>
        /// Should Underline ? 
        /// </summary>
        public bool ShouldUnderline
        {
            get { return shouldUnderline; }
            set { shouldUnderline = value; }
        }

        /// <summary>
        /// CssClass
        /// </summary>
        public string CssClass
        {
            get 
            {
                switch (levelNumber)
                {
                    case 0:
                        cssClass = "BODY";
                        break;
                    default:
                        cssClass = String.Empty;
                        break;
                }
                return cssClass; 
            }
        }

        /// <summary>
        /// CssStyle
        /// </summary>
        public string CssStyle
        {
            get 
            {
                if (!String.IsNullOrEmpty(size))
                {
                    size = size.Trim();
                    int sizeValue = 3;
                    int.TryParse(size.Substring(size.Length-1), out sizeValue);
                    
                    int sizeInPixels = (4 * sizeValue) + 4;

                    //if (size.StartsWith("+"))
                    //{

                    //}
                    //else if (size.StartsWith("-"))
                    //{

                    //}
                    //else
                    //{

                    //}

                    cssStyle += "font-size:" + sizeInPixels.ToString() + "px; ";
                }

                if (!String.IsNullOrEmpty(color))
                {
                    cssStyle += "color:" + color + "; ";
                }

                if (shouldBold)
                {
                    cssStyle += "font-weight:bold; ";
                }
                else
                {
                    cssStyle += "font-weight:normal; ";
                }

                if (shouldItalicize)
                {
                    cssStyle += "font-style:italic; ";
                }
                else
                {
                    cssStyle += "font-style:normal; ";
                }

                if (shouldUnderline)
                {
                    cssStyle += "text-decoration:underline; ";
                }
                else
                {
                    cssStyle += "text-decoration:none; ";
                }

                return cssStyle; 
            }
        }

        /// <summary>
        /// CssText
        /// </summary>
        public string CssText
        {
            get 
            {
                string cssText=String.Empty;
                if(!String.IsNullOrEmpty(this.CssClass))
                {
                    cssText = this.CssClass + "{" + this.CssStyle + "}";
                }
                return cssText; 
            }
        }

        /// <summary>
        /// Reset to defaults
        /// </summary>
        public void Reset()
        {
            //use defaults
            text = String.Empty;
            color = "BLACK";
            size = "3";
            shouldAppend = false;
            shouldBold = false;
            shouldItalicize = false;
            shouldUnderline = false;
        }
    }
}
