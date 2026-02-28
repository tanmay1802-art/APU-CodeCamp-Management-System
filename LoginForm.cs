// ================================================================
// FORM    : LoginForm
// FILE    : LoginForm.cs
// PURPOSE : First screen shown. Validates username and password,
//           identifies user role, and opens the correct dashboard.
//
// CONTROLS NEEDED (add these in Designer):
//   txtUsername  - TextBox
//   txtPassword  - TextBox  (PasswordChar = *)
//   btnLogin     - Button
//   lblError     - Label    (ForeColor = Red, Visible = false)
// ================================================================

using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace CodeCampSystem
{
    public partial class LoginForm : Form
    {
        DatabaseHelper db = new DatabaseHelper();

        public LoginForm()
        {
            InitializeComponent();
            this.Text = "APU CodeCamp - Login";
        }

        // --------------------------------------------------------
        // btnLogin_Click: validates login and opens correct dashboard
        // --------------------------------------------------------
        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            // Input validation
            if (username == "" || password == "")
            {
                lblError.Text    = "Please enter username and password.";
                lblError.Visible = true;
                return;
            }

            // Query database for matching user
            string sql = @"SELECT UserID, Username, Role, FullName, Email, Phone, Address
                           FROM Users
                           WHERE Username = @Username AND Password = @Password";

            SqlParameter[] parameters = {
                new SqlParameter("@Username", username),
                new SqlParameter("@Password", password)
            };

            DataTable result = db.ExecuteQuery(sql, parameters);

            if (result.Rows.Count == 0)
            {
                lblError.Text    = "Invalid username or password. Please try again.";
                lblError.Visible = true;
                txtPassword.Clear();
                return;
            }

            // Store session details
            DataRow row = result.Rows[0];
            SessionManager.UserID   = Convert.ToInt32(row["UserID"]);
            SessionManager.Username = row["Username"].ToString();
            SessionManager.Role     = row["Role"].ToString();
            SessionManager.FullName = row["FullName"].ToString();

            // Load role-specific ID (TrainerID, LecturerID, StudentID)
            LoadRoleID(SessionManager.UserID, SessionManager.Role);

            // Open correct dashboard based on role
            OpenDashboard(SessionManager.Role);
        }

        // --------------------------------------------------------
        // LoadRoleID: gets the role-specific ID for the user
        // --------------------------------------------------------
        private void LoadRoleID(int userID, string role)
        {
            string sql = "";

            if (role == "Trainer")
                sql = "SELECT TrainerID FROM Trainers WHERE UserID = @UserID";
            else if (role == "Lecturer")
                sql = "SELECT LecturerID FROM Lecturers WHERE UserID = @UserID";
            else if (role == "Student")
                sql = "SELECT StudentID FROM Students WHERE UserID = @UserID";
            else
            {
                SessionManager.RoleID = 0; // Admin has no separate role table
                return;
            }

            SqlParameter[] p = { new SqlParameter("@UserID", userID) };
            object result = db.ExecuteScalar(sql, p);

            if (result != null)
                SessionManager.RoleID = Convert.ToInt32(result);
        }

        // --------------------------------------------------------
        // OpenDashboard: hides login, shows correct dashboard
        // --------------------------------------------------------
        private void OpenDashboard(string role)
        {
            this.Hide();

            if (role == "Admin")
            {
                AdminDashboard admin = new AdminDashboard();
                admin.FormClosed += (s, args) => this.Close();
                admin.Show();
            }
            else if (role == "Trainer")
            {
                TrainerDashboard trainer = new TrainerDashboard();
                trainer.FormClosed += (s, args) => this.Close();
                trainer.Show();
            }
            else if (role == "Lecturer")
            {
                LecturerDashboard lecturer = new LecturerDashboard();
                lecturer.FormClosed += (s, args) => this.Close();
                lecturer.Show();
            }
            else if (role == "Student")
            {
                StudentDashboard student = new StudentDashboard();
                student.FormClosed += (s, args) => this.Close();
                student.Show();
            }
        }

        // --------------------------------------------------------
        // txtPassword_KeyPress: allow pressing Enter to login
        // --------------------------------------------------------
        private void txtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
                btnLogin_Click(sender, e);
        }
    }
}
