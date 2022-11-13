using Microsoft.AspNetCore.Components;

namespace Diffle 
{
    public class FetchDataBase : ComponentBase
    {
        public WeatherForecast[]? forecasts;

        [Inject]
        private IWeatherForecastService WeatherForecastService { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            forecasts = await WeatherForecastService.GetForecasts();
        }
    }
}

