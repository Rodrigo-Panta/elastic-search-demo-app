namespace Products.Application;

public record SearchProductQueryDto(
    string? queryTerm,
    string? category,
    decimal? maxPrice,
    decimal? minPrice,
    int? maxResults
);