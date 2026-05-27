using FlappyExample.Game;
using UnityEngine;
using UnityEngine.UI;

namespace FlappyExample.UI
{
	public sealed class FlappyHud : MonoBehaviour
	{
		private FlappyGameController _controller;

		private Text _scoreText;
		private GameObject _menuPanel;
		private Text _menuHighScoreText;
		private GameObject _gameOverPanel;
		private Text _gameOverScoreText;
		private Text _gameOverBestText;
		private Button _restartButton;
		private Button _continueButton;

		public void Bind(FlappyGameController controller)
		{
			_controller = controller;
			BuildUi();
			_controller.StateChanged += Refresh;
			_controller.ScoreChanged += OnScoreChanged;
			Refresh();
		}

		private void OnDestroy()
		{
			if (_controller == null)
			{
				return;
			}

			_controller.StateChanged -= Refresh;
			_controller.ScoreChanged -= OnScoreChanged;
		}

		private void BuildUi()
		{
			_scoreText = CreateText("ScoreText", transform, new Vector2(0.5f, 0.92f), new Vector2(500, 80), 48,
				TextAnchor.MiddleCenter, "0");

			_menuPanel = CreatePanel("MenuPanel", transform, new Color(0f, 0f, 0f, 0.35f));
			CreateText("Title", _menuPanel.transform, new Vector2(0.5f, 0.62f), new Vector2(700, 120), 56,
				TextAnchor.MiddleCenter, "Flappy DRG");
			CreateText("Hint", _menuPanel.transform, new Vector2(0.5f, 0.48f), new Vector2(700, 80), 32,
				TextAnchor.MiddleCenter, "Tap / Space to start");
			_menuHighScoreText = CreateText("MenuBest", _menuPanel.transform, new Vector2(0.5f, 0.38f),
				new Vector2(700, 60), 28, TextAnchor.MiddleCenter, "Best: 0");

			_gameOverPanel = CreatePanel("GameOverPanel", transform, new Color(0f, 0f, 0f, 0.45f));
			CreateText("GameOverTitle", _gameOverPanel.transform, new Vector2(0.5f, 0.62f), new Vector2(700, 100),
				48, TextAnchor.MiddleCenter, "Game Over");
			_gameOverScoreText = CreateText("FinalScore", _gameOverPanel.transform, new Vector2(0.5f, 0.52f),
				new Vector2(700, 70), 36, TextAnchor.MiddleCenter, "Score: 0");
			_gameOverBestText = CreateText("FinalBest", _gameOverPanel.transform, new Vector2(0.5f, 0.44f),
				new Vector2(700, 60), 28, TextAnchor.MiddleCenter, "Best: 0");

			_restartButton = CreateButton("RestartButton", _gameOverPanel.transform, new Vector2(0.5f, 0.3f),
				new Vector2(360, 72), "Restart", _controller.RestartGame);
			_continueButton = CreateButton("ContinueButton", _gameOverPanel.transform, new Vector2(0.5f, 0.2f),
				new Vector2(360, 72), "Continue (ad)", _controller.RequestContinue);
		}

		public void Refresh()
		{
			if (_controller == null)
			{
				return;
			}

			_scoreText.text = _controller.Score.ToString();
			_menuHighScoreText.text = $"Best: {_controller.HighScore}";
			_gameOverScoreText.text = $"Score: {_controller.Score}";
			_gameOverBestText.text = $"Best: {_controller.HighScore}";

			_menuPanel.SetActive(_controller.State == FlappyGameState.Menu);
			_gameOverPanel.SetActive(_controller.State == FlappyGameState.GameOver);
			_scoreText.gameObject.SetActive(_controller.State == FlappyGameState.Playing);
			_continueButton.gameObject.SetActive(_controller.CanContinue);
		}

		private void OnScoreChanged(int score)
		{
			_scoreText.text = score.ToString();
		}

		private static GameObject CreatePanel(string name, Transform parent, Color color)
		{
			var panel = new GameObject(name, typeof(RectTransform), typeof(Image));
			panel.transform.SetParent(parent, false);
			var rect = panel.GetComponent<RectTransform>();
			rect.anchorMin = Vector2.zero;
			rect.anchorMax = Vector2.one;
			rect.offsetMin = Vector2.zero;
			rect.offsetMax = Vector2.zero;
			panel.GetComponent<Image>().color = color;
			return panel;
		}

		private static Text CreateText(string name, Transform parent, Vector2 anchor, Vector2 size, int fontSize,
			TextAnchor alignment, string content)
		{
			var go = new GameObject(name, typeof(RectTransform), typeof(Text));
			go.transform.SetParent(parent, false);
			var rect = go.GetComponent<RectTransform>();
			rect.anchorMin = anchor;
			rect.anchorMax = anchor;
			rect.pivot = new Vector2(0.5f, 0.5f);
			rect.sizeDelta = size;
			rect.anchoredPosition = Vector2.zero;

			var text = go.GetComponent<Text>();
			text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
			text.fontSize = fontSize;
			text.alignment = alignment;
			text.color = Color.white;
			text.text = content;
			text.horizontalOverflow = HorizontalWrapMode.Overflow;
			return text;
		}

		private static Button CreateButton(string name, Transform parent, Vector2 anchor, Vector2 size, string label,
			UnityEngine.Events.UnityAction onClick)
		{
			var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
			go.transform.SetParent(parent, false);
			var rect = go.GetComponent<RectTransform>();
			rect.anchorMin = anchor;
			rect.anchorMax = anchor;
			rect.pivot = new Vector2(0.5f, 0.5f);
			rect.sizeDelta = size;
			rect.anchoredPosition = Vector2.zero;

			var image = go.GetComponent<Image>();
			image.color = new Color(0.15f, 0.45f, 0.85f, 0.95f);

			var button = go.GetComponent<Button>();
			button.onClick.AddListener(onClick);

			CreateText("Label", go.transform, new Vector2(0.5f, 0.5f), size, 28, TextAnchor.MiddleCenter, label);
			return button;
		}
	}
}
