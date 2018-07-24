using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThrowEx
{
	class Program
	{
		static void Main(string[] args)
		{
			Thread.Sleep(TimeSpan.FromSeconds(5));
			throw new Exception("ow");
		}
	}
}
