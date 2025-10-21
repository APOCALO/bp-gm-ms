using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Application.Common
{
    public abstract class ApiBaseHandler<TRequest, TResponse> : IRequestHandler<TRequest, ErrorOr<ApiResponse<TResponse>>>
        where TRequest : IRequest<ErrorOr<ApiResponse<TResponse>>>
    {
        protected readonly ILogger<ApiBaseHandler<TRequest, TResponse>> _logger;
        private readonly Stopwatch _stopwatch = new Stopwatch();

        protected ApiBaseHandler(ILogger<ApiBaseHandler<TRequest, TResponse>> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ErrorOr<ApiResponse<TResponse>>> Handle(TRequest request, CancellationToken cancellationToken)
        {
            _stopwatch.Start();

            // Log inicio de la gestión de solicitudes
            _logger.LogInformation("Handling {RequestName} with request: {@Request}", typeof(TRequest).Name, request);

            ErrorOr<ApiResponse<TResponse>> response;

            // Llamar a la implementación específica de la clase derivada
            response = await HandleRequest(request, cancellationToken);

            // Parar el cronómetro y registrar el tiempo transcurrido
            _stopwatch.Stop();

            // Verificar si la respuesta tiene valor (si no tiene error)
            if (!response.IsError)
            {
                // Asignar el tiempo de respuesta si la respuesta es válida
                response.Value.ResponseTime = _stopwatch.Elapsed.TotalMilliseconds;

                // Log de finalización exitosa de la solicitud
                _logger.LogInformation("{RequestName} processed successfully with response: {@Response}", typeof(TRequest).Name, response.Value);
            }
            else
            {
                // Log de error si la solicitud no fue exitosa
                _logger.LogError("{RequestName} failed with errors: {@Errors}", typeof(TRequest).Name, response.Errors);
            }

            return response;
        }

        // Método abstracto que debe implementar la clase derivada
        protected abstract Task<ErrorOr<ApiResponse<TResponse>>> HandleRequest(TRequest request, CancellationToken cancellationToken);

        // Método para acceder al tiempo de respuesta
        protected double GetElapsedMilliseconds() => _stopwatch.Elapsed.TotalMilliseconds;
    }
}
