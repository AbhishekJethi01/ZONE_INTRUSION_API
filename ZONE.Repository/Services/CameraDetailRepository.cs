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
    public class CameraDetailRepository: RepositoryBase<CameraDetail>, ICameraDetailRepository
    {
        private ZoneDbContext _zoneDbContext;
        public CameraDetailRepository(ZoneDbContext zoneDbContext) : base(zoneDbContext)
        {
            _zoneDbContext = zoneDbContext;
        }
    }
}
