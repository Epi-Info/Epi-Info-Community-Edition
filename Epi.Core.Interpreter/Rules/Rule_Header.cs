using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    /// <summary>
    /// Class for the HEADER command
    /// </summary>
    public class Rule_Header : AnalysisRule
    {
        int level;
        string text;
        string color;
        string size;
        bool shouldAppend = false;
        bool shouldBold = false;
        bool shouldItalicize = false;
        bool shouldUnderline = false;
        bool shouldDrop = false;
        bool shouldReset = false;

        #region Constructors

        /// <summary>
        /// Constructor for the Rule_Header reduction.
        /// </summary>
        /// <param name="pToken">The token to build the reduction with.</param>
        public Rule_Header(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            //HEADER 4 "This is HEADER 4" (BOLD,ITALIC,UNDERLINE) TEXTFONT Teal 3 
            //<H4><FONT COLOR="#008080" SIZE="3"><B><I><U>This is HEADER 4</U></I></B></FONT><BR>FREQ ILL<BR></H4><BR>

            //this.commandText = this.ExtractTokens(pToken.Tokens);
            /*
            !***			Header Statement			***!
            <Header_Title_String_Statement> ::= HEADER DecLiteral <TitleStringPart>
            <Header_Title_Font_Statement> ::= HEADER DecLiteral <TitleFontPart>
            <Header_Title_String_And_Font_Statement> ::= HEADER DecLiteral <TitleStringPart> <TitleFontPart>

            <TitleStringPart> ::= <DefaultTitleStringPart> | <TitleStringPartWithAppend>
            <DefaultTitleStringPart> ::= String <EffectList>
            <TitleStringPartWithAppend> ::= String <EffectList> APPEND

            <EffectLeft> ::= Identifier | <EffectLeft> ',' Identifier
            <EffectList> ::= '(' <EffectLeft> ')' | !Null
 
            <TitleFontPart> ::= TEXTFONT <Color> DecLiteral
                                            | TEXTFONT <Color>
                                            | TEXTFONT DecLiteral

            <Color> 							::= Identifier
                                                | HexLiteral
            !***			End 					***!
            */
            int.TryParse(this.GetCommandElement(pToken.Tokens, 1), out level);
            
            if (pToken.Tokens.Length > 2)
            {
                string textAndEffects = this.GetCommandElement(pToken.Tokens, 2);
                if (textAndEffects.StartsWith("\"") && textAndEffects.IndexOf('\"',1)!=1)
                {
                    int quotePos = textAndEffects.IndexOf('\"', 1);
                    text = textAndEffects.Substring(1, quotePos - 1);
                    int firstParen = textAndEffects.IndexOf('(', quotePos + 1);
                    
                    string[] effectList = new string[0];

                    if (firstParen > 0)
                    {
                        int secondParen = textAndEffects.IndexOf(')', firstParen + 1);
                        effectList = textAndEffects.Substring(firstParen + 1, secondParen - firstParen - 1).Split(','); 
                    }

                    for (int i = 0; i < effectList.Length; i++)
                    {
                        switch(effectList[i].Trim().ToUpperInvariant())
                        {
                            case "BOLD":
                                shouldBold = true;
                                break;
                            case "ITALIC":
                                shouldItalicize = true;
                                break;
                            case "UNDERLINE":
                                shouldUnderline = true;
                                break;
                        }
                    }

                    if (textAndEffects.ToUpperInvariant().EndsWith("APPEND"))
                    {
                        shouldAppend = true;
                    }
                }
                else if (textAndEffects.Trim().StartsWith("DROP"))
                {
                    shouldDrop = true;
                }
                else if (textAndEffects.Trim().StartsWith("RESET"))
                {
                    shouldReset = true;
                }
            }

            if (pToken.Tokens.Length > 3)
            {
                string[] fontList = this.GetCommandElement(pToken.Tokens, 3).Split(' ');
                if (fontList.Length > 1)
                {
                    color = fontList[1].Trim();
                }
                
                if (fontList.Length > 2)
                {
                    size = fontList[2].Trim();
                }
                if (fontList.Length > 3)
                {
                    size = size + fontList[3].Trim();
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Executes the Rule_Header reduction.
        /// </summary>
        /// <returns>Returns the result of executing the reduction.</returns>
        public override object Execute()
        {
            if (this.Context.AnalysisCheckCodeInterface != null)
            {
                StringBuilder HTMLString = new StringBuilder();

                Dictionary<string, string> args = new Dictionary<string, string>();
                args.Add("COMMANDNAME", CommandNames.HEADER);
                args.Add("LEVELNUMBER", level.ToString());
                args.Add("TEXT", text);
                args.Add("COLOR", color);
                args.Add("SIZE", size);
                args.Add("SHOULDAPPEND", shouldAppend.ToString());
                args.Add("SHOULDBOLD", shouldBold.ToString());
                args.Add("SHOULDITALICIZE", shouldItalicize.ToString());
                args.Add("SHOULDUNDERLINE", shouldUnderline.ToString());
                args.Add("SHOULDDROP", shouldDrop.ToString());
                args.Add("SHOULDRESET", shouldReset.ToString());

                this.Context.AnalysisCheckCodeInterface.Display(args);
            }

            return null;
        }

        #endregion
    }
}
