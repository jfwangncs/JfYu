using Newtonsoft.Json;

namespace JfYu.WeChat.Model.Response
{
    public class AccessTokenResponse
    {
        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; } = null!;

        [JsonProperty(PropertyName = "expires_in")]
        public int Expires { get; set; }
    }
}
