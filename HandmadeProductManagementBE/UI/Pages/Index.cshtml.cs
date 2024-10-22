using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagementBE;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace UI.Pages
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public IndexModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public List<WeatherForecast> WeatherForecasts { get; set; }

        public async Task OnGet()
        {
            var response = await _httpClient.GetAsync("https://localhost:7159/weatherforecast");
            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                WeatherForecasts = JsonSerializer.Deserialize<List<WeatherForecast>>(responseData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            else
            {
                WeatherForecasts = [];
            }
        }
    }
}
