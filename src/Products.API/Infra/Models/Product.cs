using System;
using System.Collections.Generic;

namespace Products.API.Data.Models;

public partial class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int Categoryid { get; set; }

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public DateTime Createdat { get; set; }

    public virtual Category Category { get; set; } = null!;
}
