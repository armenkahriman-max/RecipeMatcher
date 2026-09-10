using System.ComponentModel.DataAnnotations;

namespace RecipeMatcher.Web.Models.ViewModel;

public class EditRecipeViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Preparation time is required")]
    [Range(1, 1440, ErrorMessage= "Preparation time must be between 1 and 1440 min.")]
    public int PreparationMinutes { get; set; }
    public IReadOnlyList<IngredientOptionViewModel> Ingredients { get; set; } = [];

    public int[] IngredientIds { get; set; } = [];
}