namespace CoffeePeek.Shops.Domain.Abstracts;

public abstract record ValueObjectBase<T>
{
    public required T Value { get; init; }

    protected ValueObjectBase(T value)
    {
        Value = value;
    }
    
    protected ValueObjectBase() { }
}

public abstract record ValueObjectBase
{
    protected ValueObjectBase() { }
}