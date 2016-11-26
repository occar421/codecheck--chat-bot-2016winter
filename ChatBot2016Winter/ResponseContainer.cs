using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Text;

namespace ChatBot2016Winter
{
	public enum MessageType
	{
		Message,
		Bot
	}

	public class ResponseContainer
	{
		[JsonProperty(PropertyName = "type")]
		[JsonConverter(typeof(StringEnumConverter), new object[] { true })] // CamelCaseText = true
		public MessageType Type { get; set; }

		[JsonProperty(PropertyName = "text")]
		public string Text { get; set; }

		[JsonProperty(PropertyName = "success")]
		public bool IsSuccess { get; set; }

		static JsonSerializerSettings jsonSetting { get; } = new JsonSerializerSettings()
		{
			ContractResolver = new CamelCasePropertyNamesContractResolver(),
			Formatting = Formatting.Indented,
			NullValueHandling = NullValueHandling.Ignore
		};

		public byte[] ToBytes()
		{
			return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this));
		}
	}
}
