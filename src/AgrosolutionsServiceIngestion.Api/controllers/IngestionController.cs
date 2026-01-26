using AgrosolutionsServiceIngestion.Application.UseCase;
using AgrosolutionsServiceIngestion.Shared.Events;
using Microsoft.AspNetCore.Mvc;

namespace AgrosolutionsServiceIngestion.Api.controllers
{
    [ApiController]
    [Route("api/ingestion")]
    public class IngestionController : ControllerBase
    {
        private readonly PublishSensorRawUseCase _useCase;

        public IngestionController(PublishSensorRawUseCase useCase)
        {
            _useCase = useCase;
        }

        [HttpPost("sensor")]
        public async Task<IActionResult> Ingest([FromBody] SensorRawCollectedEvent dto)
        {
            await _useCase.ExecuteAsync(dto);
            return Accepted();
        }
    }

}
