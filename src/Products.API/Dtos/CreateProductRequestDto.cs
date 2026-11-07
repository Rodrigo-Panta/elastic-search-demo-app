public record CreateProductRequestDto
{
    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int Categoryid { get; set; }

    public decimal Price { get; set; }

    public int Stock { get; set; }


}