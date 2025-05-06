using CoffeePeek.Domain.Databases;
using CoffeePeek.Domain.Entities.Shop;
using CoffeePeek.Domain.UnitOfWork;

namespace CoffeePeek.Domain.Repositories;

public class ShopRepository(CoffeePeekDbContext context) : Repository<Shop>(context);