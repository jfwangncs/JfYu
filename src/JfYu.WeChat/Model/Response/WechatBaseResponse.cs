using Newtonsoft.Json;

namespace JfYu.WeChat.Model.Response
{
    public class WechatBaseResponse
    {
        [JsonProperty(PropertyName = "errcode")]
        public int ErrorCode { get; set; }

        [JsonProperty(PropertyName = "errmsg")]
        public string? ErrorMessage { get; set; }
    }
}
