using SGE.Core.Entities;
using SGE.Infrastructure.Data;
using SGE.Infrastructure.Repositories;

public class AttendanceRepository : Repository<Attendance>
{
    public AttendanceRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Attendance?> GetById()
    {
        return await _dbSet.FindAsync(1);
    }
}