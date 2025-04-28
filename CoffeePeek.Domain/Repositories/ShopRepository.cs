using CoffeePeek.Data;
using CoffeePeek.Domain.Databases;
using CoffeePeek.Domain.Entities.Shop;

namespace CoffeePeek.Domain.Repositories;

public class ShopRepository(CoffeePeekDbContext context) : Repository<Shop>(context);