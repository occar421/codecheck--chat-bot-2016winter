using Newtonsoft.Json;
using System.Text;

namespace ChatBot2016Winter
{
	public class ResponseContainer
	{
		[JsonProperty(PropertyName = "data")]
		public string Data { get; set; }

		[JsonProperty(PropertyName = "id")]
		public int Id { get; set; }

		public byte[] ToBytes()
		{
			return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this));
		}
	}
}
