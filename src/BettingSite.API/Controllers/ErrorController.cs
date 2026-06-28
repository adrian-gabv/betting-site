using BettingSite.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BettingSite.API.Controllers
{
    public class ErrorController(DataContext context) : BaseApiController
    {
        private readonly DataContext _context = context;

        [Authorize]
        [HttpGet("auth")]
        public ActionResult<string> GetSecret()
        {
            return "Secret text";
        }

        [HttpGet("not-found")]
        public ActionResult<string> GetNotFound()
        {
            var error = _context.Users.Find(-1);
            if (error == null) return NotFound();
            return Ok(error);
        }

        [HttpGet("server-error")]
        public ActionResult<string> GetServerError()
        {
            var error = _context.Users.Find(-1);
            var errorReturn = error!.ToString();
            return errorReturn;
        }

        [HttpGet("bad-request")]
        public ActionResult<string> GetBadRequest()
        {
            return BadRequest("This wasn't a good request");
        }
    }
}
