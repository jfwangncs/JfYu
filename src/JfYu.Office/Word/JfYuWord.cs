using JfYu.Office.Word.Constant;
using JfYu.Office.Word.Extensions;
using NPOI.XWPF.UserModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JfYu.Office.Word
{
    /// <summary>
    /// Class for generating Word documents from templates.
    /// </summary>
    public class JfYuWord : IJfYuWord
    {
        /// <inheritdoc/>
        public void GenerateWordByTemplate(string templatePath, string outputFilePath, List<JfYuWordReplacement> replacements)
        {
            if (!File.Exists(templatePath))
                throw new FileNotFoundException("can't find template file.");

            using FileStream stream = File.OpenRead(templatePath);
            using XWPFDocument doc = new(stream);
            var allRuns = doc.Paragraphs.SelectMany(q => q.Runs).ToList();
            var tables = doc.Tables;
            foreach (var table in tables)
            {
                foreach (var row in table.Rows)
                {
                    foreach (var cell in row.GetTableCells())
                    {
                        foreach (var para in cell.Paragraphs)
                        {
                            allRuns.AddRange(para.Runs);
                        }
                    }
                }
            }
            foreach (var run in allRuns)
            {
                string text = run.Text;
                var textReplacements = replacements.Where(q => q.Value is JfYuWordString);
                foreach (var replacement in textReplacements)
                {
                    string placeholder = $"{{{replacement.Key}}}";
                    if (text.Contains(placeholder))
                    {
                        run.SetText(text.Replace(placeholder, ((JfYuWordString)replacement.Value).Text), 0);
                        text = run.Text;
                    }
                }
                var picReplacements = replacements.Where(q => q.Value is JfYuWordPicture);
                foreach (var replacement in picReplacements)
                {
                    string placeholder = $"{{{replacement.Key}}}";
                    if (text.Contains(placeholder))
                    {
                        JfYuWordExtension.CreatePicture(run, replacement);
                    }
                }
            }
            using FileStream fs = new(outputFilePath, FileMode.Create);
            doc.Write(fs);
        }
    }
}