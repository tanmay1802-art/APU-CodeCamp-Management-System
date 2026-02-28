// ================================================================
// FORM    : TrainerDashboard
// FILE    : TrainerDashboard.cs
// PURPOSE : Main screen for Trainer role.
//           Features:
//             a. Add class information
//             b. Update and delete class information
//             c. View list of students enrolled and paid
//             d. Send feedback to Administrator
//             e. Update own profile
// ================================================================

using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace CodeCampSystem
{
    public partial class TrainerDashboard : Form
    {
        DatabaseHelper db = new DatabaseHelper();

        public TrainerDashboard()
        {
            InitializeComponent();
            this.Text       = "APU CodeCamp - Trainer Dashboard";
            lblWelcome.Text = "Welcome, " + SessionManager.FullName;
        }

        private void TrainerDashboard_Load(object sender, EventArgs e)
        {
            LoadModuleCombo();
            LoadClasses();
            LoadEnrolledStudents();
            LoadTrainerProfile();
        }

        // ============================================================
        // TAB A & B: CLASS MANAGEMENT (Add / Update / Delete)
        // Controls needed in tabClasses:
        //   dgvClasses     - DataGridView
        //   cboModule      - ComboBox
        //   cboLevel       - ComboBox (Beginner/Intermediate/Advance)
        //   txtFee         - TextBox
        //   txtSchedule    - TextBox
        //   dtpStartDate   - DateTimePicker
        //   btnAddClass    - Button
        //   btnUpdateClass - Button
        //   btnDeleteClass - Button
        // ============================================================

        private void LoadModuleCombo()
        {
            // Trainer can only add class for their assigned module
            string sql = @"SELECT m.ModuleID,
                                  m.ModuleName + ' (' + m.ModuleCode + ')' AS Display
                           FROM TrainerAssignments ta
                           JOIN Modules m ON ta.ModuleID = m.ModuleID
                           WHERE ta.TrainerID = @TrainerID AND ta.IsActive = 1";

            SqlParameter[] p = { new SqlParameter("@TrainerID", SessionManager.RoleID) };
            DataTable dt = db.ExecuteQuery(sql, p);

            cboModule.DisplayMember = "Display";
            cboModule.ValueMember   = "ModuleID";
            cboModule.DataSource    = dt;

            cboLevel.Items.Clear();
            cboLevel.Items.Add("Beginner");
            cboLevel.Items.Add("Intermediate");
            cboLevel.Items.Add("Advance");
        }

        private void LoadClasses()
        {
            string sql = @"SELECT c.ClassID, m.ModuleName, c.ClassLevel,
                                  'RM ' + CAST(c.Fee AS VARCHAR) AS Fee,
                                  c.Schedule,
                                  CONVERT(VARCHAR, c.StartDate, 103) AS StartDate,
                                  CONVERT(VARCHAR, c.EndDate, 103)   AS EndDate,
                                  c.MaxStudents,
                                  CASE c.IsActive WHEN 1 THEN 'Active' ELSE 'Closed' END AS Status
                           FROM Classes c
                           JOIN Modules m ON c.ModuleID = m.ModuleID
                           WHERE c.TrainerID = @TrainerID
                           ORDER BY c.StartDate DESC";

            SqlParameter[] p = { new SqlParameter("@TrainerID", SessionManager.RoleID) };
            dgvClasses.DataSource = db.ExecuteQuery(sql, p);
        }

        private void btnAddClass_Click(object sender, EventArgs e)
        {
            if (cboModule.SelectedItem == null || cboLevel.SelectedItem == null ||
                txtFee.Text.Trim() == "" || txtSchedule.Text.Trim() == "")
            {
                MessageBox.Show("Please fill in all class details.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate fee is a positive number
            decimal fee;
            if (!decimal.TryParse(txtFee.Text.Trim(), out fee) || fee <= 0)
            {
                MessageBox.Show("Please enter a valid fee amount (e.g. 150.00).",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int    moduleID = Convert.ToInt32(cboModule.SelectedValue);
            string level    = cboLevel.SelectedItem.ToString();

            // End date = start date + 28 days (4 weeks)
            DateTime startDate = dtpStartDate.Value.Date;
            DateTime endDate   = startDate.AddDays(28);

            string sql = @"INSERT INTO Classes
                               (TrainerID, ModuleID, ClassLevel, Fee, Schedule, StartDate, EndDate)
                           VALUES
                               (@TrainerID, @ModuleID, @Level, @Fee, @Schedule, @Start, @End)";

            SqlParameter[] p = {
                new SqlParameter("@TrainerID", SessionManager.RoleID),
                new SqlParameter("@ModuleID",  moduleID),
                new SqlParameter("@Level",     level),
                new SqlParameter("@Fee",       fee),
                new SqlParameter("@Schedule",  txtSchedule.Text.Trim()),
                new SqlParameter("@Start",     startDate),
                new SqlParameter("@End",       endDate)
            };

            int rows = db.ExecuteNonQuery(sql, p);

            if (rows > 0)
            {
                MessageBox.Show("Class added successfully! End date: " + endDate.ToString("dd/MM/yyyy"),
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadClasses();
                ClearClassInputs();
            }
        }

        private void btnUpdateClass_Click(object sender, EventArgs e)
        {
            if (dgvClasses.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a class to update.",
                    "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtFee.Text.Trim() == "" || txtSchedule.Text.Trim() == "")
            {
                MessageBox.Show("Fee and Schedule cannot be empty.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal fee;
            if (!decimal.TryParse(txtFee.Text.Trim(), out fee) || fee <= 0)
            {
                MessageBox.Show("Please enter a valid fee amount.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int classID = Convert.ToInt32(dgvClasses.SelectedRows[0].Cells["ClassID"].Value);

            string sql = @"UPDATE Classes
                           SET Fee = @Fee, Schedule = @Schedule
                           WHERE ClassID = @ClassID AND TrainerID = @TrainerID";

            SqlParameter[] p = {
                new SqlParameter("@Fee",       fee),
                new SqlParameter("@Schedule",  txtSchedule.Text.Trim()),
                new SqlParameter("@ClassID",   classID),
                new SqlParameter("@TrainerID", SessionManager.RoleID)
            };

            int rows = db.ExecuteNonQuery(sql, p);

            if (rows > 0)
            {
                MessageBox.Show("Class updated successfully!",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadClasses();
            }
        }

        private void btnDeleteClass_Click(object sender, EventArgs e)
        {
            if (dgvClasses.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a class to delete.",
                    "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int    classID = Convert.ToInt32(dgvClasses.SelectedRows[0].Cells["ClassID"].Value);
            string module  = dgvClasses.SelectedRows[0].Cells["ModuleName"].Value.ToString();

            // Check if students are enrolled
            string checkSql = @"SELECT COUNT(*) FROM Enrolments
                                 WHERE ClassID = @ClassID AND IsCompleted = 0";
            SqlParameter[] cp = { new SqlParameter("@ClassID", classID) };
            int enrolled = Convert.ToInt32(db.ExecuteScalar(checkSql, cp));

            if (enrolled > 0)
            {
                MessageBox.Show("Cannot delete: " + enrolled + " student(s) are currently enrolled.",
                    "Cannot Delete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                "Delete class: " + module + "?",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            // Soft delete
            string sql = "UPDATE Classes SET IsActive = 0 WHERE ClassID = @ClassID";
            SqlParameter[] p = { new SqlParameter("@ClassID", classID) };
            db.ExecuteNonQuery(sql, p);

            MessageBox.Show("Class deleted successfully.",
                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadClasses();
        }

        private void dgvClasses_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvClasses.SelectedRows.Count == 0) return;
            DataGridViewRow row = dgvClasses.SelectedRows[0];

            // Populate inputs from selected row
            string feeStr = row.Cells["Fee"].Value.ToString().Replace("RM ", "");
            txtFee.Text      = feeStr;
            txtSchedule.Text = row.Cells["Schedule"].Value.ToString();
        }

        private void ClearClassInputs()
        {
            txtFee.Clear();
            txtSchedule.Clear();
            cboLevel.SelectedIndex = 0;
        }

        // ============================================================
        // TAB C: VIEW ENROLLED STUDENTS (PAID ONLY)
        // Controls needed in tabStudents:
        //   dgvStudents - DataGridView
        //   cboFilterClass - ComboBox (filter by class)
        // ============================================================

        private void LoadEnrolledStudents()
        {
            string sql = @"SELECT s.TPNumber, u.FullName AS StudentName,
                                  m.ModuleName, c.ClassLevel, c.Schedule,
                                  e.MonthOfEnrol, e.PaymentStatus,
                                  CONVERT(VARCHAR, e.EnrolmentDate, 103) AS EnrolledOn
                           FROM Enrolments e
                           JOIN Students s  ON e.StudentID = s.StudentID
                           JOIN Users    u  ON s.UserID    = u.UserID
                           JOIN Classes  c  ON e.ClassID   = c.ClassID
                           JOIN Modules  m  ON c.ModuleID  = m.ModuleID
                           WHERE c.TrainerID = @TrainerID
                             AND e.PaymentStatus = 'Paid'
                           ORDER BY e.EnrolmentDate DESC";

            SqlParameter[] p = { new SqlParameter("@TrainerID", SessionManager.RoleID) };
            dgvStudents.DataSource = db.ExecuteQuery(sql, p);
        }

        // ============================================================
        // TAB D: SEND FEEDBACK TO ADMIN
        // Controls needed in tabFeedback:
        //   txtFeedbackSubject - TextBox
        //   txtFeedbackMessage - TextBox (MultiLine)
        //   btnSendFeedback    - Button
        // ============================================================

        private void btnSendFeedback_Click(object sender, EventArgs e)
        {
            if (txtFeedbackSubject.Text.Trim() == "" || txtFeedbackMessage.Text.Trim() == "")
            {
                MessageBox.Show("Please enter a subject and message.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string sql = @"INSERT INTO Feedback (TrainerID, Subject, Message)
                           VALUES (@TrainerID, @Subject, @Message)";

            SqlParameter[] p = {
                new SqlParameter("@TrainerID", SessionManager.RoleID),
                new SqlParameter("@Subject",   txtFeedbackSubject.Text.Trim()),
                new SqlParameter("@Message",   txtFeedbackMessage.Text.Trim())
            };

            int rows = db.ExecuteNonQuery(sql, p);

            if (rows > 0)
            {
                MessageBox.Show("Feedback sent to Administrator successfully!",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtFeedbackSubject.Clear();
                txtFeedbackMessage.Clear();
            }
        }

        // ============================================================
        // TAB E: UPDATE PROFILE
        // Controls needed in tabProfile:
        //   txtProfileName  - TextBox
        //   txtProfileEmail - TextBox
        //   txtProfilePhone - TextBox
        //   txtProfileAddr  - TextBox
        //   txtOldPass      - TextBox
        //   txtNewPass      - TextBox
        //   btnSaveProfile  - Button
        // ============================================================

        private void LoadTrainerProfile()
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
                MessageBox.Show("Name and email cannot be empty.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!txtProfileEmail.Text.Contains("@") || !txtProfileEmail.Text.Contains("."))
            {
                MessageBox.Show("Please enter a valid email address.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string sql = @"UPDATE Users
                           SET FullName = @Name, Email = @Email,
                               Phone = @Phone, Address = @Addr
                           WHERE UserID = @ID";

            SqlParameter[] p = {
                new SqlParameter("@Name",  txtProfileName.Text.Trim()),
                new SqlParameter("@Email", txtProfileEmail.Text.Trim()),
                new SqlParameter("@Phone", txtProfilePhone.Text.Trim()),
                new SqlParameter("@Addr",  txtProfileAddr.Text.Trim()),
                new SqlParameter("@ID",    SessionManager.UserID)
            };

            db.ExecuteNonQuery(sql, p);

            if (txtOldPass.Text.Trim() != "" && txtNewPass.Text.Trim() != "")
            {
                string checkSql = "SELECT COUNT(*) FROM Users WHERE UserID = @ID AND Password = @Old";
                SqlParameter[] cp = {
                    new SqlParameter("@ID",  SessionManager.UserID),
                    new SqlParameter("@Old", txtOldPass.Text.Trim())
                };

                if (Convert.ToInt32(db.ExecuteScalar(checkSql, cp)) == 0)
                {
                    MessageBox.Show("Old password is incorrect.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string passSQL = "UPDATE Users SET Password = @New WHERE UserID = @ID";
                SqlParameter[] pp = {
                    new SqlParameter("@New", txtNewPass.Text.Trim()),
                    new SqlParameter("@ID",  SessionManager.UserID)
                };
                db.ExecuteNonQuery(passSQL, pp);
            }

            MessageBox.Show("Profile updated successfully!",
                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
