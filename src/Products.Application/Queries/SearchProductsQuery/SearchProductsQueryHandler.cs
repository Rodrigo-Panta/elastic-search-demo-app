using System;
using MediatR;

namespace Products.Application;

public class SearchProductsQueryHandler(IProductIndexer indexer) : IRequestHandler<SearchProductsQuery, SearchProductsQueryResult>
{
    public async Task<SearchProductsQueryResult> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        return new SearchProductsQueryResult(
            await indexer.SearchAsync(request.dto));
    }
}
