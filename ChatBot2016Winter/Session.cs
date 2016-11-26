using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace ChatBot2016Winter
{
	using SubPrograms;

	public class Session
	{
		static ConcurrentDictionary<int, Session> AllSessions { get; }
			= new ConcurrentDictionary<int, Session>();

		private WeakReference<WebSocket> socket;
		public int Id { get; }
		static IImmutableSet<string> BotWordsSpace { get; }
			= new[] { "bot", "@bot" }.ToImmutableHashSet();
		static IImmutableSet<string> BotWordsColon { get; }
			= new[] { "bot" }.ToImmutableHashSet();

		public Session(WebSocket socket)
		{
			this.socket = new WeakReference<WebSocket>(socket);
			Id = Counter.Instance.IssueId();
		}

		public async Task Deal()
		{
			if (AllSessions.TryAdd(Id, this))
			{
				var buffer = new byte[1024];
				WebSocket s;

				if (!socket.TryGetTarget(out s)) { return; }
				var received = await s.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

				while (received.MessageType == WebSocketMessageType.Text)
				{
					RequestContainer request;
					try
					{
						request = RequestContainer.FromBytes(buffer, 0, received.Count);
						var repeatContainer = new ResponseContainer
						{
							IsSuccess = true,
							Type = MessageType.Message,
							Text = request.Text
						};
						await Broadcast(repeatContainer.ToBytes());
					}
					catch
					{
						var repeatContainer = new ResponseContainer
						{
							IsSuccess = false,
							Type = MessageType.Bot,
							Text = "Invalid json format."
						};
						var bytes = repeatContainer.ToBytes();
						if (socket.TryGetTarget(out s))
						{
							await s.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
							received = await s.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
							continue;
						}
						else
						{
							break;
						}
					}

					// Bot Command Section
					var commandElements = request.Text.Split(' ');

					bool isBotCommandSpace = BotWordsSpace.Contains(commandElements[0]);
					bool isBotCommandColon = BotWordsColon.Any(x => request.Text.StartsWith(x + ":"));

					if (isBotCommandSpace || isBotCommandColon)
					{
						var subProgramArgs = isBotCommandSpace ?
							// bot ping foo bar
							// @bot ping foo bar
							// => ["ping", "foo", "bar"]	: Skip(1)
							commandElements.Skip(1).ToArray() :
							// bot:ping foo bar
							// => "ping foo bar"			: Split(':')[1]
							// => ["ping", "foo", "bar"]	: Split(' ')
							request.Text.Split(':')[1].Split(' ');

						ISubProgram program = null;
						switch (subProgramArgs[0])
						{
							case "ping":
								program = new Ping();
								break;

							case "help":
								program = new Help();
								break;

							case "cmd":
								program = new Cmd();
								break;

							default:
								program = new UserProgram();
								break;
						}

						program?.Run(subProgramArgs);
						if (program?.Responses != null)
						{
							foreach (var res in program.Responses)
							{
								await Broadcast(res.ToBytes());
							}
						}
					}

					if (!socket.TryGetTarget(out s)) { return; }
					received = await s.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
				}

				if (!socket.TryGetTarget(out s)) { return; }
				await s.CloseAsync(received.CloseStatus.Value, received.CloseStatusDescription, CancellationToken.None);
			}
			else
			{
				throw new TaskCanceledException();
			}
		}

		private static async Task Broadcast(byte[] bytes)
		{
			await Task.WhenAll(AllSessions.Select(x =>
			{
				WebSocket s;
				if (!x.Value.socket.TryGetTarget(out s) || s.State != WebSocketState.Open)
				{
					return Task.CompletedTask;
				}
				return s.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
			}));
		}
	}
}