using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ZONE.DOMAIN.Model
{
    public class AlprResponse<T> where T : class
    {
        public AlprResponse(HttpStatusCode statusCode, T data, string message = "")
        {
            StatusCode = statusCode;
            Data = data;
            Message = message;
            switch (StatusCode)
            {
                case HttpStatusCode.OK:
                    Status = "Success";
                    break;
                case HttpStatusCode.BadRequest:
                    Status = "Error";
                    break;
                case HttpStatusCode.InternalServerError:
                    Status = "InternalServerError";
                    break;
                case HttpStatusCode.NotFound:
                    Status = "NotFound";
                    break;
                case HttpStatusCode.Found:
                    Status = "Found";
                    break;
                case HttpStatusCode.Conflict:
                    Status = "Conflict";
                    break;
                case HttpStatusCode.Unauthorized:
                    Status = "Unauthorized";
                    break;
                default:
                    Status = "";
                    break;
            }
        }

        public HttpStatusCode StatusCode { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }
}
