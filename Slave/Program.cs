using Commons;
using Commons.Commands;
using Slave.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slave
{
    class Program
    {
        private static readonly Listener listener = new Listener();
        private static readonly string stdErrFile = "ErrorLogs.txt";
        private static string prompt = "mxf2dash-Slave>";
        public static string InterfaceSeparator { get; set; } = "--------------------";
        private static string commandString;
        private static ICommand command;

        private static GenericFactory<string, ICommand> CommandFactory = new GenericFactory<string, ICommand>();

        //Follow the example in the Master Program.cs to see how to register commands.
        //Left as is for when you will want to expand on the app
        private static void RegisterCommands()
        {
            CommandFactory.DefaultValue = new InvalidCommand();
            List<Tuple<string, ICommand>> commands = new List<Tuple<string, ICommand>>()
            {
                new Tuple<string, ICommand>("start",new StartCommand(listener)),
                new Tuple<string, ICommand>("stop", new StopCommand(listener))
            };

            commands.ForEach(pair => CommandFactory.RegisterItem(pair.Item1, pair.Item2));
        }


        static void RedirectStdErr(string filename)
        {
            TextWriter newStdErr = new StreamWriter(filename, true);
            Console.SetError(newStdErr);
        }
        static void Init(string[] args)
        {
            RedirectStdErr(stdErrFile);

            Settings.Instance.ListeningPort = int.Parse(args[0]);
            Settings.Instance.WorkPower = int.Parse(args[1]);
        }

        static void Main(string[] args)
        {
#if DEBUG
            args = new string[] { "5001", "10" };
#endif
            Init(args);

            //Registering Commands to use later
            RegisterCommands();

            //Infinite loop so that we hold the program on indefinitely
            //Can get commands
            while (true)
            {
                Console.Write(prompt);
                commandString = Console.ReadLine();
                command = CommandFactory.GetItem(commandString);
                command.Execute();
            }
        }
    }
}
