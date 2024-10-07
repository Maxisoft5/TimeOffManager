using Microsoft.EntityFrameworkCore;
using TimeOffManager.Core.Services.Interfaces;
using TimeOffManager.DataAccess.Context;
using TimeOffManager.DataAccess.Models;

namespace TimeOffManager.Core.Services
{
    public class TimeOffService : ITimeOffService
    {
        private readonly DataContext _dataContext;

        public TimeOffService(DataContext dataContext) 
        {
            _dataContext = dataContext;
        }

        public async Task<TimeOff> AddTimeOffRequest(TimeOff timeOff, int approverId)
        {
            timeOff.ApproverId = approverId;
            await _dataContext.TimeOffs.AddAsync(timeOff);
            await _dataContext.SaveChangesAsync();  
            return timeOff;
        }

        public async Task<IEnumerable<TimeOff>> GetTimeOffsByUserIdAndPeriod(int userId, int[] month, int year, bool approved)
        {
            var first = month[0];
            var last = month[month.Length - 1];

            
            var timeOffs = await _dataContext.TimeOffs.Where(x => x.IsApproved == approved && x.EmployeeId == userId 
              && x.StartDate.Year == year && x.StartDate.Month >= first && x.EndDate.Month <= last).ToListAsync();

            return timeOffs;
        }
    }
}
