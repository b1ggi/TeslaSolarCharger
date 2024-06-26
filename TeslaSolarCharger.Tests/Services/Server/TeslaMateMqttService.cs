﻿using System;
using System.Collections.Generic;
using System.Linq;
using TeslaSolarCharger.Server.Services;
using TeslaSolarCharger.Shared.Dtos.Contracts;
using TeslaSolarCharger.Shared.Dtos.Settings;
using Xunit;
using Xunit.Abstractions;

namespace TeslaSolarCharger.Tests.Services.Server;

public class TeslaMateMqttService : TestBase
{
    public TeslaMateMqttService(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Theory]
    [InlineData("1")]
    [InlineData("3")]
    [InlineData("4")]
    [InlineData("5")]
    [InlineData("8")]
    public void ReducesActualCurrentToLastSetAmpIfDifferenceIsOneAndBelow5AAndEqualToRequestedCurrent(string value)
    {
        var cars = new List<DtoCar>()
        {
            new DtoCar()
            {
                Id = 1,
                    LastSetAmp = 3,
                    ChargerRequestedCurrent = 3,
                    TeslaMateCarId = 1,
            },
        };
        Mock.Mock<ISettings>().Setup(s => s.Cars).Returns(cars);

        var mqttService = Mock.Create<TeslaSolarCharger.Server.Services.TeslaMateMqttService>();

        var teslamateValue = new TeslaMateValue()
        {
            CarId = 1,
            Topic = "charger_actual_current",
            Value = value,
        };
        mqttService.UpdateCar(teslamateValue);

        switch (value)
        {
            case "1":
                Assert.Equal(1, cars.First().ChargerActualCurrent);
                break;
            case "3":
            case "4":
                Assert.Equal(3, cars.First().ChargerActualCurrent);
                break;
            case "5":
                Assert.Equal(5, cars.First().ChargerActualCurrent);
                break;
            case "8":
                Assert.Equal(8, cars.First().ChargerActualCurrent);
                break;
            default:
                throw new NotImplementedException();
        }
    }
}
