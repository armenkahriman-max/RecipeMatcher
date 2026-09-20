using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using RecipeMatcher.Web.Data;
using RecipeMatcher.Web.Models;

namespace RecipeMatcher.Web.Tests;

public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;
    protected readonly HttpClient NoRedirectClient;

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        NoRedirectClient = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    protected async Task ResetDatabaseAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }
    protected async Task<Recipe> AddRecipeAsync(string name, int minutes, params string[] ingredientNames)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var recipe = new Recipe
        {
            Name = name,
            PreparationMinutes = minutes
        };

        foreach (var ingredientName in ingredientNames)
        {
            var ingredient = await db.Ingredients
                .FirstOrDefaultAsync(i => i.Name == ingredientName);
            if (ingredient is null)
            {
                ingredient = new Ingredient { Name = ingredientName };
                db.Ingredients.Add(ingredient);
                await db.SaveChangesAsync();
            }
            recipe.RecipeIngredients.Add(new RecipeIngredient
            {
                IngredientId = ingredient.Id
            });
        }

        db.Recipes.Add(recipe);
        await db.SaveChangesAsync();
        return recipe;
    }

    protected static string GetAntiforgeryToken(string html)
    {
        var match = Regex.Match(
            html,
            @"name=""__RequestVerificationToken""[^>]*value=""([^""]+)""",
            RegexOptions.IgnoreCase);
        Assert.True(match.Success, "Page did not contain an antiforgery token.");
        return match.Groups[1].Value;
    }

    protected async Task<string> GetAntiforgeryTokenAsync(string url)
    {
        var html = await Client.GetStringAsync(url);
        return GetAntiforgeryToken(html);
    }

    protected static FormUrlEncodedContent Form(Dictionary<string, string> fields)
        => new(fields);
}