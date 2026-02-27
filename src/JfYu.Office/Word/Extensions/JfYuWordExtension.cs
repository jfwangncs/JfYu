using JfYu.Office.Word.Constant;
using NPOI.Util;
using NPOI.XWPF.UserModel;
using System;
using System.IO;

namespace JfYu.Office.Word.Extensions
{
    /// <summary>
    /// Provides extension methods for Word document manipulation using NPOI.
    /// Includes methods for inserting images and replacing placeholders in document runs.
    /// </summary>
    public static class JfYuWordExtension
    {
        /// <summary>
        /// Inserts a picture into the specified run, optionally replacing a {key} placeholder.
        /// If the run's text contains the placeholder {key}, the placeholder is removed and the image is inserted at that position.
        /// The method intelligently splits text around the placeholder, preserving any text before and after it.
        /// </summary>
        /// <param name="run">The XWPFRun where the picture will be inserted or that contains the placeholder text.</param>
        /// <param name="replacement">The replacement object containing the placeholder key and JfYuWordPicture value with image data and dimensions.</param>
        /// <exception cref="ArgumentNullException">Thrown when run or replacement is null.</exception>
        /// <remarks>
        /// The image dimensions (Width and Height) in JfYuWordPicture are converted to EMUs (English Metric Units) before insertion.
        /// Typical values: 100 for small images, 200 for medium, 400 for larger images.
        /// </remarks>
        public static void CreatePicture(XWPFRun run, JfYuWordReplacement replacement)
        {
#if NETSTANDARD2_0
            ArgumentNullExceptionExtension.ThrowIfNull(run);
            ArgumentNullExceptionExtension.ThrowIfNull(replacement);
#else
            ArgumentNullException.ThrowIfNull(run);
            ArgumentNullException.ThrowIfNull(replacement);
#endif
            var placeholder = $"{{{replacement.Key}}}";
            var text = run.Text;
            var texts = text.Split([placeholder], StringSplitOptions.None);
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