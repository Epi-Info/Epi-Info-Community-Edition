using System;

namespace Epi.Diagnostics
{
    /// <summary>
    /// Class Debugger
    /// </summary>
	public class Debugger
	{
        /// <summary>
        /// Constructor
        /// </summary>
		public Debugger()
		{
	
		}

        /// <summary>
        /// Break the debugger
        /// </summary>
		public static void Break()
		{
#if (DEBUG)
            //System.Diagnostics.Debugger.Break();
#endif
		}

        /// <summary>
        /// Log the exception that occurred
        /// </summary>
        /// <param name="ex">Exception that happened</param>
		public static void LogException(Exception ex)
		{
			System.Diagnostics.Debug.WriteLine(ex.ToString());
		}
	}
}
