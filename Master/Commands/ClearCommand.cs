using Commons.Commands;
using System;

namespace Master.Commands
{
	class ClearCommand : ICommand
	{
		public void Execute()
		{
			Console.Clear();
		}
	}
}
