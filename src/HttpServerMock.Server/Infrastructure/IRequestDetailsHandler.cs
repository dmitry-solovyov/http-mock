using System.Threading.Tasks;

namespace HttpServerMock.Server.Infrastructure
{
    public interface IRequestDetailsHandler
    {
        bool CanHandle(IRequestDetails requestDetails);

        Task<IResponseDetails?> HandleResponse(IRequestDetails requestDetails);
    }
}
