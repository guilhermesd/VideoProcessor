using Application.Middleares;
using Crosscutting.Exceptions;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace Tests.Unidade.Application.Middleares
{
    public class ExceptionsMiddlewareTests
    {
        [Fact]
        public async Task Invoke_Should_Handle_InvalidOperationException_Correctly()
        {
            // Arrange
            var mockContext = new DefaultHttpContext(); // Use DefaultHttpContext em vez de Mock
            var responseStream = new MemoryStream();
            mockContext.Response.Body = responseStream;

            var mockRequestDelegate = new RequestDelegate(context =>
            {
                throw new InvalidOperationException("Test exception");
            });

            var middleware = new ExceptionsMiddleware(mockRequestDelegate);

            // Act
            await middleware.Invoke(mockContext);

            // Assert
            Assert.Equal((int)HttpStatusCode.BadRequest, mockContext.Response.StatusCode);
            responseStream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(responseStream);
            var responseBody = await reader.ReadToEndAsync();
            Assert.Contains("Test exception", responseBody);
        }

        [Fact]
        public async Task Invoke_Should_Handle_ArgumentException_Correctly()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var responseStream = new MemoryStream();
            context.Response.Body = responseStream;

            var mockRequestDelegate = new RequestDelegate(_ =>
            {
                throw new ArgumentException("Argument exception");
            });

            var middleware = new ExceptionsMiddleware(mockRequestDelegate);

            // Act
            await middleware.Invoke(context);

            // Assert
            Assert.Equal((int)HttpStatusCode.BadRequest, context.Response.StatusCode);
            responseStream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(responseStream);
            var responseBody = await reader.ReadToEndAsync();
            Assert.Contains("Argument exception", responseBody);
        }

        [Fact]
        public async Task Invoke_Should_Call_Next_RequestDelegate()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var responseStream = new MemoryStream();
            context.Response.Body = responseStream;

            var wasCalled = false;
            var mockRequestDelegate = new RequestDelegate(_ =>
            {
                wasCalled = true;
                return Task.CompletedTask;
            });

            var middleware = new ExceptionsMiddleware(mockRequestDelegate);

            // Act
            await middleware.Invoke(context);

            // Assert
            Assert.True(wasCalled, "O próximo RequestDelegate não foi chamado.");
        }

        [Fact]
        public async Task Invoke_Should_Handle_NotFoundException_Correctly()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var responseStream = new MemoryStream(); 
            context.Response.Body = responseStream; 

            var mockRequestDelegate = new RequestDelegate(_ =>
            {
                throw new NotFoundException("Argument exception");
            });

            var middleware = new ExceptionsMiddleware(mockRequestDelegate);

            // Act
            await middleware.Invoke(context);

            // Assert
            Assert.Equal((int)HttpStatusCode.NotFound, context.Response.StatusCode);
            responseStream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(responseStream);
            var responseBody = await reader.ReadToEndAsync();
            Assert.Contains("Argument exception", responseBody);
        }

    }
}
