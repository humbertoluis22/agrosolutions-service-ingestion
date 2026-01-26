using AgrosolutionsServiceIngestion.Shared.DTOs;
using AgrosolutionsServiceIngestion.Application.Handlers;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/sensors")]
public class SensorsController : ControllerBase
{
    private readonly CreateSensorHandler _handler;

    public SensorsController(CreateSensorHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateSensorRequest request)
    {
        await _handler.ExecuteAsync(request);
        return Accepted();
    }
}
