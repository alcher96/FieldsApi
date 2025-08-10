namespace FieldsApi.Domain.Entities;

public class Field
{
    public int Id { get; set; }
    public string? Name { get; init; }
    public double Size { get; set; }
    public Centroid? Centroid { get; set; }
    public List<Point>? Polygon { get; init; }
}