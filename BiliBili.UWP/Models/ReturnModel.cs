namespace BiliBili.UWP.Models
{
	public class ReturnModel<T>
	{
		public T data { get; set; }
		public string message { get; set; }
		public bool success { get; set; }
	}

	public class ReturnModel
	{
		public dynamic data { get; set; }
		public string message { get; set; }
		public bool success { get; set; }
	}
}