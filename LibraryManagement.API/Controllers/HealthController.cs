using LibraryManagement.BLL;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly HealthBLL _healthBLL;
        public HealthController (HealthBLL healthBLL)
        {
            _healthBLL = healthBLL;
        }

        [HttpGet("/")]
        public IActionResult GetStatus()
        {
            var result = _healthBLL.GetSystemStatus();

            if (result.IsSuccess)
            {
                return Ok(new
                {
                    Status = "Success",
                    Message = result.Message,
                    Database = result.DbStatus
                });
            }

            return StatusCode(500, new
            {
                Status = "Error",
                Message = result.Message,
                DatabaseError = result.DbStatus
            });
        }
    }
}
