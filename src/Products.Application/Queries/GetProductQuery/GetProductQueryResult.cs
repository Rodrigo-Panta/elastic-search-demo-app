using System;

namespace Products.Application;

public record GetProductQueryResult(int Id, string Name, string Description, decimal Price, int Stock, DateTime CreatedAt, GetProductCategoryResult Category);
public record GetProductCategoryResult (int Id, string Name);

