using Crosscutting.Exceptions;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;

namespace Application.Middleares
{
    public class ExceptionsMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var originalBodyStream = httpContext.Response.Body;

            using var responseBody = new MemoryStream();
            httpContext.Response.Body = responseBody;

            try
            {
                await _next(httpContext);
                // Volta o ponteiro para o início do stream para leitura
                responseBody.Seek(0, SeekOrigin.Begin);
                // Copia o conteúdo do responseBody para o original
                await responseBody.CopyToAsync(originalBodyStream);
                httpContext.Response.Body = originalBodyStream;
            }
            catch (InvalidOperationException ex)
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                httpContext.Response.ContentType = "application/json";
                var result = JsonSerializer.Serialize(new
                {
                    Mensagem = ex.Message
                });
                await httpContext.Response.WriteAsync(result);
                httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
            catch (ArgumentException ex)
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                httpContext.Response.ContentType = "application/json";
                var result = JsonSerializer.Serialize(new
                {
                    Mensagem = ex.Message
                });
                await httpContext.Response.WriteAsync(result);
                httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
            catch (NotFoundException ex)
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                httpContext.Response.ContentType = "application/json";
                var result = JsonSerializer.Serialize(new
                {
                    Mensagem = ex.Message
                });
                await httpContext.Response.WriteAsync(result);
                httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }
    }
}