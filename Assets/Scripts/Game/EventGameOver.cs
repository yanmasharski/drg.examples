using DRG.Analytics;

namespace FlappyExample.Game
{
	public sealed class EventGameOver : AnalyticsEvent
	{
		public EventGameOver(int score, int highScore, bool usedContinue) : base("game_over")
		{
			Set("score", score);
			Set("high_score", highScore);
			Set("used_continue", usedContinue);
		}
	}
}
