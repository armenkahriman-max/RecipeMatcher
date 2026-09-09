using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeMatcher.Web.Data;
using RecipeMatcher.Web.Models;
namespace RecipeMatcher.Web.Controllers;

public class IngredientsController(AppDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index()
    {
        var ingredients = await dbContext.Ingredients
        .OrderBy(ingredient => ingredient.Name)
        .ToListAsync();

        return View(ingredients);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Ingredient ingredient)
    {
        if (await dbContext.Ingredients.AnyAsync(i => i.Name == ingredient.Name))
        {
            ModelState.AddModelError(nameof(ingredient.Name), "Name alredy exists.");
        }
        if (!ModelState.IsValid)
        {
            return View(ingredient);
        }

        dbContext.Ingredients.Add(ingredient);
        await dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }


    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var ingredit = await dbContext.Ingredients.FindAsync(id);

        if (ingredit is null)
        {
            return NotFound();
        }
        return View(ingredit);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, Ingredient ingredient)
    {
        if (id != ingredient.Id)
        {
            return NotFound();
        }
        if (await dbContext.Ingredients.AnyAsync(i =>
        i.Name == ingredient.Name && i.Id != id))

        {
            ModelState.AddModelError(nameof(ingredient.Name), "Name already exists.");
        }

        if (!ModelState.IsValid)
        {
            return View(ingredient);
        }

        var existing = await dbContext.Ingredients.FindAsync(id);

        if (existing is null)
        {
            return NotFound();
        }

        existing.Name = ingredient.Name;

        await dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var ingredient = await dbContext.Ingredients.FindAsync(id);
        if (ingredient is null)
        {
            return NotFound();
        }
        return View(ingredient);
    }

    [HttpPost]
    [ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var ingredient = await dbContext.Ingredients.FindAsync(id);
        if (ingredient is null)
        {
            return NotFound();
        }
        dbContext.Ingredients.Remove(ingredient);
        await dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}