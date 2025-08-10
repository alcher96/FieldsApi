using FieldsApi.Domain.Entities;

namespace FieldsApi.Domain.Repositories;

public interface IFieldRepository
{
    Task<List<Field>> GetAllFields(CancellationToken cancellationToken);
    Task<Field> GetFieldById(int id,CancellationToken cancellationToken);
}