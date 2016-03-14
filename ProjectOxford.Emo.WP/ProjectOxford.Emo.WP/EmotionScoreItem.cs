using System;

namespace ProjectOxford.Emo.WP
{
    class EmotionScoreItem : IComparable
    {
        public string Name { get; set; }
        public float ScoreValue { get; set; }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            EmotionScoreItem otherItem = obj as EmotionScoreItem;

            if (otherItem != null)
                return ScoreValue.CompareTo(otherItem.ScoreValue);
            else
                throw new ArgumentException("Object is not a EmotionScoreItem");
        }
    }
}
