using Amazon.AspNetCore.Identity.Cognito;
using Amazon.Extensions.CognitoAuthentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebAdvert.Web.Models.Accounts;


// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebAdvert.Web.Controllers
{
    public class Accounts : Controller
    {
        private readonly SignInManager<CognitoUser> _signInManager;
        private readonly UserManager<CognitoUser> _userManager;
        private readonly CognitoUserPool _pool;




        /// <summary>
        /// Different to applicationuser as in .net core
        /// Dependency injection of Cognito tool
        /// </summary>
        /// <param name="signInManager"></param>
        /// <param name="userManager"></param>
        /// <param name="pool"></param>
        public Accounts(SignInManager<CognitoUser> signInManager, UserManager<CognitoUser> userManager, CognitoUserPool pool, SignInManager<CognitoUser> signInManager2)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _pool = pool;

        }
        public async Task<IActionResult> SignUp()
        {

            var model = new SignupModel();
            return View(model);

        }

        [HttpPost]
        public async Task<IActionResult> SignUp(SignupModel model)
        {

            if (ModelState.IsValid)
            {

                var user = _pool.GetUser(model.Email);
                if (user.Status != null)
                {
                    ModelState.AddModelError("User Exists", "User with this email already exists");
                    return View(model);
                }
                user.Attributes.Add(CognitoAttribute.Name.AttributeName, model.Email);//Due to cognito pool settings
                var createdUser = await _userManager.CreateAsync(user, model.Password);
                if (createdUser.Succeeded)
                {
                    RedirectToAction("Confirm");
                }
            }
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Confirm(ConfirmModel model)
        {

            return View(model);

        }

        [HttpPost]
        [ActionName("Confirm")]
        public async Task<IActionResult> Confirm_Post(ConfirmModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if(user == null)
                {
                    ModelState.AddModelError("Not Found", "A user with given email address was not found");
                    return View(model);
                }

                var result = await ((CognitoUserManager<CognitoUser>)_userManager)
                    .ConfirmSignUpAsync(user, model.Code, true).ConfigureAwait(false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var item in result.Errors)
                    {
                        ModelState.AddModelError(item.Code, item.Description);
                    }
                    return View(model);
                }
            }

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Login(LoginModel model)
        {
            return View(model);
        }
        [HttpPost]
        [ActionName("Login")]
        public async Task<IActionResult> LoginPost(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false).ConfigureAwait(false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("LoginError", "Email and password do not match");
                }
            }

            return View("Login", model);
        }



    }
}
