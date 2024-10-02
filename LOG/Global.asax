<%@ Application Language="C#" %>
<%@ Import Namespace="System.Web" %>
<%@ Import Namespace="log4net" %>

<script runat="server">
    protected void Application_Start(object sender, EventArgs e)
    {
        // Use Server.MapPath to ensure the path is correct in a web application
        string log4netConfigPath = Server.MapPath("~/log4net.config");
        log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(log4netConfigPath));
    }
</script>