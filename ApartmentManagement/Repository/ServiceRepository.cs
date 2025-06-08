using ApartmentManagement.Data;
using ApartmentManagement.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
        public async Task<IEnumerable<ServiceRequest>> GetServiceRequestsByApartmentIdAsync(string apartmentIdSubset)
        {
            return await _context.ServiceRequests
                .Include(a => a.apartment)
                .Include(r => r.resident)
                .Where(a => a.apartment.apartment_id.Contains(apartmentIdSubset))
                .ToListAsync();
        }

        public async Task<IEnumerable<ServiceRequest>> GetServiceRequestsByStatusAsync(string status)
        {
            if (string.IsNullOrEmpty(status))
            {
                return await _context.ServiceRequests
                    .Include(a => a.apartment)
                    .Include(r => r.resident)
                    .ToListAsync();
            }
            return await _context.ServiceRequests
                .Include(a => a.apartment)
                .Include(r => r.resident)
                .Where(sr => sr.status == status)
                .ToListAsync();
        }
        public async Task<bool> CreateServiceRequestsAsync(ServiceRequest service)
        {
            service.request_date = DateTime.UtcNow; // Set the creation timestamp
            service.completed_date = new DateTime(9999, 12, 3).ToUniversalTime();
            service.request_id = "";
            service.status = "In Progress";

            _context.ServiceRequests.Add(service);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<Resident> GetResidentByPhoneNumberAsync(string phoneNumber)
        {
            return await _context.Residents
                                  .Where(r => r.phone_number == phoneNumber)
                                  .FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<ServiceRequest>> SortServiceRequestsAsync(string sortType)
        {
            IQueryable<ServiceRequest> query = _context.ServiceRequests
                .Include(a => a.apartment)
                .Include(r => r.resident);

            switch (sortType)
            {
                case "Newest Date":
                    query = query.OrderByDescending(sr => sr.request_date);
                    break;
                case "Apartment Number":
                    query = query.OrderBy(sr => sr.apartment_id);
                    break;
                default:
                    throw new ArgumentException("Invalid sort type");
            }

            return await query.ToListAsync();
        }

        public async Task<bool> SetStatusCompleted(string requestId)
        {
            // Use reflection to retrieve the current schema from the DbContext
            var schemaField = _context.GetType().GetField("_schema", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            string schema = "mien_dong"; // default schema if not found
            if (schemaField != null)
            {
                var schemaValue = schemaField.GetValue(_context) as string;
                if (!string.IsNullOrEmpty(schemaValue))
                {
                    schema = schemaValue;
                }
            }

            var sql = $"UPDATE {schema}.service_requests SET status = 'Completed' WHERE request_id = {{0}}";
            var rowsAffected = await _context.Database.ExecuteSqlRawAsync(sql, requestId);
            return rowsAffected > 0;
        }
    }
}
