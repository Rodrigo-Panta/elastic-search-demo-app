namespace Products.Domain;


public record Stock : ValueObject
{
    public int Value { get; }

    public Stock(int value)
    {
        if(value <= 0)
            throw new ArgumentException("InvalidStock");
        Value = value;
    }
}