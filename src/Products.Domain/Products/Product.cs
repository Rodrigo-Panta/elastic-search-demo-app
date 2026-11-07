using System;
using System.Collections.Generic;

namespace Products.Domain;

public partial class Product : EntityBase
{

    public Product(int id, string name, string description, Category category, Price price, Stock? stock = null, DateTime? createdAt = null) 
    {
        Id = id;
        Name = name;
        Description = description;
        Category = category;
        Price = price;
        Stock = stock ?? new Stock(0);
        CreatedAt = createdAt ?? DateTime.Now;    
    }

    public string Name { get; private set; }

    public string Description { get; private set; }

    public Category Category { get; private set; }

    public Price Price { get; private set; }

    public Stock Stock { get; private set; }

    public  DateTime CreatedAt { get; private set; }


    public void ChangeStock(int amount)
    {
        Stock = new Stock(Stock!.Value + amount);

    }

    public void ChangePrice(Price price)
    {
        Price = price;
    }

    
}
