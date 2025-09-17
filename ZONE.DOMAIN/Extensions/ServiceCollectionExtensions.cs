using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZONE.DOMAIN.Interfaces;
using ZONE.DOMAIN.Services;

namespace ZONE.DOMAIN.Extensions
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddDomainService(this IServiceCollection services)
        {
            // Register domain service
            services.AddScoped<IEventDetailDomain, EventDetailDomain>();
            services.AddScoped<ICameraDetailDomain, CameraDetailDomain>();
            services.AddScoped<IObjectTypeDomain, ObjectTypeDoman>();
            services.AddScoped<IUserDetailDomain, UserDetailDomain>();

            return services;
        }
    }
}
