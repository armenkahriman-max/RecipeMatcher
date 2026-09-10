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
    public async Task<IActionResult> Index(int[]? ingredientIds)
    {
        ingredientIds ??= [];

        var model = new MatcherViewModel
        {
            IngredientIds = ingredientIds,
            Ingredients = await GetIngredientOptionsAsync(ingredientIds)
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