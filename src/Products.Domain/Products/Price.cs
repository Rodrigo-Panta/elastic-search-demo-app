namespace Products.Domain;

public record Price : ValueObject
{
    public decimal Value { get; }

    public Price(decimal value)
    {
        if(value <= 0)
        {
            throw new ArgumentException("InvalidPrice");
        }
        
        Value = value;
    }
}