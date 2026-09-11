using Microsoft.AspNetCore.Mvc;
using RecipeMatcher.Web.Data;
using RecipeMatcher.Web.Models.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace RecipeMatcher.Web.Controllers;

public class MatcherController(AppDbContext dbContext) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = new MatcherViewModel
        {
            Ingredients = await GetIngredientOptionsAsync()
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Index(List<int> ingredientIds)
    {
        ingredientIds ??= new List<int>();

        var queryResults = await dbContext.Recipes
        .Select(recipe => new
        {
            recipe.Id,
            recipe.Name,
            recipe.PreparationMinutes,
            MissingNames = recipe.RecipeIngredients
            .Where(ri => !ingredientIds.Contains(ri.IngredientId))
            .Select(ri => ri.Ingredient.Name)
            .ToList(),
            MissingCount = recipe.RecipeIngredients
            .Count(ri => !ingredientIds.Contains(ri.IngredientId))
        })
        .OrderBy(r => r.MissingCount)
        .ThenBy(r => r.Name)
        .ToListAsync();

        var results = queryResults
        .Select(r => new MatchedResultViewModel
        {
            RecipeId = r.Id,
            Name = r.Name,
            PreparationMinutes = r.PreparationMinutes,
            MissingIngredients = r.MissingNames,
            MissingCount = r.MissingCount
        })
        .ToList();

        var model = new MatcherViewModel
        {
            IngredientIds = ingredientIds.ToArray(),
            Ingredients = await GetIngredientOptionsAsync(ingredientIds.ToArray()),
            Results = results
        };
        return View(model);


    }

    private async Task<IReadOnlyList<IngredientOptionViewModel>> GetIngredientOptionsAsync(
        int[]? selectedIds = null)
    {
        selectedIds ??= [];

        var ingredients = await dbContext.Ingredients
        .OrderBy(i => i.Name)
        .ToListAsync();

        return ingredients
        .Select(i => new IngredientOptionViewModel
        {
            Id = i.Id,
            Name = i.Name,
            Selected = selectedIds.Contains(i.Id)
        })
        .ToList();
    }
}