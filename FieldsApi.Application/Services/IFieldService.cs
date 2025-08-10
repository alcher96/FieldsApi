using FieldsApi.Application.DTO;

namespace FieldsApi.Application.Services;

public interface IFieldService
{
    Task<List<FieldDto>> GetAllFields(CancellationToken cancellationToken = default);
    Task<double> GetFieldSize(int id,CancellationToken cancellationToken = default);
    Task<double> GetDistanceToCentroid(int id, double lat, double lng,CancellationToken cancellationToken = default);
    Task<(int Id, string Name)?> GetFieldContainingPoint(double lat, double lng,CancellationToken cancellationToken = default);
}