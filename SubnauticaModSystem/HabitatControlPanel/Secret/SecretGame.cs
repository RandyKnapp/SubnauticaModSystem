using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HabitatControlPanel.Secret
{
	class SecretGame : MonoBehaviour, IPointerClickHandler, IPointerHoverHandler
	{
		private const float GameWidth = 160;
		private const float GameHeight = 120;
		private const float LeftWall = -GameWidth / 2;
		private const float RightWall = GameWidth / 2;
		private const float TopWall = GameHeight / 2;
		private const float BottomWall = -GameHeight / 2;
		private const float BallSize = 6;
		private const float BallSpeed = 100;
		private const float PaddleWidth = 30;
		private const float PaddleHeight = 5;
		private const float BrickSpacing = 1;
		private const float BricksPerRow = 12;
		private const float BrickRows = 5;
		private const float BrickWidth = (GameWidth - ((BricksPerRow + 1) * BrickSpacing)) / BricksPerRow;
		private const float BrickHeight = 6;
		private static readonly Color[] BrickColors = { Color.red, new Color32(255, 165, 0, 255), Color.yellow, Color.green, Color.cyan };

		private RectTransform rectTransform;
		private Image background;
		private Transform ball;
		private Transform paddle;
		private bool ballAttachedToPaddle;
		private Vector3 ballVelocity;
		private List<GameObject> bricks = new List<GameObject>();
		private int ballCount;
		private List<Image> balls = new List<Image>();

		[SerializeField]
		private Text text;

		public Action onLoseGame = delegate { };

		enum Reflection { Horizontal, Vertical }

		private void Awake()
		{
			rectTransform = transform as RectTransform;
		}

		public void StartGame()
		{
			if (background == null)
			{
				background = gameObject.AddComponent<Image>();
				background.color = new Color(0, 0, 0);
			}

			ballCount = 2;

			if (ball == null)
			{
				var ballImage = new GameObject("Ball").AddComponent<Image>();
				ballImage.sprite = ImageUtils.LoadSprite(Mod.GetAssetPath("Circle.png"));
				RectTransformExtensions.SetParams(ballImage.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), transform);
				RectTransformExtensions.SetSize(ballImage.rectTransform, BallSize, BallSize);
				ball = ballImage.transform;

				for (var i = 0; i < ballCount; ++i)
				{
					var b = Instantiate(ballImage);
					b.transform.SetParent(transform, false);
					var x = ((GameWidth - BallSize) / 2) - ((BallSize + 1) * i);
					var y = ((GameHeight + BallSize) / 2) + 1;
					b.rectTransform.anchoredPosition = new Vector2(x, y);
					balls.Add(b);
				}
			}

			if (paddle == null)
			{
				var paddleImage = new GameObject("paddle").AddComponent<Image>();
				RectTransformExtensions.SetParams(paddleImage.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), transform);
				RectTransformExtensions.SetSize(paddleImage.rectTransform, PaddleWidth, PaddleHeight);
				paddle = paddleImage.transform;
			}

			text.gameObject.SetActive(false);

			ResetPaddleAndBall();
			AddBricks();
		}

		private void ResetPaddleAndBall()
		{
			ball.gameObject.SetActive(true);
			paddle.gameObject.SetActive(true);

			ballAttachedToPaddle = true;
			SetPaddlePosition(0);
		}

		private void AddBricks()
		{
			bricks.Clear();

			var startY = (GameHeight / 2) - (BrickHeight * 2.5f);
			var vSpacing = BrickHeight + BrickSpacing;
			var startX = (-GameWidth / 2) + (BrickWidth / 2) + BrickSpacing;
			var hSpacing = BrickWidth + BrickSpacing;
			for (int row = 0; row < BrickRows; ++row)
			{
				for (int col = 0; col < BricksPerRow; ++col)
				{
					var x = startX + col * hSpacing;
					var y = startY - row * vSpacing;
					var c = BrickColors[row];
					AddBrick(c, x, y);
				}
			}
		}

		private void AddBrick(Color c, float x, float y)
		{
			var brick = new GameObject("Brick").AddComponent<Image>();
			RectTransformExtensions.SetParams(brick.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), transform);
			RectTransformExtensions.SetSize(brick.rectTransform, BrickWidth, BrickHeight);
			brick.rectTransform.anchoredPosition = new Vector2(x, y);
			brick.color = c;

			bricks.Add(brick.gameObject);
		}

		private void OnDisable()
		{
			foreach (var brick in bricks)
			{
				Destroy(brick);
			}
			bricks.Clear();
		}

		private void LoseBall()
		{
			if (ballCount == 0)
			{
				LoseGame();
				return;
			}

			ballCount--;
			ResetPaddleAndBall();
		}

		private void LoseGame()
		{
			ball.gameObject.SetActive(false);
			paddle.gameObject.SetActive(false);
			SetText("GAME OVER");
			onLoseGame();
		}

		private void WinGame()
		{
			ball.gameObject.SetActive(false);
			paddle.gameObject.SetActive(false);
			SetText("A WINNER IS YOU!");

			var rewards = new TechType[] {
				TechType.Titanium, TechType.Copper, TechType.AcidMushroom,
				TechType.Lead, TechType.Quartz, TechType.Silver
			};
			var reward = rewards[UnityEngine.Random.Range(0, rewards.Length)];
			GiveReward(reward);
		}

		private void GiveReward(TechType techType)
		{
			GameObject gameObject = CraftData.InstantiateFromPrefab(techType, false);
			if (gameObject != null)
			{
				gameObject.transform.position = MainCamera.camera.transform.position + MainCamera.camera.transform.forward * 2f;
				CrafterLogic.NotifyCraftEnd(gameObject, techType);
				Pickupable component = gameObject.GetComponent<Pickupable>();
				if (component != null && !Inventory.main.Pickup(component, false))
				{
					ErrorMessage.AddError(Language.main.Get("InventoryFull"));
				}
			}
		}

		private void SetText(string content)
		{
			text.gameObject.SetActive(true);
			text.text = content;
		}

		private void Update()
		{
			if (!ballAttachedToPaddle && ball.gameObject.activeSelf)
			{
				ball.localPosition += ballVelocity * Time.deltaTime;

				if (BallIntersectsWall(out Reflection reflect, out bool bottom))
				{
					if (bottom)
					{
						LoseBall();
					}
					Bounce(reflect);
				}
				else if (BallIntersectsPaddle(out float paddleHitLocation))
				{
					PaddleBounce(paddleHitLocation);
				}
				else if (BallIntersectsAnyBrick(out GameObject brick, out reflect))
				{
					Destroy(brick);
					Bounce(reflect);
				}

				if (bricks.Count == 0)
				{
					WinGame();
				}
			}

			UpdateBallCounter();
		}

		private void UpdateBallCounter()
		{
			for (var i = 0; i < balls.Count; ++i)
			{
				var b = balls[i];
				b.gameObject.SetActive(i < ballCount);
			}
		}

		private bool BallIntersectsWall(out Reflection reflect, out bool bottom)
		{
			var s2 = BallSize / 2;
			var p = ball.localPosition;
			var xMin = p.x - s2;
			var xMax = p.x + s2;
			var yMin = p.y - s2;
			var yMax = p.y + s2;
			var bvx = ballVelocity.x;
			var bvy = ballVelocity.y;

			bottom = false;
			if ((xMin <= LeftWall && bvx < 0) || (xMax >= RightWall && bvx > 0))
			{
				reflect = Reflection.Horizontal;
				return true;
			}
			else if (yMax >= TopWall && bvy > 0)
			{
				reflect = Reflection.Vertical;
				return true;
			}
			else if ((yMin <= BottomWall - BallSize) && bvy < 0)
			{
				reflect = Reflection.Vertical;
				bottom = true;
				return true;
			}

			reflect = 0;
			return false;
		}

		private void Bounce(Reflection reflect)
		{
			if (reflect == Reflection.Horizontal)
			{
				ballVelocity.x = -ballVelocity.x;
			}
			else if (reflect == Reflection.Vertical)
			{
				ballVelocity.y = -ballVelocity.y;
			}
		}

		private bool BallIntersectsPaddle(out float paddleHitLocation)
		{
			var x = ball.localPosition.x;
			var y = ball.localPosition.y - (BallSize / 2);
			var pxMin = paddle.localPosition.x - (PaddleWidth / 2);
			var pxMax = paddle.localPosition.x + (PaddleWidth / 2);
			var py = paddle.localPosition.y + (PaddleHeight / 2);

			if (y < py && x > pxMin && x < pxMax)
			{
				paddleHitLocation = Mathf.InverseLerp(pxMin, pxMax, x);
				return true;
			}

			paddleHitLocation = 0;
			return false;
		}

		private void PaddleBounce(float paddleHitLocation)
		{
			float angleMax = 35;
			float angleMin = 180 - angleMax;
			float angle = Mathf.Lerp(angleMin, angleMax, paddleHitLocation);

			ballVelocity = (new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle), 0).normalized) * BallSpeed;
		}

		private bool BallIntersectsAnyBrick(out GameObject result, out Reflection reflect)
		{
			foreach (var brick in bricks)
			{
				if (BallIntersectsBrick(brick.transform, out reflect))
				{
					result = brick;
					bricks.Remove(brick);
					return true;
				}
			}

			result = null;
			reflect = 0;
			return false;
		}

		private bool BallIntersectsBrick(Transform brick, out Reflection reflect)
		{
			var r = BallSize / 2;
			var angleR = 0.707f * r;
			var ballX = ball.localPosition.x;
			var ballY = ball.localPosition.y;

			var rectX = brick.localPosition.x;
			var rectY = brick.localPosition.y;
			var w2 = BrickWidth / 2;
			var h2 = BrickHeight / 2;

			var points = new Vector2[] {
				new Vector2(ballX + r, ballY),
				new Vector2(ballX - r, ballY),
				new Vector2(ballX, ballY + r),
				new Vector2(ballX, ballY - r),
				new Vector2(ballX + angleR, ballY + angleR),
				new Vector2(ballX - angleR, ballY + angleR),
				new Vector2(ballX + angleR, ballY - angleR),
				new Vector2(ballX - angleR, ballY - angleR),
			};
			var reflects = new Reflection[] {
				Reflection.Horizontal,
				Reflection.Horizontal,
				Reflection.Vertical,
				Reflection.Vertical,
				Reflection.Horizontal,
				Reflection.Horizontal,
				Reflection.Horizontal,
				Reflection.Horizontal,
			};

			for (var i = 0; i < points.Length; ++i)
			{
				var point = points[i];
				if (PointIntersectRect(point.x, point.y, rectX - w2, rectX + w2, rectY + h2, rectY - h2))
				{
					reflect = reflects[i];
					return true;
				}
			}

			reflect = 0;
			return false;
		}

		private bool PointIntersectRect(float x, float y, float left, float right, float top, float bottom)
		{
			if (x < left || x > right || y > top || y < bottom)
			{
				return false;
			}
			return true;
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (ballAttachedToPaddle)
			{
				ballAttachedToPaddle = false;
				var r = (UnityEngine.Random.Range(0.0f, 1.0f) > 0.5f);
				ballVelocity = (new Vector3(r ? 1 : -1, 1).normalized) * BallSpeed;
			}
		}

		public void OnPointerHover(PointerEventData eventData)
		{
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.enterEventCamera, out Vector2 point))
			{
				SetPaddlePosition(point.x);
			}
		}

		private void SetPaddlePosition(float pointerX)
		{
			var max = (GameWidth / 2) - (PaddleWidth / 2);
			var min = -max;
			var x = Mathf.Clamp(pointerX, min, max);
			var y = (-GameHeight / 2) + PaddleHeight;
			paddle.localPosition = new Vector3(x, y, 0);

			if (ballAttachedToPaddle)
			{
				ball.localPosition = paddle.localPosition + new Vector3(0, (PaddleHeight + BallSize) / 2);
			}
		}



		public static SecretGame Create(Transform parent)
		{
			var game = new GameObject("Game", typeof(RectTransform)).AddComponent<SecretGame>();
			RectTransformExtensions.SetParams(game.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
			RectTransformExtensions.SetSize(game.rectTransform, GameWidth, GameHeight);
			game.rectTransform.anchoredPosition = new Vector2(0, 30);

			var lockerPrefab = Resources.Load<GameObject>("Submarine/Build/SmallLocker");
			var textPrefab = lockerPrefab.GetComponentInChildren<Text>();
			textPrefab.fontSize = 8;
			textPrefab.color = HabitatControlPanel.ScreenContentColor;

			game.text = GameObject.Instantiate(textPrefab);
			game.text.fontSize = 16;
			game.text.gameObject.name = "Text";
			game.text.rectTransform.SetParent(game.transform, false);
			RectTransformExtensions.SetSize(game.text.rectTransform, GameWidth, 50);
			game.text.rectTransform.anchoredPosition = new Vector2(0, -10);

			Destroy(lockerPrefab);

			return game;
		}
	}
}
