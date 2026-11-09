using System.Text.Json.Serialization;

public record CreateProductRequestDto
{
    [property: JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [property: JsonPropertyName("description")]
    public string Description { get; set; } = null!;

    [property: JsonPropertyName("category_id")]
    public int Categoryid { get; set; }

    [property: JsonPropertyName("price")]  
    public decimal Price { get; set; }

    [property: JsonPropertyName("stock")]  
    public int Stock { get; set; }


}