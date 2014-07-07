using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;
using System.Data;
using Epi.Core;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    /// <summary>
    /// The rule for the LIST command
    /// </summary>
    public partial class Rule_List : AnalysisRule
    {
        private bool HasRun = false;

        List<string> IdentifierList = new List<string>();
        bool IsExceptionList = false;
        string CommandText = null;
        string ListOption = null;
        bool isHTMLOption = false;
        Configuration config = null;
        string repOfYes;
        string repOfNo;
        string repOfMissing;
        bool isEpi7Project = false;
        bool isEpi7View = false;
        View CurrentView = null;


        /// <summary>
        /// Constructor for Rule_List
        /// </summary>
        /// <param name="pToken">The token used to build the reduction.</param>
        public Rule_List(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            config = Configuration.GetNewInstance();
            repOfYes = config.Settings.RepresentationOfYes;
            repOfNo = config.Settings.RepresentationOfNo;
            repOfMissing = config.Settings.RepresentationOfMissing;

            CommandText = this.ExtractTokens(pToken.Tokens);

            foreach (Token T in pToken.Tokens)
            {
                if (T is NonterminalToken)
                {
                    NonterminalToken NT = (NonterminalToken)T;
                    switch (NT.Symbol.ToString())
                    {
                        case "<List_Identifier_List>":
                            this.SetIdentifierList(NT);
                            break;
                        case "<ListOpt>":
                            this.SetListOpt(NT);
                            break;
                        case "<ListGridOpt>":
                            this.SetListOpt(NT);
                            break;
                    }
                }
                else
                {
                    TerminalToken TT = (TerminalToken)T;
                    switch (TT.Symbol.ToString())
                    {
                        case "*":
                            this.IdentifierList.Add("*");
                            break;
                        case "EXCEPT":
                            this.IsExceptionList = true;
                            break;
                    }
                }
            }


            if (this.IdentifierList.Count > 1 && this.IdentifierList[0] == "*")
            {
                this.IdentifierList.Remove("*");
            }

            /*
            !***  			List Statement 			***!
            <List_Statement>                         ::= LIST 
                                            | LIST <ListOpt>
                                            | LIST '*'
                                            | LIST '*' <ListOpt>
                                            | LIST  <List_Identifier_List> 
                                            | LIST  <List_Identifier_List> <ListOpt>
                                            | LIST '*' EXCEPT <List_Identifier_List> 
                                            | LIST '*' EXCEPT <List_Identifier_List> <ListOpt>

            <ListOpt> 							::= <ListGridOpt>
                                            | <ListUpdateOpt>
                                            | <ListHTMLOpt>

            <ListGridOpt>							::= GRIDTABLE
            <ListUpdateOpt>						::= UPDATE
		 
            <ListHTMLOpt>							::= <ListHTMLOptOneColumn>
                                                | <ListHTMLOptTwoColumns>
                                                | <ListHTMLOptThreeColumns>
                                                | <ListHTMLOptNoImage>
                                                | <ListHTMLOptNowrap>
                                                | <ListHTMLOptLine>
                                                | !Null
		 
            <ListHTMLOptOneColumn> 						::= <ListHTMLOpt> COULMNSIZE '=' DecLiteral
            <ListHTMLOptTwoColumns> 					::= <ListHTMLOpt> COULMNSIZE '=' DecLiteral	',' DecLiteral
            <ListHTMLOptThreeColumns> 					::= <ListHTMLOpt> COULMNSIZE '=' DecLiteral ',' DecLiteral ','	 DecLiteral
            <ListHTMLOptNoImage>	 					::= <ListHTMLOpt> NOIMAGE
            <ListHTMLOptNoWrap> 						::= <ListHTMLOpt> NOWRAP
            <ListHTMLOptLine> 						::= <ListHTMLOpt> LINENUMBERS 
            !***  			End 					***!
            */
        }


        private void SetListOpt(NonterminalToken pT)
        {
            /*<ListOpt>::= <ListGridOpt>
                       | <ListUpdateOpt>
                       | <ListHTMLOpt>*/
            switch (pT.Symbol.ToString())
            {
                case "<ListGridOpt>":
                    this.ListOption = "GRIDTABLE";
                    break;
                case "<ListUpdateOpt>":
                    this.ListOption = "UPDATE";
                    break;
                case "<ListHTMLOpt>":
                    this.SetHTMLOption((NonterminalToken)pT.Tokens[0]); 
                    break;
            }
        }

        private void SetHTMLOption(NonterminalToken pT)
        {
            /*<ListHTMLOpt> ::= <ListHTMLOptOneColumn>
                            | <ListHTMLOptTwoColumns>
                            | <ListHTMLOptThreeColumns>
                            | <ListHTMLOptNoImage>
                            | <ListHTMLOptNowrap>
                            | <ListHTMLOptLine>
                            | !Null
             */
            switch (pT.Symbol.ToString())
            {
                case "<ListHTMLOptOneColumn>":
                case "<ListHTMLOptTwoColumns>":
                case "<ListHTMLOptThreeColumns>":
                case "<ListHTMLOptNoImage>":
                case "<ListHTMLOptNowrap>":
                case "<ListHTMLOptLine>":
                    this.ListOption = this.GetCommandElement(pT.Tokens, 0).Trim();
                    this.isHTMLOption = true;
                    break;
            }
        }

        private void SetIdentifierList(NonterminalToken pT)
        {
            //<List_Identifier_List>::= Identifier | Identifier <List_Identifier_List>

            this.IdentifierList.Add(this.GetCommandElement(pT.Tokens, 0).ToUpper().Trim( new char[] { '[', ']'}) );
            if (pT.Tokens.Length > 1)
            {
                this.SetIdentifierList((NonterminalToken)pT.Tokens[1]);
            }
            
        }

        /// <summary>
        /// performs execution of the List command
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;

            //System.Data.IDataReader DATAReader = Context.GetCurrentDataTableReader();
            if (!HasRun)
            {
                if (this.Context.CurrentRead != null)
                {
                    this.isEpi7Project = this.Context.CurrentRead.IsEpi7ProjectRead;
                }

                if (this.isEpi7Project)
                {
                    if (this.Context.CurrentProject.Views.Exists(this.Context.CurrentRead.Identifier))
                    {
                        this.CurrentView = this.Context.CurrentProject.GetViewByName(this.Context.CurrentRead.Identifier);
                    }
                }

                List<DataRow> DR = this.Context.GetOutput(new List<string>());//.Select("", this.Context.SortExpression.ToString());

                if (this.IdentifierList.Count == 1 && this.IdentifierList[0] == "*")
                {
                    this.IdentifierList.Clear();

                    foreach (DataColumn C in this.Context.DataSet.Tables["Output"].Columns)
                    {
                        this.IdentifierList.Add(C.ColumnName.ToUpper());
                    }
                }

                this.Context.ExpandGroupVariables(this.IdentifierList, ref this.IsExceptionList);

                StringBuilder builder = new StringBuilder();
                if (!string.IsNullOrEmpty(this.ListOption) && this.ListOption.ToLower().Equals("gridtable"))
                {
                    this.Context.AnalysisCheckCodeInterface.ShowGridTable(DR, IdentifierList, this.CurrentView);
                }
                else
                {
                    builder.Append("<table cellpadding=\"2\">");
                    PrintHeaderRow(this.Context.DataSet.Tables["Output"], builder);
                    foreach (DataRow dataRow in DR)
                    {
                        PrintRow(this.Context.DataSet.Tables["Output"], dataRow, builder);
                    }
                    builder.Append("</table>");
                }
                //result = string.Format("number of records read {0}", Output.Rows.Count);
                Dictionary<string, string> args = new Dictionary<string, string>();
                args.Add("COMMANDNAME", CommandNames.LIST);
                args.Add("DATA", builder.ToString());
                args.Add("COMMANDTEXT", CommandText);

                this.Context.AnalysisCheckCodeInterface.Display(args);
                this.HasRun = true;
            }

            return result;
        }

        private string GetImageExtension(System.IO.MemoryStream stream)
        {
            string result = "";
            try
            {
                System.Drawing.Image image = System.Drawing.Image.FromStream(stream);
                Type Type = typeof(System.Drawing.Imaging.ImageFormat);
                System.Reflection.PropertyInfo[] imageFormatList = Type.GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                for (int i = 0; i != imageFormatList.Length; i++)
                {
                    System.Drawing.Imaging.ImageFormat formatClass = (System.Drawing.Imaging.ImageFormat)imageFormatList[i].GetValue(null, null);
                    if (formatClass.Guid.Equals(image.RawFormat.Guid))
                    {
                        result =  imageFormatList[i].Name.ToLower();
                    }
                }
            }
            catch (Exception ex)
            {
                // do nothing
            }

            return result;
        }


        private void PrintRow(System.Data.DataTable pDataTable, System.Data.DataRow pRow, StringBuilder pBuilder)
        {
            pBuilder.Append("<tr>");

            //object[] Items = pRow.ItemArray;

            for (int i = 0; i < this.IdentifierList.Count; i++)
            {
                string ColumnName = this.IdentifierList[i];

                DataColumn C = pDataTable.Columns[ColumnName];
                if (PrintColumn(C.ColumnName))
                {
                    string columnDataType = C.DataType.ToString();
                    switch (columnDataType)
                    {
                        case "System.Int16":
                        case "System.Int32":
                        case "System.Int64":
                        case "System.Double":
                        case "System.Single":
                        case "System.Decimal":
                            pBuilder.Append("<td align=\"right\">");
                            break;
                        default:
                            pBuilder.Append("<td>");
                            break;
                    }

                    //pBuilder.Append("<td>");

                    if (pRow[C.ColumnName] == DBNull.Value)
                    {
                        pBuilder.Append(repOfMissing);
                    }
                    else switch (C.DataType.Name)
                        {

                            case "Boolean":
                                pBuilder.Append((Convert.ToBoolean(pRow[C.ColumnName]) ? repOfYes : repOfNo));
                                break;
                            case "Byte":
                                if (this.isEpi7Project)
                                {
                                    pBuilder.Append((Convert.ToBoolean(pRow[C.ColumnName]) ? repOfYes : repOfNo));
                                }
                                else
                                {
                                    pBuilder.Append(pRow[C.ColumnName]);
                                }
                                break;
                            case "Byte[]":
                                string extension = GetImageExtension(new System.IO.MemoryStream((byte[])pRow[C.ColumnName]));
                                string imgFileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString("N") + "." + extension;

                                System.IO.FileStream imgStream = System.IO.File.OpenWrite(imgFileName);
                                System.IO.BinaryWriter imgWriter = new System.IO.BinaryWriter(imgStream);
                                imgWriter.Write((byte[])pRow[C.ColumnName]);
                                imgWriter.Close();
                                imgStream.Close();
                                pBuilder.Append("<img src=\"" + imgFileName + "\"/>");
                                break;
                            case "Single":
                                pBuilder.Append(string.Format("{0:0.##}", pRow[C.ColumnName]));
                                break;
                            case "Double":
                            case "Float":
                                pBuilder.Append(string.Format("{0:0.##}", pRow[C.ColumnName]));
                                break;
                            case "DateTime":

                                if (this.CurrentView == null)
                                {
                                    pBuilder.Append(pRow[C.ColumnName]);
                                }
                                else
                                {
                                    IVariable var = (IVariable) this.Context.GetVariable(C.ColumnName);

                                    if (var == null) break;
                                    
                                    if (var.VarType == VariableType.DataSource)
                                    {
                                        try
                                        {
                                            if (this.CurrentView.Fields.Exists(C.ColumnName) && this.CurrentView.Fields[C.ColumnName] is Epi.Fields.DateField)
                                            {
                                                pBuilder.Append(((DateTime)pRow[C.ColumnName]).ToShortDateString());//
                                            }
                                            else if (this.CurrentView.Fields.Exists(C.ColumnName) && this.CurrentView.Fields[C.ColumnName] is Epi.Fields.TimeField)
                                            {
                                                pBuilder.Append(((DateTime)pRow[C.ColumnName]).ToShortTimeString());//
                                            }
                                            else
                                            {
                                                pBuilder.Append(pRow[C.ColumnName]);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            pBuilder.Append(pRow[C.ColumnName]);
                                        }
                                    }
                                    else
                                    {
                                        if (pRow[C.ColumnName] != DBNull.Value)
                                        {
                                            System.DateTime PrintDate = (System.DateTime)pRow[C.ColumnName];
                                            if (var.DataType ==  DataType.Date)
                                            {
                                                pBuilder.Append(PrintDate.ToShortDateString());
                                            }
                                            else if (var.DataType == DataType.Time)
                                            {
                                                pBuilder.Append(PrintDate.ToShortTimeString());
                                            }
                                            else
                                            {
                                                pBuilder.Append(pRow[C.ColumnName]);
                                            }
                                        }
                                        else
                                        {
                                            pBuilder.Append(pRow[C.ColumnName]);
                                        }
                                    }
                                }
                                break;
                            default:
                                pBuilder.Append(pRow[C.ColumnName]);
                                break;
                        }
                    pBuilder.Append("</td>");
                }
            }
            pBuilder.Append("</tr>");
        }

  

        private void PrintHeaderRow(System.Data.DataTable pDataTable, StringBuilder pBuilder)
        {
            pBuilder.Append("<tr>");
            for (int i = 0; i < this.IdentifierList.Count; i++)
            {
                string ColumnName = this.IdentifierList[i];
                DataColumn C = pDataTable.Columns[ColumnName];
                if (PrintColumn(C.ColumnName))
                {
                    pBuilder.Append("<th>");
                    pBuilder.Append(C.ColumnName);
                    pBuilder.Append("</th>");
                }
            }

            pBuilder.Append("</tr>");
        }

        private bool PrintColumn(string pName)
        {
            bool result = false;

            if (this.IsExceptionList)
            {
                return !this.IdentifierList.Contains(pName.ToUpper());
            }
            else
            {
                if (this.IdentifierList.Count == 1 && this.IdentifierList[0] == "*")
                {

                    result = true;
                }
                else
                {
                    return this.IdentifierList.Contains(pName.ToUpper());
                }
            }


            return result;
        }

        
    }
}
