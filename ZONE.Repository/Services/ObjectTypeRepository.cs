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
    public class ObjectTypeRepository: RepositoryBase<ObjectType>, IObjectTypeRepository
    {
        private ZoneDbContext _zoneDbContext;
        public ObjectTypeRepository(ZoneDbContext zoneDbContext) : base(zoneDbContext)
        {
            _zoneDbContext = zoneDbContext;
        }
    }
}
