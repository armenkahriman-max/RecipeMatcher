using System.Data.Common;
using System.Net;
using Microsoft.EntityFrameworkCore.Storage;
using RecipeMatcher.Web.Data;
using RecipeMatcher.Web.Models;

namespace RecipeMatcher.Web.Tests.IntegrationTests;

public class RecipeDetailsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    public readonly HttpClient _client;

    public RecipeDetailsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_Details_existing_recipes_return_OK()
    {
        int id;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureCreatedAsync();

            var recipe = new Recipe
            {
                Name = "Details test pie",
                PreparationMinutes = 25
            };
            db.Recipes.Add(recipe);
            await db.SaveChangesAsync();
            id = recipe.Id;
        }

        var response = await _client.GetAsync($"/recipes/details/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Details test pie", html);
    }

    [Fact]
    public async Task Get_Details_unknow_id_returns_not_found()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureCreatedAsync();
        }
        var response = await _client.GetAsync("/recipes/details/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

