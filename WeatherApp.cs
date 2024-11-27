using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Net.Http;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
var app = builder.Build();
app.UseRouting();
app.UseStaticFiles();
app.UseEndpoints(endpoints =>
{
    endpoints.MapGet("/", async context =>
    {
        await context.Response.WriteAsync(@"
            <html>
            <head>
                <title>Simple Weather App</title>
            </head>
            <body>
                <h1>Weather App</h1>
                <form method='post' action='/get-weather'>
                    <input type='text' name='city' placeholder='Enter city name' required>
                    <button type='submit'>Get Weather</button>
                </form>
            </body>
            </html>
        ");
    });

    endpoints.MapPost("/get-weather", async context =>
    {
        var form = await context.Request.ReadFormAsync();
        string city = form["city"];

        if (string.IsNullOrEmpty(city))
        {
            await context.Response.WriteAsync("Please provide a valid city name. <a href='/'>Try again</a>");
            return;
        }

        string apiKey = "c3aeddacff29ebf82e72021835c84c1e";
        string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&units=metric&appid={apiKey}";

        using var client = new HttpClient();
        HttpResponseMessage response = await client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var jsonString = await response.Content.ReadAsStringAsync();
            dynamic weatherData = JsonConvert.DeserializeObject(jsonString);

            string weatherHtml = $@"
                <html>
                <head>
                    <title>Weather Result</title>
                </head>
                <body>
                    <h1>Weather in {weatherData.name}</h1>
                    <p><strong>Description:</strong> {weatherData.weather[0].description}</p>
                    <p><strong>Temperature:</strong> {weatherData.main.temp} °C</p>
                    <p><strong>Feels Like:</strong> {weatherData.main.feels_like} °C</p>
                    <p><strong>Humidity:</strong> {weatherData.main.humidity} %</p>
                    <a href='/'>Search Again</a>
                </body>
                </html>
            ";

            await context.Response.WriteAsync(weatherHtml);
        }
        else
        {
            await context.Response.WriteAsync($"City not found. <a href='/'>Try again</a>");
        }
    });
});

app.Run();
