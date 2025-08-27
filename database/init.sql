-- Student Management System Database Initialization Script
USE master;
GO

-- Create database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'StudentManagementDB')
BEGIN
    CREATE DATABASE StudentManagementDB;
END
GO

USE StudentManagementDB;
GO

-- Create Users table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Username NVARCHAR(50) NOT NULL UNIQUE,
        Password NVARCHAR(255) NOT NULL,
        Email NVARCHAR(100) NOT NULL UNIQUE,
        Role NVARCHAR(20) NOT NULL CHECK (Role IN ('Student', 'Teacher', 'Admin')),
        CreatedAt DATETIME2 DEFAULT GETDATE(),
        UpdatedAt DATETIME2 DEFAULT GETDATE(),
        IsActive BIT DEFAULT 1
    );
END
GO

-- Create Students table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Students')
BEGIN
    CREATE TABLE Students (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        StudentNumber NVARCHAR(20) NOT NULL UNIQUE,
        FirstName NVARCHAR(50) NOT NULL,
        LastName NVARCHAR(50) NOT NULL,
        DateOfBirth DATE,
        Gender NVARCHAR(10) CHECK (Gender IN ('Male', 'Female', 'Other')),
        Phone NVARCHAR(20),
        Address NVARCHAR(200),
        EnrollmentDate DATE DEFAULT GETDATE(),
        Major NVARCHAR(100),
        Class NVARCHAR(50),
        CreatedAt DATETIME2 DEFAULT GETDATE(),
        UpdatedAt DATETIME2 DEFAULT GETDATE(),
        FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
    );
END
GO

-- Create Teachers table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Teachers')
BEGIN
    CREATE TABLE Teachers (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        TeacherNumber NVARCHAR(20) NOT NULL UNIQUE,
        FirstName NVARCHAR(50) NOT NULL,
        LastName NVARCHAR(50) NOT NULL,
        DateOfBirth DATE,
        Gender NVARCHAR(10) CHECK (Gender IN ('Male', 'Female', 'Other')),
        Phone NVARCHAR(20),
        Address NVARCHAR(200),
        Email NVARCHAR(100),
        Department NVARCHAR(100),
        Title NVARCHAR(50),
        HireDate DATE DEFAULT GETDATE(),
        CreatedAt DATETIME2 DEFAULT GETDATE(),
        UpdatedAt DATETIME2 DEFAULT GETDATE(),
        FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
    );
END
GO

-- Create Courses table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Courses')
BEGIN
    CREATE TABLE Courses (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CourseCode NVARCHAR(20) NOT NULL UNIQUE,
        CourseName NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500),
        Credits INT NOT NULL,
        TeacherId INT,
        MaxStudents INT DEFAULT 50,
        StartDate DATE,
        EndDate DATE,
        Schedule NVARCHAR(100),
        Location NVARCHAR(100),
        Status NVARCHAR(20) DEFAULT 'Active' CHECK (Status IN ('Active', 'Inactive', 'Completed')),
        CreatedAt DATETIME2 DEFAULT GETDATE(),
        UpdatedAt DATETIME2 DEFAULT GETDATE(),
        FOREIGN KEY (TeacherId) REFERENCES Teachers(Id) ON DELETE SET NULL
    );
END
GO

-- Create Enrollments table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Enrollments')
BEGIN
    CREATE TABLE Enrollments (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        StudentId INT NOT NULL,
        CourseId INT NOT NULL,
        EnrollmentDate DATETIME2 DEFAULT GETDATE(),
        Status NVARCHAR(20) DEFAULT 'Enrolled' CHECK (Status IN ('Enrolled', 'Dropped', 'Completed')),
        Grade DECIMAL(5,2),
        CreatedAt DATETIME2 DEFAULT GETDATE(),
        UpdatedAt DATETIME2 DEFAULT GETDATE(),
        FOREIGN KEY (StudentId) REFERENCES Students(Id) ON DELETE CASCADE,
        FOREIGN KEY (CourseId) REFERENCES Courses(Id) ON DELETE CASCADE,
        UNIQUE(StudentId, CourseId)
    );
END
GO

-- Create Assignments table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Assignments')
BEGIN
    CREATE TABLE Assignments (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CourseId INT NOT NULL,
        Title NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500),
        DueDate DATETIME2 NOT NULL,
        MaxScore DECIMAL(5,2) DEFAULT 100,
        Weight DECIMAL(3,2) DEFAULT 1.00,
        CreatedAt DATETIME2 DEFAULT GETDATE(),
        UpdatedAt DATETIME2 DEFAULT GETDATE(),
        FOREIGN KEY (CourseId) REFERENCES Courses(Id) ON DELETE CASCADE
    );
END
GO

-- Create AssignmentSubmissions table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AssignmentSubmissions')
BEGIN
    CREATE TABLE AssignmentSubmissions (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        AssignmentId INT NOT NULL,
        StudentId INT NOT NULL,
        SubmissionDate DATETIME2 DEFAULT GETDATE(),
        Content NVARCHAR(MAX),
        FilePath NVARCHAR(500),
        Score DECIMAL(5,2),
        Feedback NVARCHAR(500),
        Comments NVARCHAR(500),
        Status NVARCHAR(20) DEFAULT 'Submitted' CHECK (Status IN ('Submitted', 'Graded', 'Late')),
        CreatedAt DATETIME2 DEFAULT GETDATE(),
        UpdatedAt DATETIME2 DEFAULT GETDATE(),
        FOREIGN KEY (AssignmentId) REFERENCES Assignments(Id) ON DELETE CASCADE,
        FOREIGN KEY (StudentId) REFERENCES Students(Id) ON DELETE CASCADE
    );
END
GO

-- Create Grades table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Grades')
BEGIN
    CREATE TABLE Grades (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        StudentId INT NOT NULL,
        CourseId INT NOT NULL,
        AssignmentId INT,
        Score DECIMAL(5,2) NOT NULL,
        Grade NVARCHAR(2),
        Comments NVARCHAR(500),
        GradedBy INT,
        GradedAt DATETIME2 DEFAULT GETDATE(),
        CreatedAt DATETIME2 DEFAULT GETDATE(),
        UpdatedAt DATETIME2 DEFAULT GETDATE(),
        FOREIGN KEY (StudentId) REFERENCES Students(Id) ON DELETE CASCADE,
        FOREIGN KEY (CourseId) REFERENCES Courses(Id) ON DELETE CASCADE,
        FOREIGN KEY (AssignmentId) REFERENCES Assignments(Id) ON DELETE SET NULL,
        FOREIGN KEY (GradedBy) REFERENCES Teachers(Id) ON DELETE SET NULL
    );
END
GO

-- Create Notifications table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Notifications')
BEGIN
    CREATE TABLE Notifications (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Title NVARCHAR(100) NOT NULL,
        Content NVARCHAR(500) NOT NULL,
        Type NVARCHAR(20) DEFAULT 'General' CHECK (Type IN ('General', 'Course', 'Assignment', 'Grade')),
        TargetUserId INT,
        TargetRole NVARCHAR(20),
        IsRead BIT DEFAULT 0,
        CreatedAt DATETIME2 DEFAULT GETDATE(),
        FOREIGN KEY (TargetUserId) REFERENCES Users(Id) ON DELETE SET NULL
    );
END
GO

-- Create Attendance table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Attendance')
BEGIN
    CREATE TABLE Attendance (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        StudentId INT NOT NULL,
        CourseId INT NOT NULL,
        Date DATE NOT NULL,
        Status NVARCHAR(20) DEFAULT 'Present' CHECK (Status IN ('Present', 'Absent', 'Late', 'Excused')),
        Notes NVARCHAR(200),
        RecordedBy INT,
        CreatedAt DATETIME2 DEFAULT GETDATE(),
        FOREIGN KEY (StudentId) REFERENCES Students(Id) ON DELETE CASCADE,
        FOREIGN KEY (CourseId) REFERENCES Courses(Id) ON DELETE CASCADE,
        FOREIGN KEY (RecordedBy) REFERENCES Teachers(Id) ON DELETE SET NULL
    );
END
GO

-- Create indexes for better performance
-- Users table indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Username')
    CREATE UNIQUE INDEX IX_Users_Username ON Users(Username);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Email')
    CREATE UNIQUE INDEX IX_Users_Email ON Users(Email);
GO

-- Students table indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Students_StudentNumber')
    CREATE UNIQUE INDEX IX_Students_StudentNumber ON Students(StudentNumber);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Students_UserId')
    CREATE UNIQUE INDEX IX_Students_UserId ON Students(UserId);
GO

-- Teachers table indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Teachers_TeacherNumber')
    CREATE UNIQUE INDEX IX_Teachers_TeacherNumber ON Teachers(TeacherNumber);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Teachers_UserId')
    CREATE UNIQUE INDEX IX_Teachers_UserId ON Teachers(UserId);
GO

-- Courses table indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Courses_CourseCode')
    CREATE UNIQUE INDEX IX_Courses_CourseCode ON Courses(CourseCode);
GO

-- Enrollments table indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Enrollments_StudentId_CourseId')
    CREATE UNIQUE INDEX IX_Enrollments_StudentId_CourseId ON Enrollments(StudentId, CourseId);
GO

-- Insert sample data
-- Insert admin user
IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'admin')
BEGIN
    -- admin123 的 SHA256 Base64哈希: jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=
    INSERT INTO Users (Username, Password, Email, Role) 
    VALUES ('admin', 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', 'admin@school.com', 'Admin');
END
GO

-- Insert sample teacher
IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'teacher1')
BEGIN
    -- teacher123 的 SHA256 Base64哈希: zeOD7ujuekQArfehX3FvF5ouuXZGs34InrjW0E5mNBY=
    INSERT INTO Users (Username, Password, Email, Role) 
    VALUES ('teacher1', 'zeOD7ujuekQArfehX3FvF5ouuXZGs34InrjW0E5mNBY=', 'teacher1@school.com', 'Teacher');
    
    INSERT INTO Teachers (UserId, TeacherNumber, FirstName, LastName, Department, Title, Email)
    VALUES (SCOPE_IDENTITY(), 'T001', 'Zhang', 'Teacher', 'Computer Science', 'Associate Professor', 'teacher1@school.com');
END
GO

-- Insert sample student
IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'student1')
BEGIN
    -- student123 的 SHA256 Base64哈希: 7yK3eL6/53R6iLhLm4vI7ryuBgrEqE7mQ9Pj1D0XeT8=
    INSERT INTO Users (Username, Password, Email, Role) 
    VALUES ('student1', '7yK3eL6/53R6iLhLm4vI7ryuBgrEqE7mQ9Pj1D0XeT8=', 'student1@school.com', 'Student');
    
    INSERT INTO Students (UserId, StudentNumber, FirstName, LastName, Major, Class)
    VALUES (SCOPE_IDENTITY(), 'S001', 'Li', 'Student', 'Computer Science and Technology', 'CS2021-1');
END
GO

-- Insert sample course
IF NOT EXISTS (SELECT * FROM Courses WHERE CourseCode = 'CS101')
BEGIN
    INSERT INTO Courses (CourseCode, CourseName, Description, Credits, MaxStudents, Schedule, Location)
    VALUES ('CS101', 'Introduction to Computer Science', 'Basic computer science course', 3, 50, 'Monday 8:00-9:40', 'Building A Room 101');
END
GO

-- Insert sample enrollment
IF NOT EXISTS (SELECT * FROM Enrollments WHERE StudentId = (SELECT Id FROM Students WHERE StudentNumber = 'S001') AND CourseId = (SELECT Id FROM Courses WHERE CourseCode = 'CS101'))
BEGIN
    INSERT INTO Enrollments (StudentId, CourseId)
    VALUES ((SELECT Id FROM Students WHERE StudentNumber = 'S001'), (SELECT Id FROM Courses WHERE CourseCode = 'CS101'));
END
GO

-- Insert sample assignment
IF NOT EXISTS (SELECT * FROM Assignments WHERE Title = 'First Assignment')
BEGIN
    INSERT INTO Assignments (CourseId, Title, Description, DueDate, MaxScore, Weight)
    VALUES ((SELECT Id FROM Courses WHERE CourseCode = 'CS101'), 'First Assignment', 'Complete the basic programming exercises', DATEADD(day, 7, GETDATE()), 100, 0.3);
END
GO

-- Insert sample notification
IF NOT EXISTS (SELECT * FROM Notifications WHERE Title = 'Welcome to Student Management System')
BEGIN
    INSERT INTO Notifications (Title, Content, Type, TargetRole)
    VALUES ('Welcome to Student Management System', 'Welcome to our student management system. Please explore the features available to you.', 'General', 'Student');
END
GO

PRINT 'Database initialization completed successfully!';
PRINT 'Sample data has been inserted.';
PRINT 'Default login credentials:';
PRINT 'Admin: admin / admin123';
PRINT 'Teacher: teacher1 / teacher123';
PRINT 'Student: student1 / student123'; 