using System.Collections;
using NUnit.Framework;
using PickerWheel.Scripts;
using UnityEngine.TestTools;

namespace PickerWheel.Tests
{
    public class TestSuite
    {
        [UnityTest]
        public IEnumerator TestFormatScore()
        {
            var score = 10000;
            var resultString = FormatValues.FormatScore(score);
            yield return null;
            if (score >= 1000)
            {
                Assert.True(resultString.Contains("k") || resultString.Contains("m"));
            }
            else
            {
                Assert.False(resultString.Contains("k") || resultString.Contains("m"));
            }
        }
    }
}
