namespace Products.Domain;

public record PagedResult<T>(IEnumerable<T> Values, int? lastId, int totalCount);