using System.Threading.Tasks;
using Amazon.Extensions.CognitoAuthentication;
using Microsoft.AspNetCore.Identity;
using Amazon.AspNetCore.Identity.Cognito;
using Microsoft.AspNetCore.Mvc;
using WebAdvert.Web.Models.Accounts;
using Amazon.Runtime.Internal.Transform;


// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebAdvert.Web.Controllers
{
    public class Account : Controller
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
        public Account(SignInManager<CognitoUser> signInManager, UserManager<CognitoUser> userManager, CognitoUserPool pool, SignInManager<CognitoUser> signInManager2)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _pool = pool;
            
        }
        public async Task<IActionResult> SignUp()
        {

            var model = new SignupModel();
            return View();

        }

        [HttpPost]
        public async Task<IActionResult> SignUp(SignupModel model) 
        {

            if (ModelState.IsValid) {

                var user = _pool.GetUser(model.Email);
                if(user.Status != null)
                {
                    ModelState.AddModelError("User Exists", "User with this email already exists");
                    return View(model);
                }
                user.Attributes.Add(CognitoAttribute.Name.ToString(), model.Email);//Due to cognito pool settings
                var createdUser = await _userManager.CreateAsync(user, model.Password);
                if (createdUser.Succeeded)
                {
                    RedirectToAction("Confirm");
                }
            }
            return View();

        }

    }
}
