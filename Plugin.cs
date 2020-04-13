using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Bumblebee.Events;
using Bumblebee.Plugins;
using System.Linq;
using Bumblebee;

namespace Qbcode.Aggregate
{
    [RouteBinder(ApiLoader = false)]
    public class Plugin : IRequestingHandler, IPlugin, IPluginStatus, IPluginInfo
    {
        public string Name
        {
            get
            {
                return "qbcode.aggregate";
            }
        }

        public string Description
        {
            get
            {
                return "路由聚合";
            }
        }

        public PluginLevel Level
        {
            get
            {
                return PluginLevel.High4;
            }
        }

        public bool Enabled { get; set; }

        public string IconUrl
        {
            get
            {
                return "";
            }
        }

        public string EditorUrl
        {
            get
            {
                return "";
            }
        }

        public string InfoUrl
        {
            get
            {
                return "";
            }
        }

        public void Execute(EventRequestingArgs e)
        {
            string sourceBaseUrl = e.Request.GetSourceBaseUrl();
            Aggregate aggregate;
            if (this.mAggregates.TryGetValue(sourceBaseUrl, out aggregate) && aggregate.Actions != null && aggregate.Actions.Count > 0)
            {
                e.Cancel = true;
                e.ResultType = ResultType.Completed;
                e.Response.Header["aaa"] =  "666666";
                aggregate.Execute(e.Gateway, e.Request, e.Response);
            }
        }

        public void Init(Gateway gateway, Assembly assembly)
        {
            this.mGateway = gateway;
            gateway.HttpServer.ResourceCenter.LoadManifestResource(assembly);
        }

        public void LoadSetting(JToken setting)
        {
            if (setting != null)
            {
                this.setting = setting.ToObject<SettingInfo>();
                Dictionary<string, Aggregate> dictionary = new Dictionary<string, Aggregate>(StringComparer.OrdinalIgnoreCase);

                List<ActionInfo> Actions = this.setting.Actions ?? new List<ActionInfo>();
                List<AggregateInfo> Aggregates = this.setting.Aggregates ?? new List<AggregateInfo>();

                foreach (AggregateInfo item in Aggregates)
                {
                    Aggregate m = new Aggregate
                    {
                        Actions = Actions.Where(c => item.Names.Contains(c.Name))
                        .Select(p => new AggregateItem
                        {
                            Host = p.Host,
                            Name = p.Name,
                            Url = p.Url
                        }).ToList(),
                        OutputError = item.OutputError,
                        Url = item.Url
                    };
                    dictionary[item.Url] = m;
                    m.Build();
                }
                this.mAggregates = dictionary;
            }
        }

        /*
		 
			 {
				 "Url":"",
				 "OutputError":false,
				 "Actions":[
					{
						"Name":"",
						"Host":"",
						"Url":""
					} 
				]
			}

			{
				"Actions":[
					{
						"Name":"auth",
						"Host":"http://localhost:8080",
						"Url":"/api/auth/user/info"
					},
					{
						"Name":"score",
						"Host":"http://localhost:8081",
						"Url":"/api/score/info"
					},
					{
						"Name":"admin",
						"Host":"http://localhost:8082",
						"Url":"/api/admin/info"
					}
				],
				"Aggregates":[
					{
						"Url":"/api/sj/www/get",
						"Names":["score","auth"],
                        "OutputError":false
					},
					{
						"Url":"/api/sj/admin/get",
						"Names":["score","auth","admin"],
                        "OutputError":false
					}
				]
			}
			 
			 */

        public object SaveSetting()
        {
            return this.setting;
        }

        private SettingInfo setting = new SettingInfo();

        private Dictionary<string, Aggregate> mAggregates = new Dictionary<string, Aggregate>(StringComparer.OrdinalIgnoreCase);

        private Gateway mGateway;
    }

    public class SettingInfo
    {
        public List<ActionInfo> Actions { get; set; } = new List<ActionInfo>();
        public List<AggregateInfo> Aggregates { get; set; } = new List<AggregateInfo>();
    }

    public class AggregateInfo
    {
        public string Url { get; set; } = string.Empty;
        public List<string> Names { get; set; } = new List<string>();

        public bool OutputError { get; set; } = false;
    }

    public class ActionInfo
    {
        public string Url { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
    }
}
