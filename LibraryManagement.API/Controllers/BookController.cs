using LibraryManagement.BLL;
using LibraryManagement.DAL.Exceptions;
using LibraryManagement.DAL.Models;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly BookBLL _bookBLL;

        public BookController (BookBLL bookBLL)
        {
            _bookBLL = bookBLL;
        }

        // GET: api/books
        [HttpGet]
        public IActionResult GetAll()
        {
            var books = _bookBLL.GetAllBooks();
            return Ok(books);
        }

        // GET: api/books/{id}
        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var book = _bookBLL.GetBookById(id);
            if (book == null)
            {
                return NotFound(new { Message = $"Book with ID {id} was not found." });
            }
            return Ok(book);
        }

        // POST: api/books
        [HttpPost]
        public IActionResult Create([FromBody] Book book)
        {
            var result = _bookBLL.AddBook(book);
            if (!result.IsValid)
            {
                return BadRequest(new { Message = result.Message });
            }

            book.BookId = result.NewId;
            return CreatedAtAction(nameof(GetById), new { id = book.BookId }, book);
        }

        // PUT: api/books/{id}
        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] Book book)
        {
            book.BookId = id;
            var result = _bookBLL.UpdateBook(id, book);
            if (!result.IsValid)
            {
                return BadRequest(new { Message = result.Message });
            }

            return Ok(new {Message = result.Message});
        }

        // DELETE: api/book/{id}
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var result = _bookBLL.DeleteBook(id);
                if (!result.IsSuccess)
                {
                    return NotFound(new { Message = result.Message });
                }

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
