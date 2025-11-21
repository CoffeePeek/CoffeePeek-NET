using CoffeePeek.Domain.Databases;
using CoffeePeek.Domain.Entities.Address;
using CoffeePeek.Domain.Repositories.Interfaces;
using CoffeePeek.Domain.UnitOfWork;

namespace CoffeePeek.Domain.Repositories;

public class CityRepository(CoffeePeekDbContext context) : Repository<City>(context), ICityRepository;