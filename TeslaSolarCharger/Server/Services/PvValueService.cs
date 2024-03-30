using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Globalization;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using TeslaSolarCharger.Model.Contracts;
using TeslaSolarCharger.Model.Entities.TeslaSolarCharger;
using TeslaSolarCharger.Server.Contracts;
using TeslaSolarCharger.Services.Services.Contracts;
using TeslaSolarCharger.Shared.Contracts;
using TeslaSolarCharger.Shared.Dtos.Contracts;
using TeslaSolarCharger.Shared.Dtos.RestValueConfiguration;
using TeslaSolarCharger.Shared.Enums;
using TeslaSolarCharger.Shared.Resources.Contracts;
using TeslaSolarCharger.SharedBackend.Contracts;
using TeslaSolarCharger.SharedBackend.MappingExtensions;
using TeslaSolarCharger.SharedModel.Enums;

namespace TeslaSolarCharger.Server.Services;

public class PvValueService : IPvValueService
{
    private readonly ILogger<PvValueService> _logger;
    private readonly ISettings _settings;
    private readonly IInMemoryValues _inMemoryValues;
    private readonly IConfigurationWrapper _configurationWrapper;
    private readonly ITelegramService _telegramService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IConstants _constants;
    private readonly ITeslaSolarChargerContext _context;
    private readonly IRestValueExecutionService _restValueExecutionService;
    private readonly IMapperConfigurationFactory _mapperConfigurationFactory;
    private readonly IRestValueConfigurationService _restValueConfigurationService;

    public PvValueService(ILogger<PvValueService> logger, ISettings settings,
        IInMemoryValues inMemoryValues, IConfigurationWrapper configurationWrapper,
        ITelegramService telegramService,IDateTimeProvider dateTimeProvider,
        IConstants constants, ITeslaSolarChargerContext context,
        IRestValueExecutionService restValueExecutionService,
        IMapperConfigurationFactory mapperConfigurationFactory,
        IRestValueConfigurationService restValueConfigurationService)
    {
        _logger = logger;
        _settings = settings;
        _inMemoryValues = inMemoryValues;
        _configurationWrapper = configurationWrapper;
        _telegramService = telegramService;
        _dateTimeProvider = dateTimeProvider;
        _constants = constants;
        _context = context;
        _restValueExecutionService = restValueExecutionService;
        _mapperConfigurationFactory = mapperConfigurationFactory;
        _restValueConfigurationService = restValueConfigurationService;
    }

    public async Task ConvertToNewConfiguration()
    {
        if (await _context.RestValueConfigurations.AnyAsync())
        {
            return;
        }
        //Do not change order of the following methods
        await ConvertGridValueConfiguration();
        await ConvertInverterValueConfiguration();
        await ConvertHomeBatterySocConfiguration();
        await ConvertHomeBatteryPowerConfiguration();
    }

    private async Task ConvertHomeBatteryPowerConfiguration()
    {
        var homeBatteryPowerRequestUrl = _configurationWrapper.HomeBatteryPowerUrl();
        var frontendConfiguration = _configurationWrapper.FrontendConfiguration();
        if (!string.IsNullOrWhiteSpace(homeBatteryPowerRequestUrl) && frontendConfiguration is
                { HomeBatteryValuesSource: SolarValueSource.Modbus or SolarValueSource.Rest })
        {
            var patternType = frontendConfiguration.HomeBatteryPowerNodePatternType ?? NodePatternType.Direct;
            var newHomeBatteryPowerConfiguration = await _context.RestValueConfigurations
                .Where(r => r.Url == homeBatteryPowerRequestUrl)
                .FirstOrDefaultAsync();
            if (newHomeBatteryPowerConfiguration == default)
            {
                newHomeBatteryPowerConfiguration = new RestValueConfiguration()
                {
                    Url = homeBatteryPowerRequestUrl,
                    NodePatternType = patternType,
                    HttpMethod = HttpVerb.Get,
                };
                _context.RestValueConfigurations.Add(newHomeBatteryPowerConfiguration);
                var homeBatteryPowerHeaders = _configurationWrapper.HomeBatteryPowerHeaders();
                foreach (var homeBatteryPowerHeader in homeBatteryPowerHeaders)
                {
                    newHomeBatteryPowerConfiguration.Headers.Add(new RestValueConfigurationHeader()
                    {
                        Key = homeBatteryPowerHeader.Key,
                        Value = homeBatteryPowerHeader.Value,
                    });
                }
            }
            var resultConfiguration = new RestValueResultConfiguration()
            {
                CorrectionFactor = _configurationWrapper.HomeBatteryPowerCorrectionFactor(),
                UsedFor = ValueUsage.HomeBatteryPower,
            };
            if (newHomeBatteryPowerConfiguration.NodePatternType == NodePatternType.Xml)
            {
                resultConfiguration.NodePattern = _configurationWrapper.HomeBatteryPowerXmlPattern();
                resultConfiguration.XmlAttributeHeaderName = _configurationWrapper.HomeBatteryPowerXmlAttributeHeaderName();
                resultConfiguration.XmlAttributeHeaderValue = _configurationWrapper.HomeBatteryPowerXmlAttributeHeaderValue();
                resultConfiguration.XmlAttributeValueName = _configurationWrapper.HomeBatteryPowerXmlAttributeValueName();
            }
            else if (newHomeBatteryPowerConfiguration.NodePatternType == NodePatternType.Json)
            {
                resultConfiguration.NodePattern = _configurationWrapper.HomeBatteryPowerJsonPattern();
            }
            newHomeBatteryPowerConfiguration.RestValueResultConfigurations.Add(resultConfiguration);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
    }

    private async Task ConvertHomeBatterySocConfiguration()
    {
        var homeBatterySocRequestUrl = _configurationWrapper.HomeBatterySocUrl();
        var frontendConfiguration = _configurationWrapper.FrontendConfiguration();
        if (!string.IsNullOrWhiteSpace(homeBatterySocRequestUrl) && frontendConfiguration is
                { HomeBatteryValuesSource: SolarValueSource.Modbus or SolarValueSource.Rest })
        {
            var patternType = frontendConfiguration.HomeBatterySocNodePatternType ?? NodePatternType.Direct;
            var newHomeBatterySocConfiguration = await _context.RestValueConfigurations
                .Where(r => r.Url == homeBatterySocRequestUrl)
                .FirstOrDefaultAsync();
            if (newHomeBatterySocConfiguration == default)
            {
                newHomeBatterySocConfiguration = new RestValueConfiguration()
                {
                    Url = homeBatterySocRequestUrl,
                    NodePatternType = patternType,
                    HttpMethod = HttpVerb.Get,
                };
                _context.RestValueConfigurations.Add(newHomeBatterySocConfiguration);
                var hombatteryHeaders = _configurationWrapper.HomeBatterySocHeaders();
                foreach (var homeBatteryHeader in hombatteryHeaders)
                {
                    newHomeBatterySocConfiguration.Headers.Add(new RestValueConfigurationHeader()
                    {
                        Key = homeBatteryHeader.Key,
                        Value = homeBatteryHeader.Value,
                    });
                }
            }
            var resultConfiguration = new RestValueResultConfiguration()
            {
                CorrectionFactor = _configurationWrapper.HomeBatterySocCorrectionFactor(),
                UsedFor = ValueUsage.HomeBatterySoc,
            };
            if (newHomeBatterySocConfiguration.NodePatternType == NodePatternType.Xml)
            {
                resultConfiguration.NodePattern = _configurationWrapper.HomeBatterySocXmlPattern();
                resultConfiguration.XmlAttributeHeaderName = _configurationWrapper.HomeBatterySocXmlAttributeHeaderName();
                resultConfiguration.XmlAttributeHeaderValue = _configurationWrapper.HomeBatterySocXmlAttributeHeaderValue();
                resultConfiguration.XmlAttributeValueName = _configurationWrapper.HomeBatterySocXmlAttributeValueName();
            }
            else if (newHomeBatterySocConfiguration.NodePatternType == NodePatternType.Json)
            {
                resultConfiguration.NodePattern = _configurationWrapper.HomeBatterySocJsonPattern();
            }
            newHomeBatterySocConfiguration.RestValueResultConfigurations.Add(resultConfiguration);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
    }

    private async Task ConvertInverterValueConfiguration()
    {
        var inverterRequestUrl = _configurationWrapper.CurrentInverterPowerUrl();
        var frontendConfiguration = _configurationWrapper.FrontendConfiguration();
        if (!string.IsNullOrWhiteSpace(inverterRequestUrl) && frontendConfiguration is
                { InverterValueSource: SolarValueSource.Modbus or SolarValueSource.Rest })
        {
            var patternType = frontendConfiguration.InverterPowerNodePatternType ?? NodePatternType.Direct;
            var newInverterConfiguration = await _context.RestValueConfigurations
                .Where(r => r.Url == inverterRequestUrl)
                .FirstOrDefaultAsync();
            if (newInverterConfiguration == default)
            {
                newInverterConfiguration = new RestValueConfiguration()
                {
                    Url = inverterRequestUrl,
                    NodePatternType = patternType,
                    HttpMethod = HttpVerb.Get,
                };
                _context.RestValueConfigurations.Add(newInverterConfiguration);
                var inverterRequestHeaders = _configurationWrapper.CurrentInverterPowerHeaders();
                foreach (var inverterRequestHeader in inverterRequestHeaders)
                {
                    newInverterConfiguration.Headers.Add(new RestValueConfigurationHeader()
                    {
                        Key = inverterRequestHeader.Key,
                        Value = inverterRequestHeader.Value,
                    });
                }
            }
            var resultConfiguration = new RestValueResultConfiguration()
            {
                CorrectionFactor = _configurationWrapper.CurrentInverterPowerCorrectionFactor(),
                UsedFor = ValueUsage.InverterPower,
            };
            if (newInverterConfiguration.NodePatternType == NodePatternType.Xml)
            {
                resultConfiguration.NodePattern = _configurationWrapper.CurrentInverterPowerXmlPattern();
                resultConfiguration.XmlAttributeHeaderName = _configurationWrapper.CurrentInverterPowerXmlAttributeHeaderName();
                resultConfiguration.XmlAttributeHeaderValue = _configurationWrapper.CurrentInverterPowerXmlAttributeHeaderValue();
                resultConfiguration.XmlAttributeValueName = _configurationWrapper.CurrentInverterPowerXmlAttributeValueName();
            }
            else if (newInverterConfiguration.NodePatternType == NodePatternType.Json)
            {
                resultConfiguration.NodePattern = _configurationWrapper.CurrentInverterPowerJsonPattern();
            }
            newInverterConfiguration.RestValueResultConfigurations.Add(resultConfiguration);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
    }

    private async Task ConvertGridValueConfiguration()
    {
        var gridRequestUrl = _configurationWrapper.CurrentPowerToGridUrl();
        var frontendConfiguration = _configurationWrapper.FrontendConfiguration();
        if (!string.IsNullOrWhiteSpace(gridRequestUrl) && frontendConfiguration is
                { GridValueSource: SolarValueSource.Modbus or SolarValueSource.Rest })
        {
            var patternType = frontendConfiguration.GridPowerNodePatternType ?? NodePatternType.Direct;
            var newGridConfiguration = new RestValueConfiguration()
            {
                Url = gridRequestUrl,
                NodePatternType = patternType,
                HttpMethod = HttpVerb.Get,
            };
            _context.RestValueConfigurations.Add(newGridConfiguration);
            var gridRequestHeaders = _configurationWrapper.CurrentPowerToGridHeaders();
            foreach (var gridRequestHeader in gridRequestHeaders)
            {
                newGridConfiguration.Headers.Add(new RestValueConfigurationHeader()
                {
                    Key = gridRequestHeader.Key,
                    Value = gridRequestHeader.Value,
                });
            }
            var resultConfiguration = new RestValueResultConfiguration()
            {
                CorrectionFactor = _configurationWrapper.CurrentPowerToGridCorrectionFactor(),
                UsedFor = ValueUsage.GridPower,
            };
            if (newGridConfiguration.NodePatternType == NodePatternType.Xml)
            {
                resultConfiguration.NodePattern = _configurationWrapper.CurrentPowerToGridXmlPattern();
                resultConfiguration.XmlAttributeHeaderName = _configurationWrapper.CurrentPowerToGridXmlAttributeHeaderName();
                resultConfiguration.XmlAttributeHeaderValue = _configurationWrapper.CurrentPowerToGridXmlAttributeHeaderValue();
                resultConfiguration.XmlAttributeValueName = _configurationWrapper.CurrentPowerToGridXmlAttributeValueName();
            }
            else if (newGridConfiguration.NodePatternType == NodePatternType.Json)
            {
                resultConfiguration.NodePattern = _configurationWrapper.CurrentPowerToGridJsonPattern();
            }
            newGridConfiguration.RestValueResultConfigurations.Add(resultConfiguration);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
    }

    public async Task UpdatePvValues()
    {
        _logger.LogTrace("{method}()", nameof(UpdatePvValues));
        var valueUsages = new HashSet<ValueUsage>
        {
            ValueUsage.InverterPower,
            ValueUsage.GridPower,
            ValueUsage.HomeBatteryPower,
            ValueUsage.HomeBatterySoc,
        };
        var restConfigurations = await _restValueConfigurationService
            .GetFullRestValueConfigurationsByPredicate(c => c.RestValueResultConfigurations.Any(r => valueUsages.Contains(r.UsedFor))).ConfigureAwait(false);
        var resultSums = new Dictionary<ValueUsage, decimal>();
        foreach (var restConfiguration in restConfigurations)
        {
            try
            {
                var responseString = await _restValueExecutionService.GetResult(restConfiguration).ConfigureAwait(false);
                var resultConfigurations = await _restValueConfigurationService.GetResultConfigurationsByConfigurationId(restConfiguration.Id).ConfigureAwait(false);
                var results = new Dictionary<int, decimal>();
                foreach (var resultConfiguration in resultConfigurations)
                {
                    results.Add(resultConfiguration.Id, _restValueExecutionService.GetValue(responseString, restConfiguration.NodePatternType, resultConfiguration));
                }
                foreach (var result in results)
                {
                    var valueUsage = resultConfigurations.First(r => r.Id == result.Key).UsedFor;
                    if (!resultSums.ContainsKey(valueUsage))
                    {
                        resultSums[valueUsage] = 0;
                    }
                    resultSums[valueUsage] += result.Value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting result for {restConfigurationId} with URL {url}", restConfiguration.Id, restConfiguration.Url);
            }
        }

        _settings.InverterPower = resultSums.TryGetValue(ValueUsage.InverterPower, out var inverterPower) ? (int?)inverterPower : null;
        _settings.Overage = resultSums.TryGetValue(ValueUsage.GridPower, out var gridPower) ? (int?)gridPower : null;
        _settings.HomeBatteryPower = resultSums.TryGetValue(ValueUsage.HomeBatteryPower, out var homeBatteryPower) ? (int?)homeBatteryPower : null;
        _settings.HomeBatterySoc = resultSums.TryGetValue(ValueUsage.HomeBatterySoc, out var homeBatterySoc) ? (int?)homeBatterySoc : null;
        _settings.LastPvValueUpdate = _dateTimeProvider.DateTimeOffSetNow();
    }

    

    private async Task<int?> GetValueByHttpResponse(HttpResponseMessage? httpResponse, string? jsonPattern, string? xmlPattern,
        double correctionFactor, NodePatternType nodePatternType, string? xmlAttributeHeaderName, string? xmlAttributeHeaderValue, string? xmlAttributeValueName)
    {
        int? intValue;
        if (httpResponse == null)
        {
            _logger.LogError("HttpResponse is null, extraction of value is not possible");
            return null;
        }
        if (!httpResponse.IsSuccessStatusCode)
        {
            intValue = null;
            _logger.LogError("Could not get value. {statusCode}, {reasonPhrase}", httpResponse.StatusCode,
                httpResponse.ReasonPhrase);
            await _telegramService.SendMessage(
                    $"Getting value did result in statuscode {httpResponse.StatusCode} with reason {httpResponse.ReasonPhrase}")
                .ConfigureAwait(false);
        }
        else
        {
            intValue = await GetIntegerValue(httpResponse, jsonPattern, xmlPattern, correctionFactor, nodePatternType, xmlAttributeHeaderName, xmlAttributeHeaderValue, xmlAttributeValueName).ConfigureAwait(false);
        }

        return intValue;
    }

    private async Task<HttpResponseMessage> GetHttpResponse(HttpRequestMessage request)
    {
        _logger.LogTrace("{method}({request}) [called by {callingMethod}]", nameof(GetHttpResponse), request, new StackTrace().GetFrame(1)?.GetMethod()?.Name);
        var httpClientHandler = new HttpClientHandler();

        if (_configurationWrapper.ShouldIgnoreSslErrors())
        {
            _logger.LogWarning("PV Value SSL errors are ignored.");
            httpClientHandler.ServerCertificateCustomValidationCallback = MyRemoteCertificateValidationCallback;
        }

        using var httpClient = new HttpClient(httpClientHandler);
        var response = await httpClient.SendAsync(request).ConfigureAwait(false);
        return response;
    }

    private bool MyRemoteCertificateValidationCallback(HttpRequestMessage requestMessage, X509Certificate2? certificate, X509Chain? chain, SslPolicyErrors sslErrors)
    {
        return true; // Ignoriere alle Zertifikatfehler
    }

    private static HttpRequestMessage GenerateHttpRequestMessage(string? gridRequestUrl, Dictionary<string, string> requestHeaders)
    {
        if (string.IsNullOrEmpty(gridRequestUrl))
        {
            throw new ArgumentNullException(nameof(gridRequestUrl));
        }

        var request = new HttpRequestMessage(HttpMethod.Get, gridRequestUrl);
        request.Headers.Add("Accept", "*/*");
        foreach (var requestHeader in requestHeaders)
        {
            request.Headers.Add(requestHeader.Key, requestHeader.Value);
        }

        return request;
    }

    public void ClearOverageValues()
    {
        _inMemoryValues.OverageValues.Clear();
    }

    public void AddOverageValueToInMemoryList(int overage)
    {
        _logger.LogTrace("{method}({overage})", nameof(AddOverageValueToInMemoryList), overage);
        _inMemoryValues.OverageValues.Add(overage);

        var valuesToSave = (int)(_configurationWrapper.ChargingValueJobUpdateIntervall().TotalSeconds /
                            _configurationWrapper.PvValueJobUpdateIntervall().TotalSeconds);

        if (_inMemoryValues.OverageValues.Count > valuesToSave)
        {
            _inMemoryValues.OverageValues.RemoveRange(0, _inMemoryValues.OverageValues.Count - valuesToSave);
        }
    }

    internal bool IsSameRequest(HttpRequestMessage? httpRequestMessage1, HttpRequestMessage httpRequestMessage2)
    {
        _logger.LogTrace("{method}({request1}, {request2})", nameof(IsSameRequest), httpRequestMessage1, httpRequestMessage2);
        if (httpRequestMessage1 == null)
        {
            _logger.LogTrace("Not same request as first request is null.");
            return false;
        }
        if (httpRequestMessage1.Method != httpRequestMessage2.Method)
        {
            _logger.LogDebug("not same request as request1 method is {request1} and request2 method is {request2}",
                httpRequestMessage1.Method, httpRequestMessage2.Method);
            return false;
        }

        if (httpRequestMessage1.RequestUri != httpRequestMessage2.RequestUri)
        {
            _logger.LogDebug("not same request as request1 Uri is {request1} and request2 Uri is {request2}",
                httpRequestMessage1.RequestUri, httpRequestMessage2.RequestUri);
            return false;
        }

        if (httpRequestMessage1.Headers.Count() != httpRequestMessage2.Headers.Count())
        {
            _logger.LogDebug("not same request as request1 header count is {request1} and request2 header count is {request2}",
                httpRequestMessage1.Headers.Count(), httpRequestMessage2.Headers.Count());
            return false;
        }

        foreach (var httpRequestHeader in httpRequestMessage1.Headers)
        {
            var message2Header = httpRequestMessage2.Headers.FirstOrDefault(h => h.Key.Equals(httpRequestHeader.Key));
            if (message2Header.Key == default)
            {
                return false;
            }
            var message2HeaderValue = message2Header.Value.ToList();
            foreach (var headerValue in httpRequestHeader.Value)
            {
                if (!message2HeaderValue.Any(v => string.Equals(v, headerValue, StringComparison.InvariantCulture)))
                {
                    return false;
                }
            }
        }


        return true;
    }



    private async Task<int?> GetIntegerValue(HttpResponseMessage response, string? jsonPattern, string? xmlPattern, double correctionFactor,
        NodePatternType nodePatternType, string? xmlAttributeHeaderName, string? xmlAttributeHeaderValue, string? xmlAttributeValueName)
    {
        _logger.LogTrace("{method}({httpResonse}, {jsonPattern}, {xmlPattern}, {correctionFactor})",
            nameof(GetIntegerValue), response, jsonPattern, xmlPattern, correctionFactor);

        var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        return GetIntegerValueByString(result, jsonPattern, xmlPattern, correctionFactor, nodePatternType, xmlAttributeHeaderName, xmlAttributeHeaderValue, xmlAttributeValueName);
    }

    public int? GetIntegerValueByString(string valueString, string? jsonPattern, string? xmlPattern, double correctionFactor,
        NodePatternType nodePatternType, string? xmlAttributeHeaderName, string? xmlAttributeHeaderValue, string? xmlAttributeValueName)
    {
        _logger.LogTrace("{method}({valueString}, {jsonPattern}, {xmlPattern}, {correctionFactor})",
            nameof(GetIntegerValueByString), valueString, jsonPattern, xmlPattern, correctionFactor);
        var pattern = string.Empty;

        if (nodePatternType == NodePatternType.Json)
        {
            pattern = jsonPattern;
        }
        else if (nodePatternType == NodePatternType.Xml)
        {
            pattern = xmlPattern;
        }

        var doubleValue = GetValueFromResult(pattern, valueString, nodePatternType, xmlAttributeHeaderName, xmlAttributeHeaderValue, xmlAttributeValueName);

        return (int?)(doubleValue * correctionFactor);
    }
    
    internal double GetValueFromResult(string? pattern, string result, NodePatternType patternType,
        string? xmlAttributeHeaderName, string? xmlAttributeHeaderValue, string? xmlAttributeValueName)
    {
        switch (patternType)
        {
            //allow JSON values to be null, as this is needed by SMA inverters: https://tff-forum.de/t/teslasolarcharger-laden-nach-pv-ueberschuss-mit-beliebiger-wallbox/170369/2728?u=mane123
            case NodePatternType.Json:
                _logger.LogTrace("Extract overage value from json {result} with {pattern}", result, pattern);
                result = (JObject.Parse(result).SelectToken(pattern ?? throw new ArgumentNullException(nameof(pattern))) ??
                          throw new InvalidOperationException("Could not find token by pattern")).Value<string>() ?? "0";
                break;
            case NodePatternType.Xml:
                _logger.LogTrace("Extract overage value from xml {result} with {pattern}", result, pattern);
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(result);
                var nodes = xmlDocument.SelectNodes(pattern ?? throw new ArgumentNullException(nameof(pattern))) ?? throw new InvalidOperationException("Could not find any nodes by pattern");
                switch (nodes.Count)
                {
                    case < 1:
                        throw new InvalidOperationException($"Could not find any nodes with pattern {pattern}");
                    case 1:
                        result = nodes[0]?.LastChild?.Value ?? "0";
                        break;
                    case > 2:
                        for (var i = 0; i < nodes.Count; i++)
                        {
                            if (nodes[i]?.Attributes?[xmlAttributeHeaderName ?? throw new ArgumentNullException(nameof(xmlAttributeHeaderName))]?.Value == xmlAttributeHeaderValue)
                            {
                                result = nodes[i]?.Attributes?[xmlAttributeValueName ?? throw new ArgumentNullException(nameof(xmlAttributeValueName))]?.Value ?? "0";
                                break;
                            }
                        }
                        break;
                }
                break;
        }

        return GetdoubleFromStringResult(result);
    }

    internal double GetdoubleFromStringResult(string? inputString)
    {
        _logger.LogTrace("{method}({param})", nameof(GetdoubleFromStringResult), inputString);
        return double.Parse(inputString ?? throw new ArgumentNullException(nameof(inputString)), CultureInfo.InvariantCulture);
    }
}
