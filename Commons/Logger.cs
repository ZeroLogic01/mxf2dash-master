using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
	public static class Logger
	{
		private static readonly string separator = "---------------------------";

		public enum LogType
		{
			ERROR,
			INFO
		};

		private static void DefaultExit()
		{
			Console.WriteLine("Press any key to continue...");
			Console.ReadKey();
			Environment.Exit(-1);
		}

		private static TextWriter ErrStreamSetup(TextWriter errStream)
		{
			if (errStream is null)
			{
				errStream = Console.Error;
			}
			return errStream;
		}
		private static string GetCurrentMoment()
		{
			return DateTime.Now.ToString();
		}
		public static void Log(Exception e, LogType logType = LogType.ERROR, string prompt = null, TextWriter errStream = null)
		{
			TextWriter streamToUse = ErrStreamSetup(errStream);

			string currentMoment = GetCurrentMoment();

			streamToUse.WriteLine(separator);
			streamToUse.WriteLine(currentMoment + " - " + logType.ToString());
			streamToUse.WriteLine(e.Message);
			streamToUse.WriteLine(e.StackTrace);
			streamToUse.WriteLine(separator);
			streamToUse.Flush();

			Console.WriteLine(prompt);
		}
		public static void Log(string toLog, LogType logType = LogType.ERROR, string prompt = null, TextWriter errStream = null)
		{
			TextWriter streamToUse = ErrStreamSetup(errStream);

			string currentMoment = GetCurrentMoment();

			streamToUse.Flush();
			Console.WriteLine(prompt);
		}
		public static void Log(StreamReader streamToLog, LogType logType = LogType.INFO, string prompt = null, TextWriter errStream = null)
		{
			TextWriter streamToUse = ErrStreamSetup(errStream);

			string currentMoment = GetCurrentMoment();

			streamToUse.WriteLine(separator);
			streamToUse.WriteLine(currentMoment + " - " + logType.ToString());

			streamToUse.WriteLine(streamToLog.ReadToEnd());

			streamToUse.WriteLine(separator);

			streamToUse.Flush();
			Console.WriteLine(prompt);
		}

		public static void LogWithShutDown(Exception e, Action shutDown = null, string prompt = null, TextWriter errStream = null)
		{
			Log(e, LogType.ERROR, prompt, errStream);
			if (shutDown is null)
			{
				DefaultExit();
			}
			shutDown();
		}
		public static void LogWithShutDown(string toLog, Action shutDown = null, string prompt = null, TextWriter errStream = null)
		{
			Log(toLog, LogType.ERROR, prompt, errStream);
			if (shutDown is null)
			{
				DefaultExit();
			}
			shutDown();
		}
	}
}
