using Microsoft.AspNetCore.Mvc;
using System;
using LibraryManagement.BLL;
using LibraryManagement.API.DTOs;
using LibraryManagement.DAL.Exceptions;

namespace LibraryManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoansController : ControllerBase
    {
        private readonly LoanBLL _loanBLL;

        public LoansController(LoanBLL loanBLL)
        {
            _loanBLL = loanBLL;
        }

        /// <summary>
        /// استعارة كتاب جديد
        /// POST /api/loans/borrow
        /// </summary>
        [HttpPost("borrow")]
        public IActionResult BorrowBook([FromBody] BorrowBookRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                int newLoanId = _loanBLL.BorrowBook(request.BookId, request.MemberId, request.BorrowDays);

                // إرجاع 201 Created مع الـ Location Header لعنوان الاستعارة الجديدة
                return CreatedAtAction(nameof(BorrowBook), new { id = newLoanId }, new
                {
                    Message = "تمت عملية الاستعارة بنجاح.",
                    LoanId = newLoanId
                });
            }
            catch (BusinessException ex)
            {
                // خطأ في قواعد العمل (مثل: تجاوز 3 كتب، الكتاب مخلص، العضو غير موجود)
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                // خطأ تقني غير متوقع (انقطاع السيرفر، السيرفر وقع، إلخ)
                return StatusCode(500, new { Message = "حدث خطأ غير متوقع في الخادم. يرجى المحاولة لاحقاً." });
            }
        }

        /// <summary>
        /// إرجاع كتاب مستعار
        /// PUT /api/loans/{id}/return
        /// </summary>
        [HttpPut("{id}/return")]
        public IActionResult ReturnBook(int id)
        {
            try
            {
                _loanBLL.ReturnBook(id);
                return Ok(new { Message = "تم إرجاع الكتاب بنجاح." });
            }
            catch (BusinessException ex)
            {
                // خطأ في قواعد العمل (مثل: الكتاب اترجع قبل كده، رقم الاستعارة مش موجود)
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                // خطأ تقني غير متوقع
                return StatusCode(500, new { Message = "حدث خطأ غير متوقع في الخادم. يرجى المحاولة لاحقاً." });
            }
        }
    }
}