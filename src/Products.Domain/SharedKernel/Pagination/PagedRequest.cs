namespace Products.Domain;

public record PagedRequest(int pageSize, int lastId);