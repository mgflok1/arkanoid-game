using UnityEngine;
using TMPro;
using Zenject;
using MiniIT.SCORE;
using DG.Tweening;

namespace MiniIT.SCORE
{
    /// <summary>
    /// Updates UI text with animated score counter using DOTween.
    /// </summary>
    public class ScoreUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI scoreText = null;

        [Inject] private IScoreManager scoreManager = null;

        private float displayedScore = 0f;

        private void Awake()
        {
            if (scoreManager != null)
            {
                scoreManager.OnScoreChanged += OnScoreChanged;
            }
        }

        private void Start()
        {
            OnScoreChanged(scoreManager.CurrentScore);
        }

        private void OnDestroy()
        {
            if (scoreManager != null)
            {
                scoreManager.OnScoreChanged -= OnScoreChanged;
            }
        }

        /// <summary>
        /// Called when the actual score changes. Animates the displayed value.
        /// </summary>
        /// <param name="targetScore">New total score value.</param>
        private void OnScoreChanged(int targetScore)
        {
            DOTween.To(() => displayedScore, x => displayedScore = x, targetScore, 0.5f)
                   .SetEase(Ease.OutCubic)
                   .OnUpdate(UpdateScoreText);
        }

        /// <summary>
        /// Updates the TextMeshPro text with the current animated value.
        /// </summary>
        private void UpdateScoreText()
        {
            scoreText.text = Mathf.RoundToInt(displayedScore).ToString();
        }
    }
}