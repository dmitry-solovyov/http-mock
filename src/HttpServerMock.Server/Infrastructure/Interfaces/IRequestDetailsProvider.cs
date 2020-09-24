using System.Threading.Tasks;

namespace HttpServerMock.Server.Infrastructure.Interfaces
{
    public interface IRequestDetailsProvider
    {
        Task<IRequestDetails> GetRequestDetails();
    }
}