namespace JfYu.WeChat.Constant
{
    /// <summary>
    /// Constants for WeChat Mini Program API endpoints
    /// </summary>
    public static class MiniProgramConstant
    {
        /// <summary>
        /// Base URL for WeChat API
        /// </summary>
        public static readonly string Url = "https://api.weixin.qq.com";

        /// <summary>
        /// Endpoint for user login authentication (code to session)
        /// </summary>
        public static readonly string LoginUrl = "sns/jscode2session";

        /// <summary>
        /// Endpoint for obtaining access token
        /// </summary>
        public static readonly string GetAccessTokenUrl = "cgi-bin/token";

        /// <summary>
        /// Endpoint for retrieving user phone number
        /// </summary>
        public static readonly string GetPhoneUrl = "wxa/business/getuserphonenumber";
    }
}
