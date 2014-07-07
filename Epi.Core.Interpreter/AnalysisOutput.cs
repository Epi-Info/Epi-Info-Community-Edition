using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Epi.Core.AnalysisInterpreter
{
    public class AnalysisOutput
    {
        string outputPath;
        XmlTextWriter writer;
        Rule_Context context; 

        public string OutputPath
        {
            get { return outputPath; }
            set { outputPath = value; }
        }

        public AnalysisOutput(Rule_Context context)
        {
            this.context = context;
            outputPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            outputPath = Path.Combine(outputPath, "AnalysisOutput.xml");
        }

        public AnalysisOutput(Rule_Context context, string outputPath)
        {
            this.context = context;
            this.outputPath = outputPath;
        }

        private FileStream GetStream()
        {
            return new FileStream(outputPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        }

        public void PersistVariables()
        {
            Dictionary<string, object> variableValueList = context.VariableValueList;
            Epi.Collections.NamedObjectCollection<Epi.IVariable> scopeVariables = ((Epi.MemoryRegion)(context.MemoryRegion)).GetVariablesInScope();

            if (variableValueList.Count > 0 || scopeVariables.Count > 0)
            {
                writer = new XmlTextWriter(GetStream(), null);
                writer.Formatting = Formatting.Indented;
                writer.WriteStartDocument();
                writer.WriteStartElement("Variables");

                try
                {
                    foreach (Epi.IVariable var in scopeVariables)
                    {
                        if (var != null)
                        {
                            Epi.IVariable unboxed = (Epi.IVariable)var;
                            writer.WriteStartElement("Variable");
                            writer.WriteAttributeString("Name", unboxed.Name);
                            writer.WriteAttributeString("VarType", unboxed.VarType.ToString());
                            writer.WriteAttributeString("DataType", unboxed.DataType.ToString());

                            if(unboxed.VarType != VariableType.DataSource)
                            {
                                writer.WriteAttributeString("Value", unboxed.Expression);
                            }

                            writer.WriteEndElement();
                        }
                    }

                    foreach (KeyValuePair<string, object> kvp in variableValueList)
                    {
                        if (kvp.Key != null)
                        {
                            writer.WriteStartElement("Variable");
                            writer.WriteAttributeString("Name", kvp.Key);
                            writer.WriteAttributeString("Value", kvp.Value.ToString());
                            writer.WriteEndElement();
                        }
                    }
                    
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                    writer.Flush();
                    writer.Close();
                }
                finally
                {
                    if (writer != null) writer.Close();
                }
            }
        }

        public void Close()
        {
            writer.WriteEndDocument();
            writer.Close();
        }

        public void PersistFrequency()
        {
            writer = new XmlTextWriter(GetStream(), null);
            writer.Formatting = Formatting.Indented;
            writer.WriteStartDocument();

            writer.WriteStartElement("Tables");
            writer.WriteEndElement();

            writer.WriteEndDocument();
            writer.Close();
        }
    }
}
