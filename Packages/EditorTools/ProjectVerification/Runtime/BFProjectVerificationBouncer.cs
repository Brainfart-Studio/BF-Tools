using UnityEngine;

namespace BFTools.EditorTools.ProjectVerification
{
    public class BFProjectVerificationBouncer : MonoBehaviour
    {
        private class Ball
        {
            public Transform transform;
            public Vector2 velocity;
            public float radius;
        }

        private static readonly Color[] CmykColors =
        {
            new Color(0f, 1f, 1f),   // Cyan
            new Color(1f, 0f, 1f),   // Magenta
            new Color(1f, 1f, 0f),   // Yellow
            new Color(0f, 0f, 0f)    // Black
        };

        private const int BallCount = 3;
        private const float MinSpeed = 3f;
        private const float MaxSpeed = 6f;
        private const float MinSize = 0.5f;
        private const float MaxSize = 1.5f;

        private readonly Ball[] balls = new Ball[BallCount];
        private float halfWidth;
        private float halfHeight;

        private void Start()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                enabled = false;
                return;
            }

            cam.orthographic = true;
            halfHeight = cam.orthographicSize;
            halfWidth = halfHeight * cam.aspect;

            Sprite circleSprite = GenerateCircleSprite();

            for (int i = 0; i < BallCount; i++)
            {
                GameObject go = new GameObject($"Ball_{i}");
                go.transform.SetParent(transform);

                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = circleSprite;
                sr.color = CmykColors[Random.Range(0, CmykColors.Length)];

                float size = Random.Range(MinSize, MaxSize);
                go.transform.localScale = new Vector3(size, size, 1f);

                float speed = Random.Range(MinSpeed, MaxSpeed);
                Vector2 dir = Random.insideUnitCircle.normalized;
                if (dir == Vector2.zero)
                    dir = Vector2.right;

                go.transform.position = new Vector2(
                    Random.Range(-halfWidth, halfWidth),
                    Random.Range(-halfHeight, halfHeight));

                balls[i] = new Ball
                {
                    transform = go.transform,
                    velocity = dir * speed,
                    radius = size * 0.5f
                };
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            foreach (Ball ball in balls)
            {
                if (ball == null)
                    continue;

                Vector3 pos = ball.transform.position;
                pos += (Vector3)(ball.velocity * dt);

                if (pos.x - ball.radius < -halfWidth)
                {
                    pos.x = -halfWidth + ball.radius;
                    ball.velocity.x = -ball.velocity.x;
                }
                else if (pos.x + ball.radius > halfWidth)
                {
                    pos.x = halfWidth - ball.radius;
                    ball.velocity.x = -ball.velocity.x;
                }

                if (pos.y - ball.radius < -halfHeight)
                {
                    pos.y = -halfHeight + ball.radius;
                    ball.velocity.y = -ball.velocity.y;
                }
                else if (pos.y + ball.radius > halfHeight)
                {
                    pos.y = halfHeight - ball.radius;
                    ball.velocity.y = -ball.velocity.y;
                }

                ball.transform.position = pos;
            }
        }

        private static Sprite GenerateCircleSprite()
        {
            const int size = 128;
            Texture2D tex = new Texture2D(size, size);
            Color32[] pixels = new Color32[size * size];
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    pixels[y * size + x] = dist <= radius ? Color.white : new Color32(0, 0, 0, 0);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }
}