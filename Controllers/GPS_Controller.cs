using Google.Apis.Discovery;
using GPS_Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Security.Claims;

namespace GPS_Server.Controllers
{
    [Route("gps")]
    [ApiController]
    public class GPS_Controller : ControllerBase
    {
        private readonly ILogger<GPS_Controller> _logger;
        private readonly TCUContext _tcuContext;
        private readonly UserManager<IdentityUser> _userManager;
        public GPS_Controller(ILogger<GPS_Controller> logger, UserManager<IdentityUser> userManager, TCUContext tcuContext)
        {
            _logger = logger;
            _tcuContext = tcuContext;
            _userManager= userManager;
        }

        [HttpGet("tcu")]
        [Authorize(Policy = "MobileOnly")]
        public async Task<IActionResult> GetTcu()
        {
            if (User.Identity == null)
                return Unauthorized();

            string? deviceId = (from _claim in User.Claims
                                where _claim.Type == "deviceId"
                                select _claim.Value).FirstOrDefault();
            if (deviceId == null)
                return Unauthorized();

            string? userId = (from _claim in User.Claims
                              where _claim.Type == ClaimTypes.NameIdentifier
                              select _claim.Value).FirstOrDefault();
            if (userId == null)
                return Unauthorized();

            IdentityUser user = await _userManager.FindByIdAsync(userId);

            var tcu = (from _tcu in _tcuContext.Tcus
                       where _tcu.UserId == user.Id
                       select _tcu).FirstOrDefault();

            if (tcu == null)
                return NotFound();

            var tcuId=new JObject
            {
                new JProperty("id",tcu.TcuId )
            };
            return Ok(tcuId);
        }
    }
}
