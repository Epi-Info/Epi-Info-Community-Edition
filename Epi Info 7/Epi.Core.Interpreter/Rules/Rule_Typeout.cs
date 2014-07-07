using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;
using System.Data;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public class Rule_Typeout : AnalysisRule
    {
        bool _hasRun = false;

        string _strOrFile = string.Empty;
        string _color = null;
        string _size = null;

        string[] _effectList = null;

        // <Type_Out_String_Statement> 				::= TYPEOUT String <EffectList>
        // <Type_Out_File_Statement> 				::= TYPEOUT File
        // <Type_Out_String_With_Font_Statement>    ::= TYPEOUT String <EffectList> <TitleFontPart>
        // <Type_Out_File_With_Font_Statement> 		::= TYPEOUT File <EffectList> <TitleFontPart>

        // <EffectLeft>     ::= Identifier  
        //                  | <EffectLeft> ',' Identifier

        // <EffectList>     ::= '(' <EffectLeft> ')' 
        //                  | !Null

        // <TitleFontPart>	::= TEXTFONT <Color> DecLiteral
        //					| TEXTFONT <Color>
        //					| TEXTFONT DecLiteral

        public Rule_Typeout(Rule_Context context, NonterminalToken nonterminalToken) : base(context)
        {
            _strOrFile = GetCommandElement(nonterminalToken.Tokens, 1).Trim('\"').Trim('\'');

            if (nonterminalToken.Tokens.Length >= 3)
            {
                _effectList = GetCommandElement(nonterminalToken.Tokens, 2).Split(' ');
                SetTitleFont((NonterminalToken)nonterminalToken.Tokens[2]);
            }
            if (nonterminalToken.Tokens.Length >= 4)
            {
                SetTitleFont((NonterminalToken)nonterminalToken.Tokens[3]);
            }
        }

        public override object Execute()
        {
            object result = null;

            if (!_hasRun)
            {
                StringBuilder builder = new StringBuilder();
                string style = "{0}";

                if (_effectList != null)
                {
                    foreach (string part in _effectList)
                    {
                        if (part.ToUpper().Equals("BOLD"))
                        {
                            style = string.Format(style, "<B>{0}</B>");
                        }
                        if (part.ToUpper().Equals("ITALIC"))
                        {
                            style = string.Format(style, "<I>{0}</I>");
                        }
                        if (part.ToUpper().Equals("UNDERLINE"))
                        {
                            style = string.Format(style, "<U>{0}</U>");
                        }
                    }
                }

                StringBuilder font = new StringBuilder();
                font.Append("<FONT");

                if (_size != null)
                {
                    int size = 0;
                    if (int.TryParse(_size, out size))
                    {
                        font.Append(" SIZE=\"" + _size + "\"");
                    }
                }

                if (_color != null)
                {
                    font.Append(" COLOR=\"" + _color + "\"");
                }

                font.Append(">{0}</FONT>");

                if (!string.IsNullOrEmpty(_size) || !string.IsNullOrEmpty(_color))
                {
                    style = font.ToString().Replace("{0}", style);
                }
                style = string.Format(style, _strOrFile);
                builder.Append(style);
                builder.Append("<BR/>");

                Dictionary<string, string> args = new Dictionary<string, string>();
                args.Add("COMMANDNAME", CommandNames.TYPEOUT);
                args.Add("COMMANDTEXT", string.Empty);
                args.Add("DATA", builder.ToString());
                this.Context.AnalysisCheckCodeInterface.Display(args);
                _hasRun = true;
            }

            return result;
        }

        private void SetTitleFont(NonterminalToken nonterminalToken)
        {
            _color = null;
            _size = null;

            for (int i = 0; i < nonterminalToken.Tokens.Length; i++)
            {
                if (nonterminalToken.Tokens[i] is NonterminalToken)
                {
                    _color = this.GetCommandElement(nonterminalToken.Tokens, i);
                }
                else
                {
                    Token token = (Token)nonterminalToken.Tokens[i];

                    switch (token.ToString().ToUpper())
                    {
                        case "TEXTFONT":
                            break;
                        case "-":
                        case "+":
                            _size = nonterminalToken.Tokens[i].ToString() + nonterminalToken.Tokens[i + 1].ToString();
                            i++;
                            break;
                        default:
                            _size = token.ToString();
                            break;
                    }
                }
            }
        }
    }
}
