//using Master.Communication;
using Commons.Commands;

namespace Master.Commands
{
	internal class StopCommand : ICommand
	{
		private Watcher Watcher;
		public StopCommand(Watcher watcher)
		{
			Watcher = watcher;
		}

		public void Execute()
		{
			Watcher.Stop();
		}
	}
}