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
	class HTTPServer  : WebSocketBehavior {
		private int port = 6557;
		private HttpServer server;
		private Func<string> getStatus;

		public HTTPServer(Func<string> getStatusJSON) {
			this.getStatus = getStatusJSON;
		}

		public void Initialise() {
			this.server = new HttpServer(this.port);
			this.server.OnGet += (sender, e) => {
				this.OnHTTPGet(e);
			};

			MelonModLogger.Log("Starting HTTP server on port " + this.port.ToString());
			this.server.Start();
		}

		public void Shutdown() {
			MelonModLogger.Log("Shutting down HTTP server");
			this.server.Stop();
		}

		public void OnHTTPGet(HttpRequestEventArgs e) {
			var req = e.Request;
			var res = e.Response;

			if (req.RawUrl == "/status.json") {
				res.StatusCode = 200;
				res.ContentType = "application/json";
				res.ContentEncoding = Encoding.UTF8;

				res.WriteContent(Encoding.UTF8.GetBytes(getStatus()));

				return;
			}

			res.StatusCode = 404;
			res.WriteContent(new byte[] { });
		}
	}
}
