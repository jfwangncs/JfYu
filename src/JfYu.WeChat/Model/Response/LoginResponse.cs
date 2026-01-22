using Newtonsoft.Json;

namespace JfYu.WeChat.Model.Response
{
    public class WechatLoginResponse : WechatBaseResponse
    {
        public string? OpenId { get; set; }

        public string? UnionId { get; set; }

        [JsonProperty(PropertyName = "session_key")]
        public string? SessionKey { get; set; }
    }
}
