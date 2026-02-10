using DotNetCore.CAP;

namespace CoffeePeek.Shared.Extensions.CAP;

public static class CAPPublisherExtensions
{
    /// <summary>
    /// Публикует событие с автоматической генерацией имени по конвенции
    /// </summary>
    /// <typeparam name="TEvent">Тип события</typeparam>
    /// <param name="publisher">CAP publisher</param>
    /// <param name="eventName">Имя события</param>
    /// <param name="event">Событие для публикации</param>
    /// <param name="cancellationToken">Токен отмены</param>
    public static Task PublishAsync<TEvent>(
        this ICapPublisher publisher,
        string eventName,
        TEvent @event,
        CancellationToken cancellationToken = default)
        where TEvent : class
    {
        return publisher.PublishAsync(eventName, @event, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Публикует событие с автоматической генерацией имени по конвенции и кастомными заголовками
    /// </summary>
    /// <typeparam name="TEvent">Тип события</typeparam>
    /// <param name="publisher">CAP publisher</param>
    /// <param name="eventName">Имя события</param>
    /// <param name="event">Событие для публикации</param>
    /// <param name="headers">Заголовки сообщения</param>
    /// <param name="cancellationToken">Токен отмены</param>
    public static Task PublishAsync<TEvent>(
        this ICapPublisher publisher,
        string eventName,
        TEvent @event,
        IDictionary<string, string> headers,
        CancellationToken cancellationToken = default)
        where TEvent : class
    {
        return publisher.PublishAsync(eventName, @event, headers, cancellationToken: cancellationToken);
    }
}
