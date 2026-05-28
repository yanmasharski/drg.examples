using DRG.Analytics;

namespace FlappyExample.Game
{
	public sealed class EventGameStart : AnalyticsEvent
	{
		public EventGameStart(int attemptNumber) : base("game_start")
		{
			Set("attempt_number", attemptNumber);
		}
	}
}
