using Hangfire;
using Hangfire.Dashboard;
using Hangfire.Dashboard.Pages;

namespace SuspendJob
{
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
}