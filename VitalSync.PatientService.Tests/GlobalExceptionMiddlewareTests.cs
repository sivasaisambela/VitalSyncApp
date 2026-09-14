using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using VitalSync.PatientService.Api.Middleware;
using Xunit;

namespace VitalSync.PatientService.Tests
{
    public class GlobalExceptionMiddlewareTests
    {
        private readonly Mock<ILogger<GlobalExceptionMiddleware>> _loggerMock;

        public GlobalExceptionMiddlewareTests()
        {
            _loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
        }

        [Fact]
        public async Task InvokeAsync_NoException_CallsNext()
        {
            var called = false;
            RequestDelegate next = (ctx) => { called = true; return Task.CompletedTask; };
            var middleware = new GlobalExceptionMiddleware(next, _loggerMock.Object);
            var httpContext = new DefaultHttpContext();

            await middleware.InvokeAsync(httpContext);

            Assert.True(called);
        }

        [Fact]
        public async Task InvokeAsync_KeyNotFoundException_Returns404()
        {
            RequestDelegate next = (ctx) => throw new KeyNotFoundException("Item not found");
            var middleware = new GlobalExceptionMiddleware(next, _loggerMock.Object);
            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();

            await middleware.InvokeAsync(httpContext);

            Assert.Equal(404, httpContext.Response.StatusCode);
            Assert.Equal("application/json", httpContext.Response.ContentType);
        }

        [Fact]
        public async Task InvokeAsync_InvalidOperationException_Returns400()
        {
            RequestDelegate next = (ctx) => throw new InvalidOperationException("Invalid operation");
            var middleware = new GlobalExceptionMiddleware(next, _loggerMock.Object);
            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();

            await middleware.InvokeAsync(httpContext);

            Assert.Equal(400, httpContext.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_UnhandledException_Returns500()
        {
            RequestDelegate next = (ctx) => throw new Exception("Unexpected error");
            var middleware = new GlobalExceptionMiddleware(next, _loggerMock.Object);
            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();

            await middleware.InvokeAsync(httpContext);

            Assert.Equal(500, httpContext.Response.StatusCode);
        }
    }
}
