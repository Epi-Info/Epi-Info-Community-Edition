using System;
using Epi.DataSets;

namespace Epi
{ 
    /// <summary>
	/// Base class for Variable.
	/// </summary>
    public abstract class VariableBase : IVariable
    {
        
        #region Protected Attributes
        /// <summary>
        /// Expression for all variable sub classes
        /// </summary>
        protected string expression = "Null";  //zack,
        /// <summary>
        /// The DataType of the variable
        /// </summary>
        /// <remarks>
        /// If no datatype is given when the variable is defined
        /// datatype is assigned to the type of the RHS expression in an ASSIGN statement
        /// </remarks>
        protected DataType dataType = DataType.Unknown;

        #endregion Protected Attributes

        #region Private Attributes
        private string name;
        private readonly VariableType varType;
        private string promptText;
        #endregion Private Attributes

        #region Constructors

        /// <summary>
        /// Constructor for VariableBase class
        /// </summary>
        /// <param name="name"></param>
        /// <param name="varType"></param>
        public VariableBase(string name, VariableType varType)
            : this(name, DataType.Unknown, varType)
        { }

        /// <summary>
        /// Constructor for the VariableBase Class that creates a variable of a certain datatype
        /// </summary>
        /// <param name="name">Variable name.</param>
        /// <param name="dataType">Varaiable data type enumeration.</param>
        /// <param name="varType">Variable type enumeration.</param>
        public VariableBase(string name, DataType dataType, VariableType varType)
        {
            this.name = name;
            this.dataType = dataType;
            this.varType = varType;
        }

        //public VariableBase(string name, string value, DataType dataType, VariableType varType)
        //{
        //    #region Input validation
        //    if (string.IsNullOrEmpty(name))
        //    {
        //        throw new ArgumentNullException("name");
        //    }
        //    #endregion Input validation
        //    this.name = name;
        //    this.varType = varType;
        //    this.dataType = dataType;
        //    this.val = value;
        //}
		#endregion Constructors

		#region Public Properties
        /// <summary>
        /// Property get for the type of the variable.
        /// </summary>
        public VariableType VarType
        {
            get
            {
                return varType;
            }
        }
        /// <summary>
        /// Property get/set for the name of the variable
        /// </summary>
		public string Name
		{
			get
			{
				return name;
			}
            set
            {
                name = value;
            }
		}

        /// <summary>
        /// Property get/set for the variable's expression
        /// </summary>
        public virtual string Expression
        {
            get
            {
                return expression;
            }
            set
            {
                expression = value;
            }
        }

        /// <summary>
        /// Property get/set for the variable's DataType
        /// </summary>
        /// <remarks>
        /// set is only allowed if this is an untyped variable
        /// This is necessary because whenever an untyped variable is assigned 
        /// it must take on the datatype of the RHS value
        /// </remarks>
        public DataType DataType
		{
			get
			{
				return dataType;
			}
            set
            {
                if (dataType == DataType.Unknown)
                {
                    dataType = value;
                }
            }
		}

        /// <summary>
        /// Property get/set for prompt of the variable.
        /// </summary>
        public string PromptText
        {
            get
            {
                return promptText;
            }
            set
            {
                promptText = value;
            }
        }
        
        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// The Dispose Method
        /// </summary>
		public virtual void Dispose()
        {
        }

        /// <summary>
        /// Determines if the type of this varuiable matches any of the types in the combination.
        /// </summary>
        /// <param name="typeCombination"></param>
        /// <returns></returns>
        public bool IsVarType(VariableType typeCombination)
        {
            return IsVarType(this.varType, typeCombination);
        }

        #endregion Public Methods

        #region Static methods
        /// <summary>
        /// returns true if the variable passed in is of the type specified
        /// </summary>
        /// <param name="varType"></param>
        /// <param name="typeCombination"></param>
        /// <returns>bool</returns>
        public static bool IsVarType(VariableType varType, VariableType typeCombination)
        {
            return ((varType & typeCombination) > 0);
        }
        #endregion Static methods
    }

    /// <summary>
    /// A concrete (not abstract) variable class
    /// </summary>
    public class Variable : VariableBase
    {
        /// <summary>
        /// Constructor for the concrete variable class
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dataType"></param>
        /// <param name="varType"></param>
        public Variable(string name, DataType dataType, VariableType varType)
            : base(name, dataType, varType)
        {
        }

    }
}
