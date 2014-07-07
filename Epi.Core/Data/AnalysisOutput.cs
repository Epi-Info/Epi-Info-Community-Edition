using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Epi.Data
{
    class AnalysisOutput
    {
        string outputPath;
        XmlTextWriter writer;

        public string OutputPath
        {
            get { return outputPath; }
            set { outputPath = value; }
        }

        public AnalysisOutput()
        {
            outputPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            outputPath = Path.Combine(outputPath, "AnalysisOutput.xml");
            Open();
        }

        public AnalysisOutput(string outputPath)
        {
            this.outputPath = outputPath;
            Open();
        }

        private FileStream GetStream()
        {
            return new FileStream(outputPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        }

        private void Open()
        {
            writer = new XmlTextWriter(GetStream(), null);
            writer.WriteStartDocument();
        }

        public void Close()
        {
            writer.WriteEndDocument();
            writer.Close();
        }

        public void AddVariable(string name, string type, string value)
        {
            writer.WriteStartElement(name);
        }

        public void AddFrequency()
        {

        }
    }
}
