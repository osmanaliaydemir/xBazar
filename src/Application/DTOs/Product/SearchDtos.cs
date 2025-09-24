namespace Application.DTOs.Product;

public class ProductSearchQueryDto
{
    public string? SearchTerm { get; set; }
    public Guid? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SortBy { get; set; } // price_asc, price_desc, new, popular
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class FacetItemDto
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ProductSearchResultDto
{
    public List<ProductDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public List<FacetItemDto> CategoryFacets { get; set; } = new();
    public List<FacetItemDto> PriceFacets { get; set; } = new();
}
