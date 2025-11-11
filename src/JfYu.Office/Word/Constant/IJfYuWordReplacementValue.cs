namespace JfYu.Office.Word.Constant
{
    /// <summary>
    /// Base interface for Word document replacement values.
    /// Implemented by JfYuWordString for text replacements and JfYuWordPicture for image insertions.
    /// </summary>
    public interface IJfYuWordReplacementValue
    {
    }

    /// <summary>
    /// Represents an image to be inserted into a Word document at a placeholder position.
    /// The image will replace the {key} placeholder and be embedded in the document.
    /// </summary>
    public class JfYuWordPicture : IJfYuWordReplacementValue
    {
        /// <summary>
        /// Gets or sets the image data as a byte array. Supports common formats like PNG, JPG, etc.
        /// </summary>
        public byte[] Bytes { get; set; } = [];

        /// <summary>
        /// Gets or sets the width of the inserted image in EMUs (English Metric Units).
        /// Common values: 100 = small, 200 = medium, 400 = large.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the inserted image in EMUs (English Metric Units).
        /// Common values: 100 = small, 200 = medium, 400 = large.
        /// </summary>
        public int Height { get; set; }
    }

    /// <summary>
    /// Represents text to be used as a replacement value in Word documents.
    /// The text will replace all occurrences of {key} placeholders in the template.
    /// </summary>
    public class JfYuWordString : IJfYuWordReplacementValue
    {
        /// <summary>
        /// Gets or sets the replacement text that will replace the placeholder.
        /// </summary>
        public string Text { get; set; } = "";
    }
}