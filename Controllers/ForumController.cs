using ASP_111.Data;
using ASP_111.Models.Forum.Index;
using ASP_111.Models.Forum.Section;
using ASP_111.Models.User;
using ASP_111.Services.AuthUser;
using ASP_111.Services.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ASP_111.Models.Forum.Topic;
using System.Security.Claims;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.AspNetCore.Http;
using ASP_111.Data.Entities;
using ASP_111.Services;

namespace ASP_111.Controllers
{
    public class ForumController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<ForumController> _logger;
        private readonly IAuthUserService _authUserService;
        private readonly IValidationService _validationService;
        private readonly FormatDateTimeService _formatDateTimeService;

        public ForumController(DataContext dataContext, ILogger<ForumController> logger, IAuthUserService authUserService, IValidationService validationService, FormatDateTimeService formatDateTimeService)
        {
            _dataContext = dataContext;
            _logger = logger;
            _authUserService = authUserService;
            _validationService = validationService;
            _formatDateTimeService = formatDateTimeService;
        }

        public IActionResult Section( [FromRoute] Guid id )
        {
            SectionPageModel model = null!;

            var section = _dataContext.Sections
                .Include(s => s.Author)
                .FirstOrDefault(s => s.Id == id);

            if (section == null)
            {
                Response.StatusCode = StatusCodes.Status404NotFound;
                return NotFound();
            }

            if (HttpContext.Session.Keys.Contains("FormData"))
            {
                String? data = HttpContext.Session.GetString("FormData");
                if (data != null)
                {
                    model = System.Text.Json.JsonSerializer
                        .Deserialize<SectionPageModel>(data)!;
                }
                else
                {
                    model = null!;
                }
                HttpContext.Session.Remove("FormData");
            }
            model ??= new();

            model.SectionId = id.ToString();


            model.Section = new ForumSectionViewModel
            {
                Id = section.Id.ToString(),
                Title = section.Title,
                Description = section.Description,
                CreateDt = _formatDateTimeService.Fromat(section.CreateDt),
                Author = new(section.Author),
            };


            model.Topics =
         _dataContext.Topics
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

            
            //
            return View(model);
        }

        public IActionResult Index()
        {
            ForumIndexModel model = null!;

            if (HttpContext.Session.Keys.Contains("FormData"))
            {
                String? data = HttpContext.Session.GetString("FormData");
                if (data != null)
                {
                    model = System.Text.Json.JsonSerializer
                        .Deserialize<ForumIndexModel>(data)!;
                }
                else
                {
                    model = null!;
                }
                HttpContext.Session.Remove("FormData");
            }
            model ??= new();

            model.Sections = _dataContext
                    .Sections
                    .Include(s => s.Author)
                    .Where(s => s.DeleteDt == null)
                    .OrderBy(s => s.CreateDt)
                    .AsEnumerable()
                    .Select(s => new ForumSectionViewModel
                    {
                        Id = s.Id.ToString(),
                        Title = s.Title,
                        Description = s.Description,
                        CreateDt = s.CreateDt.ToShortDateString(),
                        ImageUrl = s.ImageUrl == null
                            ? $"/img/section/no-photo.png"
                            : $"/img/section/{s.ImageUrl}",
                        Author = new(s.Author),
                    });
            // проверяем есть ли в сессии сообщение о валидации формы,
            // если есть, извлекаем, десериализуем и передаем на 
            // представление (все сообщения) вместе с данными формы, которые
            // подставятся обратно в поля формы


            return View(model);
            // В представлении проверяем наличие данных валидации
            // если они есть, то в целом форма не принята,
            // выводим сообщения под каждым полем:
            // если сообщение null, то нет ошибок, поле принято
            // иначе - ошибка и ее текст в сообщении
        }

        [HttpPost]
        public RedirectToActionResult AddSection(ForumSectionFormModel model)
        {

          

            var messages = _validationService.ErrorMessages(model);
            foreach (var (key, message) in messages)
            {
                if (message != null)
                {
                    // есть сообщение об ошибке - 
                    // сериализуем все сообщения, сохраняем в сессии и
                    // перенаправляем на Index

                    model.ImageFile = null!;

                    ForumIndexModel viewModel = new()
                    {
                        FormModel = model,
                        ErrorValidationMessages = messages,
                    };

                    HttpContext.Session.SetString("FormData", JsonSerializer.Serialize(viewModel));

                    return RedirectToAction(nameof(Index));
                }
            }
            // проверяем что пользователь аутентифицирован
            Guid? userId = _authUserService.GetUserId(HttpContext);
            if (userId != null)
            {
                String? ImageUrl = null;
                if (model.ImageFile != null && model.ImageFile.Length < 1048576)
                {
                    String ext = Path.GetExtension(model.ImageFile.FileName);

                    // формируем имя для файла
                    ImageUrl = Guid.NewGuid().ToString() + ext;

                    using var fstream = new FileStream("wwwroot/img/section/" + ImageUrl, FileMode.Create);
                    model.ImageFile.CopyTo(fstream);
                }


                _dataContext.Sections.Add(new()
                {
                    Id = Guid.NewGuid(),
                    Title = model.Title,
                    Description = model.Description,
                    CreateDt = DateTime.Now,
                    ImageUrl = ImageUrl,
                    DeleteDt = null,
                    AuthorId = userId.Value,
                });
                _dataContext.SaveChanges();
                _logger.LogInformation("Add OK");
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Topic([FromRoute] Guid id)
        {
            var topic = _dataContext
                .Topics
                .Include(t => t.Author)
                .FirstOrDefault(t => t.Id == id);

            if (topic == null)
            {
                return NotFound();
            }

            TopicPageModel model = new()
            {
                Topic = new(topic)
            };
            model.Themes = _dataContext
                .Themes
                .Include(t => t.Author)
                .Include(t => t.Comments)
                .Where(t => t.TopicId == topic.Id && t.DeleteDt == null)
                .Select(t => new ThemeViewModel(t))
                .ToList();

            if (HttpContext.Session.Keys.Contains("AddThemeMessage"))
            {
                model.ErrorMessages =
                    JsonSerializer.Deserialize<Dictionary<String, String?>>(
                        HttpContext.Session.GetString("AddThemeMessage")!);

                HttpContext.Session.Remove("AddThemeMessage");
            }

            return View(model);
        }

        [HttpPost]
        public RedirectToActionResult AddTheme(ThemeFormModel formModel)
        {
            var messages = _validationService.ErrorMessages(formModel);
            foreach (var (key, message) in messages)
            {
                if (message != null)  // есть сообщение об ошибке
                {
                    HttpContext.Session.SetString(
                        "AddThemeMessage",
                        JsonSerializer.Serialize(messages)
                    );
                    return RedirectToAction(nameof(Topic), new { id = formModel.TopicId });
                }
            }
            // проверяем что пользователь аутентифицирован
            Guid? userId = _authUserService.GetUserId(HttpContext);
            if (userId != null)
            {
                Guid themeId = Guid.NewGuid();
                DateTime dt = DateTime.Now;

                _dataContext.Themes.Add(new()
                {
                    Id = themeId,
                    AuthorId = userId.Value,
                    TopicId = formModel.TopicId,
                    Title = formModel.Title,
                    CreateDt = dt,
                });
                _dataContext.Comments.Add(new()
                {
                    Id = Guid.NewGuid(),
                    AuthorId = userId.Value,
                    Content = formModel.Content,
                    ThemeId = themeId,
                    CreateDt = dt,
                });
                // _dataContext.SaveChanges();
            }

            return RedirectToAction(nameof(Topic), new { id = formModel.TopicId });
        }

        [HttpPost]
        public RedirectToActionResult AddTopic(TopicFormModel model)
        {
            var messages = _validationService.ErrorMessages(model);
            foreach (var (key, message) in messages)
            {
                if (message != null)
                {
                    model.ImageFile = null!;

                    SectionPageModel viewModel = new()
                    {
                        FormModel = model,
                        ErrorMessages = messages,
                    };

                    HttpContext.Session.SetString("FormData", JsonSerializer.Serialize(viewModel));

                    return RedirectToAction(nameof(Section), new
                    {
                        id = model.SectionId,
                    });
                    // есть сообщение об ошибке - 
                    // сериализуем все сообщения, сохраняем в сессии и
                    // перенаправляем на Index
                }
            }
            // проверяем что пользователь аутентифицирован
            Guid? userId = _authUserService.GetUserId(HttpContext);
            if (userId != null)
            {
                String? ImageUrl = null;
                if (model.ImageFile != null && model.ImageFile.Length < 1048576)
                {
                    String ext = Path.GetExtension(model.ImageFile.FileName);

                    // формируем имя для файла
                    ImageUrl = Guid.NewGuid().ToString() + ext;

                    using var fstream = new FileStream("wwwroot/img/section/" + ImageUrl, FileMode.Create);
                    model.ImageFile.CopyTo(fstream);
                }

                _dataContext.Topics.Add(new()
                {
                    Id = Guid.NewGuid(),
                    SectionId = model.SectionId,
                    Title = model.Title,
                    Description = model.Description,
                    CreateDt = DateTime.Now,
                    ImageUrl = ImageUrl,
                    DeleteDt = null,
                    AuthorId = userId.Value,
                });
                _dataContext.SaveChanges();
            }
            return RedirectToAction(nameof(Section), new
            {
                id = model.SectionId,
            });
        }
    }
}
/* Задача: валидация (сервис валидации)
 * Задание: реализовать средства проверки моделей форм на правильность данных
 * Особенности: разные поля нужно проверять по-разному, а в разных моделях
 *  бывают одинаковые правила проверки.
 * + Нужно формирование сообщений о результатах проверки 
 * Готовые решения:
 *  https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation
 *  
 * Идея:
 * class Model {
 *  ...
 *  [ValidationRules(ValidationRule.NotEmpty, ValidationRule.Name)]
 *  String name
 *  
 *  [ValidationRules(ValidationRule.NotEmpty, ValidationRule.Password)]
 *  String password
 *  
 *  [ValidationRules(ValidationRule.Login)]
 *  String login
 *  
 *  }
 *  
 *  _service.ErrorMessages(model) 
 *    [ "name" => "Не может быть пустым", 
 *      "password" => "должен содержать цифру",
 *      "login" => null ]
 */
