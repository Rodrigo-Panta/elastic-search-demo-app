namespace Products.Domain;

public record ProductCreatedEvent(
    int ProductId,
    string Name
);