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
    public partial class KeepAlive : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["Connection"].ConnectionString);
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] != null)
            {
                UpdateSession();
            }
        }
        public void ConnectionOpen()
        {
            if (con.State == System.Data.ConnectionState.Closed)
            {
                con.Open();
            }
        }
        public void UpdateSession()
        {
            SqlCommand cmd = new SqlCommand("UPDATE LoginSession SET LoginTime = GETDATE() WHERE UserID = @UserID", con);
            cmd.Parameters.AddWithValue("@UserID", (int)Session["UserID"]);
            ConnectionOpen();
            cmd.ExecuteNonQuery();
        }
    }
}