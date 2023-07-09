using ASP_111.Data;
using ASP_111.Models.Forum.Index;
using ASP_111.Models.Forum.Section;
using ASP_111.Services.AuthUser;
using ASP_111.Services.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace ASP_111.Controllers
{
    public class ForumController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<ForumController> _logger;
        private readonly IAuthUserService _authUserService;
        private readonly IValidationService _validationService;

        public ForumController(DataContext dataContext, ILogger<ForumController> logger, IAuthUserService authUserService, IValidationService validationService)
        {
            _dataContext = dataContext;
            _logger = logger;
            _authUserService = authUserService;
            _validationService = validationService;
        }

        public ViewResult Section([FromRoute] Guid id)
        {
            SectionViewModel sectionViewModel = new()
            {
                SectionId = id.ToString(),
            };
            if (HttpContext.Session.Keys.Contains("AddTopicMessage"))
            {
                sectionViewModel.ErrorMessages = JsonSerializer.Deserialize<Dictionary<string, string?>>(HttpContext.Session.GetString("AddTopicMessage"));
                HttpContext.Session.Remove("AddTopicMessage");
            }
                // проверяем есть ли в сессии сообщение о валидации формы,
                // если есть, извлекаем, десериализуем и передаем на 
                // представление (все сообщения) вместе с данными формы, которые
                // подставятся обратно в поля формы

            return View(sectionViewModel);
        }

        public IActionResult Index()
        {
            int n = 0;
            ForumIndexModel model = new()
            {
                Sections = _dataContext
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
                            ? $"/img/section/section{n++}.png"
                            : $"/img/section/{s.ImageUrl}",
                        Author = new(s.Author),
                    }),
                // проверяем есть ли в сессии сообщение о валидации формы,
                // если есть, извлекаем, десериализуем и передаем на 
                // представление (все сообщения) вместе с данными формы, которые
                // подставятся обратно в поля формы
            };


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
                }
            }
            // проверяем что пользователь аутентифицирован
            Guid? userId = _authUserService.GetUserId(HttpContext);
            if (userId != null)
            {
                _dataContext.Sections.Add(new()
                {
                    Id = Guid.NewGuid(),
                    Title = model.Title,
                    Description = model.Description,
                    CreateDt = DateTime.Now,
                    ImageUrl = null,
                    DeleteDt = null,
                    AuthorId = userId.Value,
                });
                _dataContext.SaveChanges();
                _logger.LogInformation("Add OK");
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public RedirectToActionResult AddTopic(TopicFormModel model)
        {
            var messages = _validationService.ErrorMessages(model);
            foreach (var (key, message) in messages)
            {
                if (message != null)
                {
                    HttpContext.Session.SetString("AddTopicMessage", JsonSerializer.Serialize(messages));

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
                String? nameAvatar = null;
                if (model.ImageFile != null)
                {

                    // определяем расширение файла
                    String ext = Path.GetExtension(model.ImageFile.FileName);
                    // проверить расширение на перечень допустимых

                    // формируем имя для файла
                    nameAvatar = Guid.NewGuid().ToString() + ext;

                    using var fstream = new FileStream("wwwroot/img/" + nameAvatar, FileMode.Create);
                    model.ImageFile.CopyTo(fstream);
                }
                _dataContext.Topics.Add(new()
                {
                    Id = Guid.NewGuid(),
                    SectionId = model.SectionId,
                    Title = model.Title,
                    Description = model.Description,
                    CreateDt = DateTime.Now,
                    ImageUrl = nameAvatar,
                    DeleteDt = null,
                    AuthorId = userId.Value,
                });
                _dataContext.SaveChanges();
                _logger.LogInformation("Add OK");
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
