using CoffeePeek.Data;
using CoffeePeek.Domain.Databases;
using CoffeePeek.Domain.Entities.Address;

namespace CoffeePeek.Domain.Repositories;

public class CityRepository(CoffeePeekDbContext context) : Repository<City>(context);