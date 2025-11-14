using JfYu.Office.Word.Constant;
using System.Collections.Generic;

namespace JfYu.Office.Word
{
    /// <summary>
    /// Interface for generating Word documents from templates.
    /// Supports text and image replacements using placeholder syntax: {key}
    /// </summary>
    public interface IJfYuWord
    {
        /// <summary>
        /// Generates a Word document based on a template and a list of replacements.
        /// Replaces all {key} placeholders in the template (including in paragraphs and tables) with the corresponding values.
        /// </summary>
        /// <param name="templatePath">The path to the Word template file. Must be a valid .docx file.</param>
        /// <param name="outputFilePath">The path where the generated Word file will be saved.</param>
        /// <param name="replacements">A list of replacements to be applied to the template. Supports both text (JfYuWordString) and image (JfYuWordPicture) replacements.</param>
        /// <exception cref="System.IO.FileNotFoundException">Thrown when the template file does not exist.</exception>
        void GenerateWordByTemplate(string templatePath, string outputFilePath, List<JfYuWordReplacement> replacements);
    }
}