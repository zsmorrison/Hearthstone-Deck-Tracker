namespace Hearthstone_Deck_Tracker.HsReplay
{
	public class XmlMetaData
	{
		public XmlMetaData(string key, object value)
		{
			Key = key;
			Value = value.ToString();
		}

		public string Key { get; set; }
		public string Value { get; set; }
	}
}