using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBot2016Winter.SubPrograms
{
	public class Ping
	{
		public ResponseContainer Response { get; private set; }

		public void Run(string[] args)
		{
			Response = new ResponseContainer
			{
				IsSuccess = true,
				Type = MessageType.Bot,
				Text = "pong"
			};
		}
	}
}
