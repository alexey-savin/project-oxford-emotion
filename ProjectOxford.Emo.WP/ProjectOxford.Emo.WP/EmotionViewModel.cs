using Microsoft.ProjectOxford.Emotion.Contract;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace ProjectOxford.Emo.WP
{
    class EmotionViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Emotion _emotion = null;
        private ObservableCollection<EmotionScoreItem> _top3Emotion = new ObservableCollection<EmotionScoreItem>();

        public Emotion Emotion
        {
            get { return _emotion; }
            set
            {
                _emotion = value;
                OnPropertyChanged("Emotion");
            }
        }

        public ObservableCollection<EmotionScoreItem> Top3Emotion
        {
            get
            {
                return _top3Emotion;
            }
        }

        public ObservableCollection<EmotionScoreItem> Top3EmotionTest
        {
            get
            {
                List<EmotionScoreItem> result = new List<EmotionScoreItem>();

                result.Add(new EmotionScoreItem { Name = "Счастье", ScoreValue = 0.5f });
                result.Add(new EmotionScoreItem { Name = "Злость", ScoreValue = 0.3f });
                result.Add(new EmotionScoreItem { Name = "Презрение", ScoreValue = 0.1f });

                foreach (EmotionScoreItem item in result
                    .OrderByDescending(esi => esi.ScoreValue)
                    .Take(3))
                {
                    _top3Emotion.Add(item);
                }

                return _top3Emotion;
            }
        }

        public void Clear()
        {
            Emotion = null;
            RefreshTop3Emotion();
        }

        public void RefreshTop3Emotion()
        {
            _top3Emotion.Clear();

            if (_emotion != null)
            {
                List<EmotionScoreItem> result = new List<EmotionScoreItem>();

                result.Add(new EmotionScoreItem { Name = "Счастье", ScoreValue = _emotion.Scores.Happiness });
                result.Add(new EmotionScoreItem { Name = "Злость", ScoreValue = _emotion.Scores.Anger });
                result.Add(new EmotionScoreItem { Name = "Презрение", ScoreValue = _emotion.Scores.Contempt });
                result.Add(new EmotionScoreItem { Name = "Отвращение", ScoreValue = _emotion.Scores.Disgust });
                result.Add(new EmotionScoreItem { Name = "Нейтрально", ScoreValue = _emotion.Scores.Neutral });
                result.Add(new EmotionScoreItem { Name = "Грусть", ScoreValue = _emotion.Scores.Sadness });
                result.Add(new EmotionScoreItem { Name = "Удивление", ScoreValue = _emotion.Scores.Surprise });
                result.Add(new EmotionScoreItem { Name = "Страх", ScoreValue = _emotion.Scores.Fear });

                foreach (EmotionScoreItem item in result
                    .OrderByDescending(esi => esi.ScoreValue)
                    .Take(3))
                {
                    _top3Emotion.Add(item);
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
