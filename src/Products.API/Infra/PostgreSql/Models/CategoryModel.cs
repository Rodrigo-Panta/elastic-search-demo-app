using System;
using System.Collections.Generic;
using Products.Domain;

namespace Products.API.Data.Models;

public partial class CategoryModel
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<ProductModel> Products { get; set; } = new List<ProductModel>();

    public Category ToEntity()
    {
        return new Category(Id, Name);
    }
}
