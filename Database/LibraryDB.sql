/* ============================================================
   Library Management System - Database Script
   Creates: LibraryDB, 4 tables, and all stored procedures.
   Safe to re-run: tables are created only if missing, and
   procedures use CREATE OR ALTER.
   ============================================================ */

IF DB_ID(N'LibraryDB') IS NULL
    CREATE DATABASE LibraryDB;
GO

USE LibraryDB;
GO

/* ===================== TABLES ===================== */

IF OBJECT_ID(N'dbo.Authors', N'U') IS NULL
CREATE TABLE dbo.Authors (
    AuthorId    INT IDENTITY(1,1) PRIMARY KEY,
    Name        NVARCHAR(150) NOT NULL,
    Nationality NVARCHAR(100) NULL
);
GO

IF OBJECT_ID(N'dbo.Books', N'U') IS NULL
CREATE TABLE dbo.Books (
    BookId          INT IDENTITY(1,1) PRIMARY KEY,
    Title           NVARCHAR(200) NOT NULL,
    AuthorId        INT NOT NULL,
    ISBN            NVARCHAR(50) NOT NULL,
    TotalCopies     INT NOT NULL DEFAULT 1,
    AvailableCopies INT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Books_Authors FOREIGN KEY (AuthorId) REFERENCES dbo.Authors(AuthorId)
);
GO

IF OBJECT_ID(N'dbo.Members', N'U') IS NULL
CREATE TABLE dbo.Members (
    MemberId    INT IDENTITY(1,1) PRIMARY KEY,
    Name        NVARCHAR(150) NOT NULL,
    Email       NVARCHAR(150) NOT NULL UNIQUE,
    PhoneNumber NVARCHAR(20) NULL,
    JoinDate    DATETIME NOT NULL DEFAULT GETDATE()
);
GO

IF OBJECT_ID(N'dbo.Loans', N'U') IS NULL
CREATE TABLE dbo.Loans (
    LoanId     INT IDENTITY(1,1) PRIMARY KEY,
    BookId     INT NOT NULL,
    MemberId   INT NOT NULL,
    LoanDate   DATETIME NOT NULL DEFAULT GETDATE(),
    DueDate    DATETIME NOT NULL,
    ReturnDate DATETIME NULL,              -- NULL = not returned yet
    Status     TINYINT NOT NULL DEFAULT 1, -- 1: Active, 2: Returned, 3: Overdue
    CONSTRAINT FK_Loans_Books   FOREIGN KEY (BookId)   REFERENCES dbo.Books(BookId),
    CONSTRAINT FK_Loans_Members FOREIGN KEY (MemberId) REFERENCES dbo.Members(MemberId)
);
GO

/* ===================== HEALTH ===================== */

CREATE OR ALTER PROCEDURE sp_CheckServerHealth
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        1 AS IsHealthy,
        N'Database connection via Stored Procedure is active' AS [Message],
        GETDATE() AS CurrentServerTime;
END;
GO

/* ===================== AUTHORS ===================== */

CREATE OR ALTER PROCEDURE sp_GetAllAuthors
AS
BEGIN
    SET NOCOUNT ON;
    SELECT AuthorId, Name, Nationality FROM Authors;
END;
GO

CREATE OR ALTER PROCEDURE sp_GetAuthorById
    @AuthorId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT AuthorId, Name, Nationality FROM Authors WHERE AuthorId = @AuthorId;
END;
GO

CREATE OR ALTER PROCEDURE sp_AddAuthor
    @Name NVARCHAR(150),
    @Nationality NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Authors (Name, Nationality) VALUES (@Name, @Nationality);
    SELECT SCOPE_IDENTITY() AS NewAuthorId;
END;
GO

CREATE OR ALTER PROCEDURE sp_UpdateAuthor
    @AuthorId INT,
    @Name NVARCHAR(150),
    @Nationality NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Authors
    SET Name = @Name, Nationality = @Nationality
    WHERE AuthorId = @AuthorId;

    SELECT @@ROWCOUNT AS RowsAffected;
END;
GO

CREATE OR ALTER PROCEDURE sp_DeleteAuthor
    @AuthorId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Authors WHERE AuthorId = @AuthorId;
    SELECT @@ROWCOUNT AS RowsAffected;
END;
GO

/* ===================== BOOKS ===================== */

CREATE OR ALTER PROCEDURE sp_GetAllBooks
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        b.BookId, b.Title, b.ISBN, b.TotalCopies, b.AvailableCopies,
        a.AuthorId, a.Name AS AuthorName
    FROM Books b
    INNER JOIN Authors a ON b.AuthorId = a.AuthorId;
END;
GO

CREATE OR ALTER PROCEDURE sp_GetBookById
    @BookId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        b.BookId, b.Title, b.ISBN, b.TotalCopies, b.AvailableCopies,
        a.AuthorId, a.Name AS AuthorName
    FROM Books b
    INNER JOIN Authors a ON b.AuthorId = a.AuthorId
    WHERE b.BookId = @BookId;
END;
GO

CREATE OR ALTER PROCEDURE sp_AddBook
    @Title NVARCHAR(200),
    @AuthorId INT,
    @ISBN NVARCHAR(50),
    @TotalCopies INT,
    @AvailableCopies INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Books (Title, AuthorId, ISBN, TotalCopies, AvailableCopies)
    VALUES (@Title, @AuthorId, @ISBN, @TotalCopies, @AvailableCopies);

    SELECT SCOPE_IDENTITY() AS NewBookId;
END;
GO

CREATE OR ALTER PROCEDURE sp_UpdateBook
    @BookId INT,
    @Title NVARCHAR(200),
    @AuthorId INT,
    @ISBN NVARCHAR(50),
    @TotalCopies INT,
    @AvailableCopies INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Books
    SET Title = @Title,
        AuthorId = @AuthorId,
        ISBN = @ISBN,
        TotalCopies = @TotalCopies,
        AvailableCopies = @AvailableCopies
    WHERE BookId = @BookId;

    SELECT @@ROWCOUNT AS RowsAffected;
END;
GO

CREATE OR ALTER PROCEDURE sp_DeleteBook
    @BookId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Books WHERE BookId = @BookId;
    SELECT @@ROWCOUNT AS RowsAffected;
END;
GO

/* ===================== MEMBERS ===================== */

CREATE OR ALTER PROCEDURE sp_GetAllMembers
AS
BEGIN
    SET NOCOUNT ON;
    SELECT MemberId, Name, Email, PhoneNumber, JoinDate FROM Members;
END;
GO

CREATE OR ALTER PROCEDURE sp_GetMemberById
    @MemberId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT MemberId, Name, Email, PhoneNumber, JoinDate
    FROM Members
    WHERE MemberId = @MemberId;
END;
GO

CREATE OR ALTER PROCEDURE sp_AddMember
    @Name NVARCHAR(150),
    @Email NVARCHAR(150),
    @PhoneNumber NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Members WHERE Email = @Email)
    BEGIN
        RAISERROR(N'Email is already registered.', 16, 1);
        RETURN;
    END

    INSERT INTO Members (Name, Email, PhoneNumber)
    VALUES (@Name, @Email, @PhoneNumber);

    SELECT SCOPE_IDENTITY() AS NewMemberId;
END;
GO

CREATE OR ALTER PROCEDURE sp_UpdateMember
    @MemberId INT,
    @Name NVARCHAR(150),
    @Email NVARCHAR(150),
    @PhoneNumber NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Members WHERE Email = @Email AND MemberId <> @MemberId)
    BEGIN
        RAISERROR(N'Email is already in use by another member.', 16, 1);
        RETURN;
    END

    UPDATE Members
    SET Name = @Name, Email = @Email, PhoneNumber = @PhoneNumber
    WHERE MemberId = @MemberId;

    SELECT @@ROWCOUNT AS RowsAffected;
END;
GO

CREATE OR ALTER PROCEDURE sp_DeleteMember
    @MemberId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Members WHERE MemberId = @MemberId;
    SELECT @@ROWCOUNT AS RowsAffected;
END;
GO

/* ===================== LOANS ===================== */

CREATE OR ALTER PROCEDURE sp_BorrowBook
    @BookId INT,
    @MemberId INT,
    @BorrowDays INT = 14
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRANSACTION;

    BEGIN TRY
        -- 1. Book exists and has available copies (row locked to prevent race conditions)
        DECLARE @AvailableCopies INT;

        SELECT @AvailableCopies = AvailableCopies
        FROM Books WITH (UPDLOCK)
        WHERE BookId = @BookId;

        IF @AvailableCopies IS NULL
            RAISERROR(N'الكتاب المطلوب غير موجود.', 16, 1);

        IF @AvailableCopies <= 0
            RAISERROR(N'عفواً، لا توجد نسخ متاحة حالياً من هذا الكتاب.', 16, 1);

        -- 2. Member exists
        IF NOT EXISTS (SELECT 1 FROM Members WHERE MemberId = @MemberId)
            RAISERROR(N'العضو الموضح غير موجود.', 16, 1);

        -- 3. Max 3 active loans per member (locked to prevent race conditions)
        DECLARE @ActiveCount INT;

        SELECT @ActiveCount = COUNT(*)
        FROM Loans WITH (UPDLOCK, HOLDLOCK)
        WHERE MemberId = @MemberId AND ReturnDate IS NULL;

        IF @ActiveCount >= 3
            RAISERROR(N'العضو تجاوز الحد الأقصى المسموح به (3 كتب) للاستعارات النشطة.', 16, 1);

        -- 4. Member must not already have an active loan on the same book
        IF EXISTS (
            SELECT 1
            FROM Loans WITH (UPDLOCK, HOLDLOCK)
            WHERE MemberId = @MemberId AND BookId = @BookId AND ReturnDate IS NULL
        )
            RAISERROR(N'العضو مستعير لهذا الكتاب بالفعل ولم يرجعه بعد.', 16, 1);

        -- 5. Decrease available copies
        UPDATE Books
        SET AvailableCopies = AvailableCopies - 1
        WHERE BookId = @BookId;

        -- 6. Create the loan record
        DECLARE @LoanDate DATETIME = GETDATE();
        DECLARE @DueDate DATETIME = DATEADD(DAY, @BorrowDays, @LoanDate);

        INSERT INTO Loans (BookId, MemberId, LoanDate, DueDate, Status)
        VALUES (@BookId, @MemberId, @LoanDate, @DueDate, 1);

        DECLARE @NewLoanId INT = SCOPE_IDENTITY();

        COMMIT TRANSACTION;

        SELECT @NewLoanId AS NewLoanId;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();

        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE sp_ReturnBook
    @LoanId INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRANSACTION;

    BEGIN TRY
        DECLARE @BookId INT;
        DECLARE @ReturnDate DATETIME;

        -- Lock the loan row so the same loan can't be returned twice concurrently
        SELECT
            @BookId = BookId,
            @ReturnDate = ReturnDate
        FROM Loans WITH (UPDLOCK)
        WHERE LoanId = @LoanId;

        IF @BookId IS NULL
            RAISERROR(N'عملية الاستعارة غير موجودة.', 16, 1);

        IF @ReturnDate IS NOT NULL
            RAISERROR(N'هذا الكتاب تم إرجاعه بالفعل سابقاً.', 16, 1);

        UPDATE Loans
        SET ReturnDate = GETDATE(),
            Status = 2
        WHERE LoanId = @LoanId;

        UPDATE Books
        SET AvailableCopies = AvailableCopies + 1
        WHERE BookId = @BookId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();

        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO
