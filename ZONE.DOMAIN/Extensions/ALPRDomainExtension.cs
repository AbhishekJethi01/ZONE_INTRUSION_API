using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZONE.DOMAIN.Extensions
{
    public static class ALPRDomainExtension
    {
        public static T PatchEntity<T, P>(this T entity, P entityDto) where T : class where P : class
        {
            var propertiesToUpdate = typeof(P).GetProperties().Where(prop => prop.GetValue(entityDto) != null).ToList();
            foreach (var prop in propertiesToUpdate)
            {
                var property = entity.GetType().GetProperty(prop.Name);
                if (property != null && property.CanWrite && prop.Name.ToLower() != "userid" && prop.Name.ToLower() != "cameraid")
                {
                    var value = prop.GetValue(entityDto);
                    if (value is string strValue)
                    {
                        value = strValue.Trim();
                    }
                    if (property.PropertyType == typeof(TimeOnly?) && value is string timeValue)
                    {
                        // Handle conversion from string to TimeOnly?
                        if (string.IsNullOrEmpty(timeValue))
                        {
                            property.SetValue(entity, null);
                        }
                        else
                        {
                            property.SetValue(entity, TimeOnly.Parse(timeValue));
                        }
                    }
                    else
                    {
                        property.SetValue(entity, value);
                    }
                }
            }
            return entity;
        }
    }
}
