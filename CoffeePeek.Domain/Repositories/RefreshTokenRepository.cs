using CoffeePeek.Domain.Databases;
using CoffeePeek.Domain.Entities.Auth;
using CoffeePeek.Domain.UnitOfWork;

namespace CoffeePeek.Domain.Repositories;

public class RefreshTokenRepository(CoffeePeekDbContext context) : Repository<RefreshToken>(context);