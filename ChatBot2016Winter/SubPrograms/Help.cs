using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBot2016Winter.SubPrograms
{
	public class Help : ISubProgram
	{
		public IEnumerable<ResponseContainer> Responses { get; private set; }

		public void Run(string[] args)
		{
			Responses = new[]
			{
				new ResponseContainer
				{
					IsSuccess=true,
					Text="bot ping: returns \"pong\".\n" 
						+"bot cmd: control users' programs. See 'bot cmd help'"
						+"bot help: this command."
						+"bot [program name]: runs user-made program if exists. See 'bot cmd list'",
					Type=MessageType.Bot
				}
			};
		}
	}
}
