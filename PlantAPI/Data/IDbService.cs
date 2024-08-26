using PlantAPI.Models;

namespace PlantAPI.Data
{
    public interface IDbService
    {
        Task<List<Capture>> GetAllCaptures();
        Task<Capture?> AddCapture(Capture obj);
        Task<List<Capture>> GetCapturesByDeviceId(string deviceId);
    }
}