using System.Net;
using System.Text.RegularExpressions;
using RecipeMatcher.Web.Data;

namespace RecipeMatcher.Web.Tests.IntegrationTests;

public class CreateRecipeValidationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CreateRecipeValidationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_Creates_with_empty_name_shows_error_and_does_not_save()
    {
        int countBefore;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureCreatedAsync();
            countBefore = db.Recipes.Count();
        }

        var getResponse = await _client.GetAsync("/recipes/create");
        getResponse.EnsureSuccessStatusCode();
        var createHtml = await getResponse.Content.ReadAsStringAsync();

        var token = GetAntiforgeryToken(createHtml);

        var form = new Dictionary<string, string>
        {
            ["Name"] = "",
            ["PreparationMinutes"] = "20",
            ["__RequestVerificationToken"] = token
        };

        var postResponse = await _client.PostAsync(
            "/recipes/create",
            new FormUrlEncodedContent(form));

        var html = await postResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
        Assert.Contains("Name", html, StringComparison.OrdinalIgnoreCase);
        Assert.True(
            html.Contains("required", StringComparison.OrdinalIgnoreCase)
            || html.Contains("field-validation-error", StringComparison.OrdinalIgnoreCase)
            || html.Contains("validation", StringComparison.OrdinalIgnoreCase),
            "Expected a validation error in the HTML");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var countAfter = db.Recipes.Count();
            Assert.Equal(countBefore, countAfter);
        }
    }

    private static string GetAntiforgeryToken(string html)
    {
        var match = Regex.Match(
            html,
            @"name=""__RequestVerificationToken""[^>]*value=""([^""]+)""",
            RegexOptions.IgnoreCase);

        Assert.True(match.Success, "Create page did not contain an antiforgery token.");
        return match.Groups[1].Value;
    }
}