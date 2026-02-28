// ================================================================
// FORM    : StudentDashboard
// FILE    : StudentDashboard.cs
// PURPOSE : Main screen for Student role.
//           Features:
//             a. View schedule of enrolled sessions
//             b. Send request to lecturer to enrol in extra class
//             c. Cancel a pending request
//             d. View invoice and make payment
//             e. Update own profile
// ================================================================

using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace CodeCampSystem
{
    public partial class StudentDashboard : Form
    {
        DatabaseHelper db = new DatabaseHelper();

        public StudentDashboard()
        {
            InitializeComponent();
            this.Text       = "APU CodeCamp - Student Dashboard";
            lblWelcome.Text = "Welcome, " + SessionManager.FullName;
        }

        private void StudentDashboard_Load(object sender, EventArgs e)
        {
            LoadSchedule();
            LoadAvailableClasses();
            LoadMyRequests();
            LoadInvoice();
            LoadStudentProfile();
        }

        // ============================================================
        // TAB A: VIEW SCHEDULE
        // Controls needed in tabSchedule:
        //   dgvSchedule - DataGridView
        // ============================================================

        private void LoadSchedule()
        {
            string sql = @"SELECT m.ModuleName, c.ClassLevel, c.Schedule,
                                  CONVERT(VARCHAR, c.StartDate, 103) AS StartDate,
                                  CONVERT(VARCHAR, c.EndDate,   103) AS EndDate,
                                  u.FullName AS Trainer,
                                  e.PaymentStatus,
                                  CASE e.IsCompleted WHEN 1 THEN 'Completed' ELSE 'Active' END AS Status
                           FROM Enrolments e
                           JOIN Classes  c ON e.ClassID  = c.ClassID
                           JOIN Modules  m ON c.ModuleID = m.ModuleID
                           JOIN Trainers t ON c.TrainerID = t.TrainerID
                           JOIN Users    u ON t.UserID    = u.UserID
                           WHERE e.StudentID = @StudentID
                           ORDER BY c.StartDate";

            SqlParameter[] p = { new SqlParameter("@StudentID", SessionManager.RoleID) };
            dgvSchedule.DataSource = db.ExecuteQuery(sql, p);
        }

        // ============================================================
        // TAB B: SEND ENROLMENT REQUEST TO LECTURER
        // Controls needed in tabRequest:
        //   dgvAvailableClasses - DataGridView (shows all active classes)
        //   btnSendRequest      - Button
        // ============================================================

        private void LoadAvailableClasses()
        {
            string sql = @"SELECT c.ClassID, m.ModuleName, c.ClassLevel,
                                  'RM ' + CAST(c.Fee AS VARCHAR) AS Fee,
                                  c.Schedule,
                                  CONVERT(VARCHAR, c.StartDate, 103) AS StartDate,
                                  u.FullName AS Trainer
                           FROM Classes c
                           JOIN Modules  m ON c.ModuleID  = m.ModuleID
                           JOIN Trainers t ON c.TrainerID = t.TrainerID
                           JOIN Users    u ON t.UserID    = u.UserID
                           WHERE c.IsActive = 1
                             AND c.ClassID NOT IN (
                                 SELECT ClassID FROM Enrolments WHERE StudentID = @SID
                             )
                           ORDER BY m.ModuleName";

            SqlParameter[] p = { new SqlParameter("@SID", SessionManager.RoleID) };
            dgvAvailableClasses.DataSource = db.ExecuteQuery(sql, p);
        }

        private void btnSendRequest_Click(object sender, EventArgs e)
        {
            if (dgvAvailableClasses.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a class to request enrolment.",
                    "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int classID = Convert.ToInt32(dgvAvailableClasses.SelectedRows[0].Cells["ClassID"].Value);

            // Check not already requested
            string checkSql = @"SELECT COUNT(*) FROM EnrolmentRequests
                                 WHERE StudentID = @SID AND ClassID = @CID AND Status = 'Pending'";
            SqlParameter[] checkP = {
                new SqlParameter("@SID", SessionManager.RoleID),
                new SqlParameter("@CID", classID)
            };
            int existing = Convert.ToInt32(db.ExecuteScalar(checkSql, checkP));

            if (existing > 0)
            {
                MessageBox.Show("You have already sent a request for this class.",
                    "Duplicate Request", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string sql = @"INSERT INTO EnrolmentRequests (StudentID, ClassID, RequestedBy)
                           VALUES (@SID, @CID, 'Student')";

            SqlParameter[] p = {
                new SqlParameter("@SID", SessionManager.RoleID),
                new SqlParameter("@CID", classID)
            };

            int rows = db.ExecuteNonQuery(sql, p);

            if (rows > 0)
            {
                MessageBox.Show("Enrolment request sent to lecturer successfully!" +
                    "\nYou will be enrolled once the lecturer approves it.",
                    "Request Sent", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadMyRequests();
            }
        }

        // ============================================================
        // TAB C: VIEW & CANCEL PENDING REQUESTS
        // Controls needed in tabMyRequests:
        //   dgvMyRequests  - DataGridView
        //   btnCancelReq   - Button
        // ============================================================

        private void LoadMyRequests()
        {
            string sql = @"SELECT r.RequestID, m.ModuleName, c.ClassLevel,
                                  c.Schedule,
                                  'RM ' + CAST(c.Fee AS VARCHAR) AS Fee,
                                  CONVERT(VARCHAR, r.RequestDate, 103) AS RequestDate,
                                  r.Status
                           FROM EnrolmentRequests r
                           JOIN Classes c ON r.ClassID  = c.ClassID
                           JOIN Modules m ON c.ModuleID = m.ModuleID
                           WHERE r.StudentID = @SID
                           ORDER BY r.RequestDate DESC";

            SqlParameter[] p = { new SqlParameter("@SID", SessionManager.RoleID) };
            dgvMyRequests.DataSource = db.ExecuteQuery(sql, p);
        }

        private void btnCancelReq_Click(object sender, EventArgs e)
        {
            if (dgvMyRequests.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a request to cancel.",
                    "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string status = dgvMyRequests.SelectedRows[0].Cells["Status"].Value.ToString();

            if (status != "Pending")
            {
                MessageBox.Show("Only PENDING requests can be cancelled.",
                    "Cannot Cancel", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int requestID = Convert.ToInt32(dgvMyRequests.SelectedRows[0].Cells["RequestID"].Value);

            DialogResult confirm = MessageBox.Show(
                "Are you sure you want to cancel this request?",
                "Confirm Cancel", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            string sql = "UPDATE EnrolmentRequests SET Status = 'Cancelled' WHERE RequestID = @ID";
            SqlParameter[] p = { new SqlParameter("@ID", requestID) };
            db.ExecuteNonQuery(sql, p);

            MessageBox.Show("Request cancelled successfully.",
                "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadMyRequests();
        }

        // ============================================================
        // TAB D: VIEW INVOICE AND MAKE PAYMENT
        // Controls needed in tabPayment:
        //   dgvInvoice   - DataGridView (shows unpaid enrolments)
        //   lblTotal     - Label (shows total amount due)
        //   btnPayNow    - Button
        //   dgvPayHistory - DataGridView (shows payment history)
        // ============================================================

        private void LoadInvoice()
        {
            // Load unpaid enrolments
            string sql = @"SELECT e.EnrolmentID, m.ModuleName, c.ClassLevel,
                                  c.Fee, e.MonthOfEnrol, e.PaymentStatus
                           FROM Enrolments e
                           JOIN Classes c ON e.ClassID  = c.ClassID
                           JOIN Modules m ON c.ModuleID = m.ModuleID
                           WHERE e.StudentID    = @SID
                             AND e.PaymentStatus = 'Unpaid'";

            SqlParameter[] p = { new SqlParameter("@SID", SessionManager.RoleID) };
            DataTable dt = db.ExecuteQuery(sql, p);
            dgvInvoice.DataSource = dt;

            // Calculate total
            decimal total = 0;
            foreach (DataRow row in dt.Rows)
                total += Convert.ToDecimal(row["Fee"]);

            lblTotal.Text = "Total Amount Due: RM " + total.ToString("F2");

            // Load payment history
            LoadPaymentHistory();
        }

        private void LoadPaymentHistory()
        {
            string sql = @"SELECT p.ReceiptNumber, m.ModuleName, c.ClassLevel,
                                  p.Amount,
                                  CONVERT(VARCHAR, p.PaymentDate, 103) AS PaidOn
                           FROM Payments p
                           JOIN Enrolments e ON p.EnrolmentID = e.EnrolmentID
                           JOIN Classes    c ON e.ClassID     = c.ClassID
                           JOIN Modules    m ON c.ModuleID    = m.ModuleID
                           WHERE e.StudentID = @SID
                           ORDER BY p.PaymentDate DESC";

            SqlParameter[] p = { new SqlParameter("@SID", SessionManager.RoleID) };
            dgvPayHistory.DataSource = db.ExecuteQuery(sql, p);
        }

        private void btnPayNow_Click(object sender, EventArgs e)
        {
            if (dgvInvoice.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an invoice to pay.",
                    "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int     enrolID = Convert.ToInt32(dgvInvoice.SelectedRows[0].Cells["EnrolmentID"].Value);
            decimal fee     = Convert.ToDecimal(dgvInvoice.SelectedRows[0].Cells["Fee"].Value);
            string  module  = dgvInvoice.SelectedRows[0].Cells["ModuleName"].Value.ToString();

            DialogResult confirm = MessageBox.Show(
                "Confirm payment of RM " + fee.ToString("F2") + " for " + module + "?",
                "Confirm Payment", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            // Generate receipt number: RCP + timestamp
            string receipt = "RCP" + DateTime.Now.ToString("yyyyMMddHHmmss");

            // Insert payment record
            string insertPayment = @"INSERT INTO Payments (EnrolmentID, Amount, ReceiptNumber)
                                     VALUES (@EID, @Amount, @Receipt)";
            SqlParameter[] pp = {
                new SqlParameter("@EID",     enrolID),
                new SqlParameter("@Amount",  fee),
                new SqlParameter("@Receipt", receipt)
            };
            db.ExecuteNonQuery(insertPayment, pp);

            // Mark enrolment as Paid
            string updateEnrol = "UPDATE Enrolments SET PaymentStatus = 'Paid' WHERE EnrolmentID = @ID";
            SqlParameter[] ep = { new SqlParameter("@ID", enrolID) };
            db.ExecuteNonQuery(updateEnrol, ep);

            MessageBox.Show("Payment successful!\n" +
                "Receipt Number: " + receipt + "\n" +
                "Amount Paid: RM " + fee.ToString("F2"),
                "Payment Confirmed", MessageBoxButtons.OK, MessageBoxIcon.Information);

            LoadInvoice();
            LoadSchedule();
        }

        // ============================================================
        // TAB E: UPDATE PROFILE
        // ============================================================

        private void LoadStudentProfile()
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

            if (!txtProfileEmail.Text.Contains("@") || !txtProfileEmail.Text.Contains("."))
            {
                MessageBox.Show("Please enter a valid email address.",
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

            if (txtOldPass.Text.Trim() != "" && txtNewPass.Text.Trim() != "")
            {
                string checkSql = "SELECT COUNT(*) FROM Users WHERE UserID=@ID AND Password=@Old";
                SqlParameter[] cp = {
                    new SqlParameter("@ID",  SessionManager.UserID),
                    new SqlParameter("@Old", txtOldPass.Text.Trim())
                };
                if (Convert.ToInt32(db.ExecuteScalar(checkSql, cp)) == 0)
                {
                    MessageBox.Show("Current password is incorrect.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                string passSQL = "UPDATE Users SET Password=@New WHERE UserID=@ID";
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
