using JfYu.WeChat.Constant;
using Xunit;

namespace JfYu.UnitTests.WeChat
{
    /// <summary>
    /// Unit tests for MiniProgramConstant - API endpoint constants
    /// </summary>
    [Collection("WeChat")]
    public class MiniProgramConstantTests
    {
        [Fact]
        public void Url_IsCorrectBaseUrl()
        {
            // Assert
            Assert.Equal("https://api.weixin.qq.com", MiniProgramConstant.Url);
        }

        [Fact]
        public void Url_StartsWithHttps()
        {
            // Assert
            Assert.StartsWith("https://", MiniProgramConstant.Url);
        }

        [Fact]
        public void LoginUrl_IsCorrectEndpoint()
        {
            // Assert
            Assert.Equal("sns/jscode2session", MiniProgramConstant.LoginUrl);
        }

        [Fact]
        public void GetAccessTokenUrl_IsCorrectEndpoint()
        {
            // Assert
            Assert.Equal("cgi-bin/token", MiniProgramConstant.GetAccessTokenUrl);
        }

        [Fact]
        public void GetPhonenUrl_IsCorrectEndpoint()
        {
            // Assert
            Assert.Equal("wxa/business/getuserphonenumber", MiniProgramConstant.GetPhoneUrl);
        }

        [Fact]
        public void AllUrls_AreNotNullOrEmpty()
        {
            // Assert
            Assert.False(string.IsNullOrEmpty(MiniProgramConstant.Url));
            Assert.False(string.IsNullOrEmpty(MiniProgramConstant.LoginUrl));
            Assert.False(string.IsNullOrEmpty(MiniProgramConstant.GetAccessTokenUrl));
            Assert.False(string.IsNullOrEmpty(MiniProgramConstant.GetPhoneUrl));
        }       

        [Fact]
        public void CanConstructFullUrls_WithoutDoubleSlashes()
        {
            // Act
            var loginUrl = $"{MiniProgramConstant.Url}/{MiniProgramConstant.LoginUrl}";
            var tokenUrl = $"{MiniProgramConstant.Url}/{MiniProgramConstant.GetAccessTokenUrl}";
            var phoneUrl = $"{MiniProgramConstant.Url}/{MiniProgramConstant.GetPhoneUrl}";

            // Assert
            Assert.DoesNotContain("//", loginUrl.Replace("https://", ""));
            Assert.DoesNotContain("//", tokenUrl.Replace("https://", ""));
            Assert.DoesNotContain("//", phoneUrl.Replace("https://", ""));
            Assert.StartsWith("https://", loginUrl);
            Assert.StartsWith("https://", tokenUrl);
            Assert.StartsWith("https://", phoneUrl);
        }

        [Fact]
        public void Constants_AreReadonlyAndCannotBeModified()
        {
            // This test verifies that constants are indeed readonly by checking their field info
            var urlField = typeof(MiniProgramConstant).GetField(nameof(MiniProgramConstant.Url));
            var loginUrlField = typeof(MiniProgramConstant).GetField(nameof(MiniProgramConstant.LoginUrl));
            var tokenUrlField = typeof(MiniProgramConstant).GetField(nameof(MiniProgramConstant.GetAccessTokenUrl));
            var phoneUrlField = typeof(MiniProgramConstant).GetField(nameof(MiniProgramConstant.GetPhoneUrl));

            // Assert
            Assert.NotNull(urlField);
            Assert.NotNull(loginUrlField);
            Assert.NotNull(tokenUrlField);
            Assert.NotNull(phoneUrlField);
            Assert.True(urlField.IsInitOnly);
            Assert.True(loginUrlField.IsInitOnly);
            Assert.True(tokenUrlField.IsInitOnly);
            Assert.True(phoneUrlField.IsInitOnly);
        }

        [Fact]
        public void LoginUrl_MatchesWeChatApiDocumentation()
        {
            // The official WeChat API documentation uses this endpoint
            Assert.Equal("sns/jscode2session", MiniProgramConstant.LoginUrl);
        }

        [Fact]
        public void GetAccessTokenUrl_MatchesWeChatApiDocumentation()
        {
            // The official WeChat API documentation uses this endpoint
            Assert.Equal("cgi-bin/token", MiniProgramConstant.GetAccessTokenUrl);
        }

        [Fact]
        public void GetPhoneUrl_MatchesWeChatApiDocumentation()
        {
            // The official WeChat API documentation uses this endpoint  
            Assert.Equal("wxa/business/getuserphonenumber", MiniProgramConstant.GetPhoneUrl);
        }

        [Fact]
        public void AllConstants_AreStatic()
        {
            // Verify all fields are static
            var fields = typeof(MiniProgramConstant).GetFields();

            foreach (var field in fields)
            {
                Assert.True(field.IsStatic, $"Field {field.Name} should be static");
            }
        }

        [Fact]
        public void ConstantClass_HasCorrectNumberOfConstants()
        {
            // Verify we have exactly 4 public readonly static fields
            var publicStaticFields = typeof(MiniProgramConstant)
                .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            Assert.Equal(4, publicStaticFields.Length);
        }
    }
}
