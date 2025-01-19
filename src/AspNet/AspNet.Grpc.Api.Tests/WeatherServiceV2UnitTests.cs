using AspNet.Grpc.Api.Services;
using AspNet.Library.Protos;
using Grpc.Core;
using NSubstitute;

namespace AspNet.Grpc.Api.Tests
{
    public class WeatherServiceV2UnitTests
    {
        [Fact]
        public async Task GetWeatherForecastReturnsWeatherForecastResponse()
        {
            // Arrange
            var service = new WeatherServiceV2();
            var serverCallContext = Substitute.For<ServerCallContext>();

            // Act
            var result = await service.GetWeatherForecast(new WeatherForecastRequestV2(), serverCallContext);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Forecasts.Count > 0, "The collection count should be greater than 0.");
        }
    }
}
