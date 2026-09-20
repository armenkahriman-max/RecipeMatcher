using System.Net;

namespace RecipeMatcher.Web.Tests.IntegrationTests;

public class RecipesPageTests : IntegrationTestBase
{
    public RecipesPageTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Get_recipes_retruns_ok_and_contains_inserted_recipe()
    {
        await ResetDatabaseAsync();
        await AddRecipeAsync("Test Baklava", 30);


        var response = await Client.GetAsync("/recipes");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Test Baklava", html);
    }
}
