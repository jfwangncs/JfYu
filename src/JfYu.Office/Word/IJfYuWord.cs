using JfYu.Office.Word.Constant;
using System.Collections.Generic;

namespace JfYu.Office.Word
{
    /// <summary>
    /// Interface for generating Word documents from templates.
    /// </summary>
    public interface IJfYuWord
    {
        /// <summary>
        /// Generates a Word document based on a template and a list of replacements.
        /// </summary>
        /// <param name="templatePath">The path to the Word template file.</param>
        /// <param name="outputFilePath">The path where the generated Word file will be saved.</param>
        /// <param name="replacements">A list of replacements to be applied to the template.</param>
        void GenerateWordByTemplate(string templatePath, string outputFilePath, List<JfYuWordReplacement> replacements);
    }
}