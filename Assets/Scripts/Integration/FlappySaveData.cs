using System;

namespace FlappyExample.Integration
{
	[Serializable]
	public sealed class FlappySaveData
	{
		public int totalGames;
		public int totalFlaps;
		public string lastPlayedUtc = string.Empty;
	}
}
