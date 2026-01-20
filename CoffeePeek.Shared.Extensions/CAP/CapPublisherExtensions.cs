using CoffeePeek.Shared.Infrastructure.Helpers;
using DotNetCore.CAP;

namespace CoffeePeek.Shared.Extensions.CAP;

/// <summary>
/// Extension методы для ICapPublisher с автоматической генерацией имен событий
/// </summary>
public static class CapPublisherExtensions
{
    /// <summary>
    /// Публикует событие с автоматической генерацией имени по конвенции
    /// </summary>
    /// <typeparam name="TEvent">Тип события</typeparam>
    /// <param name="publisher">CAP publisher</param>
    /// <param name="event">Событие для публикации</param>
    /// <param name="cancellationToken">Токен отмены</param>
    public static Task PublishAsync<TEvent>(
        this ICapPublisher publisher,
        TEvent @event,
        CancellationToken cancellationToken = default)
        where TEvent : class
    {
        var eventName = CapEventNameHelper.GetEventName<TEvent>();
        return publisher.PublishAsync(eventName, @event, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Публикует событие с автоматической генерацией имени по конвенции и кастомными заголовками
    /// </summary>
    /// <typeparam name="TEvent">Тип события</typeparam>
    /// <param name="publisher">CAP publisher</param>
    /// <param name="event">Событие для публикации</param>
    /// <param name="headers">Заголовки сообщения</param>
    /// <param name="cancellationToken">Токен отмены</param>
    public static Task PublishAsync<TEvent>(
        this ICapPublisher publisher,
        TEvent @event,
        IDictionary<string, string> headers,
        CancellationToken cancellationToken = default)
        where TEvent : class
    {
        var eventName = CapEventNameHelper.GetEventName<TEvent>();
        return publisher.PublishAsync(eventName, @event, headers, cancellationToken: cancellationToken);
    }
}
