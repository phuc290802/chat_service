using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.Now;

        _logger.LogInformation("HTPP REQUEST: {Method} - {PATH}", context.Request.Method, context.Request.Path);

        try
        {
            await _next(context);

            var elapsed = DateTime.UtcNow - startTime;

            _logger.LogInformation("HTTP RESPONE: {Method} - {PATH} - Status: {StatusCode} - Time: {ElapsedMs}ms",
                context.Request.Method, context.Request.Path, context.Response.StatusCode, elapsed.TotalMilliseconds);
        }
        catch (Exception ex) 
        {
            var elapsed = DateTime.UtcNow - startTime;

            _logger.LogError("HTTP ERROR: {Method} {Path} - Failed after {ElapsedMs}ms - Error: {Error}",
                context.Request.Method, context.Request.Path, elapsed.TotalMilliseconds, ex.Message);
            throw;
        }
    }
}