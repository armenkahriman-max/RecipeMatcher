using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeMatcher.Web.Data;

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
}