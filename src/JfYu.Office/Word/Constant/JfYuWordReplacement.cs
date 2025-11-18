namespace JfYu.Office.Word.Constant
{
    /// <summary>
    /// Represents a key-value pair for placeholder replacement in Word documents.
    /// The Key property specifies the placeholder name (without braces), and Value contains either text or image data.
    /// Example: Key = "name", Value = new JfYuWordString { Text = "John Doe" }
    /// Template placeholder: {name} will be replaced with "John Doe"
    /// </summary>
    public class JfYuWordReplacement
    {
        /// <summary>
        /// Gets or sets the placeholder key (without braces). For example, "logo" for placeholder {logo}.
        /// </summary>
        public string Key { get; set; } = "";

        /// <summary>
        /// Gets or sets the replacement value. Can be either JfYuWordString for text replacement or JfYuWordPicture for image insertion.
        /// </summary>
        public IJfYuWordReplacementValue Value { get; set; } = new JfYuWordString();
    }
}