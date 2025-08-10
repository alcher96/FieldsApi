namespace FieldsApi.Application.DTO;

public class FieldDto
{
    public int Id { get; init; }
    public string? Name { get; init; }
    public double Size { get; init; }
    public LocationDto? Locations { get; init; }
}