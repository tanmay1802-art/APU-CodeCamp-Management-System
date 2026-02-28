// ================================================================
// FORM    : AdminDashboard
// FILE    : AdminDashboard.cs
// PURPOSE : Main screen for Administrator role.
//           Features:
//             a. Register new trainer / Remove trainer
//             b. Assign trainer to module and level
//             c. View feedback from trainers
//             d. View monthly income report
//             e. Update own profile
//
// CONTROLS NEEDED (add in Designer):
//   lblWelcome     - Label   (shows "Welcome, [name]")
//   tabControl1    - TabControl with 5 tabs:
//     tabTrainers  - Tab for trainer management
//     tabAssign    - Tab for trainer assignments
//     tabFeedback  - Tab for viewing feedback
//     tabIncome    - Tab for income report
//     tabProfile   - Tab for profile update
//   btnLogout      - Button
// ================================================================

using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace CodeCampSystem
{
    public partial class AdminDashboard : Form
    {
        DatabaseHelper db = new DatabaseHelper();

        public AdminDashboard()
        {
            InitializeComponent();
            this.Text      = "APU CodeCamp - Admin Dashboard";
            lblWelcome.Text = "Welcome, " + SessionManager.FullName;
        }

        private void AdminDashboard_Load(object sender, EventArgs e)
        {
            LoadTrainers();
            LoadModulesCombo();
            LoadFeedback();
            LoadIncomeReport();
            LoadAdminProfile();
        }

        // ============================================================
        // TAB A: TRAINER MANAGEMENT
        // Controls needed in tabTrainers:
        //   dgvTrainers      - DataGridView  (shows trainer list)
        //   txtTrainerName   - TextBox
        //   txtTrainerEmail  - TextBox
        //   txtTrainerPhone  - TextBox
        //   txtTrainerAddr   - TextBox
        //   txtTrainerSpec   - TextBox
        //   txtTrainerQual   - TextBox
        //   txtTrainerUser   - TextBox  (username for login)
        //   txtTrainerPass   - TextBox  (password for login)
        //   btnAddTrainer    - Button
        //   btnRemoveTrainer - Button
        // ============================================================

        private void LoadTrainers()
        {
            string sql = @"SELECT t.TrainerID, u.FullName, u.Email, u.Phone,
                                  t.Specialization, t.Qualification,
                                  CASE t.IsActive WHEN 1 THEN 'Active' ELSE 'Inactive' END AS Status
                           FROM Trainers t
                           JOIN Users u ON t.UserID = u.UserID
                           ORDER BY u.FullName";

            DataTable dt = db.ExecuteQuery(sql);
            dgvTrainers.DataSource = dt;
        }

        private void btnAddTrainer_Click(object sender, EventArgs e)
        {
            // Validate all inputs
            if (txtTrainerName.Text.Trim() == "" ||
                txtTrainerEmail.Text.Trim() == "" ||
                txtTrainerPhone.Text.Trim() == "" ||
                txtTrainerSpec.Text.Trim() == "" ||
                txtTrainerQual.Text.Trim() == "" ||
                txtTrainerUser.Text.Trim() == "" ||
                txtTrainerPass.Text.Trim() == "")
            {
                MessageBox.Show("Please fill in all trainer details.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate email format
            if (!txtTrainerEmail.Text.Contains("@") || !txtTrainerEmail.Text.Contains("."))
            {
                MessageBox.Show("Please enter a valid email address.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check username not already taken
            string checkSql = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
            SqlParameter[] checkP = { new SqlParameter("@Username", txtTrainerUser.Text.Trim()) };
            int count = Convert.ToInt32(db.ExecuteScalar(checkSql, checkP));
            if (count > 0)
            {
                MessageBox.Show("Username already exists. Choose a different username.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Insert into Users table
            string insertUser = @"INSERT INTO Users (Username, Password, Role, FullName, Email, Phone, Address)
                                  OUTPUT INSERTED.UserID
                                  VALUES (@Username, @Password, 'Trainer', @FullName, @Email, @Phone, @Address)";

            SqlParameter[] userParams = {
                new SqlParameter("@Username", txtTrainerUser.Text.Trim()),
                new SqlParameter("@Password", txtTrainerPass.Text.Trim()),
                new SqlParameter("@FullName",  txtTrainerName.Text.Trim()),
                new SqlParameter("@Email",     txtTrainerEmail.Text.Trim()),
                new SqlParameter("@Phone",     txtTrainerPhone.Text.Trim()),
                new SqlParameter("@Address",   txtTrainerAddr.Text.Trim())
            };

            object newUserID = db.ExecuteScalar(insertUser, userParams);

            if (newUserID == null)
            {
                MessageBox.Show("Failed to register trainer. Please try again.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Insert into Trainers table
            string insertTrainer = @"INSERT INTO Trainers (UserID, Specialization, Qualification)
                                     VALUES (@UserID, @Spec, @Qual)";

            SqlParameter[] trainerParams = {
                new SqlParameter("@UserID", Convert.ToInt32(newUserID)),
                new SqlParameter("@Spec",   txtTrainerSpec.Text.Trim()),
                new SqlParameter("@Qual",   txtTrainerQual.Text.Trim())
            };

            db.ExecuteNonQuery(insertTrainer, trainerParams);

            MessageBox.Show("Trainer registered successfully!",
                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            ClearTrainerInputs();
            LoadTrainers();
        }

        private void btnRemoveTrainer_Click(object sender, EventArgs e)
        {
            if (dgvTrainers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a trainer to remove.",
                    "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int trainerID = Convert.ToInt32(dgvTrainers.SelectedRows[0].Cells["TrainerID"].Value);
            string name   = dgvTrainers.SelectedRows[0].Cells["FullName"].Value.ToString();

            DialogResult confirm = MessageBox.Show(
                "Are you sure you want to remove trainer: " + name + "?",
                "Confirm Remove", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            // Soft delete: set IsActive = 0
            string sql = "UPDATE Trainers SET IsActive = 0 WHERE TrainerID = @TrainerID";
            SqlParameter[] p = { new SqlParameter("@TrainerID", trainerID) };
            int rows = db.ExecuteNonQuery(sql, p);

            if (rows > 0)
            {
                MessageBox.Show("Trainer removed successfully.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadTrainers();
            }
        }

        private void ClearTrainerInputs()
        {
            txtTrainerName.Clear();
            txtTrainerEmail.Clear();
            txtTrainerPhone.Clear();
            txtTrainerAddr.Clear();
            txtTrainerSpec.Clear();
            txtTrainerQual.Clear();
            txtTrainerUser.Clear();
            txtTrainerPass.Clear();
        }

        // ============================================================
        // TAB B: TRAINER ASSIGNMENT
        // Controls needed in tabAssign:
        //   cboAssignTrainer - ComboBox  (list of active trainers)
        //   cboAssignModule  - ComboBox  (list of modules)
        //   cboAssignLevel   - ComboBox  (Beginner/Intermediate/Advance)
        //   btnAssign        - Button
        //   dgvAssignments   - DataGridView
        // ============================================================

        private void LoadModulesCombo()
        {
            string sql = "SELECT ModuleID, ModuleName + ' (' + ModuleCode + ')' AS Display FROM Modules";
            DataTable dt = db.ExecuteQuery(sql);
            cboAssignModule.DisplayMember = "Display";
            cboAssignModule.ValueMember   = "ModuleID";
            cboAssignModule.DataSource    = dt;

            // Load trainers combo
            string trainerSql = @"SELECT t.TrainerID,
                                         u.FullName + ' - ' + t.Specialization AS Display
                                  FROM Trainers t
                                  JOIN Users u ON t.UserID = u.UserID
                                  WHERE t.IsActive = 1";
            DataTable trainerDt = db.ExecuteQuery(trainerSql);
            cboAssignTrainer.DisplayMember = "Display";
            cboAssignTrainer.ValueMember   = "TrainerID";
            cboAssignTrainer.DataSource    = trainerDt;

            // Level combo
            cboAssignLevel.Items.Clear();
            cboAssignLevel.Items.Add("Beginner");
            cboAssignLevel.Items.Add("Intermediate");
            cboAssignLevel.Items.Add("Advance");
            cboAssignLevel.SelectedIndex = 0;

            LoadAssignments();
        }

        private void LoadAssignments()
        {
            string sql = @"SELECT ta.AssignmentID, u.FullName AS Trainer,
                                  m.ModuleName, ta.ClassLevel,
                                  CONVERT(VARCHAR, ta.AssignedDate, 103) AS AssignedOn,
                                  CASE ta.IsActive WHEN 1 THEN 'Active' ELSE 'Inactive' END AS Status
                           FROM TrainerAssignments ta
                           JOIN Trainers t ON ta.TrainerID = t.TrainerID
                           JOIN Users    u ON t.UserID     = u.UserID
                           JOIN Modules  m ON ta.ModuleID  = m.ModuleID
                           ORDER BY ta.AssignedDate DESC";

            dgvAssignments.DataSource = db.ExecuteQuery(sql);
        }

        private void btnAssign_Click(object sender, EventArgs e)
        {
            if (cboAssignTrainer.SelectedItem == null ||
                cboAssignModule.SelectedItem == null ||
                cboAssignLevel.SelectedItem == null)
            {
                MessageBox.Show("Please select trainer, module and level.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int    trainerID = Convert.ToInt32(cboAssignTrainer.SelectedValue);
            int    moduleID  = Convert.ToInt32(cboAssignModule.SelectedValue);
            string level     = cboAssignLevel.SelectedItem.ToString();

            // Check: trainer can only teach one module at a time
            string checkSql = @"SELECT COUNT(*) FROM TrainerAssignments
                                 WHERE TrainerID = @TrainerID AND IsActive = 1";
            SqlParameter[] checkP = { new SqlParameter("@TrainerID", trainerID) };
            int existing = Convert.ToInt32(db.ExecuteScalar(checkSql, checkP));

            if (existing > 0)
            {
                MessageBox.Show("This trainer is already assigned to a module. " +
                    "Remove the existing assignment first.",
                    "Assignment Conflict", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string sql = @"INSERT INTO TrainerAssignments (TrainerID, ModuleID, ClassLevel)
                           VALUES (@TrainerID, @ModuleID, @Level)";

            SqlParameter[] p = {
                new SqlParameter("@TrainerID", trainerID),
                new SqlParameter("@ModuleID",  moduleID),
                new SqlParameter("@Level",     level)
            };

            int rows = db.ExecuteNonQuery(sql, p);

            if (rows > 0)
            {
                MessageBox.Show("Trainer assigned successfully!",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadAssignments();
            }
        }

        // ============================================================
        // TAB C: VIEW FEEDBACK
        // Controls needed in tabFeedback:
        //   dgvFeedback  - DataGridView
        //   txtFeedbackMsg - TextBox (MultiLine, ReadOnly)
        // ============================================================

        private void LoadFeedback()
        {
            string sql = @"SELECT f.FeedbackID, u.FullName AS Trainer, f.Subject,
                                  CONVERT(VARCHAR, f.FeedbackDate, 103) AS Date,
                                  CASE f.IsRead WHEN 1 THEN 'Read' ELSE 'New' END AS Status
                           FROM Feedback f
                           JOIN Trainers t ON f.TrainerID = t.TrainerID
                           JOIN Users    u ON t.UserID    = u.UserID
                           ORDER BY f.FeedbackDate DESC";

            dgvFeedback.DataSource = db.ExecuteQuery(sql);
        }

        private void dgvFeedback_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvFeedback.SelectedRows.Count == 0) return;

            int feedbackID = Convert.ToInt32(dgvFeedback.SelectedRows[0].Cells["FeedbackID"].Value);

            string sql = "SELECT Message FROM Feedback WHERE FeedbackID = @ID";
            SqlParameter[] p = { new SqlParameter("@ID", feedbackID) };
            DataTable dt = db.ExecuteQuery(sql, p);

            if (dt.Rows.Count > 0)
                txtFeedbackMsg.Text = dt.Rows[0]["Message"].ToString();

            // Mark as read
            string markSql = "UPDATE Feedback SET IsRead = 1 WHERE FeedbackID = @ID";
            db.ExecuteNonQuery(markSql, p);
            LoadFeedback();
        }

        // ============================================================
        // TAB D: MONTHLY INCOME REPORT
        // Controls needed in tabIncome:
        //   dgvIncome - DataGridView
        //   btnRefreshIncome - Button
        // ============================================================

        private void LoadIncomeReport()
        {
            string sql = @"SELECT * FROM vw_MonthlyIncome ORDER BY Month DESC, TrainerName";
            dgvIncome.DataSource = db.ExecuteQuery(sql);
        }

        private void btnRefreshIncome_Click(object sender, EventArgs e)
        {
            LoadIncomeReport();
        }

        // ============================================================
        // TAB E: UPDATE OWN PROFILE
        // Controls needed in tabProfile:
        //   txtProfileName  - TextBox
        //   txtProfileEmail - TextBox
        //   txtProfilePhone - TextBox
        //   txtProfileAddr  - TextBox
        //   txtOldPass      - TextBox (PasswordChar = *)
        //   txtNewPass      - TextBox (PasswordChar = *)
        //   btnSaveProfile  - Button
        // ============================================================

        private void LoadAdminProfile()
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
            if (txtProfileName.Text.Trim() == "" ||
                txtProfileEmail.Text.Trim() == "" ||
                txtProfilePhone.Text.Trim() == "")
            {
                MessageBox.Show("Name, email and phone cannot be empty.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!txtProfileEmail.Text.Contains("@"))
            {
                MessageBox.Show("Please enter a valid email address.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Update profile
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

            // Change password if provided
            if (txtOldPass.Text.Trim() != "" && txtNewPass.Text.Trim() != "")
            {
                string checkSql = "SELECT COUNT(*) FROM Users WHERE UserID = @ID AND Password = @Old";
                SqlParameter[] cp = {
                    new SqlParameter("@ID",  SessionManager.UserID),
                    new SqlParameter("@Old", txtOldPass.Text.Trim())
                };
                int match = Convert.ToInt32(db.ExecuteScalar(checkSql, cp));

                if (match == 0)
                {
                    MessageBox.Show("Old password is incorrect.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string updatePass = "UPDATE Users SET Password = @New WHERE UserID = @ID";
                SqlParameter[] pp = {
                    new SqlParameter("@New", txtNewPass.Text.Trim()),
                    new SqlParameter("@ID",  SessionManager.UserID)
                };
                db.ExecuteNonQuery(updatePass, pp);
            }

            MessageBox.Show("Profile updated successfully!",
                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            SessionManager.FullName = txtProfileName.Text.Trim();
            lblWelcome.Text = "Welcome, " + SessionManager.FullName;
        }

        // ============================================================
        // LOGOUT
        // ============================================================
        private void btnLogout_Click(object sender, EventArgs e)
        {
            SessionManager.Clear();
            LoginForm login = new LoginForm();
            login.Show();
            this.Close();
        }
    }
}
