using Commons.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slave.Commands
{
	internal class StartCommand : ICommand
	{
		Listener Listener;

        public StartCommand(Listener Listener)
        {
			this.Listener = Listener;
        }

		public void Execute()
		{
			Listener.Start();
		}
	}
}
