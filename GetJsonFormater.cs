using BeetleX.Buffers;
using BeetleX.Http.Clients;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;


namespace Qbcode.Aggregate
{
	public class GetJsonFormater : FormaterAttribute
	{
		public override string ContentType
		{
			get
			{
				return "application/json";
			}
		}

		public override object Deserialization(Response response, PipeStream stream, Type type, int length)
		{
			return stream.ReadToEnd();
		}

		public override void Serialization(object data, PipeStream stream)
		{
			using (stream.LockFree())
			{
				using (StreamWriter streamWriter = new StreamWriter(stream))
				{
					IDictionary dictionary = data as IDictionary;
					JsonSerializer jsonSerializer = new JsonSerializer();
					if (dictionary != null && dictionary.Count == 1)
					{
						object[] array = new object[dictionary.Count];
						dictionary.Values.CopyTo(array, 0);
						jsonSerializer.Serialize(streamWriter, array[0]);
					}
					else
					{
						jsonSerializer.Serialize(streamWriter, data);
					}
				}
			}
		}
	}
}
