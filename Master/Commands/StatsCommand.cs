using Commons.Commands;

namespace Master.Commands
{
	internal class StatsCommand : ICommand
	{
		public void Execute()
		{
			Settings.Instance.PrintConfig();
		}
	}
}