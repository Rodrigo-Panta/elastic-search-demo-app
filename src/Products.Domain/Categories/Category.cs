using System;
using System.Collections.Generic;

namespace Products.Domain;

public partial class Category
{
    public int Id { get; private set; }

    public string Name { get; private set; }

    public Category(int id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("NameShouldNotBeEmpty");
        
        if(id <= 0)
            throw new ArgumentException("InvalidId");
    
        Id = id;
        Name = name;
    }

}
