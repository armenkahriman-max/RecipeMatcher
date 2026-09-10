using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeMatcher.Web.Data;
using RecipeMatcher.Web.Models;
using RecipeMatcher.Web.Models.ViewModel;

namespace RecipeMatcher.Web.Controllers;

public class RecipesController(AppDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index()
    {
        var recipes = await dbContext.Recipes
            .OrderBy(recipe => recipe.Name)
            .ToListAsync();

        return View(recipes);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Recipe recipe)
    {
        if (!ModelState.IsValid)
        {
            return View(recipe);
        }

        dbContext.Recipes.Add(recipe);
        await dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var recipe = await dbContext.Recipes
        .Include(recipe => recipe.RecipeIngredients)
        .ThenInclude(recipeIngredient => recipeIngredient.Ingredient)
        .FirstOrDefaultAsync(recipe => recipe.Id == id);

        if (recipe is null)
        {
            return NotFound();
        }
        return View(recipe);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var recipe = await dbContext.Recipes
        .FirstOrDefaultAsync(r => r.Id == id);

        if (recipe is null)
        {
            return NotFound();
        }
        var model = new EditRecipeViewModel
        {
            Id = recipe.Id,
            Name = recipe.Name,
            PreparationMinutes = recipe.PreparationMinutes,
            Ingredients = await GetIngredientOptionAsync(id)
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, EditRecipeViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            model.Ingredients = await GetIngredientOptionAsync(id);
            return View(model);
        }

        var existing = await dbContext.Recipes
        .Include(r => r.RecipeIngredients)
        .FirstOrDefaultAsync(r => r.Id == id);

        if (existing is null)
        {
            return NotFound();
        }

        existing.Name = model.Name;
        existing.PreparationMinutes = model.PreparationMinutes;

        existing.RecipeIngredients.Clear();

        foreach( var ingredientId in model.IngredientIds.Distinct())
        {
            existing.RecipeIngredients.Add(new RecipeIngredient
            {
                RecipeId = existing.Id,
                IngredientId = ingredientId
            });

        }
        await dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));


    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var recipe = await dbContext.Recipes.FindAsync(id);
        if (recipe is null)
        {
            return NotFound();
        }

        return View(recipe);

    }

    [HttpPost]
    [ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var recipe = await dbContext.Recipes.FindAsync(id);

        if (recipe is null)
        {
            return NotFound();
        }

        dbContext.Recipes.Remove(recipe);
        await dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));

    }

    private async Task<IReadOnlyList<IngredientOptionViewModel>> GetIngredientOptionAsync(int recipeId)
    {
        var allIngredients = await dbContext.Ingredients
        .OrderBy(i => i.Name)
        .ToListAsync();

        var selectedIds = await dbContext.RecipeIngredients
        .Where(ri => ri.RecipeId == recipeId)
        .Select(ri => ri.IngredientId)
        .ToListAsync();

        return allIngredients
        .Select(i => new IngredientOptionViewModel
        {
            Id = i.Id,
            Name = i.Name,
            Selected = selectedIds.Contains(i.Id)
        })
        .ToList();
    }

}

