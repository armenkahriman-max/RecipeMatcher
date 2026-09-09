using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeMatcher.Web.Data;
using RecipeMatcher.Web.Models;

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
        var recipe = await dbContext.Recipes.FindAsync(id);
        if (recipe is null)
        {
            return NotFound();
        }

        return View(recipe);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var recipe = await dbContext.Recipes.FindAsync(id);

        if (recipe is null)
        {
            return NotFound();
        }
        return View(recipe);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, Recipe recipe)
    {
        if (id != recipe.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(recipe);
        }

        var existing = await dbContext.Recipes.FindAsync(id);

        if (existing is null)
        {
            return NotFound();
        }

        existing.Name = recipe.Name;
        existing.PreparationMinutes = recipe.PreparationMinutes;

        await dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

}

