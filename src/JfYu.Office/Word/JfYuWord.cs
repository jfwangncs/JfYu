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

            var allRuns = CollectRuns(doc);

            var textReplacements = replacements.Where(r => r.Value is JfYuWordString).ToList();
            var picReplacements = replacements.Where(r => r.Value is JfYuWordPicture).ToList();

            foreach (var run in allRuns)
            {
                ProcessTextReplacements(run, textReplacements);
                ProcessPictureReplacements(run, picReplacements);
            }

            using FileStream fs = new(outputFilePath, FileMode.Create);
            doc.Write(fs);
        }
        private static List<XWPFRun> CollectRuns(XWPFDocument doc)
        {
            var runs = doc.Paragraphs.SelectMany(p => p.Runs).ToList();

            foreach (var table in doc.Tables)
            {
                foreach (var row in table.Rows)
                {
                    foreach (var cell in row.GetTableCells())
                    {
                        foreach (var para in cell.Paragraphs)
                            runs.AddRange(para.Runs);
                    }
                }
            }

            return runs;
        }
        private static void ProcessTextReplacements(XWPFRun run, List<JfYuWordReplacement> replacements)
        {
            if (replacements.Count == 0)
                return;

            string text = run.Text;

            foreach (var rp in replacements)
            {
                string placeholder = $"{{{rp.Key}}}";

                if (text.Contains(placeholder))
                {
                    run.SetText(
                        text.Replace(placeholder, ((JfYuWordString)rp.Value).Text),
                        0);
                    text = run.Text;
                }
            }
        }
        private static void ProcessPictureReplacements(XWPFRun run, List<JfYuWordReplacement> replacements)
        {
            if (replacements.Count == 0)
                return;

            string text = run.Text;

            foreach (var rp in replacements)
            {
                string placeholder = $"{{{rp.Key}}}";

                if (text.Contains(placeholder))
                {
                    JfYuWordExtension.CreatePicture(run, rp);
                }
            }
        }
    }
}