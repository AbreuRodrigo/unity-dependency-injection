using DI.Injection;
using DI.Injection.Scope;

[DependencyScope(Scope.Request)] 
public class Item : IDependency
{
    public string Name { get; set; }
    public float Price { get; set; }

    public Item()
    {
    }

    public Item(string name, float price)
    {
        Name = name;
        Price = price;
    }
}