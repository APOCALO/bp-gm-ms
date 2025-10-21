using ErrorOr;
using MediatR;

namespace Application.Common
{
    public record BaseResponse<T> : IRequest<ErrorOr<ApiResponse<T>>>
    {
    }
}
