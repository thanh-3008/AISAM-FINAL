namespace AISAM.Services.IServices
{
    public interface IBackgroundJobHealthService
    {
        void ReportHeartbeat(string serviceName);
        void ReportSuccess(string serviceName);
        void ReportFailure(string serviceName, string error);
        Task<object> GetStatusAsync();
    }
}
