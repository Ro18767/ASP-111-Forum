using ASP_111.Data;
using ASP_111.Models.Forum.Index;
using ASP_111.Models.Forum.Section;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASP_111.Controllers
{
    [Route("api/topic")]
    [ApiController]
    public class BackTopicController : ControllerBase
    {
        private readonly DataContext _dataContext;

        public BackTopicController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet]
        public IEnumerable<TopicViewModel>? GetAll(Guid sectionId)
        {
            var section = _dataContext.Sections
                .Include(s => s.Author)
                .FirstOrDefault(s => s.Id == sectionId);

            if (section == null)
            {
                Response.StatusCode = StatusCodes.Status404NotFound;
                return null;
            }

            return _dataContext.Topics
                    .Include(t => t.Author)
                    .OrderByDescending(t => t.CreateDt)

                    .AsEnumerable()
                    .Select(t => new TopicViewModel()
                    {
                        Id = t.Id.ToString(),
                        Title = t.Title,
                        Description = t.Description,
                        CreateDt = t.CreateDt.ToShortDateString(),
                        ImageUrl = "/img/" + t.ImageUrl,
                        Author = new(t.Author),
                    }).ToList();
        }
    }
}
