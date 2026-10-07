using LibraryManagement.BLL;
using LibraryManagement.DAL.Exceptions;
using LibraryManagement.DAL.Models;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private readonly AuthorBLL _authorBLL;
        public AuthorsController(AuthorBLL authorBLL)
        {
            _authorBLL = authorBLL;
        }

        [HttpGet]
        public ActionResult GetAll() => Ok(_authorBLL.GetAllAuthors());

        [HttpGet("{id:int}")]
        public ActionResult GetById(int id)
        {
            var author = _authorBLL.GetAuthorById(id);
            return author == null ? NotFound(new { Message = "Author not found." }) : Ok(author);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Author author)
        {
            var result = _authorBLL.AddAuthor(author);
            if (!result.IsValid) return BadRequest(new {Message = result.Message});

            author.AuthorId = result.NewId;
            return CreatedAtAction(nameof(GetById), new { id = author.AuthorId }, author);
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] Author author)
        {
            author.AuthorId = id;
            var result = _authorBLL.UpdateAuthor(id, author);
            if (! result.IsValid) return BadRequest(new { Message = result.Message});

            return Ok(new { Message = result.Message });
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var result = _authorBLL.DeleteAuthor(id);
                if (!result.IsSuccess) return BadRequest(new { Message = result.Message });

                return Ok(new { Message = result.Message });
            }
            catch (BusinessException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "حدث خطأ غير متوقع في الخادم." });
            }
        }
    }
}
