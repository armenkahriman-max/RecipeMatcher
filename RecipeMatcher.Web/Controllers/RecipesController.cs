using Microsoft.AspNetCore.Mvc;

namespace RecipeMatcher.Web.Controllers;

public class RecipesController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}