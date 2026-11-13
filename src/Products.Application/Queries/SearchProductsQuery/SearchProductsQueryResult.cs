using System;

namespace Products.Application;

public record SearchProductsQueryResult(IEnumerable<ProductDocumentDto> ProductDocuments);