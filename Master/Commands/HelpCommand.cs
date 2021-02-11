using Commons.Commands;
using System;

namespace Master.Commands
{
	class HelpCommand : ICommand
	{
		public void Execute()
		{
			Console.WriteLine("There are 7 commands you can give, enumerated below:");
			Console.WriteLine("1.help - Shows this info prompt");
			Console.WriteLine("2.clear - Clear the console");
			Console.WriteLine("3.exit -  Cleanly exits the application - use this instead of closing the console");
			Console.WriteLine("4.start - Starts watching the watchfolder and sending data to the slaves");
			Console.WriteLine("5.stats - Show the current configuration and the amount of work that each slave is doing");
			Console.WriteLine("6.stop - Cleanly stops watching the watchfolder and sending data to the slaves");
			Console.WriteLine("7.negociate - The Master send a negociation message to the slave to change the working parameters");
		}
	}
}
