namespace JfYu.Office.Word.Constant
{
    /// <summary>
    /// Interface for defining replacement values in Word documents.
    /// </summary>
    public interface IJfYuWordReplacementValue
    {
    }

    /// <summary>
    /// Class representing a picture to be used as a replacement value in Word documents.
    /// </summary>
    public class JfYuWordPicture : IJfYuWordReplacementValue
    {
        /// <summary>
        /// Gets or sets the byte array of the picture.
        /// </summary>
        public byte[] Bytes { get; set; } = [];

        /// <summary>
        /// Gets or sets the width of the picture.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the picture.
        /// </summary>
        public int Height { get; set; }
    }

    /// <summary>
    /// Class representing a string to be used as a replacement value in Word documents.
    /// </summary>
    public class JfYuWordString : IJfYuWordReplacementValue
    {
        /// <summary>
        /// Gets or sets the text of the string.
        /// </summary>
        public string Text { get; set; } = "";
    }
}