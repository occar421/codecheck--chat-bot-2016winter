using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text;

namespace ChatBot2016Winter
{
	public class RequestContainer
	{
		[JsonProperty(PropertyName = "text")]
		public string Text { get; set; }

		static JsonSerializerSettings jsonSetting { get; } = new JsonSerializerSettings()
		{
			ContractResolver = new CamelCasePropertyNamesContractResolver(),
			Formatting = Formatting.Indented,
			NullValueHandling = NullValueHandling.Ignore
		};

		public static RequestContainer FromBytes(byte[] bytes, int index, int count)
		{
			var rawString = Encoding.UTF8.GetString(bytes, index, count);
			return JsonConvert.DeserializeObject<RequestContainer>(rawString);
		}
	}
}
