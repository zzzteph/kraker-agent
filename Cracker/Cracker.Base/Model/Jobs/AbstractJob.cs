using System.Text.Json.Serialization;

namespace Cracker.Base.Model.Jobs
{
    [JsonConverter(typeof(JobConverter))]
    public abstract record AbstractJob
    {
        protected AbstractJob(JobType type) => Type = type;
        

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public JobType Type { get; }
    }
}