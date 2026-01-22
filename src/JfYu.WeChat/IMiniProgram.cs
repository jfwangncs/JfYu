using JfYu.WeChat.Model.Response;
using System.Threading.Tasks;

namespace JfYu.WeChat
{
    public interface IMiniProgram
    {
        Task<AccessTokenResponse?> GetAccessTokenAsync();
        Task<GetPhoneResponse?> GetPhoneAsync(string code);
        Task<WechatLoginResponse?> LoginAsync(string code);
    }
}