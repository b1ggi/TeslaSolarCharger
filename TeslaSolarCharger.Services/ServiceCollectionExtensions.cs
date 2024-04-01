﻿using Microsoft.Extensions.DependencyInjection;
using TeslaSolarCharger.Services.Services;
using TeslaSolarCharger.Services.Services.Contracts;

namespace TeslaSolarCharger.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServicesDependencies(this IServiceCollection services) =>
            services
                .AddTransient<IRestValueConfigurationService, RestValueConfigurationService>()
                .AddTransient<IRestValueExecutionService, RestValueExecutionService>()
                .AddTransient<IModbusValueConfigurationService, ModbusValueConfigurationService>()
                .AddTransient<IModbusValueExecutionService, ModbusValueExecutionService>()
            ;
}
