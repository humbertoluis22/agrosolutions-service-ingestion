using FluentValidation;
using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Text;
using AgrosolutionsServiceIngestion.Shared.DTOs.SensorData;
using AgrosolutionsServiceIngestion.Shared.DTOs.Request;
using AgrosolutionsServiceIngestion.Shared.Enums;

namespace AgrosolutionsServiceIngestion.Application.Validators
{
    public class SensorRawRequestValidator : AbstractValidator<SensorRawRequest>
    {
        public SensorRawRequestValidator()
        {
            RuleFor(x => x.FieldId).NotEmpty();
            RuleFor(x => x.SensorId).NotEmpty();
            RuleFor(x => x.TimeStamp).NotEmpty();
            RuleFor(x => x.TypeSensor).IsInEnum();

            // Validação Polimórfica
            RuleFor(x => x).Custom((request, context) =>
            {
                if (request.Data == null)
                {
                    context.AddFailure("Data", "Payload de dados não pode ser nulo.");
                    return;
                }

                try
                {
                    var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    string jsonString = request.Data.ToJsonString();

                    switch (request.TypeSensor)
                    {
                        case SensorType.Solo:
                            var solo = JsonSerializer.Deserialize<SoloData>(jsonString, jsonOptions);
                            if (solo == null)
                                context.AddFailure("Data", "Dados de solo inválidos");
                            break;

                        case SensorType.Silo:
                            var silo = JsonSerializer.Deserialize<SiloData>(jsonString, jsonOptions);
                            if (silo == null)
                                context.AddFailure("Data", "Dados de silo inválidos.");
                            break;

                        case SensorType.Meteorologica:
                            var met = JsonSerializer.Deserialize<MeteorologicaData>(jsonString, jsonOptions);
                            if (met == null) context.AddFailure("Data", "Dados meteorológicos inválidos.");
                            break;
                    }
                }
                catch
                {
                    context.AddFailure("Data", "Estrutura do JSON data não corresponde ao tipo de sensor informado.");
                }
            });
        }
    }
}
