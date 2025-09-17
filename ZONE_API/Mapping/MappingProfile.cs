using AutoMapper;
using ZONE.Entity.Model;
using ZONE.EntityDto.Model;

namespace ZONE_API.Mapping
{
    public class MappingProfile: Profile
    {
        public MappingProfile()
        {
            CreateMap<EventDetail, EventDetailDto>();
            CreateMap<EventDetailDto, EventDetail>();
            CreateMap<EventDetail, EventDetailView>();
            CreateMap<EventDetailView, EventDetail>();

            CreateMap<CameraDetail, CameraDetailDto>();
            CreateMap<CameraDetailDto, CameraDetail>();
            CreateMap<CameraDetail, CameraDetailView>();
            CreateMap<CameraDetailView, CameraDetail>();
            CreateMap<CameraDetail, CameraStatusView>();
            CreateMap<CameraStatusView, CameraDetail>();

            CreateMap<UserDetail, UserDetailDto>();
            CreateMap<UserDetailDto, UserDetail>();
            CreateMap<UserDetail, UserDetailView>();
            CreateMap<UserDetailView, UserDetail>();
        }
    }
}
