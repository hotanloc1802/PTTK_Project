using ApartmentManagement.Data;
using ApartmentManagement.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApartmentManagement.Repository
{
    class ServiceRepository : IServiceRepository
    {
        private readonly ApartmentDbContext _context;
        public ServiceRepository(ApartmentDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<ServiceRequest>> GetAllServiceAsync()
        {
            return await _context.ServiceRequests
                .Include(a => a.apartment)
                .Include(r => r.resident)
                .ToListAsync();
        }
    }
}
