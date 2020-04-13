using System;
using System.Collections.Generic;
using BeetleX.FastHttpApi;
using BeetleX.Http.Clients;

namespace Qbcode.Aggregate
{
    public class AggregateItem
    {
        public string Name { get; set; }

        public string Host { get; set; }

        public string Url { get; set; }

        public void Build()
        {
            this.mHttpHost = new BeetleX.Http.Clients.HttpHost(this.Host);
        }

        public Request CreateRequest(HttpRequest request)
        {
            return this.mHttpHost.Get(this.GetUrl(request), (Dictionary<string, string>)request.Header.Copy(), null, new GetJsonFormater(), null);
        }

        public unsafe string GetUrl(HttpRequest request)
        {
			char[] charBuffer = BeetleX.FastHttpApi.HttpParse.GetCharBuffer();
			int num = 0;
			int num2 = 0;
			ReadOnlySpan<char> readOnlySpan = this.Url;
			for (int i = 0; i < readOnlySpan.Length; i++)
			{
				if (readOnlySpan[i] == 123)
				{
					num2 = i + 1;
				}
				else if (readOnlySpan[i] == 125)
				{
					if (num2 > 0)
					{
						string name = new string(readOnlySpan.Slice(num2, i - num2));
						string text;
						if (request.Data.TryGetString(name, out text))
						{
							for (int j = 0; j < text.Length; j++)
							{
								charBuffer[num] = text[j];
								num++;
							}
						}
					}
					num2 = 0;
				}
				else if (num2 == 0)
				{
					charBuffer[num] = (char)(readOnlySpan[i]);
					num++;
				}
			}
			return new string(charBuffer, 0, num);
        }

        private BeetleX.Http.Clients.HttpHost mHttpHost;
    }
}
