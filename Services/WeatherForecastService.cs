using System.Net.Http.Json;

class WeatherForecastService : IWeatherForecastService {
    private readonly HttpClient _httpClient;
    public WeatherForecastService(HttpClient httpClient) {
        _httpClient = httpClient;
    }

    public Task<WeatherForecast[]?> GetForecasts() {
        return _httpClient.GetFromJsonAsync<WeatherForecast[]>("sample-data/weather.json");
    }
}