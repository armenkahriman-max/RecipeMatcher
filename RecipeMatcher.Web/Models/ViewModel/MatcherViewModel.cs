namespace RecipeMatcher.Web.Models.ViewModel;

public class MatcherViewModel
{
    public IReadOnlyList<IngredientOptionViewModel> Ingredients { get; set; } = [];

    public int[] IngredientIds { get; set; } = [];

    public IReadOnlyList<MatchedResultViewModel> Results { get; set; } = [];
}