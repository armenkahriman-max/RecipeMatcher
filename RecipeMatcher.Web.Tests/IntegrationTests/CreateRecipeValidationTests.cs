using System.Net;
using RecipeMatcher.Web.Data;

namespace RecipeMatcher.Web.Tests.IntegrationTests;

public class CreateRecipeValidationTests : IntegrationTestBase
{
    public CreateRecipeValidationTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Post_Creates_with_empty_name_shows_error_and_does_not_save()
    {
        await ResetDatabaseAsync();

        var token = await GetAntiforgeryTokenAsync("/recipes/create");
        var form = new Dictionary<string, string>
        {
            ["Name"] = "",
            ["PreparationMinutes"] = "20",
            ["__RequestVerificationToken"] = token
        };

        var postResponse = await Client.PostAsync(
            "/recipes/create",
            Form(form));

        var html = await postResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
        Assert.Contains("Name", html, StringComparison.OrdinalIgnoreCase);
        Assert.True(
            html.Contains("required", StringComparison.OrdinalIgnoreCase)
            || html.Contains("field-validation-error", StringComparison.OrdinalIgnoreCase)
            || html.Contains("validation", StringComparison.OrdinalIgnoreCase),
            "Expected a validation error in the HTML");

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(0, db.Recipes.Count());

    }

    [Fact]
    public async Task Post_Creates_with_too_long_name_shows_error_and_does_not_save()
    {
        await ResetDatabaseAsync();

        var token = await GetAntiforgeryTokenAsync("/recipes/create");
        var form = new Dictionary<string, string>
        {
            ["Name"] = new string('A', 300),
            ["PreparationMinutes"] = "20",
            ["__RequestVerificationToken"] = token
        };

        var postResponse = await Client.PostAsync("/recipes/create", Form(form));
        var html = await postResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
        Assert.True(
            html.Contains("maximum", StringComparison.OrdinalIgnoreCase)
            || html.Contains("max length", StringComparison.OrdinalIgnoreCase)
            || html.Contains("too long", StringComparison.OrdinalIgnoreCase)
            || html.Contains("characters", StringComparison.OrdinalIgnoreCase)
            || html.Contains("length", StringComparison.OrdinalIgnoreCase),
            "Expected a max-length validation error in the HTML");

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(0, db.Recipes.Count());
    }

    [Fact]
    public async Task Post_Creates_with_invalid_preparation_time_shows_error_and_does_not_save()
    {
        await ResetDatabaseAsync();

        var token = await GetAntiforgeryTokenAsync("/recipes/create");
        var form = new Dictionary<string, string>
        {
            ["Name"] = "Toast",
            ["PreparationMinutes"] = "-5",
            ["__RequestVerificationToken"] = token
        };

        var postResponse = await Client.PostAsync("/recipes/create", Form(form));
        var html = await postResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
        Assert.True(
            html.Contains("PreparationMinutes", StringComparison.OrdinalIgnoreCase)
            || html.Contains("range", StringComparison.OrdinalIgnoreCase)
            || html.Contains("invalid", StringComparison.OrdinalIgnoreCase)
            || html.Contains("greater", StringComparison.OrdinalIgnoreCase)
            || html.Contains("must be", StringComparison.OrdinalIgnoreCase),
            "Expected a preparation-time validation error in the HTML");

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(0, db.Recipes.Count());
    }

      [Fact]
    public async Task Get_Create_returns_ok_and_form()
    {
        await ResetDatabaseAsync();

        var response = await Client.GetAsync("/recipes/create");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("/recipes/create", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Name", html);
        Assert.Contains("PreparationMinutes", html);
    }

     [Fact]
    public async Task Post_Create_valid_recipe_redirects_and_saves()
    {
        await ResetDatabaseAsync();

        var token = await GetAntiforgeryTokenAsync("/recipes/create");
        var form = new Dictionary<string, string>
        {
            ["Name"] = "Valid pie",
            ["PreparationMinutes"] = "20",
            ["__RequestVerificationToken"] = token
        };

        var response = await NoRedirectClient.PostAsync("/recipes/create", Form(form));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Contains(db.Recipes, r => r.Name == "Valid pie");
    }

}