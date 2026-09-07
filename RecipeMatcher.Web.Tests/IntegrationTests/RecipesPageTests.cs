using System.Net;
using RecipeMatcher.Web.Data;
using RecipeMatcher.Web.Models;

namespace RecipeMatcher.Web.Tests.IntegrationTests;

public class RecipesPageTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public RecipesPageTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_recipes_retruns_ok_and_contains_inserted_recipe()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.EnsureCreatedAsync();

        var recipe = new Recipe
        {
            Name = "Test Baklava",
            PreparationMinutes = 30
        };

        db.Recipes.Add(recipe);
        await db.SaveChangesAsync();

        var response = await _client.GetAsync("/recipes");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Test Baklava", html);
    }
}
