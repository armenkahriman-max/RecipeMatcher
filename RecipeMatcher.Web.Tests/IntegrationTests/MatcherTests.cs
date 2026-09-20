using System.Net;
using Microsoft.EntityFrameworkCore;
using RecipeMatcher.Web.Data;

namespace RecipeMatcher.Web.Tests.IntegrationTests;

public class MatcherTests : IntegrationTestBase
{
    public MatcherTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Post_Index_shows_full_match_and_does_not_treat_missing_ingredient_as_full_match()
    {
        await ResetDatabaseAsync();

        await AddRecipeAsync("Full omelette", 10, "Egg", "Salt");
        await AddRecipeAsync("Needs milk", 10, "Egg", "Salt", "Milk");

        var (eggId, saltId) = await GetIngredientIds("Egg", "Salt");
        var html = await PostMatchAsync(eggId, saltId);

        Assert.Contains("Full omelette", html);
        Assert.Contains("Needs milk", html);

        var fullPos = html.IndexOf("Full omelette", StringComparison.Ordinal);
        var needsPos = html.IndexOf("Needs milk", StringComparison.Ordinal);
        Assert.True(fullPos >= 0 && needsPos > fullPos);

        var afterNeeds = html[needsPos..];
        Assert.Contains("Milk", afterNeeds);
    }

    [Fact]
    public async Task Post_Index_orders_almost_matches_by_missing_count()
    {
        await ResetDatabaseAsync();

        await AddRecipeAsync("Needs milk", 10, "Egg", "Salt", "Milk");
        await AddRecipeAsync("Needs milk and flour", 10, "Egg", "Salt", "Milk", "Flour");

        var (eggId, saltId) = await GetIngredientIds("Egg", "Salt");
        var html = await PostMatchAsync(eggId, saltId);

        var oneMissing = html.IndexOf("Needs milk", StringComparison.Ordinal);
        var twoMissing = html.IndexOf("Needs milk and flour", StringComparison.Ordinal);

        Assert.True(oneMissing >= 0 && twoMissing > oneMissing);
    }

    private async Task<(int EggId, int SaltId)> GetIngredientIds(string first, string second)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var a = await db.Ingredients.SingleAsync(i => i.Name == first);
        var b = await db.Ingredients.SingleAsync(i => i.Name == second);
        return (a.Id, b.Id);
    }

    private async Task<string> PostMatchAsync(params int[] ingredientIds)
    {
        var token = await GetAntiforgeryTokenAsync("/Matcher");
        var fields = new List<KeyValuePair<string, string>>
        {
            new("__RequestVerificationToken", token)
        };
        foreach (var id in ingredientIds)
            fields.Add(new("ingredientIds", id.ToString()));

        var response = await Client.PostAsync("/Matcher", new FormUrlEncodedContent(fields));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.Content.ReadAsStringAsync();
    }
}