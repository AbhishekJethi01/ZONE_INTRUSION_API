using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZONE.Entity.Context;
using ZONE.Repository.Interfaces;

namespace ZONE.Repository.Services
{
    public class RepositoryService : IRepositoryService
    {
        private static ZoneDbContext _repositoryContext;

        private IEventDetailRepository _eventDetailRepository;
        public IEventDetailRepository EventDetail => _eventDetailRepository ?? (_eventDetailRepository = new EventDetailRepository(_repositoryContext));

        private ICameraDetailRepository _cameraDetailRepository;
        public ICameraDetailRepository CameraDetail => _cameraDetailRepository ?? (_cameraDetailRepository = new CameraDetailRepository(_repositoryContext));

        private IObjectTypeRepository _objectTypeRepository;
        public IObjectTypeRepository ObjectType => _objectTypeRepository ?? (_objectTypeRepository = new ObjectTypeRepository(_repositoryContext));

        private IUserDetailRepository _userDetailRepository;
        public IUserDetailRepository UserDetail => _userDetailRepository ?? (_userDetailRepository = new UserDetailRepository(_repositoryContext));

        private ICameraRoiRepository _cameraRoiRepository;
        public ICameraRoiRepository CameraRoi => _cameraRoiRepository ?? (_cameraRoiRepository = new CameraRoiRepository(_repositoryContext));
    }
}
