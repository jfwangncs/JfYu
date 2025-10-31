namespace JfYu.Office.Word.Constant
{
    /// <summary>
    /// Class representing a replacement in a Word document.
    /// </summary>
    public class JfYuWordReplacement
    {
        /// <summary>
        /// Gets or sets the key for the replacement.
        /// </summary>
        public string Key { get; set; } = "";

        /// <summary>
        /// Gets or sets the value for the replacement.
        /// </summary>
        public IJfYuWordReplacementValue Value { get; set; } = new JfYuWordString();
    }
}