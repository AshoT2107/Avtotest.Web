using Avtotest.Web.Models;
using Avtotest.Web.Repositories;

namespace Avtotest.Web.Services
{
    public class UserService
    {
        private CookiesService cookiesService;
        private UserRepository userRepository;
        public UserService()
        {
            cookiesService = new CookiesService();
            userRepository = new UserRepository();
        }
        public User? GetUserFromCookie(HttpContext context)
        {
            var userPhone = cookiesService.GetUserPhoneFromCookie(context);
            if(userPhone != null)
            {
                var user = userRepository.GetUserByPhoneNumber(userPhone);
                if(user.Phone == userPhone)
                {
                    return user;
                }
            }
            return null;
        }
    }
}
