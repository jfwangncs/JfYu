using JfYu.WeChat.Options;
using Xunit;

namespace JfYu.UnitTests.WeChat
{
    /// <summary>
    /// Unit tests for MiniProgramOptions - configuration model tests
    /// </summary>
    [Collection("WeChat")]
    public class MiniProgramOptionsTests
    {
        [Fact]
        public void MiniProgramOptions_DefaultValues_AreEmptyStrings()
        {
            // Arrange & Act
            var options = new MiniProgramOptions();

            // Assert
            Assert.NotNull(options.AppId);
            Assert.NotNull(options.Secret);
            Assert.Equal(string.Empty, options.AppId);
            Assert.Equal(string.Empty, options.Secret);
        }

        [Fact]
        public void MiniProgramOptions_SetAppId_StoresValue()
        {
            // Arrange
            var options = new MiniProgramOptions();
            var expectedAppId = "wx1234567890abcdef";

            // Act
            options.AppId = expectedAppId;

            // Assert
            Assert.Equal(expectedAppId, options.AppId);
        }

        [Fact]
        public void MiniProgramOptions_SetSecret_StoresValue()
        {
            // Arrange
            var options = new MiniProgramOptions();
            var expectedSecret = "my_secret_key_123456";// gitleaks:allow

            // Act
            options.Secret = expectedSecret;

            // Assert
            Assert.Equal(expectedSecret, options.Secret);
        }

        [Fact]
        public void MiniProgramOptions_SetBothProperties_BothStored()
        {
            // Arrange
            var options = new MiniProgramOptions();
            var expectedAppId = "wx9876543210fedcba";
            var expectedSecret = "another_secret_key";// gitleaks:allow

            // Act
            options.AppId = expectedAppId;
            options.Secret = expectedSecret;

            // Assert
            Assert.Equal(expectedAppId, options.AppId);
            Assert.Equal(expectedSecret, options.Secret);
        }

        [Fact]
        public void MiniProgramOptions_ObjectInitializer_WorksCorrectly()
        {
            // Arrange & Act
            var options = new MiniProgramOptions
            {
                AppId = "wx_init_test",
                Secret = "init_secret_test"
            };

            // Assert
            Assert.Equal("wx_init_test", options.AppId);
            Assert.Equal("init_secret_test", options.Secret);
        }

        [Fact]
        public void MiniProgramOptions_CanSetNullValues()
        {
            // Arrange
            var options = new MiniProgramOptions
            {
                AppId = "test",
                Secret = "test"
            };

            // Act
            options.AppId = null!;
            options.Secret = null!;

            // Assert
            Assert.Null(options.AppId);
            Assert.Null(options.Secret);
        }

        [Fact]
        public void MiniProgramOptions_CanBeModifiedAfterCreation()
        {
            // Arrange
            var options = new MiniProgramOptions
            {
                AppId = "initial_appid",
                Secret = "initial_secret"
            };

            // Act
            options.AppId = "modified_appid";
            options.Secret = "modified_secret";

            // Assert
            Assert.Equal("modified_appid", options.AppId);
            Assert.Equal("modified_secret", options.Secret);
        }

        [Fact]
        public void MiniProgramOptions_SupportsEmptyStringValues()
        {
            // Arrange & Act
            var options = new MiniProgramOptions
            {
                AppId = "",
                Secret = ""
            };

            // Assert
            Assert.Equal(string.Empty, options.AppId);
            Assert.Equal(string.Empty, options.Secret);
        }

        [Fact]
        public void MiniProgramOptions_SupportsWhitespaceValues()
        {
            // Arrange & Act
            var options = new MiniProgramOptions
            {
                AppId = "   ",
                Secret = "\t\n"
            };

            // Assert
            Assert.Equal("   ", options.AppId);
            Assert.Equal("\t\n", options.Secret);
        }

        [Fact]
        public void MiniProgramOptions_SupportsSpecialCharacters()
        {
            // Arrange & Act
            var options = new MiniProgramOptions
            {
                AppId = "wx_!@#$%^&*()",
                Secret = "secret_with_unicode_你好世界"
            };

            // Assert
            Assert.Equal("wx_!@#$%^&*()", options.AppId);
            Assert.Equal("secret_with_unicode_你好世界", options.Secret);
        }

        [Fact]
        public void MiniProgramOptions_SupportsVeryLongStrings()
        {
            // Arrange
            var longAppId = new string('A', 1000);
            var longSecret = new string('B', 2000);

            // Act
            var options = new MiniProgramOptions
            {
                AppId = longAppId,
                Secret = longSecret
            };

            // Assert
            Assert.Equal(1000, options.AppId.Length);
            Assert.Equal(2000, options.Secret.Length);
            Assert.Equal(longAppId, options.AppId);
            Assert.Equal(longSecret, options.Secret);
        }
    }
}
