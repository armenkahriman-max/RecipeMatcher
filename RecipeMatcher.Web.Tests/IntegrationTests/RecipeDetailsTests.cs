using System.Net;

namespace RecipeMatcher.Web.Tests.IntegrationTests;

public class RecipeDetailsTests : IntegrationTestBase
{
    public RecipeDetailsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Get_Details_existing_recipes_return_OK()
    {
        await ResetDatabaseAsync();
        var recipe = await AddRecipeAsync("Details test pie", 25);

        var response = await Client.GetAsync($"/recipes/details/{recipe.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Details test pie", html);
    }

    [Fact]
    public async Task Get_Details_unknow_id_returns_not_found()
    {
        await ResetDatabaseAsync();
        var response = await Client.GetAsync("/recipes/details/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

