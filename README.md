A common approach to handling tasks automatically in your application is to use jobs. Hangfire is an excellent tool for managing jobs easily. The stopping and resuming of jobs in Hangfire sometimes require stopping and restarting your project, so I decided to introduce new components in this framework after reviewing comments and advice carefully. in this article, I will describe how you can implement this component in your Hangfire Project.


Tech stack used: ASP.NET 6.0, Hangfire v1.8.*

To create this component you need to use these packages in your application:

        <packagereference include="Hangfire.Core" version="1.8.*"/>
        <packagereference include="Hangfire.SqlServer" version="1.8.*"/>
        <packagereference include="Hangfire.AspNetCore" version="1.8.*"/>
        <packagereference include="Microsoft.Data.SqlClient" version="*"/>


afterward, add a middleware with the below structure:




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


The Hangfire framework now has your customized component for pausing or resuming jobs without publishing code. if you want to see github repository of this article go with this middleware, you can add a Razor Page with AddRazorPage URI and by NavigationMenu you can add custom menu items in the Hangfire framework.
by adding AddCommand you can add your API with some structures such as (?<JobId>.+) for getting jobId in your API request from the URL.

you have to consider that all APIs in this middleware are kind of HTTP POST requests.
After that, it is turn to createSuspendingJobStorageExtensions. by using this extension you get Resume and Pause by job Id with these methods:


public static void Pause(this IStorageConnection connection, string jobId)
{
var recurringJob = connection.GetRecurringJobs().FirstOrDefault(a => a.Id == jobId);
if (recurringJob is null)
throw new InvalidOperationException("there is not any recurring job with this name");

        using var transaction = connection.CreateWriteTransaction();
        transaction.RemoveFromSet($"{TagRecurringJob}s", jobId);
        transaction.AddToSet($"{TagStopJob}", jobId);
        transaction.Commit();
    }

    
    

    public static void Resume(this IStorageConnection connection, string jobId)
    {
        using var transaction = connection.CreateWriteTransaction();
        transaction.RemoveFromSet(TagStopJob, jobId);
        transaction.AddToSet($"{TagRecurringJob}s", jobId);
        transaction.Commit();
    }


In SuspendJobClient static class you can have your Hangfire customize API to call in your middleware.

      public static class SuspendJobClient
    {
        public static void MarkAsResume(string jobId)
        {
            using var connection = JobStorage.Current.GetConnection();
            connection.Resume(jobId);
        }
        public static void MarkAsPause(string jobId)
        {
            using var connection = JobStorage.Current.GetConnection();
            connection.Pause(jobId);
        }
    }


and in the last step is added Razor page in your Hangfire framework:

public class SuspendJobsPage : RazorPage
{
/// <summary>
/// create suspend jobs page
/// </summary>
public override void Execute()
{
WriteLiteral("\r\n");
Layout = new LayoutPage("pause jobs");

            WriteLiteral("<meta http-equiv=\"refresh\" content=\"600\">\r\n");
            WriteLiteral("<div class=\"row\">\r\n");
            WriteLiteral("<div class=\"col-md-3\">\r\n</div>\r\n");
 
            
            WriteLiteral("<div class=\"col-md-9\">\r\n");
            WriteLiteral("<h1 class=\"page-header\">\r\nPause jobs</h1>\r\n");
            WriteLiteral("<div class=\"col-md-9\">\r\n");
            
            WriteLiteral($"<form onsubmit='Test()'>\r\n");
            WriteLiteral($"<label for=\"jobName\">Job Name:</label>\r\n");
            // WriteLiteral($"<input type='text' id=\"jobName\" name=\"jobName\">\r\n");
            WriteLiteral($"<select id=\"jobName\">\r\n");
            using var connection = JobStorage.Current.GetConnection();
            var recurringJobs = connection.GetAllRecurringJob();
            foreach (var job in recurringJobs.OrEmptyIfNull())
            {
                WriteLiteral($"<option id=\"{job.Id}\">{job.Id}</option>\r\n");
            }
            WriteLiteral($"</select>\r\n");
            
            WriteLiteral($"<input type=\"submit\" name=\"Pause job\" value=\"Pause job\">\r\n");
            WriteLiteral($"</form>\r\n");
            WriteLiteral("<script type='text/javascript'>function Test(){var inputVm = document.getElementById(\"jobName\").value; " +
                         "$.ajax({url:" +
                         "'suspendJobs/'+inputVm+'/pause'" +
                         ",type: 'POST',success: function (res){console.log('res', res);location.reload(true)},error: function (res){console.log('res', res)}})}</script>\r\n");
            WriteLiteral("\r\n</div>\r\n");
            WriteLiteral("<table class=\"table\">\r\n");
            WriteLiteral("<thead>\r\n");
            WriteLiteral("<tr>\r\n");
            WriteLiteral("<th class=\"min-width\">LastJobId</th>\r\n");
            WriteLiteral("<th>Job</th>\r\n");
            WriteLiteral("<th class=\"align-right\">CreatedAt (UTC)</th>\r\n");
            WriteLiteral("<th class=\"align-right\">Actions</th>\r\n");
            WriteLiteral("\r\n</tr>\r\n");
            WriteLiteral("\r\n</thead>\r\n");
            
            WriteLiteral("<tbody>\r\n");
            using var storageConnection = JobStorage.Current.GetConnection();
            var jobs = storageConnection.GetAllJobStopped();
            foreach (var job in jobs)
            {
                WriteLiteral("<tr>\r\n");
                WriteLiteral($"<td class=\"align-right\">#{job.LastJobId}</a></td>\r\n");
                WriteLiteral($"<td class=\"align-right\">{job.Id}</a></td>\r\n");
                WriteLiteral($"<td class=\"align-right\">{job.CreatedAt}</td>\r\n");
                WriteLiteral($"<td class=\"align-right\"><a style='cursor:pointer' data-ajax='{@Url.To($"/suspendJobs/{job.Id}/resume")}' data-confirm='Are you sure?'>Delete</a></td>\r\n");
                WriteLiteral("\r\n</tr>\r\n");
            }
            WriteLiteral("\r\n</table>\r\n");
            WriteLiteral("\r\n</div>\r\n");
 
            WriteLiteral("<div class=\"btn-toolbar\">\r\n");
            WriteLiteral("<div class=\"btn-toolbar-label\">\r\n");
            WriteLiteral($"Total items: {jobs.Count}");
            WriteLiteral("\r\n</div>\r\n");
            WriteLiteral("\r\n</div>\r\n");
        }
    }


Now, you have your customize component to pause or resume jobs on execution mode in Hangfire framework without publishing your code.
