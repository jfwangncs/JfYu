using JfYu.Request;
using JfYu.WeChat.Constant;
using JfYu.WeChat.Model.Response;
using JfYu.WeChat.Options;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace JfYu.WeChat
{
    /// <summary>
    /// Implementation of WeChat Mini Program API client
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the MiniProgram class
    /// </remarks>
    /// <param name="jfYuRequest">HTTP request service</param>
    /// <param name="miniProgramOptions">Mini Program configuration options</param>
    public class MiniProgram(IJfYuRequest jfYuRequest, IOptions<MiniProgramOptions> miniProgramOptions) : IMiniProgram
    {
        private readonly IJfYuRequest _jfYuRequest = jfYuRequest;
        private readonly MiniProgramOptions _miniProgramOptions = miniProgramOptions.Value;

        /// <summary>
        /// Authenticates user login and exchanges code for session
        /// </summary>
        /// <param name="code">Login authorization code from wx.login()</param>
        /// <returns>Login response containing OpenId, UnionId and session key</returns>
        /// <exception cref="ArgumentNullException">Thrown when code is null or empty</exception>
        public async Task<WechatLoginResponse?> LoginAsync(string code)
        {
            if (string.IsNullOrEmpty(code))
                throw new ArgumentNullException(nameof(code));

            _jfYuRequest.Url = $"{MiniProgramConstant.Url}/{MiniProgramConstant.LoginUrl}?appid={_miniProgramOptions.AppId}&secret={_miniProgramOptions.Secret}&js_code={code}&grant_type=authorization_code";
            var response = await _jfYuRequest.SendAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<WechatLoginResponse>(response);
        }

        /// <summary>
        /// Obtains access token for server API calls (valid for 2 hours)
        /// </summary>
        /// <returns>Access token response with token string and expiration time</returns>
        public async Task<AccessTokenResponse?> GetAccessTokenAsync()
        {
            _jfYuRequest.Url = $"{MiniProgramConstant.Url}/{MiniProgramConstant.GetAccessTokenUrl}?appid={_miniProgramOptions.AppId}&secret={_miniProgramOptions.Secret}&grant_type=client_credential";
            var response = await _jfYuRequest.SendAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<AccessTokenResponse>(response);
        }

        /// <summary>
        /// Retrieves user's phone number using authorization code from button callback
        /// </summary>
        /// <param name="code">Phone authorization code from getPhoneNumber button</param>
        /// <returns>Phone number information including country code and watermark</returns>
        public async Task<GetPhoneResponse?> GetPhoneAsync(string code)
        {
            var accessToken = await GetAccessTokenAsync().ConfigureAwait(false);

            _jfYuRequest.Url = $"{MiniProgramConstant.Url}/{MiniProgramConstant.GetPhonenUrl}?access_token={accessToken?.AccessToken}";
            _jfYuRequest.Method = HttpMethod.Post;
            _jfYuRequest.RequestData = JsonConvert.SerializeObject(new { code });
            var response = await _jfYuRequest.SendAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<GetPhoneResponse>(response);
        }
    }
}
