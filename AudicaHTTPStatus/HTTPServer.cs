using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Net;
using WebSocketSharp.Server;
using MelonLoader;

namespace AudicaHTTPStatus
{
	class HTTPServer {
		private int port = 6557;
		private HttpServer server;

		public HTTPServer() {
			// TODO: Inject queryable state object
		}

		public void Initialise() {
			this.server = new HttpServer(this.port);

			MelonModLogger.Log("Starting HTTP server on port " + this.port.ToString());
			this.server.Start();
		}

		public void Shutdown() {
			MelonModLogger.Log("Shutting down HTTP server");
			this.server.Stop();
		}
	}
}
