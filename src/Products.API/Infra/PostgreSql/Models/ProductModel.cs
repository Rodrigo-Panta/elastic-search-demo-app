using System;
using System.Collections.Generic;
using Products.Domain;

namespace Products.API.Data.Models;

public partial class ProductModel
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int Categoryid { get; set; }

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public DateTime Createdat { get; set; }

    public virtual CategoryModel Category { get; set; } = null!;


    public Product ToEntity()
    {
        return new Product(Id, Name, Description, new Category(Category.Id, Category.Name), new Price(Price), new Stock(Stock), Createdat);
    }
}
