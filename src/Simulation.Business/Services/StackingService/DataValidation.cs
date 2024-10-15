using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Query;
using SimulationServer.Business.Domain;
using SimulationServer.Business.Infrastructure;

namespace SimulationServer.Business.Services.StackingService;

public interface IDataValidation
{
    public Result<SensorData> Validate(SensorData sensorData);
}


public class DataValidation : IDataValidation
{
    public Result<SensorData> Validate(SensorData sensorData)
    {
        if (!IsValidLatitude(sensorData.Latitude) || !IsValidLongitude(sensorData.Longitude))
        {
            return Result<SensorData>.Fail(new ValidationException("Invalid latitude or longitude"));
        }
        return sensorData;
    }

     private bool IsValidLatitude(double latitude)
    {
        return latitude >= -90 && latitude <= 90;
    }

    private bool IsValidLongitude(double longitude)
    {
        return longitude >= -180 && longitude <= 180;
    }
}
