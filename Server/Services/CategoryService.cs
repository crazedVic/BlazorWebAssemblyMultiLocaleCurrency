using System.Text.Json;
using BlazorHelloWorld.Shared.Models;

namespace BlazorHelloWorld.Server.Services;

public class CategoryService : ICategoryService
{
    private readonly List<CategoryTranslationItem> _categories;
    private readonly string _dataPath;

    public CategoryService(IWebHostEnvironment webHostEnvironment)
    {
        _dataPath = Path.Combine(webHostEnvironment.ContentRootPath, "Data", "categories.json");
        _categories = LoadCategories();
    }

    private List<CategoryTranslationItem> LoadCategories()
    {
        if (!File.Exists(_dataPath))
        {
            return new List<CategoryTranslationItem>();
        }

        var jsonString = File.ReadAllText(_dataPath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var data = JsonSerializer.Deserialize<CategoryTranslations>(jsonString, options);
        return data?.Categories ?? new List<CategoryTranslationItem>();
    }

    public Task<string> GetCategoryTranslation(string categoryId, string language)
    {
        Console.WriteLine($"GetCategoryTranslation called with categoryId: {categoryId}, language: {language}");
        
        var category = _categories.FirstOrDefault(c => c.Id.Equals(categoryId, StringComparison.OrdinalIgnoreCase));
        
        if (category == null)
        {
            Console.WriteLine($"Category not found for id: {categoryId}");
            return Task.FromResult(categoryId);
        }

        var twoLetterCode = language.Split('-')[0].ToLower();
        Console.WriteLine($"Trying with two-letter code: {twoLetterCode}");
        
        if (category.Translations.TryGetValue(twoLetterCode, out var translation))
        {
            Console.WriteLine($"Found translation for {twoLetterCode}: {translation}");
            return Task.FromResult(translation);
        }

        Console.WriteLine($"No translation found for {twoLetterCode}, falling back to English");
        return Task.FromResult(category.Translations.GetValueOrDefault("en", categoryId));
    }

    public Task<IEnumerable<string>> GetAllCategoryIds()
    {
        return Task.FromResult(_categories.Select(c => c.Id));
    }

    public Task<Dictionary<string, string>> GetAllCategoryTranslations(string language)
    {
        var translations = _categories.ToDictionary(
            c => c.Id,
            c => c.Translations.GetValueOrDefault(language, c.Translations.GetValueOrDefault("en", c.Id))
        );
        
        return Task.FromResult(translations);
    }
} 