using Neo.IronLua;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot2016Winter.SubPrograms
{
	public class Cmd : ISubProgram
	{
		static IImmutableSet<string> DefaultProgram { get; } = new[] { "ping", "help", "cmd" }.ToImmutableHashSet();

		public IEnumerable<ResponseContainer> Responses { get; private set; }

		public void Run(string[] args)
		{
			switch (args[1])
			{
				case "add":
					Add(args);
					break;

				case "remove":
					Remove(args);
					break;

				case "list":
					List(args);
					break;

				case "help":
					Responses = new[]
					{
						new ResponseContainer
						{
							IsSuccess=true,
							Text="bot cmd [add|remove|list|help]",
							Type=MessageType.Bot
						}
					};
					break;

				default:

					break;
			}
		}

		void Add(string[] args)
		{
			#region Arguments validation
			if (args.Length < 5)
			{
				Responses = new[]
				{
					new ResponseContainer
					{
						IsSuccess=false,
						Text="[cmd add] Usage: cmd add [program name] [argc] [code...(space-char allowed)]\n"+
								"if assigns variable 'description', register it.",
						Type=MessageType.Bot
					}
				};
				return;
			}

			var programName = args[2];
			var fileName = $"{programName}.lua";
			if (File.Exists(Startup.SubProgramPathBase + fileName) || DefaultProgram.Contains(programName))
			{
				Responses = new[]
				{
					new ResponseContainer
					{
						IsSuccess = false,
						Text = $"[cmd add] program '{programName}' already exists. Try with another name.",
						Type = MessageType.Bot
					}
				};
				return;
			}
			int argc;
			if (!int.TryParse(args[3], out argc) || argc < 0)
			{
				Responses = new[]
				{
					new ResponseContainer
					{
						IsSuccess = false,
						Text = "[cmd add] args should be 0 or larger.",
						Type = MessageType.Bot
					}
				};
				return;
			}
			#endregion

			var code = string.Join(" ", args.Skip(4));
			using (var l = new Lua())
			{
				dynamic g = l.CreateEnvironment<LuaGlobalPortable>();

				var sb = new StringBuilder(); // just exists for test
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

				try
				{
					var chunk = l.CompileChunk(code, $"{programName}.lua", new LuaCompileOptions());
					if (chunk.IsCompiled)
					{
						g.args = new LuaTable();
						for (int i = 0; i < argc; i++)
						{
							g.args[i] = i.ToString(); // for test
						}
						g.description = "N/A";

						// try preventing infinite loop
						bool causesError = false;
						Task.WaitAny(
							Task.Run(() =>
							{
								try
								{
									g.dochunk(chunk);
								}
								catch (Exception)
								{
									causesError = true;
								}
							}),
							Task.Run(async () =>
							{
								await Task.Delay(TimeSpan.FromSeconds(10));
								causesError = true;
							}));
						if (causesError)
						{
							Responses = new[]
							{
								new ResponseContainer
								{
									IsSuccess = false,
									Text = "[cmd add] test timeout or throw exception.",
									Type = MessageType.Bot
								}
							};
							return;
						}

						string description = g.description;

						// prepared all info
						using (var writer = File.CreateText(Startup.SubProgramPathBase + fileName))
						{
							var sanitisedDesc = description.Replace("\n", "\\n"); // prevent newline accident
							writer.WriteLine("-- " + sanitisedDesc);
							writer.WriteLine("-- " + argc);
							writer.WriteLine(code);
						}

						Responses = new[] {
							new ResponseContainer()
							{
								IsSuccess = true,
								Text = $"Added program '{programName}'.",
								Type = MessageType.Bot
							}
						};
						return;
					}
				}
				catch (Exception)
				{
					// do nothing
				}
			}

			Responses = new[] {
				new ResponseContainer()
				{
					IsSuccess = false,
					Text = $"[cmd add] Error in validating NeoLua script of '{programName}'.",
					Type = MessageType.Bot
				}
			};
		}

		void Remove(string[] args)
		{
			#region Arguments Validation
			if (args.Length < 3)
			{
				Responses = new[]
				{
					new ResponseContainer
					{
						IsSuccess=false,
						Text="[cmd remove] Usage: cmd remove [program name]\n",
						Type=MessageType.Bot
					}
				};
				return;
			}
			#endregion

			var programName = args[2];
			var fileName = $"{programName}.lua";
			if (!File.Exists(Startup.SubProgramPathBase + fileName))
			{
				Responses = new[]
				{
					new ResponseContainer
					{
						IsSuccess = false,
						Text = $"[cmd remove] program '{programName}' doesn't exists.",
						Type = MessageType.Bot
					}
				};
				return;
			}

			try
			{
				File.Delete(Startup.SubProgramPathBase + fileName);
				Responses = new[]
				{
					new ResponseContainer
					{
						IsSuccess = true,
						Text = $"Removed program '{programName}'.",
						Type = MessageType.Bot
					}
				};
				return;
			}
			catch (Exception)
			{
				// do nothing
			}

			Responses = new[] {
				new ResponseContainer()
				{
					IsSuccess = false,
					Text = $"[cmd remove] Error in removing program '{programName}'.",
					Type = MessageType.Bot
				}
			};
		}

		void List(string[] args)
		{
			var luaFiles = Directory.EnumerateFiles(Startup.SubProgramPathBase, "*.lua");
			if (luaFiles.Any())
			{
				Responses = new[]
				{
					new ResponseContainer
					{
						IsSuccess = true,
						Text = string.Join("\n",
							luaFiles.Select(x=> {
								var fileName = Path.GetFileName(x);
								var name = fileName.Substring(0, fileName.Length-4);
								var description = File.ReadLines(x).First().Substring(3);
								return $"{name}: {description}";
							})),
						Type = MessageType.Bot
					}
				};
			}
			else
			{
				Responses = new[]
				{
					new ResponseContainer
					{
						IsSuccess = true,
						Text = $"There are no custom programs.",
						Type = MessageType.Bot
					}
				};
			}
		}
	}
}
