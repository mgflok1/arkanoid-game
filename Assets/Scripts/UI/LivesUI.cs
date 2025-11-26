using UnityEngine;
using UnityEngine.UI;
using Zenject;
using MiniIT.LIVES;

namespace MiniIT.LIVES
{
    /// <summary>
    /// Visual representation of player lives using heart icons.
    /// Red heart = active life, gray heart = lost life.
    /// </summary>
    public class LivesUI : MonoBehaviour
    {
        [Header("Heart Sprites")]
        [SerializeField] private Sprite redHeart = null;
        [SerializeField] private Sprite grayHeart = null;

        [Header("Heart Images (order: left → right)")]
        [SerializeField] private Image[] heartImages = null;

        [Inject] private ILivesManager livesManager = null;

        private void Awake()
        {
            if (livesManager != null)
            {
                livesManager.OnLivesChanged += OnLivesChanged;
            }
            else
            {
                Debug.LogError("LivesUI: ILivesManager injection failed!");
            }
        }

        private void Start()
        {
            OnLivesChanged(livesManager.CurrentLives);
        }

        private void OnDestroy()
        {
            // Prevent memory leaks
            if (livesManager != null)
            {
                livesManager.OnLivesChanged -= OnLivesChanged;
            }
        }

        /// <summary>
        /// Updates heart icons based on current number of lives.
        /// </summary>
        /// <param name="currentLives">Remaining lives count.</param>
        private void OnLivesChanged(int currentLives)
        {
            if (heartImages == null || heartImages.Length == 0)
            {
                return;
            }

            for (int i = 0; i < heartImages.Length; i++)
            {
                if (heartImages[i] != null)
                {
                    heartImages[i].sprite = (i < currentLives) ? redHeart : grayHeart;
                }
            }
        }
    }
}