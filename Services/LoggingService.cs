using Database;
using Models.DatabaseModels.Logging;

namespace ApiGateway.Services
{
    public interface ILoggingService
    {
        Task<bool> LogHttpRequest(HttpRequestLog httpRequestLog);
    }

    public class LoggingService : ILoggingService
    {
        readonly AppDbContext appDbContext;

        public LoggingService(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }

        public async Task<bool> LogHttpRequest(HttpRequestLog httpRequestLog)
        {
            try
            {
                this.appDbContext.HttpRequestLog.Add(httpRequestLog);

                var rowsAffected = await this.appDbContext.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
