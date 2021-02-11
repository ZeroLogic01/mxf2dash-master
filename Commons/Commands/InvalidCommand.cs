using System;

namespace Commons.Commands
{
	/*  InvalidCommand is generated when the user inputs an invalid command string. 
		 It`s role is to generate an error message and inform the user about it`s mistake.
	*/
	public class InvalidCommand : ICommand
	{
		private static readonly string InvalidCommandMessage = "Invalid Command";
		public void Execute()
		{
			Console.WriteLine(InvalidCommandMessage);
		}
	}
}
