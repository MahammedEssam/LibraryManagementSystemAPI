using LibraryManagement.DAL;
using LibraryManagement.DAL.Models;

namespace LibraryManagement.BLL
{
    public class AuthorBLL
    {
        private readonly AuthorDAL _authorDAL;
        public AuthorBLL(AuthorDAL authorDAL)
        {
            _authorDAL = authorDAL;
        }
        public List<Author> GetAllAuthors() => _authorDAL.GetAllAuthors();
        public Author? GetAuthorById(int id)
        {
            if(id <= 0) return null;
            return _authorDAL.GetAuthorById(id);
        }
        public (bool IsValid, string Message, int NewId) AddAuthor(Author author)
        {
            if (string.IsNullOrWhiteSpace(author.Name)) 
                return(false, "Author name is required.", 0);

            int newId = _authorDAL.AddAuthor(author);
            return (true, "Author added successfully.", newId);
        }
        public (bool IsValid, string Message) UpdateAuthor(int id, Author author)
        {
            if (id <= 0 || id != author.AuthorId)
                return (false, "Invalid Author ID.");

            if (string.IsNullOrWhiteSpace(author.Name))
                return (false, "Author name is required.");

            if(_authorDAL.GetAuthorById(id) == null)
                return (false, "Author not found.");

            bool updated = _authorDAL.UpdateAuthor(author);
            return updated ? (true, "Author updated successfully.") : (false, "Failed to update author.");
        }
        public (bool IsSuccess, string Message) DeleteAuthor(int id)
        {
            if (id <= 0) return (false, "Invalid Author ID.");

            if (_authorDAL.GetAuthorById(id) == null)
                return (false, "Author not found.");

            // Protect deletion and prevent deletion if there are linked books
            if (_authorDAL.HasBooks(id)) 
                return (false, "Cannot delete author because there are books linked to this author.");

            bool deleted = _authorDAL.DeleteAuthor(id);
            return deleted ? (true, "Author deleted successfully.") : (false, "Failed to delete author.");
        }

    }
}
