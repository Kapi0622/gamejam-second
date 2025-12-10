using System;
using Cysharp.Threading.Tasks;
using Unityroom.Client;

namespace unity1week.Core
{
    public static class UnityRoomScoreSender
    {
        private static UnityroomClient _client = new();
        
        private const string API_KEY = "";

        /// <summary>
        ///  スコアを送信する
        /// </summary>
        /// <param name="score"></param>
        /// <param name="scoreboardId"></param>
        /// <param name="onScoreUpdated"></param>
        public static async UniTask SendScore(int score, int scoreboardId = 1, Action onScoreUpdated = null)
        {
            var response = await _client.Scoreboards.SendAsync(new SendScoreRequest
            {
                ScoreboardId = scoreboardId,
                Score = score
            });

            if (response.ScoreUpdated)
            {
                onScoreUpdated?.Invoke();
            }
        }
    }   
}
