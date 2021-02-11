//using Master.Communication;
using Commons.Commands;

namespace Master.Commands
{
	internal class StartCommand : ICommand
	{
		private Watcher Watcher;
		public StartCommand(Watcher watcher)
        {
			Watcher = watcher;
        }
		public void Execute()
		{
			Watcher.Start();
		}
	}
}