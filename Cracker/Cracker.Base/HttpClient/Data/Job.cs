using Newtonsoft.Json;

namespace Cracker.Base.HttpClient.Data
{
	public class Job
	{

		/// <summary>
		/// Принимает значения JobType
		/// </summary>
		public string Type { get; set; }
		public Command Command { get; set; }

		/// <summary>
		/// hashlist, mask, wordlist
		/// </summary>
		[JsonProperty(PropertyName = "hash_id")]
		public string HashId { get; set; }

		/// <summary>
		/// Идентификатор задачи, которая разбиывается на job-ы
		/// mask, wordlist
		/// </summary>  
		[JsonProperty(PropertyName = "task_id")]
		public string Taskid { get; set; }

		/// <summary>
		/// Идентификатор выполняемого куска, используется в task start и в task end
		/// mask, wordlist
		/// </summary> 
		[JsonProperty(PropertyName = "job_id")]
		public string JobId { get; set; }

		/// <summary>
		/// template
		/// </summary>
		[JsonProperty(PropertyName = "template_id")]
		public string TemplateId { get; set; }

		[JsonProperty(PropertyName = "template_type")]
		public string TemplateType { get; set; }

	}
}