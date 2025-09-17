using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZONE.Entity.Context;
using ZONE.Entity.Model;
using ZONE.Repository.Interfaces;

namespace ZONE.Repository.Services
{
    public class EventDetailRepository: RepositoryBase<EventDetail>, IEventDetailRepository
    {
        private ZoneDbContext _zoneDbContext;
        public EventDetailRepository(ZoneDbContext zoneDbContext): base(zoneDbContext)
        {
            _zoneDbContext = zoneDbContext;
        }
    }
}
