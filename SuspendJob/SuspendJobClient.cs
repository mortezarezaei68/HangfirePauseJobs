using Hangfire;

namespace SuspendJob
{
    public static class SuspendJobClient
    {
        /// <summary>
        /// resume jobs from dashboard page
        /// </summary>
        /// <param name="jobId"></param>
        public static void MarkAsResume(string jobId)
        {
            using var connection = JobStorage.Current.GetConnection();
            connection.Resume(jobId);
        }

        /// <summary>
        /// pause jobs from dashboard page
        /// </summary>
        /// <param name="jobId"></param>
        public static void MarkAsPause(string jobId)
        {
            using var connection = JobStorage.Current.GetConnection();
            connection.Pause(jobId);
        }
    }
}