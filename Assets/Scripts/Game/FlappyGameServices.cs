using DRG.Ads;
using DRG.Analytics;
using DRG.Data.Serialization;
using DRG.Utils;
using ILogger = DRG.Core.Logs.ILogger;

namespace FlappyExample.Game
{
	public sealed class FlappyGameServices
	{
		public ILogger Logger;
		public IAdsSystem Ads;
		public IAnalyticsGateway Analytics;
		public IAppReviewDialog AppReview;
		public IDataSerializer Serializer;
		public string HighScoreKey;
		public string StatsKey;
	}
}
