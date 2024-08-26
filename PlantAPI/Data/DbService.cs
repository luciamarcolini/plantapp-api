using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantAPI.Models;
using System.Text.RegularExpressions;

namespace PlantAPI.Data
{
    public class DbService : IDbService
    {
        private readonly ApplicationDbContext _db;
        public DbService(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<List<Models.Capture>> GetAllCaptures()
        {
             return await _db.Capture.ToListAsync(); 
        }

        public async Task<Models.Capture?> AddCapture(Models.Capture obj)
        {
            _db.Capture.Add(obj);
            var result = await _db.SaveChangesAsync();
            return result >= 0 ? obj : null;
        }

        public async Task<List<Models.Capture>> GetCapturesByDeviceId(string deviceId)
        {
            return await _db.Capture.Where(x=> x.deviceId == deviceId).ToListAsync();
        }
    }
}
