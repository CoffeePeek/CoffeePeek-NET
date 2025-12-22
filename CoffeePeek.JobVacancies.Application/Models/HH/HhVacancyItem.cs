using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CoffeePeek.JobVacancies.Models;

public class HhVacancyItem
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    [JsonPropertyName("alternate_url")]
    public string AlternateUrl { get; set; } = null!;
    [JsonPropertyName("apply_alternate_url")]
    public string? ApplyAlternateUrl { get; set; } = null!;
    [JsonPropertyName("accept_incomplete_resumes")]
    public bool? AcceptIncompleteResumes { get; set; }
    [JsonPropertyName("accept_temporary")]
    public bool? AcceptTemporary { get; set; }
    public HhArea? Area { get; set; }
    public HhEmployer? Employer { get; set; }
    public HhAddress? Address { get; set; }
    public bool? Archived { get; set; }
    [JsonPropertyName("has_test")]
    public bool HasTest { get; set; }
    public bool? Internship { get; set; }
    [JsonPropertyName("night_shifts")]
    public bool NightShifts { get; set; }
    public bool? Premium { get; set; }
    [JsonPropertyName("professional_roles")]
    public List<HhProfessionalRole> ProfessionalRoles { get; set; } = [];
    public List<string>? Relations { get; set; }
    [JsonPropertyName("response_letter_required")]
    public bool ResponseLetterRequired { get; set; }
    [JsonPropertyName("response_url")]
    public string? ResponseUrl { get; set; }
    public HhSalary? Salary { get; set; }
    public HhVacancyType Type { get; set; } = null!;
    [JsonPropertyName("show_contacts")]
    public bool? ShowContacts { get; set; }
    [JsonPropertyName("sort_point_distance")]
    public double? SortPointDistance { get; set; }
    [JsonPropertyName("work_format")]
    public List<HhWorkFormat>? WorkFormat { get; set; }
    [JsonPropertyName("work_schedules")]
    public List<HhWorkSchedule>? WorkSchedules { get; set; }
    public HhCounters? Counters { get; set; }
    public HhExperience? Experience { get; set; }
    public HhExtraText? ExtraText { get; set; }
    [JsonPropertyName("show_logo_in_search")]
    public bool? ShowLogoInSearch { get; set; }
    [JsonPropertyName("video_vacancy")]
    public HhVideoVacancy? VideoVacancy { get; set; }
    public HhDepartment? Department { get; set; }
    
    [JsonPropertyName("created_at")]
    public string CreatedAtRaw { get; set; } = null!;
    [JsonPropertyName("published_at")]
    public string PublishedAtRaw { get; set; } = null!;
    [NotMapped]
    public DateTime CreatedAt => DateTime.Parse(CreatedAtRaw, null, System.Globalization.DateTimeStyles.AdjustToUniversal);
    [NotMapped]
    public DateTime PublishedAt => DateTime.Parse(PublishedAtRaw, null, System.Globalization.DateTimeStyles.AdjustToUniversal);
}