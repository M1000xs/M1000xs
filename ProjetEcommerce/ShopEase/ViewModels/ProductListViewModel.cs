using ShopEase.Models;

namespace ShopEase.ViewModels;

/// <summary>Critères de recherche/filtre du catalogue (liés à la query string).</summary>
public class ProductFilter
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool InStockOnly { get; set; }
    /// <summary>popular | newest | price_asc | price_desc | rating | name</summary>
    public string Sort { get; set; } = "popular";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 9;
}

public class ProductListViewModel
{
    public IReadOnlyList<Product> Products { get; set; } = Array.Empty<Product>();
    public IReadOnlyList<Category> Categories { get; set; } = Array.Empty<Category>();
    public ProductFilter Filter { get; set; } = new();
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)Filter.PageSize);
}

public class ProductDetailsViewModel
{
    public Product Product { get; set; } = null!;
    public IReadOnlyList<Product> Related { get; set; } = Array.Empty<Product>();
    public int QuantityInCart { get; set; }
}
