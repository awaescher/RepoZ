using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TinyIpc.Messaging;

namespace Grr
{
	class Program
	{
		private static TinyMessageBus _bus;

		static void Main(string[] args)
		{
			_bus = new TinyMessageBus("RepoZGrrChannel");
			_bus.MessageReceived += (sender, e) => Console.WriteLine(Encoding.UTF8.GetString(e.Message));

			string input = "-";
			while (!string.IsNullOrEmpty(input))
			{
				Console.Write("Write the message to send: ");
				input = Console.ReadLine();

				if (!string.IsNullOrEmpty(input))
				{
					var message = Encoding.UTF8.GetBytes(input);
					_bus.PublishAsync(message);
				}
			}
		}

		//static void StartIpcClient()
		//{
		//	using (var bus = new
		//	{

		//		while (true)
		//		{
		//			//	var message = Console.ReadLine();
		//			//	messagebus1.PublishAsync(Encoding.UTF8.GetBytes(message));
		//		}
		//	}
		//}
	}
}
