using FieldsApi.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FieldsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FieldsController(IFieldService fieldService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllFields(CancellationToken cancellationToken)
    {
        var fields = await fieldService.GetAllFields(cancellationToken);
        return Ok(fields);
    }

    [HttpGet("{id:int}/size")]
    public async Task<IActionResult> GetFieldSize(int id,CancellationToken cancellationToken)
    {
        var size = await fieldService.GetFieldSize(id);
        return Ok(new { Size = size });
    }

    [HttpGet("{id:int}/distance")]
    public async Task<IActionResult> GetDistanceToCentroid(int id, [FromQuery] double lat, [FromQuery] double lng,
        CancellationToken cancellationToken)
    {
        var distance = await fieldService.GetDistanceToCentroid(id, lat, lng,cancellationToken);
        return Ok(new { Distance = distance });
    }

    [HttpGet("contains")]
    public async Task<IActionResult> GetFieldContainingPoint([FromQuery] double lat, [FromQuery] double lng,
        CancellationToken cancellationToken)
    {
        var field = await fieldService.GetFieldContainingPoint(lat, lng,cancellationToken);
        return Ok(field != null ? new { field.Value.Id, field.Value.Name } : false);
    }
}