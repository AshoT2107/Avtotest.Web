using Avtotest.Web.Models;
using Avtotest.Web.Repositories;
using Avtotest.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Avtotest.Web.Controllers
{
    public class UserController : Controller
    {
        private UserService _userService;
        private UserRepository _userRepository;
        private CookiesService _cookiesService;
        private TicketsRepository _ticketsRepository;
        private QuestionsRepository _questionsRepository;
        public UserController(UserService userService,UserRepository userRepository,CookiesService cookiesService,TicketsRepository ticketsRepository,QuestionsRepository questionsRepository)
        {
            _userService = userService; 
            _userRepository = userRepository;
            _cookiesService = cookiesService;
            _ticketsRepository = ticketsRepository;
            _questionsRepository = questionsRepository;
        }
        public IActionResult Index()
        {
            var user = _userService.GetUserFromCookie(HttpContext);
            if(user != null)
            {
                return View(user);
            }
            return RedirectToAction("SignIn");
        }

        public IActionResult SignUp()
        {
            return View();
        }
        [HttpPost]
        public IActionResult SignUp(User user)
        {
            if (!ModelState.IsValid) 
                return View(user);
            
            user.Image = SaveUserImage(user.ImageFile);
            _userRepository.InsertUser(user);
            var _user = _userRepository.GetUserByPhoneNumber(user.Phone!);
            _ticketsRepository.InsertUserTrainingTickets(_user.Index, _questionsRepository.GetQuestionsCount() / 20, 20);
            _cookiesService.SendUserPhoneToCookie(user.Phone!, HttpContext);
            return RedirectToAction("Index");
        }
        public IActionResult SignIn()
        {
            return View();
        }
        [HttpPost]
        public IActionResult SignIn(User user)
        {
          /*  if (!ModelState.IsValid)
            {
                return View(user);
            }*/
            var _user = _userRepository.GetUserByPhoneNumber(user.Phone!);
                if(_user.Password==user.Password)
                {
                    _cookiesService.SendUserPhoneToCookie(_user.Phone!, HttpContext);
                    return RedirectToAction("Index");
                }
            return RedirectToAction("SignIn");
        }
        public IActionResult Edit()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Edit([FromForm]User user)
        {
            var _user = _userService.GetUserFromCookie(HttpContext);
            if (_user == null)
                return RedirectToAction("SignIn");
            user.Index = _user.Index;
            user.Image = SaveUserImage(user.ImageFile);
            _userRepository.UpdateUser(user);
            _cookiesService.SendUserPhoneToCookie(user.Phone, HttpContext);
            return RedirectToAction("Index");
        }

        private string? SaveUserImage(IFormFile? imageFile)
        {
            if(imageFile == null)
            {
                return "Ustoz.jpg";

            }
            var imagePath = Guid.NewGuid().ToString("N") + 
                            Path.GetExtension(imageFile.FileName);

            var ms = new MemoryStream();
            imageFile.CopyTo(ms);

            System.IO.File.WriteAllBytes(Path.Combine("wwwroot", "Profile", imagePath), ms.ToArray());
            return imagePath;
        }
    }
}
