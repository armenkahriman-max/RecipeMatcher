using System.Net;
using RecipeMatcher.Web.Data;

namespace RecipeMatcher.Web.Tests.IntegrationTests;

public class EditRecipeTests : IntegrationTestBase
{
    public EditRecipeTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]

    public async Task Get_Edit_shows_existing_values()
    {
        await ResetDatabaseAsync();
        var recipe = await AddRecipeAsync("Old pie", 10);

        var response = await Client.GetAsync($"/recipes/edit/{recipe.Id}");

        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Old pie", html);
        Assert.Contains("10", html);
    }

    [Fact]
    public async Task Post_Edit_updates_recipe()
    {
        await ResetDatabaseAsync();
        var recipe = await AddRecipeAsync("Old pie", 10);

        var token = await GetAntiforgeryTokenAsync($"/recipes/edit/{recipe.Id}");
        var form = new Dictionary<string, string>
        {
            ["Id"] = recipe.Id.ToString(),
            ["Name"] = "New pie",
            ["PreparationMinutes"] = "30",
            ["__RequestVerificationToken"] = token
        };

        var response = await NoRedirectClient.PostAsync(
            $"/recipes/edit/{recipe.Id}",
            Form(form));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var update = await db.Recipes.FindAsync(recipe.Id);

        Assert.Equal("New pie", update!.Name);
        Assert.Equal(30, update.PreparationMinutes);

    }

    [Fact]
    public async Task Post_edit_invalid_data_is_not_saved()
    {
        await ResetDatabaseAsync();
        var recipe = await AddRecipeAsync("Old pie", 10);

        var token = await GetAntiforgeryTokenAsync($"/recipes/edit/{recipe.Id}");
        var form = new Dictionary<string, string>
        {
            ["Id"] = recipe.Id.ToString(),
            ["Name"] = "",
            ["PreparationMinutes"] = "10",
            ["__RequestVerificationToken"] = token
        };

        var response = await Client.PostAsync(
            $"/recipes/edit/{recipe.Id}",
            Form(form));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("required", html, StringComparison.OrdinalIgnoreCase);

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var same = await db.Recipes.FindAsync(recipe.Id);

        Assert.Equal("Old pie", same!.Name);
        Assert.Equal(10, same.PreparationMinutes);

    }

    [Fact]
    public async Task Get_Edit_unknown_id_returns_not_found()
    {
        await ResetDatabaseAsync();
        var response = await Client.GetAsync("/recipes/edit/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

}