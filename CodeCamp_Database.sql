-- ================================================================
-- DATABASE  : APU CodeCamp Management System
-- FILE      : CodeCamp_Database.sql
-- PURPOSE   : Creates all tables, inserts sample/seed data
--
-- HOW TO USE:
--   1. Open SQL Server Management Studio (SSMS)
--   2. Connect to your SQL Server instance
--   3. Open this file and click Execute (F5)
-- ================================================================

-- Create and use the database
CREATE DATABASE CodeCampDB;
GO

USE CodeCampDB;
GO

-- ----------------------------------------------------------------
-- TABLE: Users
-- Stores login credentials and role for all system users.
-- Role values: 'Admin', 'Trainer', 'Lecturer', 'Student'
-- ----------------------------------------------------------------
CREATE TABLE Users (
    UserID      INT IDENTITY(1,1) PRIMARY KEY,
    Username    VARCHAR(50)  NOT NULL UNIQUE,
    Password    VARCHAR(100) NOT NULL,
    Role        VARCHAR(20)  NOT NULL,
    FullName    VARCHAR(100) NOT NULL,
    Email       VARCHAR(100) NOT NULL,
    Phone       VARCHAR(20)  NOT NULL,
    Address     VARCHAR(200) NOT NULL,
    CreatedDate DATETIME     DEFAULT GETDATE()
);
GO

-- ----------------------------------------------------------------
-- TABLE: Trainers
-- Extra details for users with Role = 'Trainer'
-- ----------------------------------------------------------------
CREATE TABLE Trainers (
    TrainerID      INT IDENTITY(1,1) PRIMARY KEY,
    UserID         INT          NOT NULL FOREIGN KEY REFERENCES Users(UserID),
    Specialization VARCHAR(100) NOT NULL,
    Qualification  VARCHAR(100) NOT NULL,
    IsActive       BIT          DEFAULT 1
);
GO

-- ----------------------------------------------------------------
-- TABLE: Lecturers
-- Extra details for users with Role = 'Lecturer'
-- ----------------------------------------------------------------
CREATE TABLE Lecturers (
    LecturerID INT IDENTITY(1,1) PRIMARY KEY,
    UserID     INT         NOT NULL FOREIGN KEY REFERENCES Users(UserID),
    Department VARCHAR(100) NOT NULL
);
GO

-- ----------------------------------------------------------------
-- TABLE: Students
-- Extra details for users with Role = 'Student'
-- ----------------------------------------------------------------
CREATE TABLE Students (
    StudentID   INT IDENTITY(1,1) PRIMARY KEY,
    UserID      INT         NOT NULL FOREIGN KEY REFERENCES Users(UserID),
    TPNumber    VARCHAR(20) NOT NULL UNIQUE,
    StudyLevel  VARCHAR(20) NOT NULL   -- Foundation, Level1, Level2, Level3
);
GO

-- ----------------------------------------------------------------
-- TABLE: Modules
-- Programming modules available for coaching
-- ----------------------------------------------------------------
CREATE TABLE Modules (
    ModuleID   INT IDENTITY(1,1) PRIMARY KEY,
    ModuleName VARCHAR(100) NOT NULL,
    ModuleCode VARCHAR(20)  NOT NULL UNIQUE
);
GO

-- ----------------------------------------------------------------
-- TABLE: TrainerAssignments
-- Admin assigns a trainer to a specific module and level.
-- One trainer can only teach one module at a time.
-- ----------------------------------------------------------------
CREATE TABLE TrainerAssignments (
    AssignmentID INT IDENTITY(1,1) PRIMARY KEY,
    TrainerID    INT         NOT NULL FOREIGN KEY REFERENCES Trainers(TrainerID),
    ModuleID     INT         NOT NULL FOREIGN KEY REFERENCES Modules(ModuleID),
    ClassLevel   VARCHAR(20) NOT NULL,  -- Beginner, Intermediate, Advance
    AssignedDate DATETIME    DEFAULT GETDATE(),
    IsActive     BIT         DEFAULT 1
);
GO

-- ----------------------------------------------------------------
-- TABLE: Classes
-- Coaching sessions created by trainers
-- ----------------------------------------------------------------
CREATE TABLE Classes (
    ClassID      INT IDENTITY(1,1) PRIMARY KEY,
    TrainerID    INT           NOT NULL FOREIGN KEY REFERENCES Trainers(TrainerID),
    ModuleID     INT           NOT NULL FOREIGN KEY REFERENCES Modules(ModuleID),
    ClassLevel   VARCHAR(20)   NOT NULL,  -- Beginner, Intermediate, Advance
    Fee          DECIMAL(10,2) NOT NULL,
    Schedule     VARCHAR(200)  NOT NULL,  -- e.g. "Mon/Wed 8:00PM - 10:00PM"
    StartDate    DATE          NOT NULL,
    EndDate      DATE          NOT NULL,  -- 4 weeks after start
    MaxStudents  INT           DEFAULT 20,
    IsActive     BIT           DEFAULT 1
);
GO

-- ----------------------------------------------------------------
-- TABLE: EnrolmentRequests
-- Student requests to join a class (pending lecturer approval)
-- ----------------------------------------------------------------
CREATE TABLE EnrolmentRequests (
    RequestID    INT IDENTITY(1,1) PRIMARY KEY,
    StudentID    INT         NOT NULL FOREIGN KEY REFERENCES Students(StudentID),
    ClassID      INT         NOT NULL FOREIGN KEY REFERENCES Classes(ClassID),
    RequestDate  DATETIME    DEFAULT GETDATE(),
    Status       VARCHAR(20) DEFAULT 'Pending',  -- Pending, Approved, Rejected, Cancelled
    RequestedBy  VARCHAR(20) NOT NULL             -- 'Student' or 'Lecturer'
);
GO

-- ----------------------------------------------------------------
-- TABLE: Enrolments
-- Confirmed enrolments (approved requests)
-- ----------------------------------------------------------------
CREATE TABLE Enrolments (
    EnrolmentID    INT IDENTITY(1,1) PRIMARY KEY,
    StudentID      INT         NOT NULL FOREIGN KEY REFERENCES Students(StudentID),
    ClassID        INT         NOT NULL FOREIGN KEY REFERENCES Classes(ClassID),
    LecturerID     INT         NOT NULL FOREIGN KEY REFERENCES Lecturers(LecturerID),
    EnrolmentDate  DATETIME    DEFAULT GETDATE(),
    MonthOfEnrol   VARCHAR(20) NOT NULL,
    PaymentStatus  VARCHAR(20) DEFAULT 'Unpaid',  -- Unpaid, Paid
    IsCompleted    BIT         DEFAULT 0
);
GO

-- ----------------------------------------------------------------
-- TABLE: Payments
-- Payment records for enrolments
-- ----------------------------------------------------------------
CREATE TABLE Payments (
    PaymentID     INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentID   INT           NOT NULL FOREIGN KEY REFERENCES Enrolments(EnrolmentID),
    Amount        DECIMAL(10,2) NOT NULL,
    PaymentDate   DATETIME      DEFAULT GETDATE(),
    ReceiptNumber VARCHAR(20)   NOT NULL
);
GO

-- ----------------------------------------------------------------
-- TABLE: Feedback
-- Trainer sends feedback/suggestions to Admin
-- ----------------------------------------------------------------
CREATE TABLE Feedback (
    FeedbackID   INT IDENTITY(1,1) PRIMARY KEY,
    TrainerID    INT          NOT NULL FOREIGN KEY REFERENCES Trainers(TrainerID),
    Subject      VARCHAR(200) NOT NULL,
    Message      VARCHAR(MAX) NOT NULL,
    FeedbackDate DATETIME     DEFAULT GETDATE(),
    IsRead       BIT          DEFAULT 0
);
GO

-- ================================================================
-- SEED DATA: Insert sample records for testing
-- ================================================================

-- Admin user
INSERT INTO Users (Username, Password, Role, FullName, Email, Phone, Address)
VALUES ('admin', 'admin123', 'Admin', 'System Administrator',
        'admin@apu.edu.my', '0123456789', 'APU, Technology Park Malaysia');

-- Trainer users
INSERT INTO Users (Username, Password, Role, FullName, Email, Phone, Address)
VALUES ('trainer1', 'trainer123', 'Trainer', 'John Smith',
        'john@trainer.com', '0112345678', 'Kuala Lumpur');
INSERT INTO Users (Username, Password, Role, FullName, Email, Phone, Address)
VALUES ('trainer2', 'trainer123', 'Trainer', 'Sarah Lee',
        'sarah@trainer.com', '0123456788', 'Petaling Jaya');

-- Lecturer users
INSERT INTO Users (Username, Password, Role, FullName, Email, Phone, Address)
VALUES ('lecturer1', 'lecturer123', 'Lecturer', 'Dr. Ahmad Razif',
        'ahmad@apu.edu.my', '0198765432', 'APU, Technology Park Malaysia');

-- Student users
INSERT INTO Users (Username, Password, Role, FullName, Email, Phone, Address)
VALUES ('student1', 'student123', 'Student', 'Ali Hassan',
        'ali@mail.apu.edu.my', '0134567890', 'Cyberjaya');
INSERT INTO Users (Username, Password, Role, FullName, Email, Phone, Address)
VALUES ('student2', 'student123', 'Student', 'Priya Raj',
        'priya@mail.apu.edu.my', '0145678901', 'Kuala Lumpur');

-- Trainer records
INSERT INTO Trainers (UserID, Specialization, Qualification)
VALUES (2, 'C# and .NET', 'MSc Computer Science');
INSERT INTO Trainers (UserID, Specialization, Qualification)
VALUES (3, 'Java and OOP', 'BSc Software Engineering');

-- Lecturer record
INSERT INTO Lecturers (UserID, Department)
VALUES (4, 'School of Computing');

-- Student records
INSERT INTO Students (UserID, TPNumber, StudyLevel)
VALUES (5, 'TP055001', 'Level1');
INSERT INTO Students (UserID, TPNumber, StudyLevel)
VALUES (6, 'TP055002', 'Level1');

-- Modules
INSERT INTO Modules (ModuleName, ModuleCode)
VALUES ('Object Oriented Programming', 'CT044-3-1-IOOP');
INSERT INTO Modules (ModuleName, ModuleCode)
VALUES ('Introduction to Programming', 'CT024-1-0-PROG');
INSERT INTO Modules (ModuleName, ModuleCode)
VALUES ('Data Structures and Algorithms', 'CT074-3-3-DSTR');

-- Trainer Assignments (Admin assigns trainer to module+level)
INSERT INTO TrainerAssignments (TrainerID, ModuleID, ClassLevel)
VALUES (1, 1, 'Beginner');
INSERT INTO TrainerAssignments (TrainerID, ModuleID, ClassLevel)
VALUES (2, 2, 'Intermediate');

-- Classes
INSERT INTO Classes (TrainerID, ModuleID, ClassLevel, Fee, Schedule, StartDate, EndDate)
VALUES (1, 1, 'Beginner', 150.00,
        'Monday & Wednesday 7:00PM - 9:00PM',
        '2026-02-03', '2026-02-28');
INSERT INTO Classes (TrainerID, ModuleID, ClassLevel, Fee, Schedule, StartDate, EndDate)
VALUES (2, 2, 'Intermediate', 200.00,
        'Tuesday & Thursday 6:00PM - 8:00PM',
        '2026-02-04', '2026-03-01');

GO

-- ================================================================
-- USEFUL VIEWS for reports
-- ================================================================

-- View: Student enrolment details
CREATE VIEW vw_StudentEnrolments AS
SELECT
    e.EnrolmentID,
    s.TPNumber,
    u.FullName     AS StudentName,
    m.ModuleName,
    c.ClassLevel,
    c.Fee,
    c.Schedule,
    e.MonthOfEnrol,
    e.PaymentStatus,
    e.IsCompleted,
    e.EnrolmentDate
FROM Enrolments e
JOIN Students   s  ON e.StudentID  = s.StudentID
JOIN Users      u  ON s.UserID     = u.UserID
JOIN Classes    c  ON e.ClassID    = c.ClassID
JOIN Modules    m  ON c.ModuleID   = m.ModuleID;
GO

-- View: Monthly income per trainer
CREATE VIEW vw_MonthlyIncome AS
SELECT
    u.FullName          AS TrainerName,
    m.ModuleName,
    c.ClassLevel,
    FORMAT(p.PaymentDate, 'MMMM yyyy') AS Month,
    COUNT(p.PaymentID)  AS StudentsPaid,
    SUM(p.Amount)       AS TotalIncome
FROM Payments   p
JOIN Enrolments e  ON p.EnrolmentID = e.EnrolmentID
JOIN Classes    c  ON e.ClassID     = c.ClassID
JOIN Trainers   t  ON c.TrainerID   = t.TrainerID
JOIN Users      u  ON t.UserID      = u.UserID
JOIN Modules    m  ON c.ModuleID    = m.ModuleID
GROUP BY u.FullName, m.ModuleName, c.ClassLevel, FORMAT(p.PaymentDate, 'MMMM yyyy');
GO

PRINT 'CodeCampDB created successfully with all tables and seed data.';
GO
