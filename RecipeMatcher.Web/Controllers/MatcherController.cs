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

        var matches = await dbContext.Recipes
        .Include(recipe => recipe.RecipeIngredients)
        .ThenInclude(ri => ri.Ingredient)
        .Where(recipe =>
        !recipe.RecipeIngredients.Any(ri =>
        !ingredientIds.Contains(ri.IngredientId)))
        .Select(recipe => new MatchedRecipeViewModel
        {

            Name = recipe.Name,
            PreparationTime = recipe.PreparationMinutes,
            Ingredients = recipe.RecipeIngredients
        .Select(ri => ri.Ingredient.Name)
        .ToList()
        })
        .ToListAsync();

        var model = new MatcherViewModel
        {
            IngredientIds = ingredientIds.ToArray(),
            Ingredients = await GetIngredientOptionsAsync(ingredientIds.ToArray()),
            Matches = matches
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