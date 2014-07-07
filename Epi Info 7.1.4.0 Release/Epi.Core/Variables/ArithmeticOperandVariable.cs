//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Epi
//{
//    /// <summary>
//    /// Sample code class
//    /// </summary>
//    public class ArithmeticOperandVariable : Variable 
//    {
//        private string operandVariableName;
//        private ArithmeticOperator op;
//        private double quantity;
//        public ArithmeticOperandVariable(string variableName, string operandVariableName, ArithmeticOperator op, double quantity)
//            : base(variableName, DataType.Number, VariableType.Standard)
//        {
//            this.operandVariableName = operandVariableName;
//            this.op = op;
//            this.quantity = quantity;
//        }

//        /// <summary>
//        /// Set overrides the arithemic operand value
//        /// </summary>
//        public override string Value
//        {
//            get
//            {
//                double result = GetNestedVariableValue();
//                switch (op)
//                { 
//                    case ArithmeticOperator.Add:
//                        result += quantity;
//                        break;
//                    case ArithmeticOperator.Divide:
//                        result /= quantity;
//                        break;
//                    case ArithmeticOperator.Multiply:
//                        result *= quantity;
//                        break;
//                    case ArithmeticOperator.Subtract:
//                        result -= quantity;
//                        break;
//                }
                
//                return result.ToString();
//            }
//            set
//            {
//                this.quantity = double.Parse(value);
//            }
//        }

//        private double GetNestedVariableValue()
//        {
//            double result;
//            IVariable variable = Scope.GetVariable(operandVariableName);
//            if (variable == null)
//            {
//                throw new GeneralException("Could not resolve nested variable name '" + operandVariableName + "'");
//            }
//            if (variable.DataType != DataType.Number || !double.TryParse(variable.Value, out result))
//            {
//                throw new GeneralException("Variable '" + operandVariableName + "' is not numeric or does not have a valid value.");
//            }
//            return result;
//        }

//        public enum ArithmeticOperator
//        { 
//            Add,
//            Subtract,
//            Multiply,
//            Divide
//        }
//    }
//}
