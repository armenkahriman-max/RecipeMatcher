using System.Net;
using System.Text.RegularExpressions;
using RecipeMatcher.Web.Data;
using RecipeMatcher.Web.Models;

namespace RecipeMatcher.Web.Tests.IntegrationTests;

public class EditRecipeTests : IClassFixture<CustomWebApplicationFactory>
{
    public readonly CustomWebApplicationFactory _factory;
    public readonly HttpClient _client;

    public EditRecipeTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new() { AllowAutoRedirect = false });
    }

    [Fact]

    public async Task Get_Edit_shows_existing_values()
    {
        var recipe = await AddRecipe("Old pie", 10);

        var response = await _client.GetAsync($"/recipes/edit/{recipe.Id}");

        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Old pie", html);
        Assert.Contains("10", html);
    }

    [Fact]
    public async Task Post_Edit_updates_recipe()
    {
        var recipe = await AddRecipe("Old pie", 10);

        var token = await GetTokenAsync($"/recipes/edit/{recipe.Id}");
        var form = new Dictionary<string, string>
        {
            ["Id"] = recipe.Id.ToString(),
            ["Name"] = "New pie",
            ["PreparationMinutes"] = "30",
            ["__RequestVerificationToken"] = token
        };

        var response = await _client.PostAsync(
            $"/recipes/edit/{recipe.Id}",
            new FormUrlEncodedContent(form));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var update = await db.Recipes.FindAsync(recipe.Id);

            Assert.Contains("New pie", update!.Name);
            Assert.Equal(30, update.PreparationMinutes);
        }
    }

    [Fact]
    public async Task Post_edit_invalid_data_is_not_saved()
    {
        var recipe = await AddRecipe("Old pie", 10);

        var token = await GetTokenAsync($"/recipes/edit/{recipe.Id}");
        var form = new Dictionary<string, string>
        {
            ["Id"] = recipe.Id.ToString(),
            ["Name"] = "",
            ["PreparationMinutes"] = "10",
            ["__RequestVerificationToken"] = token
        };

        var response = await _client.PostAsync(
            $"/recipes/edit/{recipe.Id}",
            new FormUrlEncodedContent(form));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("required", html, StringComparison.OrdinalIgnoreCase);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var same = await db.Recipes.FindAsync(recipe.Id);

        Assert.Equal("Old pie", same!.Name);
        Assert.Equal(10, same.PreparationMinutes);

    }

    [Fact]
    public async Task Get_Edit_unknown_id_returns_not_found()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureCreatedAsync();
        }

        var response = await _client.GetAsync("/recipes/edit/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<Recipe> AddRecipe(string name, int minutes)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();

        var recipe = new Recipe { Name = name, PreparationMinutes = minutes };
        db.Recipes.Add(recipe);
        await db.SaveChangesAsync();
        return recipe;

    }

    private async Task<string> GetTokenAsync(string url)
    {
        var html = await _client.GetStringAsync(url);
        var match = Regex.Match(
            html,
             @"name=""__RequestVerificationToken""[^>]*value=""([^""]+)""",
            RegexOptions.IgnoreCase);

        Assert.True(match.Success, "Edit page did not contain an antiforgery token.");
        return match.Groups[1].Value;

    }
}