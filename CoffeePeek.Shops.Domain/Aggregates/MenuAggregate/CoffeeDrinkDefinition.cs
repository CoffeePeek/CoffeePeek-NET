using CoffeePeek.Shared.Domain.Entities;
using CoffeePeek.Shared.Kernel.Exceptions;

namespace CoffeePeek.Shops.Domain.Aggregates.MenuAggregate;

public sealed class CoffeeDrinkDefinition : Entity<Guid>
{
    public string Slug { get; private set; } = null!;
    public string NameRu { get; private set; } = null!;
    public string NameEn { get; private set; } = null!;
    public CoffeeDrinkCategory Category { get; private set; }
    public CoffeeDrinkKind Kind { get; private set; }
    public string Aliases { get; private set; } = null!;
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }

    // ReSharper disable once UnusedMember.Local
    private CoffeeDrinkDefinition()
    {
    }

    private CoffeeDrinkDefinition(
        Guid id,
        string slug,
        string nameRu,
        string nameEn,
        CoffeeDrinkCategory category,
        string aliases,
        int sortOrder)
    {
        Id = id;
        Slug = slug;
        NameRu = nameRu;
        NameEn = nameEn;
        Category = category;
        Kind = CoffeeDrinkKind.Standard;
        Aliases = aliases;
        SortOrder = sortOrder;
        IsActive = true;
    }

    public static CoffeeDrinkDefinition CreateWithId(
        Guid id,
        string slug,
        string nameRu,
        string nameEn,
        CoffeeDrinkCategory category,
        string aliases,
        int sortOrder,
        bool isActive = true)
    {
        ValidateName(nameRu);
        ValidateName(nameEn);
        if (string.IsNullOrWhiteSpace(slug))
            throw new DomainException("Slug is required.");
        if (aliases.Length > BusinessConstants.MaxCoffeeDrinkAliasesLength)
            throw new DomainException(
                $"Aliases cannot be longer than {BusinessConstants.MaxCoffeeDrinkAliasesLength} characters.");

        return new CoffeeDrinkDefinition(
            id,
            slug.Trim().ToLowerInvariant(),
            nameRu.Trim(),
            nameEn.Trim(),
            category,
            aliases.Trim(),
            sortOrder)
        {
            IsActive = isActive
        };
    }

    public IReadOnlyList<string> AliasList =>
        Aliases.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name is required.");

        if (name.Trim().Length > BusinessConstants.MaxCoffeeDrinkNameLength)
            throw new DomainException(
                $"Name cannot be longer than {BusinessConstants.MaxCoffeeDrinkNameLength} characters.");
    }
}
