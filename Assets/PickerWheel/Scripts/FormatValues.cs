namespace PickerWheel.Scripts
{
    public static class FormatValues
    {
        private static readonly string[] Cuts = { "", "k", "m" };
        public static string FormatScore(int score)
        {
            float updateScore;
            int formatIndex;
            
            switch (score)
            {
                case >= 1000000:
                    updateScore = score * 1f / 1000000;
                    formatIndex = 2;
                    break;
                case >= 1000:
                    updateScore = score * 1f / 1000;
                    formatIndex = 1;
                    break;
                default:
                    updateScore = score;
                    formatIndex = 0;
                    break;
            }
            return updateScore + Cuts[formatIndex];
        }
    }
}
