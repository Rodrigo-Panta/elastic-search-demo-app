public record ProductDocumentDto(                 
    int Id,
    string Name,
    string Description,
    int CategoryId,
    string category,
    decimal price,
    int stock,
    DateTime createdAt);