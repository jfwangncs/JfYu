namespace JfYu.WeChat.Options
{
    /// <summary>
    /// Configuration options for WeChat Mini Program
    /// </summary>
    public class MiniProgramOptions
    {
        /// <summary>
        /// Mini Program AppId (Application ID)
        /// </summary>
        public string AppId { get; set; } = string.Empty;

        /// <summary>
        /// Mini Program AppSecret (Application Secret Key)
        /// </summary>
        public string Secret { get; set; } = string.Empty;
    }
}
