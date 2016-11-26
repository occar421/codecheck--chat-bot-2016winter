using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatBot2016Winter
{
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
					var request = RequestContainer.FromBytes(buffer, 0, received.Count);
					var repeatContainer = new ResponseContainer
					{
						IsSuccess = true,
						Type = MessageType.Message,
						Text = request.Text
					};// TODO add Id
					await Broadcast(repeatContainer.ToBytes());

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
							// => ["foo", "bar"]			: Skip(1)
							// => ["ping", "foo", "bar"]	: Prepend(...)
							commandElements.Skip(1).Prepend(request.Text.Split(':')[1]).ToArray();

						switch (subProgramArgs[0])
						{
							case "ping":
								var ping = new SubPrograms.Ping();
								ping.Run(commandElements);
								if (ping.Response != null)
								{
									await Broadcast(ping.Response.ToBytes());
								}
								break;

							// TODO case "cmd" add remove ls

							default:
								// TODO check user defined scripts
								break;
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