using Newtonsoft.Json;

namespace JfYu.WeChat.Model.Response
{
    public class GetPhoneResponse : WechatBaseResponse
    {
        [JsonProperty(PropertyName = "phone_info")]
        public PhoneInfo? PhoneInfo { get; set; }
    }

    public class PhoneInfo
    { 
        public string? PhoneNumber { get; set; } 
         
        public string? PurePhoneNumber { get; set; }
         
        public string? CountryCode { get; set; } 
         
        public WaterMark? Watermark { get; set; }  
    }

    public class WaterMark
    { 
        public string? Appid { get; set; }
         
        public long Timestamp { get; set; }
    }
}
