using System;
using MediatR;

namespace Products.Application;

public record GetProductQuery(int id) : IRequest<GetProductQueryResult>;
