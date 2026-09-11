namespace RecipeMatcher.Web.Models.ViewModel;

public class MatchedRecipeViewModel
{
    public string Name { get; set; } = "";
    public int PreparationTime { get; set; }
    public List<string> Ingredients { get; set; } = [];
}