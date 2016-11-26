using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBot2016Winter.SubPrograms
{
	interface ISubProgram
	{
		IEnumerable<ResponseContainer> Responses { get; }

		void Run(string[] args);
	}
}
