using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChatBot2016Winter
{
	public class Startup
	{
		public static string SubProgramPathBase { get; private set; }

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			SubProgramPathBase = $"{env.ContentRootPath}/SubPrograms/";

			loggerFactory.AddConsole();

			app.UseWebSockets();

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.Use(async (context, next) =>
			{
				if (context.WebSockets.IsWebSocketRequest)
				{
					using (var socket = await context.WebSockets.AcceptWebSocketAsync())
					{
						if (socket?.State == System.Net.WebSockets.WebSocketState.Open)
						{
							var session = new Session(socket);
							await session.Deal();
							return;
						}
					}
				}
				await next();
			});

			app.UseDefaultFiles();
			app.UseStaticFiles();
		}
	}
}
