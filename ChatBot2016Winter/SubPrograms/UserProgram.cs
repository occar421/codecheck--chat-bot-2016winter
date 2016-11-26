using Neo.IronLua;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ChatBot2016Winter.SubPrograms
{
	public class UserProgram : ISubProgram
	{
		public IEnumerable<ResponseContainer> Responses { get; private set; }

		public void Run(string[] args)
		{
			#region Arguments Validation
			var programName = args[0];
			var fullPath = $"{Startup.SubProgramPathBase}{programName}.lua";
			if (!File.Exists(fullPath))
			{
				Responses = new[] {
					new ResponseContainer()
					{
						IsSuccess = false,
						Text = $"Program '{args[0]}' doesn't exist.",
						Type = MessageType.Bot
					}
				};
				return;
			}

			var allLines = File.ReadAllLines(fullPath);
			var argc = int.Parse(allLines.Skip(1).First().Substring(3));
			var programArgs = args.Skip(1).ToArray();
			if (argc > programArgs.Length)
			{
				Responses = new[] {
					new ResponseContainer()
					{
						IsSuccess = false,
						Text = $"Program '{args[0]}' needs {argc} args.",
						Type = MessageType.Bot
					}
				};
				return;
			}
			#endregion

			// no infinite loop here, I hope...
			var code = string.Join("\n", allLines.Skip(2));
			using (var l = new Lua())
			{
				dynamic g = l.CreateEnvironment<LuaGlobalPortable>();

				var sb = new StringBuilder();
				Action<object[]> print = (tests) =>
				{
					foreach (var t in tests)
					{
						sb.Append(t);
					}
				};
				Action<object[]> println = (tests) =>
				{
					print(tests);
					sb.Append("\n"); // new line
				};
				g.print = print;
				g.println = println;

				var chunk = l.CompileChunk(code, $"{programName}.lua", new LuaCompileOptions());
				if (chunk.IsCompiled)
				{
					g.args = new LuaTable();
					for (int i = 0; i < argc; i++)
					{
						g.args[i] = programArgs[i];
					}
					g.description = "N/A";

					try
					{
						g.dochunk(chunk);
					}
					catch (Exception e)
					{
						Responses = new[]
						{
							new ResponseContainer
							{
								IsSuccess = false,
								Text = e.Message,
								Type = MessageType.Bot
							}
						};
						return;
					}

					Responses = new[] {
						new ResponseContainer()
						{
							IsSuccess = true,
							Text = sb.ToString(),
							Type = MessageType.Bot
						}
					};
					return;
				}
			}
		}
	}
}
