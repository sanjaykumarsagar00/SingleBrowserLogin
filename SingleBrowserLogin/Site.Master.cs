using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SingleBrowserLogin
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["UserId"] == null)
                {
                    phLogin.Visible = true;
                    phLogout.Visible = false;
                    menu.Visible = false;
                }
                else
                {
                    phLogin.Visible = false;
                    phLogout.Visible = true;
                    menu.Visible = true;
                }
            }
        }
    }
}