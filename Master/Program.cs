using Commons.Commands;
using Master;
using Master.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
    class Program
    {

        private static readonly string stdErrFile = "ErrorLogs.txt";
        private static string prompt = "mxf2dash-Master>";
        private static string commandString;
        private static ICommand command;
        private static Watcher Watcher;
        private static GenericFactory<string,ICommand> CommandFactory = new GenericFactory<string, ICommand>();

        public static string InterfaceSeparator { get; set; } = "--------------------";

        private static void RegisterCommands()
        {
            CommandFactory.DefaultValue = new InvalidCommand();
            Watcher = new Watcher();
            List<Tuple<string, ICommand>> commands = new List<Tuple<string, ICommand>>(){
                     new Tuple<string,ICommand>("start", new StartCommand(Watcher)),
                     new Tuple<string,ICommand>("stats", new StatsCommand()),
                     new Tuple<string,ICommand>("stop", new StopCommand(Watcher)),
                     new Tuple<string,ICommand>("exit", new ExitCommand()),
                     new Tuple<string,ICommand>("help", new HelpCommand()),
                     new Tuple<string,ICommand>("clear", new ClearCommand()),
                     new Tuple<string,ICommand>("negociate", new NegociateCommand()),
                };

            commands.ForEach(pair => CommandFactory.RegisterItem(pair.Item1, pair.Item2));

        }

        static void RedirectStdErr(string filename)
        {
            TextWriter newStdErr = new StreamWriter(filename,true);
            Console.SetError(newStdErr);
        }
        static void Init()
        {
            RedirectStdErr(stdErrFile);
        }

        static void Main(string[] args)
        {
            //Basic initialisation
            Init();

            //Reading the config files and loading the settings
            try
            {
                _ = Settings.Instance;
            }
            catch (Exception e)
            {
                string prompt = string.Format("Configuration was unsuccessful due to {0}, please read the ErrorLogs.txt file",e.GetType().Name);
                Logger.LogWithShutDown(e,prompt:prompt);
            }

            //Registering Commands
            RegisterCommands();

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
