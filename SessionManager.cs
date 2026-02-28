// ================================================================
// CLASS   : SessionManager
// FILE    : SessionManager.cs
// PURPOSE : Stores the currently logged-in user details so all
//           forms can access who is logged in without passing
//           objects between every form.
// ================================================================

namespace CodeCampSystem
{
    static class SessionManager
    {
        public static int    UserID     { get; set; }
        public static string Username   { get; set; }
        public static string Role       { get; set; }
        public static string FullName   { get; set; }
        public static int    RoleID     { get; set; }  // TrainerID / LecturerID / StudentID

        // Clear session on logout
        public static void Clear()
        {
            UserID   = 0;
            Username = "";
            Role     = "";
            FullName = "";
            RoleID   = 0;
        }
    }
}
