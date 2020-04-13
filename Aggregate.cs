using BeetleX.EventArgs;
using BeetleX.FastHttpApi;
using BeetleX.Http.Clients;
using Bumblebee;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Qbcode.Aggregate
{
	public class Aggregate
	{
		public string Url { get; set; }

		public bool OutputError { get; set; }

		public List<AggregateItem> Actions { get; set; } = new List<AggregateItem>();

		public void Execute(Gateway gateway, HttpRequest request, HttpResponse response)
		{
			this.OnExecute(gateway, request, response);
		}

		public void Build()
		{
			foreach (AggregateItem aggregateItem in this.Actions)
			{
				aggregateItem.Build();
			}
		}

		private async void OnExecute(Gateway gateway, HttpRequest request, HttpResponse response)
		{
			StringBuilder mStringBuilder = new StringBuilder();
			mStringBuilder.Clear();
			try
			{
				List<Task<Response>> responses = new List<Task<Response>>();
				foreach (AggregateItem aggregateItem in this.Actions)
				{ 
					Request request2 = aggregateItem.CreateRequest(request);
					responses.Add(request2.Execute());
				}
				await Task.WhenAll<Response>(responses);
				mStringBuilder.Append("{");
				for (int i = 0; i < responses.Count; i++)
				{
					Response result = responses[i].Result;
					if (i > 0)
					{
						mStringBuilder.Append(",");
					}
					mStringBuilder.Append("\"" + this.Actions[i].Name + "\":");
					if (result.Exception != null)
					{
						if (this.OutputError)
						{
							mStringBuilder.Append("{");
							mStringBuilder.Append("\"Code\":");
							mStringBuilder.Append("\"" + result.Code + "\",");
							mStringBuilder.Append("\"Message\":");
							mStringBuilder.Append("\"" + result.CodeMsg + "\",");
							mStringBuilder.Append("\"Error\":");
							mStringBuilder.Append("\"" + result.Exception.Message + "\"");
							mStringBuilder.Append("}");
						}
						else
						{
							mStringBuilder.Append("null");
						}
						if (gateway.HttpServer.EnableLog(LogType.Warring))
						{
							AggregateItem aggregateItem2 = this.Actions[i];
							if (string.IsNullOrEmpty(result.Exception.Message))
							{
								gateway.HttpServer.Log(LogType.Warring, string.Format("gateway {0} {1} {2} {3} aggregate {4}=[{5}{6} error {7}@{8}]", new object[]
								{
									request.ID,
									request.RemoteIPAddress,
									request.Method,
									request.Url,
									aggregateItem2.Name,
									aggregateItem2.Host,
									aggregateItem2.Url,
									result.Code,
									result.CodeMsg
								}));
							}
							else
							{
								gateway.HttpServer.Log(LogType.Warring, string.Format("gateway {0} {1} {2} {3} aggregate {4}=[{5}{6} error {7}]", new object[]
								{
									request.ID,
									request.RemoteIPAddress,
									request.Method,
									request.Url,
									aggregateItem2.Name,
									aggregateItem2.Host,
									aggregateItem2.Url,
									result.Exception.Message
								}));
							}
						}
					}
					else if (result.Body != null)
					{
						mStringBuilder.Append((string)result.Body);
					}
					else
					{
						mStringBuilder.Append("null");
					}
				}
				mStringBuilder.Append("}");
				JsonText data = new JsonText(mStringBuilder);
				response.Result(data);
				responses = null;
			}
			catch (Exception ex)
			{
				response.Result(new BadGateway(request.GetSourceUrl() + " aggregate error " + ex.Message, 502));
				if (gateway.HttpServer.EnableLog(LogType.Error))
				{
					gateway.HttpServer.Log(LogType.Error, string.Format("gateway {0} {1} {2} {3} aggregate error {4}@{5}", new object[]
					{
						request.ID,
						request.RemoteIPAddress,
						request.Method,
						request.Url,
						ex.Message,
						ex.StackTrace
					}));
				}
			}
		}
	}
}
