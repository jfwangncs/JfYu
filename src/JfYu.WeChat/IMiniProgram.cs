using JfYu.WeChat.Model.Response;
using System.Threading.Tasks;

namespace JfYu.WeChat
{
    /// <summary>
    /// Interface for WeChat Mini Program API operations
    /// </summary>
    public interface IMiniProgram
    {
        /// <summary>
        /// Gets access token for server-to-server API calls
        /// </summary>
        /// <returns>Access token response with token and expiration</returns>
        Task<AccessTokenResponse?> GetAccessTokenAsync();

        /// <summary>
        /// Retrieves user's phone number using authorization code
        /// </summary>
        /// <param name="code">Authorization code from WeChat</param>
        /// <returns>Phone number information</returns>
        Task<GetPhoneResponse?> GetPhoneAsync(string code);

        /// <summary>
        /// Authenticates user login using authorization code
        /// </summary>
        /// <param name="code">Login authorization code from WeChat Mini Program</param>
        /// <returns>Login response with OpenId, UnionId and session key</returns>
        Task<WechatLoginResponse?> LoginAsync(string code);
    }
}