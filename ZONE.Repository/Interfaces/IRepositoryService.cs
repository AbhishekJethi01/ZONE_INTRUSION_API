using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZONE.Repository.Interfaces
{
    public interface IRepositoryService
    {
        IEventDetailRepository EventDetail { get; }
        ICameraDetailRepository CameraDetail { get; }
        IObjectTypeRepository ObjectType { get; }
        IUserDetailRepository UserDetail { get; }
        ICameraRoiRepository CameraRoi { get; }
    }
}
