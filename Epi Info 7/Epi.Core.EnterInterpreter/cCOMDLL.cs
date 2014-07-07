using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Epi.Core.EnterInterpreter
{
    public class cCOMDLL : IDLLClass 
    {
        string _identifier;
        string _class;
        //string _filePath;
        object classInstance;

        //public cCOMDLL(string pIdentifier, string pFilePath, string pClass)
        public cCOMDLL(string pIdentifier, string pClass)
        {
            _identifier = pIdentifier;
            _class = pClass;
            //_filePath = pFilePath;
        }

        public string Identifier
        {
            get { return _identifier; }
        }

        public string Class
        {
            get { return _class; }
        }

        public DLLClassEnum Type
        {
            get { return DLLClassEnum.COM; }
        }

        public object Execute(string pMethod, object[] pArgumentList)
        {
            object result = null;

            if (classInstance == null)
            {
                classInstance =  Microsoft.VisualBasic.Interaction.CreateObject(_class, "");
            }

            result = Microsoft.VisualBasic.Interaction.CallByName(classInstance, pMethod, Microsoft.VisualBasic.CallType.Method, pArgumentList);

            return result;
        }


    }
}
