// ================================================================
// FORM    : LecturerDashboard
// FILE    : LecturerDashboard.cs
// PURPOSE : Main screen for Lecturer role.
//           Features:
//             a. Register and enrol student to modules
//             b. Approve student requests
//             c. Delete students who completed coaching
//             d. View students list (filter by level/module)
//             e. Update own profile
// ================================================================

using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace CodeCampSystem
{
    public partial class LecturerDashboard : Form
    {
        DatabaseHelper db = new DatabaseHelper();

        public LecturerDashboard()
        {
            InitializeComponent();
            this.Text       = "APU CodeCamp - Lecturer Dashboard";
            lblWelcome.Text = "Welcome, " + SessionManager.FullName;
        }

        private void LecturerDashboard_Load(object sender, EventArgs e)
        {
            LoadClassCombo();
            LoadStudentList();
            LoadPendingRequests();
            LoadLecturerProfile();
        }

        // ============================================================
        // TAB A: REGISTER AND ENROL STUDENT
        // Controls needed in tabEnrol:
        //   txtStudentName  - TextBox
        //   txtTPNumber     - TextBox
        //   txtStudentEmail - TextBox
        //   txtStudentPhone - TextBox
        //   txtStudentAddr  - TextBox
        //   cboStudyLevel   - ComboBox (Foundation/Level1/Level2/Level3)
        //   txtStudentUser  - TextBox (username for login)
        //   txtStudentPass  - TextBox
        //   cboEnrolClass   - ComboBox (available classes)
        //   cboEnrolMonth   - ComboBox (month of enrolment)
        //   btnEnrolStudent - Button
        // ============================================================

        private void LoadClassCombo()
        {
            string sql = @"SELECT c.ClassID,
                                  m.ModuleName + ' - ' + c.ClassLevel +
                                  ' (RM' + CAST(c.Fee AS VARCHAR) + ')' AS Display
                           FROM Classes c
                           JOIN Modules m ON c.ModuleID = m.ModuleID
                           WHERE c.IsActive = 1
                           ORDER BY m.ModuleName";

            DataTable dt = db.ExecuteQuery(sql);
            cboEnrolClass.DisplayMember = "Display";
            cboEnrolClass.ValueMember   = "ClassID";
            cboEnrolClass.DataSource    = dt;

            // Study levels
            cboStudyLevel.Items.Clear();
            cboStudyLevel.Items.Add("Foundation");
            cboStudyLevel.Items.Add("Level1");
            cboStudyLevel.Items.Add("Level2");
            cboStudyLevel.Items.Add("Level3");
            cboStudyLevel.SelectedIndex = 0;

            // Enrolment month (current + next 5 months)
            cboEnrolMonth.Items.Clear();
            for (int i = 0; i < 6; i++)
            {
                DateTime m = DateTime.Now.AddMonths(i);
                cboEnrolMonth.Items.Add(m.ToString("MMMM yyyy"));
            }
            cboEnrolMonth.SelectedIndex = 0;

            // Filter combos for Tab D
            LoadFilterCombos();
        }

        private void btnEnrolStudent_Click(object sender, EventArgs e)
        {
            // Validate all required fields
            if (txtStudentName.Text.Trim() == "" || txtTPNumber.Text.Trim() == "" ||
                txtStudentEmail.Text.Trim() == "" || txtStudentPhone.Text.Trim() == "" ||
                txtStudentUser.Text.Trim() == "" || txtStudentPass.Text.Trim() == "")
            {
                MessageBox.Show("Please fill in all student details.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!txtStudentEmail.Text.Contains("@") || !txtStudentEmail.Text.Contains("."))
            {
                MessageBox.Show("Please enter a valid email address.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtTPNumber.Text.Trim().Length < 5)
            {
                MessageBox.Show("Please enter a valid TP Number (e.g. TP055001).",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cboEnrolClass.SelectedItem == null)
            {
                MessageBox.Show("Please select a class to enrol the student in.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check TP number not already used
            string checkTP = "SELECT COUNT(*) FROM Students WHERE TPNumber = @TP";
            SqlParameter[] checkP = { new SqlParameter("@TP", txtTPNumber.Text.Trim().ToUpper()) };
            int tpExists = Convert.ToInt32(db.ExecuteScalar(checkTP, checkP));
            if (tpExists > 0)
            {
                MessageBox.Show("A student with this TP Number already exists.",
                    "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check username not taken
            string checkUser = "SELECT COUNT(*) FROM Users WHERE Username = @U";
            SqlParameter[] checkU = { new SqlParameter("@U", txtStudentUser.Text.Trim()) };
            if (Convert.ToInt32(db.ExecuteScalar(checkUser, checkU)) > 0)
            {
                MessageBox.Show("Username already exists.",
                    "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 1. Create User account
            string insertUser = @"INSERT INTO Users (Username, Password, Role, FullName, Email, Phone, Address)
                                  OUTPUT INSERTED.UserID
                                  VALUES (@U, @P, 'Student', @Name, @Email, @Phone, @Addr)";

            SqlParameter[] userP = {
                new SqlParameter("@U",     txtStudentUser.Text.Trim()),
                new SqlParameter("@P",     txtStudentPass.Text.Trim()),
                new SqlParameter("@Name",  txtStudentName.Text.Trim()),
                new SqlParameter("@Email", txtStudentEmail.Text.Trim()),
                new SqlParameter("@Phone", txtStudentPhone.Text.Trim()),
                new SqlParameter("@Addr",  txtStudentAddr.Text.Trim())
            };

            object newUserID = db.ExecuteScalar(insertUser, userP);
            if (newUserID == null) return;

            // 2. Create Student record
            string insertStudent = @"INSERT INTO Students (UserID, TPNumber, StudyLevel)
                                     OUTPUT INSERTED.StudentID
                                     VALUES (@UID, @TP, @Level)";

            SqlParameter[] studentP = {
                new SqlParameter("@UID",   Convert.ToInt32(newUserID)),
                new SqlParameter("@TP",    txtTPNumber.Text.Trim().ToUpper()),
                new SqlParameter("@Level", cboStudyLevel.SelectedItem.ToString())
            };

            object newStudentID = db.ExecuteScalar(insertStudent, studentP);
            if (newStudentID == null) return;

            // 3. Create Enrolment record
            int classID = Convert.ToInt32(cboEnrolClass.SelectedValue);
            string insertEnrol = @"INSERT INTO Enrolments
                                       (StudentID, ClassID, LecturerID, MonthOfEnrol)
                                   VALUES (@SID, @CID, @LID, @Month)";

            SqlParameter[] enrolP = {
                new SqlParameter("@SID",   Convert.ToInt32(newStudentID)),
                new SqlParameter("@CID",   classID),
                new SqlParameter("@LID",   SessionManager.RoleID),
                new SqlParameter("@Month", cboEnrolMonth.SelectedItem.ToString())
            };

            db.ExecuteNonQuery(insertEnrol, enrolP);

            MessageBox.Show("Student registered and enrolled successfully!\n" +
                "Student can now log in with: " + txtStudentUser.Text.Trim(),
                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            ClearEnrolInputs();
            LoadStudentList();
        }

        private void ClearEnrolInputs()
        {
            txtStudentName.Clear(); txtTPNumber.Clear();
            txtStudentEmail.Clear(); txtStudentPhone.Clear();
            txtStudentAddr.Clear(); txtStudentUser.Clear();
            txtStudentPass.Clear();
            cboStudyLevel.SelectedIndex = 0;
        }

        // ============================================================
        // TAB B: APPROVE STUDENT REQUESTS
        // Controls needed in tabRequests:
        //   dgvRequests   - DataGridView
        //   btnApprove    - Button
        //   btnReject     - Button
        // ============================================================

        private void LoadPendingRequests()
        {
            string sql = @"SELECT r.RequestID, s.TPNumber, u.FullName AS Student,
                                  m.ModuleName, c.ClassLevel, c.Schedule,
                                  'RM ' + CAST(c.Fee AS VARCHAR) AS Fee,
                                  CONVERT(VARCHAR, r.RequestDate, 103) AS RequestDate
                           FROM EnrolmentRequests r
                           JOIN Students s ON r.StudentID = s.StudentID
                           JOIN Users    u ON s.UserID    = u.UserID
                           JOIN Classes  c ON r.ClassID   = c.ClassID
                           JOIN Modules  m ON c.ModuleID  = m.ModuleID
                           WHERE r.Status = 'Pending'
                           ORDER BY r.RequestDate";

            dgvRequests.DataSource = db.ExecuteQuery(sql);
        }

        private void btnApprove_Click(object sender, EventArgs e)
        {
            if (dgvRequests.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a request to approve.",
                    "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int requestID = Convert.ToInt32(dgvRequests.SelectedRows[0].Cells["RequestID"].Value);

            // Get request details
            string getSql = "SELECT StudentID, ClassID FROM EnrolmentRequests WHERE RequestID = @ID";
            SqlParameter[] gp = { new SqlParameter("@ID", requestID) };
            DataTable dt = db.ExecuteQuery(getSql, gp);
            if (dt.Rows.Count == 0) return;

            int studentID = Convert.ToInt32(dt.Rows[0]["StudentID"]);
            int classID   = Convert.ToInt32(dt.Rows[0]["ClassID"]);

            // Create enrolment
            string insertSql = @"INSERT INTO Enrolments (StudentID, ClassID, LecturerID, MonthOfEnrol)
                                  VALUES (@SID, @CID, @LID, @Month)";
            SqlParameter[] ip = {
                new SqlParameter("@SID",   studentID),
                new SqlParameter("@CID",   classID),
                new SqlParameter("@LID",   SessionManager.RoleID),
                new SqlParameter("@Month", DateTime.Now.ToString("MMMM yyyy"))
            };
            db.ExecuteNonQuery(insertSql, ip);

            // Mark request as Approved
            string updateSql = "UPDATE EnrolmentRequests SET Status = 'Approved' WHERE RequestID = @ID";
            SqlParameter[] up = { new SqlParameter("@ID", requestID) };
            db.ExecuteNonQuery(updateSql, up);

            MessageBox.Show("Request approved. Student has been enrolled.",
                "Approved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadPendingRequests();
            LoadStudentList();
        }

        private void btnReject_Click(object sender, EventArgs e)
        {
            if (dgvRequests.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a request to reject.",
                    "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int requestID = Convert.ToInt32(dgvRequests.SelectedRows[0].Cells["RequestID"].Value);

            string sql = "UPDATE EnrolmentRequests SET Status = 'Rejected' WHERE RequestID = @ID";
            SqlParameter[] p = { new SqlParameter("@ID", requestID) };
            db.ExecuteNonQuery(sql, p);

            MessageBox.Show("Request has been rejected.",
                "Rejected", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadPendingRequests();
        }

        // ============================================================
        // TAB C & D: VIEW STUDENTS + DELETE COMPLETED
        // Controls needed in tabStudents:
        //   dgvStudentList   - DataGridView
        //   cboFilterLevel   - ComboBox
        //   cboFilterModule  - ComboBox
        //   btnFilterStudents- Button
        //   btnMarkComplete  - Button
        //   btnDeleteStudent - Button
        // ============================================================

        private void LoadFilterCombos()
        {
            cboFilterLevel.Items.Clear();
            cboFilterLevel.Items.Add("All Levels");
            cboFilterLevel.Items.Add("Foundation");
            cboFilterLevel.Items.Add("Level1");
            cboFilterLevel.Items.Add("Level2");
            cboFilterLevel.Items.Add("Level3");
            cboFilterLevel.SelectedIndex = 0;

            string modSql = "SELECT 'All Modules' AS ModuleName UNION SELECT ModuleName FROM Modules";
            DataTable modDt = db.ExecuteQuery(modSql);
            cboFilterModule.DisplayMember = "ModuleName";
            cboFilterModule.DataSource    = modDt;
        }

        private void LoadStudentList()
        {
            string sql = @"SELECT e.EnrolmentID, s.TPNumber, u.FullName AS Student,
                                  st.StudyLevel, m.ModuleName, c.ClassLevel,
                                  e.MonthOfEnrol, e.PaymentStatus,
                                  CASE e.IsCompleted WHEN 1 THEN 'Completed' ELSE 'Active' END AS CoachingStatus
                           FROM Enrolments e
                           JOIN Students s ON e.StudentID  = s.StudentID
                           JOIN Users    u ON s.UserID     = u.UserID
                           JOIN Students st ON e.StudentID = st.StudentID
                           JOIN Classes  c ON e.ClassID    = c.ClassID
                           JOIN Modules  m ON c.ModuleID   = m.ModuleID
                           WHERE e.LecturerID = @LecturerID
                           ORDER BY e.EnrolmentDate DESC";

            SqlParameter[] p = { new SqlParameter("@LecturerID", SessionManager.RoleID) };
            dgvStudentList.DataSource = db.ExecuteQuery(sql, p);
        }

        private void btnFilterStudents_Click(object sender, EventArgs e)
        {
            string level  = cboFilterLevel.SelectedItem.ToString();
            string module = cboFilterModule.SelectedItem.ToString();

            string sql = @"SELECT e.EnrolmentID, s.TPNumber, u.FullName AS Student,
                                  st.StudyLevel, m.ModuleName, c.ClassLevel,
                                  e.MonthOfEnrol, e.PaymentStatus,
                                  CASE e.IsCompleted WHEN 1 THEN 'Completed' ELSE 'Active' END AS CoachingStatus
                           FROM Enrolments e
                           JOIN Students s  ON e.StudentID  = s.StudentID
                           JOIN Users    u  ON s.UserID     = u.UserID
                           JOIN Students st ON e.StudentID  = st.StudentID
                           JOIN Classes  c  ON e.ClassID    = c.ClassID
                           JOIN Modules  m  ON c.ModuleID   = m.ModuleID
                           WHERE e.LecturerID = @LecturerID
                             AND (@Level  = 'All Levels'  OR st.StudyLevel = @Level)
                             AND (@Module = 'All Modules' OR m.ModuleName  = @Module)
                           ORDER BY e.EnrolmentDate DESC";

            SqlParameter[] p = {
                new SqlParameter("@LecturerID", SessionManager.RoleID),
                new SqlParameter("@Level",      level),
                new SqlParameter("@Module",     module)
            };

            dgvStudentList.DataSource = db.ExecuteQuery(sql, p);
        }

        private void btnMarkComplete_Click(object sender, EventArgs e)
        {
            if (dgvStudentList.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a student enrolment to mark complete.",
                    "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int enrolID = Convert.ToInt32(dgvStudentList.SelectedRows[0].Cells["EnrolmentID"].Value);

            string sql = "UPDATE Enrolments SET IsCompleted = 1 WHERE EnrolmentID = @ID";
            SqlParameter[] p = { new SqlParameter("@ID", enrolID) };
            db.ExecuteNonQuery(sql, p);

            MessageBox.Show("Student coaching marked as completed.",
                "Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadStudentList();
        }

        private void btnDeleteStudent_Click(object sender, EventArgs e)
        {
            if (dgvStudentList.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a student to delete.",
                    "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string status = dgvStudentList.SelectedRows[0].Cells["CoachingStatus"].Value.ToString();
            if (status != "Completed")
            {
                MessageBox.Show("You can only delete students who have COMPLETED their coaching.",
                    "Cannot Delete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int enrolID  = Convert.ToInt32(dgvStudentList.SelectedRows[0].Cells["EnrolmentID"].Value);
            string name  = dgvStudentList.SelectedRows[0].Cells["Student"].Value.ToString();

            DialogResult confirm = MessageBox.Show(
                "Delete student: " + name + "?",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            string sql = "DELETE FROM Enrolments WHERE EnrolmentID = @ID";
            SqlParameter[] p = { new SqlParameter("@ID", enrolID) };
            db.ExecuteNonQuery(sql, p);

            MessageBox.Show("Student enrolment deleted.",
                "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadStudentList();
        }

        // ============================================================
        // TAB E: UPDATE PROFILE (same pattern as Admin)
        // ============================================================
        private void LoadLecturerProfile()
        {
            string sql = "SELECT FullName, Email, Phone, Address FROM Users WHERE UserID = @ID";
            SqlParameter[] p = { new SqlParameter("@ID", SessionManager.UserID) };
            DataTable dt = db.ExecuteQuery(sql, p);

            if (dt.Rows.Count > 0)
            {
                txtProfileName.Text  = dt.Rows[0]["FullName"].ToString();
                txtProfileEmail.Text = dt.Rows[0]["Email"].ToString();
                txtProfilePhone.Text = dt.Rows[0]["Phone"].ToString();
                txtProfileAddr.Text  = dt.Rows[0]["Address"].ToString();
            }
        }

        private void btnSaveProfile_Click(object sender, EventArgs e)
        {
            if (txtProfileName.Text.Trim() == "" || txtProfileEmail.Text.Trim() == "")
            {
                MessageBox.Show("Name and email are required.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!txtProfileEmail.Text.Contains("@"))
            {
                MessageBox.Show("Invalid email format.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string sql = @"UPDATE Users SET FullName=@Name, Email=@Email,
                           Phone=@Phone, Address=@Addr WHERE UserID=@ID";
            SqlParameter[] p = {
                new SqlParameter("@Name",  txtProfileName.Text.Trim()),
                new SqlParameter("@Email", txtProfileEmail.Text.Trim()),
                new SqlParameter("@Phone", txtProfilePhone.Text.Trim()),
                new SqlParameter("@Addr",  txtProfileAddr.Text.Trim()),
                new SqlParameter("@ID",    SessionManager.UserID)
            };
            db.ExecuteNonQuery(sql, p);

            MessageBox.Show("Profile updated!", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            SessionManager.Clear();
            LoginForm login = new LoginForm();
            login.Show();
            this.Close();
        }
    }
}
