namespace Epi.Fields
{
	/// <summary>
	/// IFieldWithCheckCodeAfter
	/// </summary>
	public interface IFieldWithCheckCodeAfter
	{
		/// <summary>
		/// Gets/sets the AFTER check code
		/// </summary>
		string CheckCodeAfter
		{
			get;
			set;
		}
	}

    /// <summary>
    /// IFieldWithCheckCodeBefore
    /// </summary>
    public interface IFieldWithCheckCodeBefore
    {
        /// <summary>
        /// Gets/sets the BEFORE check code
        /// </summary>
        string CheckCodeBefore
        {
            get;
            set;
        }
    }

    /// <summary>
    /// IFieldWithCheckCodeClick
    /// </summary>
    public interface IFieldWithCheckCodeClick
    {
        /// <summary>
        /// Gets/sets the CLICK check code
        /// </summary>
        string CheckCodeClick
        {
            get;
            set;
        }
    }

}