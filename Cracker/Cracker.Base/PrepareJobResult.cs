﻿namespace Cracker.Base
{
	public class PrepareJobResult
	{
		public string HashCatArguments { get; set; }
		public bool IsReadyForExecution { get; set; }
		public string Error { get; set; }
	}
}