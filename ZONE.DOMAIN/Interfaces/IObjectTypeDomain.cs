using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZONE.DOMAIN.Interfaces
{
    public interface IObjectTypeDomain
    {
        Task<(List<object> result, string message)> GetObjectTypeDropdownList();
    }
}
