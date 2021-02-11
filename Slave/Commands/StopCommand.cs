//using Master.Communication;
using Commons.Commands;

namespace Slave.Commands
{
	internal class StopCommand : ICommand
	{
		Listener Listener;

		public StopCommand(Listener Listener)
		{
			this.Listener = Listener;
		}

		public void Execute()
		{
			Listener.Stop();
		}
	}
}