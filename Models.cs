// ================================================================
// FILE    : Models.cs
// PURPOSE : Data model classes used throughout the application.
//           Each class represents one database table.
//           These are plain classes (no abstract, no interface).
// ================================================================

using System;

namespace CodeCampSystem
{
    // --------------------------------------------------------
    // User: represents any logged-in user
    // --------------------------------------------------------
    class User
    {
        public int    UserID   { get; set; }
        public string Username { get; set; }
        public string Role     { get; set; }
        public string FullName { get; set; }
        public string Email    { get; set; }
        public string Phone    { get; set; }
        public string Address  { get; set; }

        // Constructor
        public User(int id, string username, string role,
                    string fullName, string email, string phone, string address)
        {
            UserID   = id;
            Username = username;
            Role     = role;
            FullName = fullName;
            Email    = email;
            Phone    = phone;
            Address  = address;
        }
    }

    // --------------------------------------------------------
    // Trainer: extra trainer details
    // --------------------------------------------------------
    class Trainer
    {
        public int    TrainerID      { get; set; }
        public int    UserID         { get; set; }
        public string FullName       { get; set; }
        public string Email          { get; set; }
        public string Phone          { get; set; }
        public string Specialization { get; set; }
        public string Qualification  { get; set; }
        public bool   IsActive       { get; set; }

        public Trainer() { }

        public Trainer(int trainerID, int userID, string fullName,
                       string email, string phone,
                       string spec, string qual, bool active)
        {
            TrainerID      = trainerID;
            UserID         = userID;
            FullName       = fullName;
            Email          = email;
            Phone          = phone;
            Specialization = spec;
            Qualification  = qual;
            IsActive       = active;
        }
    }

    // --------------------------------------------------------
    // Student: extra student details
    // --------------------------------------------------------
    class Student
    {
        public int    StudentID  { get; set; }
        public int    UserID     { get; set; }
        public string FullName   { get; set; }
        public string TPNumber   { get; set; }
        public string Email      { get; set; }
        public string Phone      { get; set; }
        public string Address    { get; set; }
        public string StudyLevel { get; set; }

        public Student() { }

        public Student(int studentID, int userID, string fullName,
                       string tpNumber, string email, string phone,
                       string address, string level)
        {
            StudentID  = studentID;
            UserID     = userID;
            FullName   = fullName;
            TPNumber   = tpNumber;
            Email      = email;
            Phone      = phone;
            Address    = address;
            StudyLevel = level;
        }
    }

    // --------------------------------------------------------
    // Module: programming module details
    // --------------------------------------------------------
    class Module
    {
        public int    ModuleID   { get; set; }
        public string ModuleName { get; set; }
        public string ModuleCode { get; set; }

        public Module() { }

        public Module(int id, string name, string code)
        {
            ModuleID   = id;
            ModuleName = name;
            ModuleCode = code;
        }
    }

    // --------------------------------------------------------
    // Class: coaching class/session
    // --------------------------------------------------------
    class CoachingClass
    {
        public int      ClassID    { get; set; }
        public int      TrainerID  { get; set; }
        public int      ModuleID   { get; set; }
        public string   ClassLevel { get; set; }
        public decimal  Fee        { get; set; }
        public string   Schedule   { get; set; }
        public DateTime StartDate  { get; set; }
        public DateTime EndDate    { get; set; }
        public int      MaxStudents{ get; set; }
        public bool     IsActive   { get; set; }

        public CoachingClass() { }
    }

    // --------------------------------------------------------
    // Enrolment: confirmed student enrolment in a class
    // --------------------------------------------------------
    class Enrolment
    {
        public int      EnrolmentID   { get; set; }
        public int      StudentID     { get; set; }
        public int      ClassID       { get; set; }
        public int      LecturerID    { get; set; }
        public string   MonthOfEnrol  { get; set; }
        public string   PaymentStatus { get; set; }
        public bool     IsCompleted   { get; set; }
        public DateTime EnrolmentDate { get; set; }

        public Enrolment() { }
    }

    // --------------------------------------------------------
    // Payment: fee payment record
    // --------------------------------------------------------
    class Payment
    {
        public int      PaymentID     { get; set; }
        public int      EnrolmentID   { get; set; }
        public decimal  Amount        { get; set; }
        public DateTime PaymentDate   { get; set; }
        public string   ReceiptNumber { get; set; }

        public Payment() { }
    }

    // --------------------------------------------------------
    // Feedback: trainer feedback to admin
    // --------------------------------------------------------
    class Feedback
    {
        public int      FeedbackID   { get; set; }
        public int      TrainerID    { get; set; }
        public string   Subject      { get; set; }
        public string   Message      { get; set; }
        public DateTime FeedbackDate { get; set; }
        public bool     IsRead       { get; set; }

        public Feedback() { }

        public Feedback(int trainerID, string subject, string message)
        {
            TrainerID    = trainerID;
            Subject      = subject;
            Message      = message;
            FeedbackDate = DateTime.Now;
            IsRead       = false;
        }
    }
}
