using System.Text;
using System.Text.RegularExpressions;

namespace CoffeePeek.Shared.Infrastructure.Helpers;

/// <summary>
/// Helper для генерации имен событий CAP по конвенции {service-name}.{component}.{action}
/// </summary>
public static class CapEventNameHelper
{
    private static readonly Dictionary<string, string> ServiceNameMapping = new()
    {
        { "Account", "account" },
        { "Shops", "shops" },
        { "Moderation", "moderation" }
    };

    /// <summary>
    /// Генерирует имя события по конвенции из типа события
    /// </summary>
    /// <typeparam name="TEvent">Тип события</typeparam>
    /// <returns>Имя события в формате {service-name}.{component}.{action}</returns>
    public static string GetEventName<TEvent>()
    {
        return GetEventName(typeof(TEvent));
    }

    /// <summary>
    /// Генерирует имя события по конвенции из типа события
    /// </summary>
    /// <param name="eventType">Тип события</param>
    /// <returns>Имя события в формате {service-name}.{component}.{action}</returns>
    public static string GetEventName(Type eventType)
    {
        if (eventType == null)
            throw new ArgumentNullException(nameof(eventType));

        var fullName = eventType.FullName ?? eventType.Name;
        var parts = fullName.Split('.');

        // Извлекаем service-name из namespace
        string serviceName = "default";
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i] == "Events" && i + 1 < parts.Length)
            {
                var potentialServiceName = parts[i + 1];
                if (ServiceNameMapping.TryGetValue(potentialServiceName, out var mappedName))
                {
                    serviceName = mappedName;
                    break;
                }
            }
        }

        // Извлекаем component и action из имени класса
        var className = eventType.Name;
        
        // Удаляем суффикс "Event"
        if (className.EndsWith("Event", StringComparison.OrdinalIgnoreCase))
        {
            className = className.Substring(0, className.Length - 5);
        }

        // Удаляем префикс "Moderation" из имени класса, если service-name уже "moderation"
        // Это предотвращает дублирование: ModerationShopApproved -> shop.approved (не moderation.shop.approved)
        if (serviceName == "moderation" && className.StartsWith("Moderation", StringComparison.OrdinalIgnoreCase))
        {
            className = className.Substring(10); // "Moderation" = 10 символов
        }

        // Разделяем PascalCase на слова и преобразуем в lowercase
        var words = SplitPascalCase(className);
        var componentAndAction = string.Join(".", words.Select(w => w.ToLowerInvariant()));

        // Формируем итоговое имя: {service-name}.{component}.{action}
        return $"{serviceName}.{componentAndAction}";
    }

    /// <summary>
    /// Разделяет PascalCase строку на отдельные слова
    /// </summary>
    private static List<string> SplitPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return new List<string>();

        // Используем регулярное выражение для разделения PascalCase
        var words = Regex.Matches(input, @"([A-Z][a-z0-9]*|[a-z0-9]+)")
            .Cast<Match>()
            .Select(m => m.Value)
            .Where(w => !string.IsNullOrEmpty(w))
            .ToList();

        return words;
    }
}
