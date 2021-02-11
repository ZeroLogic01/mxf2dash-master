using Commons.Commands;
using System;

namespace Master.Commands
{
	internal class ExitCommand : ICommand
	{
		public void Execute()
		{
			Environment.Exit(0);
		}
	}
}