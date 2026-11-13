using System;
using System.Text.Json.Serialization;
using MediatR;

namespace Products.Application;

public record SearchProductsQuery(SearchProductQueryDto dto) : IRequest<SearchProductsQueryResult>;
