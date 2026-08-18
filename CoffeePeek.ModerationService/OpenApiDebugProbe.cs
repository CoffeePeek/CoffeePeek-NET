using System.Text.Json;
using System.Text.Json.Serialization;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Moderation.Application.Features.Admin.Audit;
using CoffeePeek.ModerationService.Controllers;

namespace CoffeePeek.ModerationService;

internal static class OpenApiDebugProbe
{
    public static void Run()
    {
#region agent log
        Append("H0", "OpenApiDebugProbe.cs:15", "openapi_probe_run_entry", new { probeCount = 3 });
#endregion
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());

        ProbeTypeInfo(options, typeof(GetModerationAuditLogQuery), "H1");
        ProbeTypeInfo(options, typeof(DecideImportCandidateRequest), "H2");
        ProbeTypeInfo(options, typeof(ModerationShopDto), "H3");
#region agent log
        Append("H0", "OpenApiDebugProbe.cs:24", "openapi_probe_run_exit", new { probeCount = 3 });
#endregion
    }

    private static void ProbeTypeInfo(JsonSerializerOptions options, Type type, string hypothesisId)
    {
#region agent log
        Append(hypothesisId, "OpenApiDebugProbe.cs:30", "probe_before_get_type_info", new { type = type.FullName });
#endregion
        try
        {
            _ = options.GetTypeInfo(type);
#region agent log
            Append(hypothesisId, "OpenApiDebugProbe.cs:36", "probe_after_get_type_info_success", new { type = type.FullName });
#endregion
        }
        catch (Exception ex)
        {
#region agent log
            Append(
                hypothesisId,
                "OpenApiDebugProbe.cs:43",
                "probe_after_get_type_info_failure",
                new { type = type.FullName, exceptionType = ex.GetType().FullName, exceptionMessage = ex.Message });
#endregion
        }
    }

    private static void Append(string hypothesisId, string location, string message, object data)
    {
        File.AppendAllText(
            "/opt/cursor/logs/debug.log",
            JsonSerializer.Serialize(new { hypothesisId, location, message, data, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
    }
}
