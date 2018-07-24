using System.Collections.Generic;

namespace Cracker.Lib.HashCat
{
	public class ExecutionResult
	{
		public int? HashCatExitCode { get; set; }

		public IReadOnlyList<string> Output { get; set; }
		public IReadOnlyList<string> Errors { get; set; }
		public bool IsSuccessful { get; set; }
		public string ErrorMessage { get; set; }
	}
}
