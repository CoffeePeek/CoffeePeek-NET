using CoffeePeek.Data;
using CoffeePeek.Domain.Databases;
using CoffeePeek.Domain.Entities.Users;

namespace CoffeePeek.Domain.Repositories;

public class UserRepository(CoffeePeekDbContext context) : Repository<User>(context)
{
}