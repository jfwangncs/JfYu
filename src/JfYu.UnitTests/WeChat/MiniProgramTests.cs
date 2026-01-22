using JfYu.Request;
using JfYu.WeChat;
using JfYu.WeChat.Model.Response;
using JfYu.WeChat.Options;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace JfYu.UnitTests.WeChat
{
    /// <summary>
    /// Unit tests for MiniProgram class - covers all methods and error scenarios
    /// </summary>
    [Collection("WeChat")]
    public class MiniProgramTests
    {
        private readonly Mock<IJfYuRequest> _mockRequest;
        private readonly MiniProgramOptions _options;
        private readonly MiniProgram _miniProgram;

        public MiniProgramTests()
        {
            _mockRequest = new Mock<IJfYuRequest>();
            _options = new MiniProgramOptions
            {
                AppId = "wx1234567890abcdef",
                Secret = "test_secret_key_12345678"
            };
            var optionsWrapper = Options.Create(_options);
            _miniProgram = new MiniProgram(_mockRequest.Object, optionsWrapper);
        }

        #region LoginAsync Tests

        [Fact]
        public async Task LoginAsync_ValidCode_ReturnsSuccessResponse()
        {
            // Arrange
            var code = "valid_test_code_12345";
            var expectedResponse = new WechatLoginResponse
            {
                ErrorCode = 0,
                ErrorMessage = "ok",
                OpenId = "test_openid_123",
                UnionId = "test_unionid_456",
                SessionKey = "test_session_key_789"
            };

            var responseJson = JsonConvert.SerializeObject(expectedResponse);
            _mockRequest.SetupSet(x => x.Url = It.IsAny<string>()).Verifiable();
            _mockRequest.Setup(x => x.SendAsync(It.IsAny<CancellationToken>())).ReturnsAsync(responseJson);

            // Act
            var result = await _miniProgram.LoginAsync(code).ConfigureAwait(true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.ErrorCode);
            Assert.Equal("test_openid_123", result.OpenId);
            Assert.Equal("test_unionid_456", result.UnionId);
            Assert.Equal("test_session_key_789", result.SessionKey);
            _mockRequest.VerifySet(x => x.Url = It.Is<string>(url =>
                url.Contains("sns/jscode2session") &&
                url.Contains($"appid={_options.AppId}") &&
                url.Contains($"secret={_options.Secret}") &&
                url.Contains($"js_code={code}") &&
                url.Contains("grant_type=authorization_code")));
        }

        [Fact]
        public async Task LoginAsync_InvalidCode_ReturnsErrorResponse()
        {
            // Arrange
            var code = "invalid_code";
            var expectedResponse = new WechatLoginResponse
            {
                ErrorCode = 40029,
                ErrorMessage = "invalid code"
            };

            var responseJson = JsonConvert.SerializeObject(expectedResponse);
            _mockRequest.SetupSet(x => x.Url = It.IsAny<string>()).Verifiable();
            _mockRequest.Setup(x => x.SendAsync(It.IsAny<CancellationToken>())).ReturnsAsync(responseJson);

            // Act
            var result = await _miniProgram.LoginAsync(code).ConfigureAwait(true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(40029, result.ErrorCode);
            Assert.Equal("invalid code", result.ErrorMessage);
            Assert.Null(result.OpenId);
        }

        [Fact]
        public async Task LoginAsync_NullCode_ThrowsArgumentNullException()
        {
            // Arrange
            string? code = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _miniProgram.LoginAsync(code!).ConfigureAwait(false);
            }).ConfigureAwait(true);
        }

        [Fact]
        public async Task LoginAsync_EmptyCode_ThrowsArgumentNullException()
        {
            // Arrange
            string code = string.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _miniProgram.LoginAsync(code).ConfigureAwait(false);
            }).ConfigureAwait(true);
        }

        [Fact]
        public async Task LoginAsync_WhitespaceCode_DoesNotThrow()
        {
            // Arrange
            string code = "   ";

            var expectedResponse = new WechatLoginResponse
            {
                ErrorCode = 40029,
                ErrorMessage = "invalid code"
            };

            var responseJson = JsonConvert.SerializeObject(expectedResponse);
            _mockRequest.SetupSet(x => x.Url = It.IsAny<string>()).Verifiable();
            _mockRequest.Setup(x => x.SendAsync(It.IsAny<CancellationToken>())).ReturnsAsync(responseJson);

            // Act
            var result = await _miniProgram.LoginAsync(code).ConfigureAwait(true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(40029, result.ErrorCode);
        }

        [Fact]
        public async Task LoginAsync_SystemBusy_ReturnsErrorCode()
        {
            // Arrange
            var code = "test_code";
            var expectedResponse = new WechatLoginResponse
            {
                ErrorCode = -1,
                ErrorMessage = "system busy"
            };

            var responseJson = JsonConvert.SerializeObject(expectedResponse);
            _mockRequest.SetupSet(x => x.Url = It.IsAny<string>()).Verifiable();
            _mockRequest.Setup(x => x.SendAsync(It.IsAny<CancellationToken>())).ReturnsAsync(responseJson);

            // Act
            var result = await _miniProgram.LoginAsync(code).ConfigureAwait(true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(-1, result.ErrorCode);
            Assert.Equal("system busy", result.ErrorMessage);
        }

        #endregion

        #region GetAccessTokenAsync Tests

        [Fact]
        public async Task GetAccessTokenAsync_Success_ReturnsValidToken()
        {
            // Arrange
            var expectedResponse = new AccessTokenResponse
            {
                AccessToken = "test_access_token_123456789",
                Expires = 7200
            };

            var responseJson = JsonConvert.SerializeObject(expectedResponse);
            _mockRequest.SetupSet(x => x.Url = It.IsAny<string>()).Verifiable();
            _mockRequest.Setup(x => x.SendAsync(It.IsAny<CancellationToken>())).ReturnsAsync(responseJson);

            // Act
            var result = await _miniProgram.GetAccessTokenAsync().ConfigureAwait(true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test_access_token_123456789", result.AccessToken);
            Assert.Equal(7200, result.Expires);
            _mockRequest.VerifySet(x => x.Url = It.Is<string>(url =>
                url.Contains("cgi-bin/token") &&
                url.Contains($"appid={_options.AppId}") &&
                url.Contains($"secret={_options.Secret}") &&
                url.Contains("grant_type=client_credential")));
        }

        [Fact]
        public async Task GetAccessTokenAsync_InvalidAppId_ReturnsNull()
        {
            // Arrange
            var responseJson = JsonConvert.SerializeObject(new
            {
                errcode = 40013,
                errmsg = "invalid appid"
            });

            _mockRequest.SetupSet(x => x.Url = It.IsAny<string>()).Verifiable();
            _mockRequest.Setup(x => x.SendAsync(It.IsAny<CancellationToken>())).ReturnsAsync(responseJson);

            // Act
            var result = await _miniProgram.GetAccessTokenAsync().ConfigureAwait(true);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.AccessToken);
        }

        [Fact]
        public async Task GetAccessTokenAsync_InvalidSecret_ReturnsNull()
        {
            // Arrange
            var responseJson = JsonConvert.SerializeObject(new
            {
                errcode = 40001,
                errmsg = "invalid secret"
            });

            _mockRequest.SetupSet(x => x.Url = It.IsAny<string>()).Verifiable();
            _mockRequest.Setup(x => x.SendAsync(It.IsAny<CancellationToken>())).ReturnsAsync(responseJson);

            // Act
            var result = await _miniProgram.GetAccessTokenAsync().ConfigureAwait(true);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.AccessToken);
        }

        [Fact]
        public async Task GetAccessTokenAsync_ExpiresIn7200Seconds_ReturnsCorrectExpiry()
        {
            // Arrange
            var expectedResponse = new AccessTokenResponse
            {
                AccessToken = "valid_token",
                Expires = 7200
            };

            var responseJson = JsonConvert.SerializeObject(expectedResponse);
            _mockRequest.SetupSet(x => x.Url = It.IsAny<string>()).Verifiable();
            _mockRequest.Setup(x => x.SendAsync(It.IsAny<CancellationToken>())).ReturnsAsync(responseJson);

            // Act
            var result = await _miniProgram.GetAccessTokenAsync().ConfigureAwait(true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(7200, result.Expires);
        }

        #endregion

        #region GetPhoneAsync Tests

        [Fact]
        public async Task GetPhoneAsync_ValidCode_ReturnsPhoneInfo()
        {
            // Arrange
            var code = "valid_phone_code_123";
            var accessTokenResponse = new AccessTokenResponse
            {
                AccessToken = "test_access_token_for_phone",
                Expires = 7200
            };

            var phoneResponse = new GetPhoneResponse
            {
                ErrorCode = 0,
                ErrorMessage = "ok",
                PhoneInfo = new PhoneInfo
                {
                    PhoneNumber = "+86 138****1234",
                    PurePhoneNumber = "13812341234",
                    CountryCode = "86",
                    Watermark = new WaterMark
                    {
                        Appid = _options.AppId,
                        Timestamp = 1234567890
                    }
                }
            };

            var accessTokenJson = JsonConvert.SerializeObject(accessTokenResponse);
            var phoneResponseJson = JsonConvert.SerializeObject(phoneResponse);

            _mockRequest.SetupSequence(x => x.SendAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(accessTokenJson)
                .ReturnsAsync(phoneResponseJson);

            _mockRequest.SetupSet(x => x.Url = It.IsAny<string>()).Verifiable();
            _mockRequest.SetupSet(x => x.Method = HttpMethod.Post).Verifiable();
            _mockRequest.SetupSet(x => x.RequestData = It.IsAny<string>()).Verifiable();

            // Act
            var result = await _miniProgram.GetPhoneAsync(code).ConfigureAwait(true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.ErrorCode);
            Assert.NotNull(result.PhoneInfo);
            Assert.Equal("13812341234", result.PhoneInfo.PurePhoneNumber);
            Assert.Equal("86", result.PhoneInfo.CountryCode);
            Assert.Equal("+86 138****1234", result.PhoneInfo.PhoneNumber);
            Assert.NotNull(result.PhoneInfo.Watermark);
            Assert.Equal(_options.AppId, result.PhoneInfo.Watermark.Appid);
            Assert.Equal(1234567890, result.PhoneInfo.Watermark.Timestamp);

            _mockRequest.VerifySet(x => x.Method = HttpMethod.Post, Times.Once);
            _mockRequest.VerifySet(x => x.RequestData = It.Is<string>(data =>
                data.Contains(code)), Times.Once);
        }

        [Fact]
        public async Task GetPhoneAsync_InvalidCode_ReturnsErrorResponse()
        {
            // Arrange
            var code = "invalid_phone_code";
            var accessTokenResponse = new AccessTokenResponse
            {
                AccessToken = "test_access_token",
                Expires = 7200
            };

            var phoneResponse = new GetPhoneResponse
            {
                ErrorCode = 40029,
                ErrorMessage = "invalid code",
                PhoneInfo = null
            };

            var accessTokenJson = JsonConvert.SerializeObject(accessTokenResponse);
            var phoneResponseJson = JsonConvert.SerializeObject(phoneResponse);

            _mockRequest.SetupSequence(x => x.SendAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(accessTokenJson)
                .ReturnsAsync(phoneResponseJson);

            _mockRequest.SetupSet(x => x.Url = It.IsAny<string>()).Verifiable();
            _mockRequest.SetupSet(x => x.Method = HttpMethod.Post).Verifiable();
            _mockRequest.SetupSet(x => x.RequestData = It.IsAny<string>()).Verifiable();

            // Act
            var result = await _miniProgram.GetPhoneAsync(code).ConfigureAwait(true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(40029, result.ErrorCode);
            Assert.Equal("invalid code", result.ErrorMessage);
            Assert.Null(result.PhoneInfo);
        }

        [Fact]
        public async Task GetPhoneAsync_ExpiredAccessToken_ReturnsError()
        {
            // Arrange
            var code = "valid_code";
            var accessTokenResponse = new AccessTokenResponse
            {
                AccessToken = "expired_token",
                Expires = 7200
            };

            var phoneResponse = new GetPhoneResponse
            {
                ErrorCode = 40001,
                ErrorMessage = "invalid access_token",
                PhoneInfo = null
            };

            var accessTokenJson = JsonConvert.SerializeObject(accessTokenResponse);
            var phoneResponseJson = JsonConvert.SerializeObject(phoneResponse);

            _mockRequest.SetupSequence(x => x.SendAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(accessTokenJson)
                .ReturnsAsync(phoneResponseJson);

            _mockRequest.SetupSet(x => x.Url = It.IsAny<string>()).Verifiable();
            _mockRequest.SetupSet(x => x.Method = HttpMethod.Post).Verifiable();
            _mockRequest.SetupSet(x => x.RequestData = It.IsAny<string>()).Verifiable();

            // Act
            var result = await _miniProgram.GetPhoneAsync(code).ConfigureAwait(true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(40001, result.ErrorCode);
            Assert.Equal("invalid access_token", result.ErrorMessage);
        }

        [Fact]
        public async Task GetPhoneAsync_CallsGetAccessTokenFirst_ThenUsesToken()
        {
            // Arrange
            var code = "test_code";
            var expectedAccessToken = "expected_access_token_value";
            var accessTokenResponse = new AccessTokenResponse
            {
                AccessToken = expectedAccessToken,
                Expires = 7200
            };

            var phoneResponse = new GetPhoneResponse
            {
                ErrorCode = 0,
                ErrorMessage = "ok",
                PhoneInfo = new PhoneInfo
                {
                    PurePhoneNumber = "13800138000"
                }
            };

            var accessTokenJson = JsonConvert.SerializeObject(accessTokenResponse);
            var phoneResponseJson = JsonConvert.SerializeObject(phoneResponse);

            _mockRequest.SetupSequence(x => x.SendAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(accessTokenJson)
                .ReturnsAsync(phoneResponseJson);

            string? capturedUrl = null;
            _mockRequest.SetupSet(x => x.Url = It.IsAny<string>())
                .Callback<string>(url => capturedUrl = url);
            _mockRequest.SetupSet(x => x.Method = HttpMethod.Post).Verifiable();
            _mockRequest.SetupSet(x => x.RequestData = It.IsAny<string>()).Verifiable();

            // Act
            var result = await _miniProgram.GetPhoneAsync(code).ConfigureAwait(true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.ErrorCode);
            Assert.NotNull(capturedUrl);
            Assert.Contains($"access_token={expectedAccessToken}", capturedUrl);
            Assert.Contains("wxa/business/getuserphonenumber", capturedUrl);
        }

        [Fact]
        public async Task GetPhoneAsync_NullAccessToken_StillMakesRequest()
        {
            // Arrange
            var code = "test_code";
            var accessTokenResponse = new AccessTokenResponse
            {
                AccessToken = null!,
                Expires = 0
            };

            var phoneResponse = new GetPhoneResponse
            {
                ErrorCode = 40001,
                ErrorMessage = "invalid access_token"
            };

            var accessTokenJson = JsonConvert.SerializeObject(accessTokenResponse);
            var phoneResponseJson = JsonConvert.SerializeObject(phoneResponse);

            _mockRequest.SetupSequence(x => x.SendAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(accessTokenJson)
                .ReturnsAsync(phoneResponseJson);

            _mockRequest.SetupSet(x => x.Url = It.IsAny<string>()).Verifiable();
            _mockRequest.SetupSet(x => x.Method = HttpMethod.Post).Verifiable();
            _mockRequest.SetupSet(x => x.RequestData = It.IsAny<string>()).Verifiable();

            // Act
            var result = await _miniProgram.GetPhoneAsync(code).ConfigureAwait(true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(40001, result.ErrorCode);
        }
        [Fact]
        public async Task GetPhoneAsync_NullAccessTokenData_StillMakesRequest()
        {
            // Arrange
            var code = "test_code";
            AccessTokenResponse accessTokenResponse = null!;

            var phoneResponse = new GetPhoneResponse
            {
                ErrorCode = 40001,
                ErrorMessage = "invalid access_token"
            };

            var accessTokenJson = JsonConvert.SerializeObject(accessTokenResponse);
            var phoneResponseJson = JsonConvert.SerializeObject(phoneResponse);

            _mockRequest.SetupSequence(x => x.SendAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(accessTokenJson)
                .ReturnsAsync(phoneResponseJson);

            _mockRequest.SetupSet(x => x.Url = It.IsAny<string>()).Verifiable();
            _mockRequest.SetupSet(x => x.Method = HttpMethod.Post).Verifiable();
            _mockRequest.SetupSet(x => x.RequestData = It.IsAny<string>()).Verifiable();

            // Act
            var result = await _miniProgram.GetPhoneAsync(code).ConfigureAwait(true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(40001, result.ErrorCode);
        }
        [Fact]
        public async Task GetPhoneAsync_WithWatermark_ReturnsCompleteInfo()
        {
            // Arrange
            var code = "test_code";
            var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var accessTokenResponse = new AccessTokenResponse
            {
                AccessToken = "valid_token",
                Expires = 7200
            };

            var phoneResponse = new GetPhoneResponse
            {
                ErrorCode = 0,
                ErrorMessage = "ok",
                PhoneInfo = new PhoneInfo
                {
                    PhoneNumber = "+86 138****5678",
                    PurePhoneNumber = "13812345678",
                    CountryCode = "86",
                    Watermark = new WaterMark
                    {
                        Appid = "wx1234567890abcdef",
                        Timestamp = currentTimestamp
                    }
                }
            };

            var accessTokenJson = JsonConvert.SerializeObject(accessTokenResponse);
            var phoneResponseJson = JsonConvert.SerializeObject(phoneResponse);

            _mockRequest.SetupSequence(x => x.SendAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(accessTokenJson)
                .ReturnsAsync(phoneResponseJson);

            _mockRequest.SetupSet(x => x.Url = It.IsAny<string>()).Verifiable();
            _mockRequest.SetupSet(x => x.Method = HttpMethod.Post).Verifiable();
            _mockRequest.SetupSet(x => x.RequestData = It.IsAny<string>()).Verifiable();

            // Act
            var result = await _miniProgram.GetPhoneAsync(code).ConfigureAwait(true);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.PhoneInfo);
            Assert.NotNull(result.PhoneInfo.Watermark);
            Assert.Equal("wx1234567890abcdef", result.PhoneInfo.Watermark.Appid);
            Assert.Equal(currentTimestamp, result.PhoneInfo.Watermark.Timestamp);
        }

        #endregion
    }
}
