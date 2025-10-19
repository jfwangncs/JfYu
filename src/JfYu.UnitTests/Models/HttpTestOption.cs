namespace JfYu.UnitTests.Models
{
    public class HttpTestOption
    {
        public string Url { get; set; } = "http://127.0.0.1:8000";
        public string ErrorSSL { get; set; } = "https://expired.badssl.com";
        public string ClientSSL { get; set; } = "https://client.badssl.com";
    }
}