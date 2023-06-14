using Hangfire.Common;
using Hangfire.Storage;

namespace SuspendJob;

public static class SuspendingJobStorageExtensions
{
    private const string TagRecurringJob = "recurring-job";
    private const string TagStopJob = "recurring-jobs-stop";

    /// <summary>
    /// pause job by id
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="jobId"></param>
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

    /// <summary>
    /// resume job by id
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="jobId"></param>
    public static void Resume(this IStorageConnection connection, string jobId)
    {
        using var transaction = connection.CreateWriteTransaction();
        transaction.RemoveFromSet(TagStopJob, jobId);
        transaction.AddToSet($"{TagRecurringJob}s", jobId);
        transaction.Commit();
    }

    /// <summary>
    /// get list of suspend jobs
    /// </summary>
    /// <param name="connection"></param>
    /// <returns></returns>
    public static List<PeriodicJobDto> GetAllJobStopped(this IStorageConnection connection)
    {
        var outPut = new List<PeriodicJobDto>();
        var allJobStopped = connection.GetAllItemsFromSet(TagStopJob);
        
        var recurringJobs=connection.GetRecurringJobs();

        var currentStopJobs = allJobStopped.Except(recurringJobs.Select(a=>a.Id));


        currentStopJobs.ToList().ForEach(jobId =>
        {
            var dto = new PeriodicJobDto();

            var dataJob = connection.GetAllEntriesFromHash($"{TagRecurringJob}:{jobId}");
            dto.Id = jobId;
            dto.TimeZoneId = "UTC"; // Default

            try
            {
                if (dataJob.TryGetValue("Job", out var payload) && !String.IsNullOrWhiteSpace(payload))
                {
                    var invocationData = InvocationData.DeserializePayload(payload);
                    var job = invocationData.DeserializeJob();
                    dto.Method = job.Method.Name;
                    dto.Class = job.Type.Name;
                }
            }
            catch (JobLoadException ex)
            {
                dto.Error = ex.Message;
            }

            if (dataJob.TryGetValue("TimeZoneId", out var timeZoneId))
            {
                dto.TimeZoneId = timeZoneId;
            }

            if (dataJob.TryGetValue("NextExecution", out var nextExecution))
            {
                var tempNextExecution = JobHelper.DeserializeNullableDateTime(nextExecution);

                dto.NextExecution = tempNextExecution.HasValue
                    ? tempNextExecution.ToString()
                    : "N/A";
            }

            if (dataJob.TryGetValue("LastJobId", out var lastJobId) && !string.IsNullOrWhiteSpace(lastJobId))
            {
                dto.LastJobId = lastJobId;

                var stateData = connection.GetStateData(dto.LastJobId);
                if (stateData != null)
                {
                    dto.LastJobState = stateData.Name;
                }
            }

            if (dataJob.TryGetValue("Queue", out var queue))
            {
                dto.Queue = queue;
            }

            if (dataJob.TryGetValue("LastExecution", out var lastExecution))
            {
                var tempLastExecution = JobHelper.DeserializeNullableDateTime(lastExecution);

                dto.LastExecution = tempLastExecution.HasValue
                    ? tempLastExecution.ToString()
                    : "N/A";
            }

            if (dataJob.TryGetValue("CreatedAt", out var createdAt))
            {
                dto.CreatedAt = JobHelper.DeserializeNullableDateTime(createdAt);
                dto.CreatedAt = dto.CreatedAt?.ChangeTimeZone(dto.TimeZoneId) ?? new DateTime();
            }

            if (dataJob.TryGetValue("Error", out var error) && !String.IsNullOrEmpty(error))
            {
                dto.Error = error;
            }

            dto.Removed = false;
            dto.JobState = "Stopped";

            outPut.Add(dto);
        });

        return outPut;
    }    
    /// <summary>
    /// get list of recurring jobs
    /// </summary>
    /// <param name="connection"></param>
    /// <returns></returns>
    public static List<PeriodicJobDto> GetAllRecurringJob(this IStorageConnection connection)
    {
        var outPut = new List<PeriodicJobDto>();
        var allJobStopped = connection.GetRecurringJobs();

        allJobStopped.ToList().ForEach(jobId =>
        {
            var dto = new PeriodicJobDto();

            var dataJob = connection.GetAllEntriesFromHash($"{TagRecurringJob}:{jobId}");
            dto.Id = jobId.Id;
            dto.TimeZoneId = "UTC"; // Default

            try
            {
                if (dataJob is not null && dataJob.TryGetValue("Job", out var payload) && !String.IsNullOrWhiteSpace(payload))
                {
                    var invocationData = InvocationData.DeserializePayload(payload);
                    var job = invocationData.DeserializeJob();
                    dto.Method = job.Method.Name;
                    dto.Class = job.Type.Name;
                }
            }
            catch (JobLoadException ex)
            {
                dto.Error = ex.Message;
            }

            if (dataJob != null && dataJob.TryGetValue("TimeZoneId", out var timeZoneId))
            {
                dto.TimeZoneId = timeZoneId;
            }

            if (dataJob != null && dataJob.TryGetValue("NextExecution", out var nextExecution))
            {
                var tempNextExecution = JobHelper.DeserializeNullableDateTime(nextExecution);

                dto.NextExecution = tempNextExecution.HasValue
                    ? tempNextExecution.ToString()
                    : "N/A";
            }

            if (dataJob != null && dataJob.TryGetValue("LastJobId", out var lastJobId) && !string.IsNullOrWhiteSpace(lastJobId))
            {
                dto.LastJobId = lastJobId;

                var stateData = connection.GetStateData(dto.LastJobId);
                if (stateData != null)
                {
                    dto.LastJobState = stateData.Name;
                }
            }

            if (dataJob != null && dataJob.TryGetValue("Queue", out var queue))
            {
                dto.Queue = queue;
            }

            if (dataJob != null && dataJob.TryGetValue("LastExecution", out var lastExecution))
            {
                var tempLastExecution = JobHelper.DeserializeNullableDateTime(lastExecution);

                dto.LastExecution = tempLastExecution.HasValue
                    ? tempLastExecution.ToString()
                    : "N/A";
            }

            if (dataJob != null && dataJob.TryGetValue("CreatedAt", out var createdAt))
            {
                dto.CreatedAt = JobHelper.DeserializeNullableDateTime(createdAt);
                dto.CreatedAt = dto.CreatedAt?.ChangeTimeZone(dto.TimeZoneId) ?? new DateTime();
            }

            if (dataJob != null && dataJob.TryGetValue("Error", out var error) && !String.IsNullOrEmpty(error))
            {
                dto.Error = error;
            }

            dto.Removed = false;
            dto.JobState = jobId.LastJobState;

            outPut.Add(dto);
        });

        return outPut;
    }

    private static DateTime ChangeTimeZone(this DateTime dateTime, string timeZoneId) =>
        TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dateTime, timeZoneId);
}