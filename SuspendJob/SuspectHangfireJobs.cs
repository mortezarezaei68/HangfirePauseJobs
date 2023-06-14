using Hangfire.Dashboard;

namespace SuspendJob;

public static class SuspectHangfireJobs
{
    public static void UseHangfireSuspendPage(
        this IApplicationBuilder app)
    {
        DashboardRoutes.Routes.AddRazorPage("/suspendJobs",
            page => new SuspendJobsPage());
            
        NavigationMenu.Items.Add(
            menu => new MenuItem("suspend Jobs", menu.Url.To("/suspendJobs")));
            
        DashboardRoutes.Routes.AddCommand("/suspendJobs/(?<JobId>.+)/resume",
            context =>
            {
                SuspendJobClient.MarkAsResume(context.UriMatch.Groups["JobId"].Value);
                return true;
            });   
        DashboardRoutes.Routes.AddCommand("/suspendJobs/(?<JobName>.+)/pause",
            context =>
            {
                SuspendJobClient.MarkAsPause(context.UriMatch.Groups["JobName"].Value);
                return true;
            });
    }
}