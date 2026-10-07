using LibraryManagement.BLL;
using LibraryManagement.DAL.Exceptions;
using LibraryManagement.DAL.Models;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MembersController : ControllerBase
    {
        private readonly MemberBLL _memberBLL;

        public MembersController(MemberBLL memberBLL)
        {
            _memberBLL = memberBLL;
        }

        [HttpGet]
        public IActionResult GetAll() => Ok(_memberBLL.GetAllMembers());

        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var member = _memberBLL.GetMemberById(id);
            return member == null ? NotFound(new { Message = "Member not found." }) : Ok(member);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Member member)
        {
            var result = _memberBLL.AddMember(member);
            if (!result.IsValid) return BadRequest(new { Message = result.Message });

            member.MemberId = result.NewId;
            return CreatedAtAction(nameof(GetById), new { id = member.MemberId }, member);
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] Member member)
        {
            // نترك التحقق للـ BLL مباشرة دون تعديل member.MemberId يدوياً
            var result = _memberBLL.UpdateMember(id, member);
            if (!result.IsValid)
                return BadRequest(new { Message = result.Message });

            return Ok(new { Message = result.Message });
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var result = _memberBLL.DeleteMember(id);
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