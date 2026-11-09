public record ProductDocumentDto(                 
    int id,
    string name,
    string description,
    int categoryId,
    string category,
    decimal price,
    int stock,
    DateTime createdAt);