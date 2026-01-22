using JfYu.WeChat;
using JfYu.WeChat.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace JfYu.UnitTests.WeChat
{
    /// <summary>
    /// Unit tests for ContainerBuilderExtensions - DI registration tests
    /// </summary>
    [Collection("WeChat")]
    public class ContainerBuilderExtensionsTests
    {
        [Fact]
        public void AddMiniProgram_WithValidConfiguration_RegistersServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var appId = "wx_test_app_id";
            var secret = "test_secret_key";

            // Act
            services.AddMiniProgram(options =>
            {
                options.AppId = appId;
                options.Secret = secret;
            });

            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var miniProgram = serviceProvider.GetService<IMiniProgram>();
            Assert.NotNull(miniProgram);
            Assert.IsType<MiniProgram>(miniProgram);
        }

        [Fact]
        public void AddMiniProgram_RegistersAsScoped()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddMiniProgram(options =>
            {
                options.AppId = "test";
                options.Secret = "test";
            });

            var serviceProvider = services.BuildServiceProvider();

            // Assert
            using var scope1 = serviceProvider.CreateScope();
            using var scope2 = serviceProvider.CreateScope();

            var instance1 = scope1.ServiceProvider.GetService<IMiniProgram>();
            var instance2 = scope1.ServiceProvider.GetService<IMiniProgram>();
            var instance3 = scope2.ServiceProvider.GetService<IMiniProgram>();

            // Same scope = same instance
            Assert.Same(instance1, instance2);
            // Different scope = different instance
            Assert.NotSame(instance1, instance3);
        }

        [Fact]
        public void AddMiniProgram_WithNullSetupAction_RegistersServicesWithDefaultOptions()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddMiniProgram(null);

            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var miniProgram = serviceProvider.GetService<IMiniProgram>();
            Assert.NotNull(miniProgram);

            var options = serviceProvider.GetService<IOptions<MiniProgramOptions>>();
            Assert.NotNull(options);
            Assert.NotNull(options.Value);
            // Default values should be empty strings
            Assert.Equal(string.Empty, options.Value.AppId);
            Assert.Equal(string.Empty, options.Value.Secret);
        }

        [Fact]
        public void AddMiniProgram_ConfiguresOptions_OptionsAreAccessible()
        {
            // Arrange
            var services = new ServiceCollection();
            var expectedAppId = "wx1234567890abcdef";
            var expectedSecret = "my_secret_key_123456";

            // Act
            services.AddMiniProgram(options =>
            {
                options.AppId = expectedAppId;
                options.Secret = expectedSecret;
            });

            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var options = serviceProvider.GetService<IOptions<MiniProgramOptions>>();
            Assert.NotNull(options);
            Assert.Equal(expectedAppId, options.Value.AppId);
            Assert.Equal(expectedSecret, options.Value.Secret);
        }

        [Fact]
        public void AddMiniProgram_RegistersJfYuHttpRequest()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddMiniProgram(options =>
            {
                options.AppId = "test";
                options.Secret = "test";
            });

            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var jfYuRequest = serviceProvider.GetService<JfYu.Request.IJfYuRequest>();
            Assert.NotNull(jfYuRequest);
        }

        [Fact]
        public void AddMiniProgram_MultipleRegistrations_LastConfigurationWins()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act - Register twice with different configs
            services.AddMiniProgram(options =>
            {
                options.AppId = "first_app_id";
                options.Secret = "first_secret";
            });

            services.AddMiniProgram(options =>
            {
                options.AppId = "second_app_id";
                options.Secret = "second_secret";
            });

            var serviceProvider = services.BuildServiceProvider();

            // Assert - Last registration should be used
            var options = serviceProvider.GetService<IOptions<MiniProgramOptions>>();
            Assert.NotNull(options);
            Assert.Equal("second_app_id", options.Value.AppId);
            Assert.Equal("second_secret", options.Value.Secret);
        }

        [Fact]
        public void AddMiniProgram_CanResolveMultipleTimes()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddMiniProgram(options =>
            {
                options.AppId = "test";
                options.Secret = "test";
            });

            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            // Act
            var instance1 = scope.ServiceProvider.GetService<IMiniProgram>();
            var instance2 = scope.ServiceProvider.GetService<IMiniProgram>();
            var instance3 = scope.ServiceProvider.GetService<IMiniProgram>();

            // Assert - All should resolve successfully
            Assert.NotNull(instance1);
            Assert.NotNull(instance2);
            Assert.NotNull(instance3);
            // All should be the same instance within the same scope
            Assert.Same(instance1, instance2);
            Assert.Same(instance2, instance3);
        }

        [Fact]
        public void AddMiniProgram_EmptyConfiguration_StillRegistersSuccessfully()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddMiniProgram(options => { });

            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var miniProgram = serviceProvider.GetService<IMiniProgram>();
            Assert.NotNull(miniProgram);

            var options = serviceProvider.GetService<IOptions<MiniProgramOptions>>();
            Assert.NotNull(options);
            Assert.NotNull(options.Value);
        }
    }
}
