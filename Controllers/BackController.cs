using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ASP_111.Controllers
{
    [Route("api/back")]
    [ApiController]
    public class BackController : ControllerBase
    {
        [HttpGet]
        public object Get(int x, int y)
        {
            QueryString QueryString = Request.QueryString;
            if (!Request.Query.ContainsKey("y"))
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return new
                {
                    Error = true,
                    Message = "Parametr 'y' is required",
                };
            }
            return new
            {
                message = "hello GET",
                x,
                y,
                QueryString,
                Test = Request.Headers["Test"],
            };
        }

        [HttpPost]
        public object Post(dynamic body)
        {
            return new
            {
                message = "hello POST",
                body,
            };
        }

        [HttpPut]
        public object Put()
        {
            return new
            {
                message = "hello PUT"
            };
        }
        [HttpDelete]
        public object Delete()
        {
            return new
            {
                message = "hello DELETE"
            };
        }
    }
}

//public class PostBody
//{
//    public dynamic Data { get; set; }
//}