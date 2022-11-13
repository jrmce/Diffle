public interface IWeatherForecastService {
    public Task<WeatherForecast[]?> GetForecasts();
}