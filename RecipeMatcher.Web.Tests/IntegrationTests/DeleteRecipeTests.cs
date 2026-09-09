using System.Net;
using System.Text.RegularExpressions;
using RecipeMatcher.Web.Data;
using RecipeMatcher.Web.Models;

namespace RecipeMatcher.Web.Tests.IntegrationTests;

public class DeleteRecipeTests : IClassFixture<CustomWebApplicationFactory>
{
    public readonly CustomWebApplicationFactory _factory;
    public readonly HttpClient _client;

    public DeleteRecipeTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new() { AllowAutoRedirect = false });
    }

    [Fact]
    public async Task Get_Delete_shows_recipe()
    {
        var recipe = await AddRecipe("Cold pie", 20);

        var response = await _client.GetAsync($"/recipes/delete/{recipe.Id}");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Cold pie", html);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.NotNull(await db.Recipes.FindAsync(recipe.Id));

    }

    [Fact]
    public async Task Post_Delete_removes_recipe()
    {
        var recipe = await AddRecipe("Cold pie", 20);
        var id = recipe.Id;

        var token = await GetTokenAsync($"/recipes/delete/{id}");
        var form = new Dictionary<string, string>
        {
            ["Id"] = id.ToString(),
            ["__RequestVerificationToken"] = token
        };

        var response = await _client.PostAsync(
            $"/recipes/delete/{id}",
            new FormUrlEncodedContent(form));


        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Null(await db.Recipes.FindAsync(id));
    }

    [Fact]
    public async Task Get_Delete_unknown_id_returns_not_found()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureCreatedAsync();
        }

        var response = await _client.GetAsync($"/recipes/delete/999999");

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

        Assert.True(match.Success, "Delete page did not contain an antiforgery token.");
        return match.Groups[1].Value;
    }
}



