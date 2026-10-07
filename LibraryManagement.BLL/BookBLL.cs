using LibraryManagement.DAL;
using LibraryManagement.DAL.Models;
using LibraryManagement.DAL.Exceptions;

namespace LibraryManagement.BLL
{
    public class BookBLL
    {
        private readonly BookDAL _bookDAL;
        private readonly AuthorDAL _authorDAL;
        private readonly LoanDAL _loanDAL;

        public BookBLL(BookDAL bookDAL, AuthorDAL authorDAL, LoanDAL loanDAL)
        {
            _bookDAL = bookDAL;
            _authorDAL = authorDAL;
            _loanDAL = loanDAL;
        }

        public List<Book> GetAllBooks()
        {
            return _bookDAL.GetAllBooks();
        }

        public Book? GetBookById(int bookId)
        {
            if (bookId <= 0) return null;
            return _bookDAL.GetBookById(bookId);
        }

        public (bool IsValid, string Message, int NewId) AddBook(Book book)
        {
            var validation = ValidateBook(book);
            if (!validation.IsValid)
            {
                return (false, validation.Message, 0);
            }

            if (_authorDAL.GetAuthorById(book.AuthorId) == null)
            {
                return (false, $"Author with ID {book.AuthorId} does not exist", 0);
            }

            int newId = _bookDAL.AddBook(book);
            return (true, "Book added successfully.", newId);
        }

        public (bool IsValid, string Message) UpdateBook(int id, Book book)
        {
            if (id <= 0 || id != book.BookId)
            {
                return (false, "Invalid Book ID match.");
            }

            var validation = ValidateBook(book);
            if (!validation.IsValid)
            {
                return (false, validation.Message);
            }

            var existingBook = _bookDAL.GetBookById(id);
            if (existingBook == null)
            {
                return (false, "Book not found.");
            }

            if (_authorDAL.GetAuthorById(book.AuthorId) == null)
            {
                return (false, $"Author with ID {book.AuthorId} does not exist");
            }

            bool updated = _bookDAL.UpdateBook(book);
            return updated ? (true, "Book updated successfully.") : (false, "Faild to update book.");
        }

        public (bool IsSuccess, string Message) DeleteBook(int id)
        {
            if (id <= 0)
            {
                return (false, "Invalid Book ID.");
            }

            var existingBook = _bookDAL.GetBookById(id);
            if (existingBook == null)
            {
                return (false, "Book not found.");
            }

            // فحص الحماية: منع حذف كتاب مستعار حالياً
            if (_loanDAL.HasActiveLoansByBookId(id))
            {
                throw new BusinessException("لا يمكن حذف الكتاب لأنه مستعار حالياً من قبل أحدهم ولم يتم إرجاعه بعد.");
            }

            bool deleted = _bookDAL.DeleteBook(id);
            return deleted ? (true, "Book deleted successfully.") : (false, "Faild to delete book.");
        }

        // Validation
        private (bool IsValid, string Message) ValidateBook(Book book)
        {
            if (string.IsNullOrWhiteSpace(book.Title))
                return (false, "Book title is required.");

            if (book.AuthorId <= 0)
                return (false, "Valid Author Id is required.");

            if (string.IsNullOrWhiteSpace(book.ISBN))
                return (false, "Book ISBN is required.");

            if (book.TotalCopies < 0)
                return (false, "Total copies cannot be negative.");

            if (book.AvailableCopies < 0)
                return (false, "Available copies cannot be negative.");

            if (book.AvailableCopies > book.TotalCopies)
                return (false, "Available copies connot exceed total copies.");

            return (true, string.Empty);
        }
    }
}
