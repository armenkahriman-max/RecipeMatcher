using System.Net;
using RecipeMatcher.Web.Data;

namespace RecipeMatcher.Web.Tests.IntegrationTests;

public class DeleteRecipeTests : IntegrationTestBase
{
    public DeleteRecipeTests(CustomWebApplicationFactory factory) : base(factory)
    {

    }

    [Fact]
    public async Task Get_Delete_shows_recipe()
    {
        await ResetDatabaseAsync();
        var recipe = await AddRecipeAsync("Cold pie", 20);

        var response = await Client.GetAsync($"/recipes/delete/{recipe.Id}");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Cold pie", html);

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.NotNull(await db.Recipes.FindAsync(recipe.Id));

    }

    [Fact]
    public async Task Post_Delete_removes_recipe()
    {
        await ResetDatabaseAsync();
        var recipe = await AddRecipeAsync("Cold pie", 20);
        var id = recipe.Id;

        var token = await GetAntiforgeryTokenAsync($"/recipes/delete/{id}");
        var form = new Dictionary<string, string>
        {
            ["Id"] = id.ToString(),
            ["__RequestVerificationToken"] = token
        };

        var response = await NoRedirectClient.PostAsync(
            $"/recipes/delete/{id}",
            Form(form));


        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Null(await db.Recipes.FindAsync(id));
    }

    [Fact]
    public async Task Get_Delete_unknown_id_returns_not_found()
    {
        await ResetDatabaseAsync();

        var response = await Client.GetAsync($"/recipes/delete/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}



