using System;
using System.Collections.Concurrent;
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
					var data = Encoding.UTF8.GetString(buffer, 0, received.Count);
					var repeatContainer = new ResponseContainer { Data = data, Id = this.Id };
					await Broadcast(repeatContainer.ToBytes());

					// Bot Command Section
					var commandElement = data.Split(' ');

					// TODO here

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