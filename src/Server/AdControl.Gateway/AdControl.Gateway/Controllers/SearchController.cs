using AdControl.Gateway.Mapper;
using AdControl.Protos;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdControl.Gateway.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    private readonly ScreenService.ScreenServiceClient _screenClient;

    public SearchController(ScreenService.ScreenServiceClient screenClient)
    {
        _screenClient = screenClient;
    }

    /// <summary>
    /// Поиск по экранам и конфигам по триграммам.
    /// </summary>
    /// <response code="200">Список найденных экранов и конфигов</response>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Search([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Ok(new { screens = Array.Empty<object>(), configs = Array.Empty<object>() });

        var qTrigrams = BuildTrigrams(query);

        // Экраны
        var screensResp = await _screenClient.ListScreensAsync(
            new ListScreensRequest { Limit = 500, Offset = 0 },
            BuildAuthMetadata(HttpContext)
        ).ResponseAsync;

        var matchedScreens = screensResp.Screens
            .Where(s => IsMatch(s.Name ?? "", qTrigrams))
            .Select(s => s.MapToScreenDto())
            .ToList();

        // Конфиги
        var configsResp = await _screenClient.GetConfigsAsync(
            new GetConfigsRequest(),
            BuildAuthMetadata(HttpContext)
        );

        var matchedConfigs = configsResp.Configs
            .Where(c => IsMatch(c.Name ?? "", qTrigrams))
            .Select(c => c.MapToConfigDto())
            .ToList();

        return Ok(new
        {
            screens = matchedScreens,
            configs = matchedConfigs
        });
    }

    private static HashSet<string> BuildTrigrams(string value)
    {
        value = value.ToLower().Trim();
        if (value.Length <= 3)
            return new HashSet<string> { value };

        var set = new HashSet<string>();
        for (var i = 0; i < value.Length - 2; i++)
            set.Add(value.Substring(i, 3));
        return set;
    }

    private static bool IsMatch(string name, HashSet<string> qTrigrams)
    {
        name = name?.ToLower() ?? "";
        if (string.IsNullOrWhiteSpace(name))
            return false;

        var nameTrigrams = BuildTrigrams(name);

        var intersection = nameTrigrams.Intersect(qTrigrams).Count();
        var ratio = (double)intersection / qTrigrams.Count;

        return ratio >= 0.5;
    }

    private Metadata BuildAuthMetadata(HttpContext http)
    {
        var metadata = new Metadata();
        if (http.Request.Headers.TryGetValue("Authorization", out var auth))
            metadata.Add("Authorization", auth.ToString());
        return metadata;
    }
}