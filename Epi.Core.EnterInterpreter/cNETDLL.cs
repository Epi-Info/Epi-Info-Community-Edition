using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Epi.Core.EnterInterpreter
{
    public class cNETDLL : IDLLClass
    {
        string _identifier;
        string _class;
        string _filePath;

        System.Reflection.Assembly _assembly;
        object classInstance = null;

        //public cNETDLL(string pIdentifier, string pFilePath, string pClass)
        public cNETDLL(string pIdentifier, string pClass)
        {
            _identifier = pIdentifier;
            _class = pClass.Substring(pClass.LastIndexOf(".") + 1,pClass.Length - pClass.LastIndexOf(".") - 1);
            _filePath = pClass.Substring(0, pClass.LastIndexOf("."));
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
            get { return DLLClassEnum.NET; }
        }

        public object Execute(string pMethod, object[] pArgumentList)
        {
            object result = null;

            if(_assembly == null)
            {
                _assembly = System.Reflection.Assembly.Load(_filePath);
            }

            if (classInstance == null)
            {
                foreach (Type type in _assembly.GetTypes())
                {
                    if (type.IsPublic && type.IsClass && type.Name.Equals(_class, StringComparison.OrdinalIgnoreCase))
                    {
                        if (type.IsAbstract)
                        {



                            MethodInfo methodInfo = type.GetMethod(pMethod, System.Reflection.BindingFlags.Static | BindingFlags.Public);
                            if (methodInfo.IsGenericMethod)
                            {
                                // Binding the method info to generic arguments
                                Type[] genericArguments = new Type[] { type };
                                MethodInfo genericMethodInfo = methodInfo.MakeGenericMethod(genericArguments);

                                // Simply invoking the method and passing parameters
                                // The null parameter is the object to call the method from. Since the method is
                                // static, pass null.
                                ParameterInfo[] parameters = methodInfo.GetParameters();
                                if (parameters.Length == pArgumentList.Length)
                                {
                                    for (int i = 0; i < parameters.Length; i++)
                                    {
                                        if (pArgumentList[i] != null)
                                        {
                                            if (parameters[i].ParameterType != pArgumentList[i].GetType())
                                            {
                                                pArgumentList[i] = System.Convert.ChangeType(pArgumentList[i], parameters[i].ParameterType);
                                            }
                                        }
                                    }
                                    result = genericMethodInfo.Invoke(null, pArgumentList);
                                }
                            }
                            else
                            {

                                ParameterInfo[] parameters = methodInfo.GetParameters();
                                if(parameters.Length == pArgumentList.Length)
                                {
                                    for (int i = 0; i < parameters.Length; i++)
                                    {
                                        if (pArgumentList[i] != null)
                                        {
                                            if (parameters[i].ParameterType != pArgumentList[i].GetType())
                                            {
                                                pArgumentList[i] = System.Convert.ChangeType(pArgumentList[i], parameters[i].ParameterType);
                                            }
                                        }
                                    }
                                    result = methodInfo.Invoke(null, pArgumentList);
                                }
                            }
                        }
                        else
                        {
                            classInstance = Activator.CreateInstance(type);
                        }

                        break;
                    }
                }
            }
            else
            {
                MethodInfo[] mi = classInstance.GetType().GetMethods();

                foreach (MethodInfo m in mi)
                {
                    if (m.Name.Equals(pMethod, StringComparison.OrdinalIgnoreCase))
                    {
                        ParameterInfo[] parameters = m.GetParameters();
                        if (parameters.Length == pArgumentList.Length)
                        {
                            for (int i = 0; i < parameters.Length; i++)
                            {
                                if (pArgumentList[i] != null)
                                {
                                    if (parameters[i].ParameterType != pArgumentList[i].GetType())
                                    {
                                        pArgumentList[i] = System.Convert.ChangeType(pArgumentList[i], parameters[i].ParameterType);
                                    }
                                }
                            }
                            result = m.Invoke(classInstance, pArgumentList);
                        }
                    }
                }
            }

            return result;
        }

    }
}
