using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZONE.DOMAIN.Interfaces;
using ZONE.Entity.Context;
using ZONE.Repository.Interfaces;

namespace ZONE.DOMAIN.Services
{
    public class ObjectTypeDoman: IObjectTypeDomain
    {
        private readonly IRepositoryService _repository;
        private readonly IConfiguration _config;
        private readonly string _conn = string.Empty;
        private ZoneDbContext _context;
        private readonly IMapper _mapper;

        public ObjectTypeDoman(IRepositoryService repository, IConfiguration config, IMapper mapper)
        {
            _repository = repository;
            _config = config;
            _conn = _config.GetConnectionString("DbConnection") ?? "";
            var contextOptions = new DbContextOptionsBuilder<ZoneDbContext>().UseSqlServer(_conn).Options;
            _context = new ZoneDbContext(contextOptions);
            _mapper = mapper;
        }

        public async Task<(List<object> result, string message)> GetObjectTypeDropdownList()
        {
            try
            {
                var result = await _context.ObjectTypes
                    .AsNoTracking()
                    .GroupBy(c => c.Type.ToLower())
                    .Select(g => new { Type = g.Key })
                    .OrderBy(x => x.Type == "others" ? 1 : 0)  // put "others" last
                    .ThenBy(x => x.Type)
                    .ToListAsync();

                var message = result.Any()
                    ? "Object types retrieved successfully."
                    : "No object types found.";

                return (result.Cast<object>().ToList(), message);
            }
            catch (Exception ex)
            {
                return (new List<object>(), $"Error retrieving object types: {ex.Message}");
            }
        }


    }
}
