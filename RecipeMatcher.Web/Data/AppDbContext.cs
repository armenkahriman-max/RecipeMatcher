using Microsoft.EntityFrameworkCore;
using RecipeMatcher.Web.Models;

namespace RecipeMatcher.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
    public DbSet<Recipe> Recipes => Set<Recipe>();

    public DbSet<Ingredient> Ingredients => Set<Ingredient>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Ingredient>()
        .HasIndex(ingredient => ingredient.Name)
        .IsUnique();

        modelBuilder.Entity<RecipeIngredient>()
        .HasKey(ri => new { ri.RecipeId, ri.IngredientId });

        modelBuilder.Entity<RecipeIngredient>()
        .HasOne(ri => ri.Recipe)
        .WithMany(recipe => recipe.RecipeIngredients)
        .HasForeignKey(ri => ri.RecipeId);

        modelBuilder.Entity<RecipeIngredient>()
        .HasOne(ri => ri.Ingredient)
        .WithMany(ingredient => ingredient.RecipeIngredients)
        .HasForeignKey(ri => ri.IngredientId);
    }

    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
}