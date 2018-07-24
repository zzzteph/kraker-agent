using System;
using Cracker.Base.HashCat;

namespace Cracker.Base
{
	public class BadJobHandler:JobHandler
	{
		private readonly PrepareJobResult result;
		public BadJobHandler(string error)
		{
			this.result = new PrepareJobResult
			{
				Error = error,
				IsReadyForExecution = false
			};
		}
		public override PrepareJobResult Prepare()
		{
			return result;
		}

		public override void Clear(ExecutionResult executionResult)
		{
			throw new InvalidOperationException($"Невозможно работать с задачей: {result.Error}");
		}
	}
}