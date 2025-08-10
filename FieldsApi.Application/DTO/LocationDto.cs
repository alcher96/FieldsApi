// ReSharper disable UnusedAutoPropertyAccessor.Global
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace FieldsApi.Application.DTO;

public class LocationDto
{
    public double[] Center { get; set; }
    public double[][] Polygon { get; set; }
}