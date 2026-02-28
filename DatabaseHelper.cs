// ================================================================
// CLASS     : DatabaseHelper
// FILE      : DatabaseHelper.cs
// PURPOSE   : Centralises all database connection and query logic.
//             Every form uses this class to talk to SQL Server.
//             Uses basic ADO.NET - no LINQ or advanced ORM.
// ================================================================

using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace CodeCampSystem
{
    class DatabaseHelper
    {
        // --------------------------------------------------------
        // Connection string - update Server name to match yours.
        // If using SQL Server Express, use: .\SQLEXPRESS
        // If using LocalDB, use: (localdb)\MSSQLLocalDB
        // --------------------------------------------------------
        private string connectionString =
            @"Server=.\SQLEXPRESS;Database=CodeCampDB;Integrated Security=True;";

        // --------------------------------------------------------
        // GetConnection: returns an open SqlConnection
        // --------------------------------------------------------
        public SqlConnection GetConnection()
        {
            SqlConnection conn = new SqlConnection(connectionString);
            try
            {
                conn.Open();
                return conn;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database connection failed:\n" + ex.Message,
                    "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        // --------------------------------------------------------
        // ExecuteQuery: runs a SELECT and returns a DataTable
        // --------------------------------------------------------
        public DataTable ExecuteQuery(string sql, SqlParameter[] parameters = null)
        {
            DataTable dt = new DataTable();
            SqlConnection conn = GetConnection();
            if (conn == null) return dt;

            try
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                if (parameters != null)
                {
                    foreach (SqlParameter p in parameters)
                        cmd.Parameters.Add(p);
                }
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Query error:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
            }
            return dt;
        }

        // --------------------------------------------------------
        // ExecuteNonQuery: runs INSERT, UPDATE, DELETE
        // Returns number of rows affected, or -1 on error
        // --------------------------------------------------------
        public int ExecuteNonQuery(string sql, SqlParameter[] parameters = null)
        {
            SqlConnection conn = GetConnection();
            if (conn == null) return -1;

            try
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                if (parameters != null)
                {
                    foreach (SqlParameter p in parameters)
                        cmd.Parameters.Add(p);
                }
                return cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database error:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
            finally
            {
                conn.Close();
            }
        }

        // --------------------------------------------------------
        // ExecuteScalar: runs a query and returns a single value
        // Useful for COUNT(*), MAX(ID), etc.
        // --------------------------------------------------------
        public object ExecuteScalar(string sql, SqlParameter[] parameters = null)
        {
            SqlConnection conn = GetConnection();
            if (conn == null) return null;

            try
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                if (parameters != null)
                {
                    foreach (SqlParameter p in parameters)
                        cmd.Parameters.Add(p);
                }
                return cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Query error:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            finally
            {
                conn.Close();
            }
        }
    }
}
