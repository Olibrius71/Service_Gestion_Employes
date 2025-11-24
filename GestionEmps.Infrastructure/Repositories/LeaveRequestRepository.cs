using SGE.Application.Interfaces.Repositories;
using SGE.Core.Entities;
using SGE.Infrastructure.Data;

namespace SGE.Infrastructure.Repositories;

public class LeaveRequestRepository : Repository<Attendance>, ILeaveRequestRepository
{
    public LeaveRequestRepository(ApplicationDbContext context) : base(context) { }

    
}