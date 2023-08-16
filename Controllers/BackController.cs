using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASP_111.Controllers
{
    [Route("api/back")]
    [ApiController]
    public class BackController : ControllerBase
    {
        [HttpGet]
        public object Get()
        {
            return new
            {
                message = "hello GET"
            };
        }

        [HttpPost]
        public object Post()
        {
            return new
            {
                message = "hello POST"
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
