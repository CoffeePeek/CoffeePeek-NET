using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.ErrorHandling;
using Wolverine.Postgresql;

namespace CoffeePeek.Shared.Persistence.Extensions;

public static class WolverineModule
{
    extension(IHostBuilder hostBuilder)
    {
        public IHostBuilder AddWolverine(Assembly handlerAssembly, string connectionString)
        {
            hostBuilder.UseWolverine(opts =>
            {
                // 1. Настройка хранения сообщений (Outbox) в Postgres
                // Это создаст таблицы (или будет использовать существующие) для очереди сообщений
                opts.PersistMessagesWithPostgresql(connectionString);

                // 2. Интеграция с EF Core
                // Здесь мы говорим Wolverine использовать транзакции твоего DbContext
                // Важно: замени TDbContext на свой базовый класс или интерфейс, если он общий
                opts.UseEntityFrameworkCoreTransactions();

                // 3. Автоматическая регистрация хендлеров
                // Wolverine сам просканирует сборки. Можно указать конкретные
                opts.Discovery.IncludeAssembly(handlerAssembly);

                // 4. Настройка политик по умолчанию (Опционально)
                opts.Policies.AutoApplyTransactions(); // Можно включить транзакции для всех хендлеров сразу
            
                opts.Policies.OnException<DbUpdateException>().ScheduleRetry(new TimeSpan(0, 0, 5));
            });

            return hostBuilder;
        }
    }
}