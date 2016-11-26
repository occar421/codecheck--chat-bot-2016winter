using System.Collections.Generic;

namespace ChatBot2016Winter.SubPrograms
{
	public class Ping : ISubProgram
	{
		public IEnumerable<ResponseContainer> Responses { get; private set; }

		public void Run(string[] args)
		{
			Responses = new[] {
				new ResponseContainer
				{
					IsSuccess = true,
					Type = MessageType.Bot,
					Text = "pong"
				}
			};
		}
	}
}
