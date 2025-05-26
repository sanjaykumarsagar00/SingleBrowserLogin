using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SingleBrowserLogin
{
    public partial class UserLogin : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["Connection"].ConnectionString);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString["action"] == "logout")
                {
                    Logout();
                }
                if (Session["UserID"] != null)
                {
                    Response.Redirect("Default.aspx");
                }
            }
        }

        public void ConnectionOpen()
        {
            if (con.State == System.Data.ConnectionState.Closed)
            {
                con.Open();
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            // Step 1: Validate user credentials
            int userId = GetUserIdIfValid(username, password);
            if (userId == 0)
            {
                lblMessage.Text = "Invalid username or password.";
                return;
            }

            string sessionId = Session.SessionID;
            Int32 TotalMINUTE = -2;

            SqlCommand checkCmd = new SqlCommand(@"
                    SELECT SessionID 
                    FROM LoginSession 
                    WHERE UserID = @UserID 
                    AND LoginTime > DATEADD(MINUTE, @TotalMINUTE, GETDATE())", con);
            checkCmd.Parameters.AddWithValue("@UserID", userId);
            checkCmd.Parameters.AddWithValue("@TotalMINUTE", TotalMINUTE);
            ConnectionOpen();
            object existingSession = checkCmd.ExecuteScalar();

            if (existingSession != null)
            {
                lblMessage.Text = "This user is already logged in from another device or browser.";
                return;
            }

            SqlCommand upsertCmd = new SqlCommand(@"
                    IF EXISTS (SELECT 1 FROM LoginSession WHERE UserID = @UserID)
                        UPDATE LoginSession SET SessionID = @SessionID, LoginTime = GETDATE() WHERE UserID = @UserID
                    ELSE
                        INSERT INTO LoginSession (UserID, SessionID, LoginTime) VALUES (@UserID, @SessionID, GETDATE())
                ", con);

            upsertCmd.Parameters.AddWithValue("@UserID", userId);
            upsertCmd.Parameters.AddWithValue("@SessionID", sessionId);
            ConnectionOpen();
            upsertCmd.ExecuteNonQuery();
            // Step 4: Set session and redirect
            Session["UserID"] = userId;
            Session["Username"] = txtUsername.Text; ;
            Response.Redirect("Default.aspx");
        }

        private int GetUserIdIfValid(string username, string password)
        {
            SqlCommand cmd = new SqlCommand("SELECT ID FROM Users WHERE Username = @Username AND Password = @Password", con);
            cmd.Parameters.AddWithValue("@Username", username);
            cmd.Parameters.AddWithValue("@Password", password);
            ConnectionOpen();
            object result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        protected void Logout()
        {
            if (Session["UserID"] != null)
            {
                int userId = (int)Session["UserID"];
                SqlCommand cmd = new SqlCommand("DELETE FROM LoginSession WHERE UserID = @UserID", con);
                cmd.Parameters.AddWithValue("@UserID", userId);
                ConnectionOpen();
                cmd.ExecuteNonQuery();
            }
            Session.Abandon();
            Response.Redirect("UserLogin.aspx");
        }

    }
}