using Domain;

namespace Application.Activities;

/// <summary>Данные формы события: только то, что клиент вправе задавать.</summary>
public class ActivityInput
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Venue { get; set; } = string.Empty;

    public Activity ToActivity(Guid id) => new()
    {
        Id = id,
        Title = Title,
        Date = Date,
        Description = Description,
        Category = Category,
        City = City,
        Venue = Venue
    };
}