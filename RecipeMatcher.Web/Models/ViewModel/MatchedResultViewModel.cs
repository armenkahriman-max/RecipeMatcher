namespace RecipeMatcher.Web.Models.ViewModel;

public class MatchedResultViewModel
{
    public int RecipeId { get; set; }
    public string Name { get; set; } = "";
    public int PreparationMinutes { get; set; }
    public List<string> MissingIngredients { get; set; } = [];
    public int MissingCount { get; set; }

}
