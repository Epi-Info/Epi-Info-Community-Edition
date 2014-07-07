using System;
namespace Epi.Windows.Globalization
{
    /// <summary>
    /// Interface for all translation components used by the ResourceTool
    /// </summary>
    public interface ITranslator
    {
        /// <summary>
        /// Translate string from source culture to target culture
        /// </summary>
        /// <param name="sourceCultureName"></param>
        /// <param name="targetCultureName"></param>
        /// <param name="sourceText"></param>
        /// <returns></returns>
        string Translate(string sourceCultureName, string targetCultureName, string sourceText);
    }
}
