using JfYu.Office.Word.Constant;
using NPOI.Util;
using NPOI.XWPF.UserModel;
using System;
using System.IO;

namespace JfYu.Office.Word.Extensions
{
    /// <summary>
    /// Provides extension methods for working with Word documents.
    /// </summary>
    public static class JfYuWordExtension
    {
        /// <summary>
        /// Creates a picture in the specified run, replacing a placeholder with the image.
        /// </summary>
        /// <param name="run">The run where the picture will be inserted.</param>
        /// <param name="replacement">The replacement containing the key and the picture.</param>
        /// <exception cref="ArgumentNullException">Thrown when the run or replacement is null.</exception>
        public static void CreatePicture(XWPFRun run, JfYuWordReplacement replacement)
        {
            ArgumentNullException.ThrowIfNull(run);
            ArgumentNullException.ThrowIfNull(replacement);
            var placeholder = $"{{{replacement.Key}}}";
            var text = run.Text;
            var texts = text.Split(placeholder);
            var beforeText = texts[0];
            var afterText = "";
            if (texts.Length > 1)
                afterText = texts[1];

            // Clear the current text in the run
            run.SetText("", 0);
            int pos = run.Paragraph.Runs.IndexOf(run);

            // Insert the text before the placeholder
            if (!string.IsNullOrEmpty(beforeText))
            {
                XWPFRun beforeRun = run.Paragraph.InsertNewRun(pos);
                beforeRun.SetText(beforeText);
                pos++;
            }

            XWPFRun imageRun = run.Paragraph.InsertNewRun(pos);
            var value = (JfYuWordPicture)replacement.Value;
            using var ms = new MemoryStream(value.Bytes);
            imageRun.AddPicture(ms, (int)PictureType.PNG, $"{replacement.Key}.png", Units.ToEMU(value.Width), Units.ToEMU(value.Height));
            pos++;

            if (!string.IsNullOrEmpty(afterText))
            {
                XWPFRun afterRun = run.Paragraph.InsertNewRun(pos);
                afterRun.SetText(afterText);
            }
        }
    }
}