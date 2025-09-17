using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ZONE.DOMAIN.Model
{
    public class AlprListResponse<T> : AlprResponse<T> where T : class
    {
        public AlprListResponse(HttpStatusCode statusCode, T data, int totalCount, string message = "")
    : base(statusCode, data, message)
        {
            TotalCount = totalCount;
        }
        public int TotalCount { get; set; }
    }
}
