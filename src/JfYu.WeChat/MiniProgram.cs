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
    public class MiniProgram : IMiniProgram
    {
        private readonly IJfYuRequest _jfYuRequest;
        private readonly MiniProgramOptions _miniProgramOptions;

        public MiniProgram(IJfYuRequest jfYuRequest, IOptions<MiniProgramOptions> miniProgramOptions)
        {
            _jfYuRequest = jfYuRequest;
            _miniProgramOptions = miniProgramOptions.Value;
        }

        public async Task<WechatLoginResponse?> LoginAsync(string code)
        {
            if (string.IsNullOrEmpty(code))
                throw new ArgumentNullException(nameof(code));

            _jfYuRequest.Url = $"{MiniProgramConstant.Url}/{MiniProgramConstant.LoginUrl}?appid={_miniProgramOptions.AppId}&secret={_miniProgramOptions.Secret}&js_code={code}&grant_type=authorization_code";
            var response = await _jfYuRequest.SendAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<WechatLoginResponse>(response);
        }

        public async Task<AccessTokenResponse?> GetAccessTokenAsync()
        {
            _jfYuRequest.Url = $"{MiniProgramConstant.Url}/{MiniProgramConstant.GetAccessTokenUrl}?appid={_miniProgramOptions.AppId}&secret={_miniProgramOptions.Secret}&grant_type=client_credential";
            var response = await _jfYuRequest.SendAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<AccessTokenResponse>(response);
        }

        public async Task<GetPhoneResponse?> GetPhoneAsync(string code)
        {
            var accessToken = await GetAccessTokenAsync();

            _jfYuRequest.Url = $"{MiniProgramConstant.Url}/{MiniProgramConstant.GetPhonenUrl}?access_token={accessToken?.AccessToken}";
            _jfYuRequest.Method = HttpMethod.Post;
            _jfYuRequest.RequestData = JsonConvert.SerializeObject(new { code });
            var response = await _jfYuRequest.SendAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<GetPhoneResponse>(response);
        }
    }
}
