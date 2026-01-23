using JfYu.WeChat.Model.Response;
using Newtonsoft.Json;
using Xunit;

namespace JfYu.UnitTests.WeChat
{
    /// <summary>
    /// Unit tests for response models - JSON serialization and property tests
    /// </summary>
    [Collection("WeChat")]
    public class ResponseModelsTests
    {
        #region WechatBaseResponse Tests

        [Fact]
        public void WechatBaseResponse_DefaultValues_AreCorrect()
        {
            // Arrange & Act
            var response = new WechatBaseResponse();

            // Assert
            Assert.Equal(0, response.ErrorCode);
            Assert.Null(response.ErrorMessage);
        }

        [Fact]
        public void WechatBaseResponse_SetProperties_StoresCorrectly()
        {
            // Arrange & Act
            var response = new WechatBaseResponse
            {
                ErrorCode = 40029,
                ErrorMessage = "invalid code"
            };

            // Assert
            Assert.Equal(40029, response.ErrorCode);
            Assert.Equal("invalid code", response.ErrorMessage);
        }

        [Fact]
        public void WechatBaseResponse_JsonDeserialization_WithErrorCode()
        {
            // Arrange
            var json = @"{""errcode"": -1, ""errmsg"": ""system busy""}";

            // Act
            var response = JsonConvert.DeserializeObject<WechatBaseResponse>(json);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(-1, response.ErrorCode);
            Assert.Equal("system busy", response.ErrorMessage);
        }

        [Fact]
        public void WechatBaseResponse_JsonSerialization_UsesCorrectPropertyNames()
        {
            // Arrange
            var response = new WechatBaseResponse
            {
                ErrorCode = 40001,
                ErrorMessage = "invalid credential"
            };

            // Act
            var json = JsonConvert.SerializeObject(response);

            // Assert
            Assert.Contains("\"errcode\"", json);
            Assert.Contains("\"errmsg\"", json);
            Assert.Contains("40001", json);
            Assert.Contains("invalid credential", json);
        }

        #endregion

        #region AccessTokenResponse Tests

        [Fact]
        public void AccessTokenResponse_DefaultValues_AreCorrect()
        {
            // Arrange & Act
            var response = new AccessTokenResponse();

            // Assert
            Assert.Null(response.AccessToken);
            Assert.Equal(0, response.Expires);
        }

        [Fact]
        public void AccessTokenResponse_SetProperties_StoresCorrectly()
        {
            // Arrange & Act
            var response = new AccessTokenResponse
            {
                AccessToken = "test_token_123",
                Expires = 7200
            };

            // Assert
            Assert.Equal("test_token_123", response.AccessToken);
            Assert.Equal(7200, response.Expires);
        }

        [Fact]
        public void AccessTokenResponse_JsonDeserialization_ParsesCorrectly()
        {
            // Arrange
            var json = @"{""access_token"": ""my_access_token"", ""expires_in"": 7200}";

            // Act
            var response = JsonConvert.DeserializeObject<AccessTokenResponse>(json);

            // Assert
            Assert.NotNull(response);
            Assert.Equal("my_access_token", response.AccessToken);
            Assert.Equal(7200, response.Expires);
        }

        [Fact]
        public void AccessTokenResponse_JsonSerialization_UsesCorrectPropertyNames()
        {
            // Arrange
            var response = new AccessTokenResponse
            {
                AccessToken = "serialization_test_token",
                Expires = 3600
            };

            // Act
            var json = JsonConvert.SerializeObject(response);

            // Assert
            Assert.Contains("\"access_token\"", json);
            Assert.Contains("\"expires_in\"", json);
            Assert.Contains("serialization_test_token", json);
            Assert.Contains("3600", json);
        }

        [Fact]
        public void AccessTokenResponse_WithVeryLongToken_HandlesCorrectly()
        {
            // Arrange
            var longToken = new string('a', 5000);
            var response = new AccessTokenResponse
            {
                AccessToken = longToken,
                Expires = 7200
            };

            // Act
            var json = JsonConvert.SerializeObject(response);
            var deserialized = JsonConvert.DeserializeObject<AccessTokenResponse>(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(5000, deserialized.AccessToken.Length);
            Assert.Equal(longToken, deserialized.AccessToken);
        }

        #endregion

        #region WechatLoginResponse Tests

        [Fact]
        public void WechatLoginResponse_DefaultValues_AreCorrect()
        {
            // Arrange & Act
            var response = new WechatLoginResponse();

            // Assert
            Assert.Null(response.OpenId);
            Assert.Null(response.UnionId);
            Assert.Null(response.SessionKey);
            Assert.Equal(0, response.ErrorCode);
            Assert.Null(response.ErrorMessage);
        }

        [Fact]
        public void WechatLoginResponse_SetProperties_StoresCorrectly()
        {
            // Arrange & Act
            var response = new WechatLoginResponse
            {
                OpenId = "test_openid_123",
                UnionId = "test_unionid_456",
                SessionKey = "test_session_789",
                ErrorCode = 0,
                ErrorMessage = "ok"
            };

            // Assert
            Assert.Equal("test_openid_123", response.OpenId);
            Assert.Equal("test_unionid_456", response.UnionId);
            Assert.Equal("test_session_789", response.SessionKey);
            Assert.Equal(0, response.ErrorCode);
            Assert.Equal("ok", response.ErrorMessage);
        }

        [Fact]
        public void WechatLoginResponse_JsonDeserialization_SuccessResponse()
        {
            // Arrange
            var json = @"{
                ""openid"": ""o6_bmjrPTlm6_2sgVt7hMZOPfL2M"",
                ""session_key"": ""tiihtNczf5v6AKRyjwEUhQ=="",// gitleaks:allow
                ""unionid"": ""oGZUI0egBJY1zhBYw2KhdUfwVJJE"",
                ""errcode"": 0,
                ""errmsg"": ""ok""
            }";

            // Act
            var response = JsonConvert.DeserializeObject<WechatLoginResponse>(json);

            // Assert
            Assert.NotNull(response);
            Assert.Equal("o6_bmjrPTlm6_2sgVt7hMZOPfL2M", response.OpenId);
            Assert.Equal("tiihtNczf5v6AKRyjwEUhQ==", response.SessionKey);
            Assert.Equal("oGZUI0egBJY1zhBYw2KhdUfwVJJE", response.UnionId);
            Assert.Equal(0, response.ErrorCode);
            Assert.Equal("ok", response.ErrorMessage);
        }

        [Fact]
        public void WechatLoginResponse_JsonDeserialization_ErrorResponse()
        {
            // Arrange
            var json = @"{""errcode"": 40029, ""errmsg"": ""invalid code""}";

            // Act
            var response = JsonConvert.DeserializeObject<WechatLoginResponse>(json);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(40029, response.ErrorCode);
            Assert.Equal("invalid code", response.ErrorMessage);
            Assert.Null(response.OpenId);
            Assert.Null(response.UnionId);
            Assert.Null(response.SessionKey);
        }

        [Fact]
        public void WechatLoginResponse_JsonSerialization_UsesCorrectPropertyNames()
        {
            // Arrange
            var response = new WechatLoginResponse
            {
                OpenId = "test_openid",
                SessionKey = "test_session",
                UnionId = "test_unionid"
            };

            // Act
            var json = JsonConvert.SerializeObject(response);

            // Assert
            Assert.Contains("\"OpenId\"", json);
            Assert.Contains("\"session_key\"", json);
            Assert.Contains("\"UnionId\"", json);
        }

        [Fact]
        public void WechatLoginResponse_InheritsFromBaseResponse()
        {
            // Arrange & Act
            var response = new WechatLoginResponse();

            // Assert
            Assert.IsType<WechatBaseResponse>(response, exactMatch: false);
        }

        #endregion

        #region GetPhoneResponse Tests

        [Fact]
        public void GetPhoneResponse_DefaultValues_AreCorrect()
        {
            // Arrange & Act
            var response = new GetPhoneResponse();

            // Assert
            Assert.Null(response.PhoneInfo);
            Assert.Equal(0, response.ErrorCode);
            Assert.Null(response.ErrorMessage);
        }

        [Fact]
        public void GetPhoneResponse_SetPhoneInfo_StoresCorrectly()
        {
            // Arrange & Act
            var response = new GetPhoneResponse
            {
                PhoneInfo = new PhoneInfo
                {
                    PhoneNumber = "+86 138****1234",
                    PurePhoneNumber = "13812341234",
                    CountryCode = "86"
                }
            };

            // Assert
            Assert.NotNull(response.PhoneInfo);
            Assert.Equal("+86 138****1234", response.PhoneInfo.PhoneNumber);
            Assert.Equal("13812341234", response.PhoneInfo.PurePhoneNumber);
            Assert.Equal("86", response.PhoneInfo.CountryCode);
        }

        [Fact]
        public void GetPhoneResponse_JsonDeserialization_CompleteResponse()
        {
            // Arrange
            var json = @"{
                ""errcode"": 0,
                ""errmsg"": ""ok"",
                ""phone_info"": {
                    ""phoneNumber"": ""+86 138****5678"",
                    ""purePhoneNumber"": ""13812345678"",
                    ""countryCode"": ""86"",
                    ""watermark"": {
                        ""appid"": ""wx1234567890"",
                        ""timestamp"": 1234567890
                    }
                }
            }";

            // Act
            var response = JsonConvert.DeserializeObject<GetPhoneResponse>(json);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(0, response.ErrorCode);
            Assert.Equal("ok", response.ErrorMessage);
            Assert.NotNull(response.PhoneInfo);
            Assert.Equal("+86 138****5678", response.PhoneInfo.PhoneNumber);
            Assert.Equal("13812345678", response.PhoneInfo.PurePhoneNumber);
            Assert.Equal("86", response.PhoneInfo.CountryCode);
            Assert.NotNull(response.PhoneInfo.Watermark);
            Assert.Equal("wx1234567890", response.PhoneInfo.Watermark.Appid);
            Assert.Equal(1234567890, response.PhoneInfo.Watermark.Timestamp);
        }

        [Fact]
        public void GetPhoneResponse_JsonDeserialization_ErrorResponse()
        {
            // Arrange
            var json = @"{""errcode"": 40029, ""errmsg"": ""invalid code""}";

            // Act
            var response = JsonConvert.DeserializeObject<GetPhoneResponse>(json);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(40029, response.ErrorCode);
            Assert.Equal("invalid code", response.ErrorMessage);
            Assert.Null(response.PhoneInfo);
        }

        [Fact]
        public void GetPhoneResponse_InheritsFromBaseResponse()
        {
            // Arrange & Act
            var response = new GetPhoneResponse();

            // Assert
            Assert.IsType<WechatBaseResponse>(response, exactMatch: false);
        }

        #endregion

        #region PhoneInfo Tests

        [Fact]
        public void PhoneInfo_DefaultValues_AreNull()
        {
            // Arrange & Act
            var phoneInfo = new PhoneInfo();

            // Assert
            Assert.Null(phoneInfo.PhoneNumber);
            Assert.Null(phoneInfo.PurePhoneNumber);
            Assert.Null(phoneInfo.CountryCode);
            Assert.Null(phoneInfo.Watermark);
        }

        [Fact]
        public void PhoneInfo_SetAllProperties_StoresCorrectly()
        {
            // Arrange & Act
            var phoneInfo = new PhoneInfo
            {
                PhoneNumber = "+86 138****1234",
                PurePhoneNumber = "13812341234",
                CountryCode = "86",
                Watermark = new WaterMark
                {
                    Appid = "wx123",
                    Timestamp = 1234567890
                }
            };

            // Assert
            Assert.Equal("+86 138****1234", phoneInfo.PhoneNumber);
            Assert.Equal("13812341234", phoneInfo.PurePhoneNumber);
            Assert.Equal("86", phoneInfo.CountryCode);
            Assert.NotNull(phoneInfo.Watermark);
            Assert.Equal("wx123", phoneInfo.Watermark.Appid);
            Assert.Equal(1234567890, phoneInfo.Watermark.Timestamp);
        }

        [Fact]
        public void PhoneInfo_DifferentCountryCodes_HandledCorrectly()
        {
            // Arrange & Act
            var chinaPhone = new PhoneInfo { CountryCode = "86" };
            var usPhone = new PhoneInfo { CountryCode = "1" };
            var ukPhone = new PhoneInfo { CountryCode = "44" };

            // Assert
            Assert.Equal("86", chinaPhone.CountryCode);
            Assert.Equal("1", usPhone.CountryCode);
            Assert.Equal("44", ukPhone.CountryCode);
        }

        [Fact]
        public void PhoneInfo_WithoutWatermark_IsValid()
        {
            // Arrange & Act
            var phoneInfo = new PhoneInfo
            {
                PhoneNumber = "+1 555****1234",
                PurePhoneNumber = "5551234567",
                CountryCode = "1"
            };

            // Assert
            Assert.NotNull(phoneInfo);
            Assert.Null(phoneInfo.Watermark);
        }

        #endregion

        #region WaterMark Tests

        [Fact]
        public void WaterMark_DefaultValues_AreCorrect()
        {
            // Arrange & Act
            var watermark = new WaterMark();

            // Assert
            Assert.Null(watermark.Appid);
            Assert.Equal(0, watermark.Timestamp);
        }

        [Fact]
        public void WaterMark_SetProperties_StoresCorrectly()
        {
            // Arrange & Act
            var watermark = new WaterMark
            {
                Appid = "wx1234567890abcdef",
                Timestamp = 1609459200
            };

            // Assert
            Assert.Equal("wx1234567890abcdef", watermark.Appid);
            Assert.Equal(1609459200, watermark.Timestamp);
        }

        [Fact]
        public void WaterMark_LargeTimestamp_HandlesCorrectly()
        {
            // Arrange
            var largeTimestamp = long.MaxValue - 1000;

            // Act
            var watermark = new WaterMark
            {
                Timestamp = largeTimestamp
            };

            // Assert
            Assert.Equal(largeTimestamp, watermark.Timestamp);
        }

        [Fact]
        public void WaterMark_NegativeTimestamp_HandlesCorrectly()
        {
            // Arrange & Act
            var watermark = new WaterMark
            {
                Timestamp = -1
            };

            // Assert
            Assert.Equal(-1, watermark.Timestamp);
        }

        [Fact]
        public void WaterMark_JsonSerialization_RoundTrip()
        {
            // Arrange
            var originalWatermark = new WaterMark
            {
                Appid = "wx_test_app",
                Timestamp = 1234567890123
            };

            // Act
            var json = JsonConvert.SerializeObject(originalWatermark);
            var deserializedWatermark = JsonConvert.DeserializeObject<WaterMark>(json);

            // Assert
            Assert.NotNull(deserializedWatermark);
            Assert.Equal(originalWatermark.Appid, deserializedWatermark.Appid);
            Assert.Equal(originalWatermark.Timestamp, deserializedWatermark.Timestamp);
        }

        #endregion

        #region Complex JSON Scenarios

        [Fact]
        public void ComplexResponse_WithAllFields_DeserializesCorrectly()
        {
            // Arrange
            var json = @"{
                ""errcode"": 0,
                ""errmsg"": ""ok"",
                ""phone_info"": {
                    ""phoneNumber"": ""+86 138****1234"",
                    ""purePhoneNumber"": ""13812341234"",
                    ""countryCode"": ""86"",
                    ""watermark"": {
                        ""appid"": ""wx1234567890abcdef"",
                        ""timestamp"": 1234567890
                    }
                }
            }";

            // Act
            var response = JsonConvert.DeserializeObject<GetPhoneResponse>(json);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(0, response.ErrorCode);
            Assert.NotNull(response.PhoneInfo);
            Assert.NotNull(response.PhoneInfo.Watermark);
            Assert.Equal("wx1234567890abcdef", response.PhoneInfo.Watermark.Appid);
        }

        [Fact]
        public void MissingOptionalFields_DeserializesWithNulls()
        {
            // Arrange
            var json = @"{""errcode"": 0}";

            // Act
            var response = JsonConvert.DeserializeObject<WechatLoginResponse>(json);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(0, response.ErrorCode);
            Assert.Null(response.ErrorMessage);
            Assert.Null(response.OpenId);
            Assert.Null(response.UnionId);
            Assert.Null(response.SessionKey);
        }

        [Fact]
        public void EmptyJson_DeserializesWithDefaults()
        {
            // Arrange
            var json = @"{}";

            // Act
            var response = JsonConvert.DeserializeObject<WechatBaseResponse>(json);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(0, response.ErrorCode);
            Assert.Null(response.ErrorMessage);
        }

        [Fact]
        public void ExtraJsonFields_AreIgnored()
        {
            // Arrange
            var json = @"{
                ""errcode"": 0,
                ""errmsg"": ""ok"",
                ""extra_field"": ""should_be_ignored"",
                ""another_unknown"": 123
            }";

            // Act
            var response = JsonConvert.DeserializeObject<WechatBaseResponse>(json);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(0, response.ErrorCode);
            Assert.Equal("ok", response.ErrorMessage);
        }

        #endregion
    }
}
