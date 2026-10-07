using System.Net.Mail;
using LibraryManagement.DAL;
using LibraryManagement.DAL.Models;
using LibraryManagement.DAL.Exceptions;

namespace LibraryManagement.BLL
{
    public class MemberBLL
    {
        private readonly MemberDAL _memberDAL;
        private readonly LoanDAL _loanDAL;

        public MemberBLL(MemberDAL memberDAL, LoanDAL loanDAL)
        {
            _memberDAL = memberDAL;
            _loanDAL = loanDAL;
        }

        public List<Member> GetAllMembers() => _memberDAL.GetAllMembers();

        public Member? GetMemberById(int id)
        {
            if (id <= 0) return null;
            return _memberDAL.GetMemberById(id);
        }

        public (bool IsValid, string Message, int NewId) AddMember(Member member)
        {
            var validation = ValidateMember(member);
            if (!validation.IsValid)
                return (false, validation.Message, 0);

            if (_memberDAL.IsEmailExists(member.Email))
                return (false, "Email address is already in use.", 0);

            var result = _memberDAL.AddMember(member);
            return (result.IsSuccess, result.Message, result.NewId);
        }

        public (bool IsValid, string Message) UpdateMember(int id, Member member)
        {
            if (id <= 0)
                return (false, "Invalid Member ID.");

            // نتحقق من مطابقة المعرف في الـ URL مع المعرف في الـ Body لو كان ممرراً
            if (member.MemberId != 0 && id != member.MemberId)
                return (false, "Route ID does not match Payload MemberId.");

            // تعيين المعرف للـ Model للتأكد من استخدام المعرف الصحيح في الـ DAL
            member.MemberId = id;

            var validation = ValidateMember(member);
            if (!validation.IsValid)
                return (false, validation.Message);

            if (_memberDAL.GetMemberById(id) == null)
                return (false, "Member not found.");

            if (_memberDAL.IsEmailExists(member.Email, id))
                return (false, "Email address is already in use by another member.");

            bool updated = _memberDAL.UpdateMember(member);
            return updated ? (true, "Member details updated successfully.") : (false, "Failed to update member.");
        }

        public (bool IsSuccess, string Message) DeleteMember(int id)
        {
            if (id <= 0) return (false, "Invalid Member ID.");

            if (_memberDAL.GetMemberById(id) == null)
                return (false, "Member not found.");

            // فحص الحماية: منع حذف عضو لديه استعارات نشطة
            if (_loanDAL.HasActiveLoansByMemberId(id))
            {
                throw new BusinessException("لا يمكن حذف العضو لأنه يمتلك استعارات نشطة حالياً ولم يقم بإرجاع الكتب بعد.");
            }

            bool deleted = _memberDAL.DeleteMember(id);
            return deleted ? (true, "Member deleted successfully.") : (false, "Failed to delete member.");
        }

        private (bool IsValid, string Message) ValidateMember(Member member)
        {
            if (string.IsNullOrWhiteSpace(member.Name))
                return (false, "Member name is required.");

            if (string.IsNullOrWhiteSpace(member.Email))
                return (false, "Email is required.");

            if (!IsValidEmail(member.Email))
                return (false, "Invalid email format.");

            return (true, string.Empty);
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var mailAddress = new MailAddress(email);
                return mailAddress.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}