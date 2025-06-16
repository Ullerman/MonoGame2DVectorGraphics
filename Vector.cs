using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VectorGraphics
{
    public class PrimitiveBatch
    {
        public Texture2D whitePixel;
        public GraphicsDevice graphicsDevice;
        public PrimitiveBatch(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            CreateTextures();
        }

        public void CreateTextures()
        {
            whitePixel = new Texture2D(graphicsDevice, 1, 1);
            whitePixel.SetData(new[] { Color.White });
        }

        public abstract class Shape
        {
            public Vector2 position;
            public Color color;
            public bool filled;
            public Shape(Vector2 position, Color color, bool filled = true)
            {
                this.position = position;
                this.color = color;
                this.filled = filled;
            }
            public abstract void Draw(SpriteBatch spriteBatch, PrimitiveBatch primitiveBatch);
        }

        public class Line : Shape
        {
            public Vector2 end;
            public float width;

            public Line(Vector2 start, Vector2 end, Color color, float width)
                : base(start, color, false)
            {
                this.end = end;
                this.width = width;
            }

            public Line(Vector2 start, float angle, float distance, Color color, float width)
                : base(start, color, false)
            {
                this.end = new Vector2(
                    (float)Math.Cos(angle) * distance,
                    (float)Math.Sin(angle) * distance
                );
                this.width = width;
            }

            public override void Draw(SpriteBatch spriteBatch, PrimitiveBatch primitiveBatch)
            {
                Vector2 edge = end - position;
                float angle = (float)Math.Atan2(edge.Y, edge.X);
                spriteBatch.Draw(
                    primitiveBatch.whitePixel,
                    new Microsoft.Xna.Framework.Rectangle(
                        (int)(position.X),
                        (int)(position.Y),
                        (int)edge.Length(),
                        (int)width
                    ),
                    null,
                    color,
                    angle,
                    Vector2.Zero,
                    SpriteEffects.None,
                    0
                );
            }
        }

        public class Circle : Shape
        {
            public float radius;
            public Circle(Vector2 position, float radius, Color color, bool filled = true)
                : base(position, color, filled)
            {
                this.radius = radius;
            }

            public override void Draw(SpriteBatch spriteBatch, PrimitiveBatch primitiveBatch)
            {
                int cx = (int)position.X;
                int cy = (int)position.Y;
                int r = (int)radius;
                int x = 0;
                int y = r;
                int d = 3 - 2 * r;

                void PlotCirclePoints(int xc, int yc, int x, int y)
                {
                    spriteBatch.Draw(primitiveBatch.whitePixel, new Vector2(xc + x, yc + y), color);
                    spriteBatch.Draw(primitiveBatch.whitePixel, new Vector2(xc - x, yc + y), color);
                    spriteBatch.Draw(primitiveBatch.whitePixel, new Vector2(xc + x, yc - y), color);
                    spriteBatch.Draw(primitiveBatch.whitePixel, new Vector2(xc - x, yc - y), color);
                    spriteBatch.Draw(primitiveBatch.whitePixel, new Vector2(xc + y, yc + x), color);
                    spriteBatch.Draw(primitiveBatch.whitePixel, new Vector2(xc - y, yc + x), color);
                    spriteBatch.Draw(primitiveBatch.whitePixel, new Vector2(xc + y, yc - x), color);
                    spriteBatch.Draw(primitiveBatch.whitePixel, new Vector2(xc - y, yc - x), color);
                }
                void FillCircle(int x, int y, int r)
                {
                    for (int i = -r; i <= r; i++)
                    {
                        for (int j = -r; j <= r; j++)
                        {
                            if (i * i + j * j <= r * r)
                            {
                                spriteBatch.Draw(primitiveBatch.whitePixel, new Vector2(x + i, y + j), color);
                            }
                        }
                    }
                }


                while (y >= x)
                {
                    PlotCirclePoints(cx, cy, x, y);
                    x++;
                    if (filled)
                        FillCircle(cx, cy, r);
                    if (d > 0)
                    {
                        y--;
                        d = d + 4 * (x - y) + 10;
                    }
                    else
                    {
                        d = d + 4 * x + 6;
                    }
                }
            }
        }

        public class Rectangle : Shape
        {
            public Vector2 size;
            public Rectangle(Vector2 position, Vector2 size, Color color, bool filled = true)
                : base(position, color, filled)
            {
                this.size = size;
            }
            public Rectangle(Microsoft.Xna.Framework.Rectangle rectangle, Color color, bool filled = true)
                : base(new Vector2(rectangle.X, rectangle.Y), color, filled)
            {
                this.size = new Vector2(rectangle.Width, rectangle.Height);
            }
            public override void Draw(SpriteBatch spriteBatch, PrimitiveBatch primitiveBatch)
            {
                spriteBatch.Draw(
                    primitiveBatch.whitePixel,
                    new Microsoft.Xna.Framework.Rectangle(
                        (int)(position.X),
                        (int)(position.Y),
                        (int)size.X,
                        (int)size.Y
                    ),
                    color
                );
            }
        }

        public class RoundedRectangle : Rectangle
        {
            public float cornerRadius;
            public RoundedRectangle(Vector2 position, Vector2 size, float cornerRadius, Color color)
                : base(position, size, color)
            {
                this.cornerRadius = cornerRadius;
            }
            public override void Draw(SpriteBatch spriteBatch, PrimitiveBatch primitiveBatch)
            {
                float innerWidth = size.X - 2 * cornerRadius;
                float innerHeight = size.Y - 2 * cornerRadius;
                var centerRect = new PrimitiveBatch.Rectangle(
                    position + new Vector2(cornerRadius, cornerRadius),
                    new Vector2(innerWidth, innerHeight),
                    color
                );
                centerRect.Draw(spriteBatch, primitiveBatch);
                var topRect = new PrimitiveBatch.Rectangle(
                    position + new Vector2(cornerRadius, 0),
                    new Vector2(innerWidth, cornerRadius),
                    color
                );
                topRect.Draw(spriteBatch, primitiveBatch);
                var bottomRect = new PrimitiveBatch.Rectangle(
                    position + new Vector2(cornerRadius, size.Y - cornerRadius),
                    new Vector2(innerWidth, cornerRadius),
                    color
                );
                bottomRect.Draw(spriteBatch, primitiveBatch);
                var leftRect = new PrimitiveBatch.Rectangle(
                    position + new Vector2(0, cornerRadius),
                    new Vector2(cornerRadius, innerHeight),
                    color
                );
                leftRect.Draw(spriteBatch, primitiveBatch);
                var rightRect = new PrimitiveBatch.Rectangle(
                    position + new Vector2(size.X - cornerRadius, cornerRadius),
                    new Vector2(cornerRadius, innerHeight),
                    color
                );
                rightRect.Draw(spriteBatch, primitiveBatch);
                var topLeft = new PrimitiveBatch.Circle(
                    position + new Vector2(cornerRadius, cornerRadius),
                    cornerRadius,
                    color
                );
                topLeft.Draw(spriteBatch, primitiveBatch);
                var topRight = new PrimitiveBatch.Circle(
                    position + new Vector2(size.X - cornerRadius, cornerRadius),
                    cornerRadius,
                    color
                );
                topRight.Draw(spriteBatch, primitiveBatch);
                var bottomLeft = new PrimitiveBatch.Circle(
                    position + new Vector2(cornerRadius, size.Y - cornerRadius),
                    cornerRadius,
                    color
                );
                bottomLeft.Draw(spriteBatch, primitiveBatch);
                var bottomRight = new PrimitiveBatch.Circle(
                    position + new Vector2(size.X - cornerRadius, size.Y - cornerRadius),
                    cornerRadius,
                    color
                );
                bottomRight.Draw(spriteBatch, primitiveBatch);
            }
        }

        public class RectangleTexture
        {
            public Vector2 size;
            private Texture2D texture;
            public Texture2D CreateTexture(Vector2 size, PrimitiveBatch primitiveBatch)
            {
                this.size = size;
                texture = new Texture2D(primitiveBatch.graphicsDevice, (int)size.X, (int)size.Y);
                Color[] data = new Color[(int)size.X * (int)size.Y];
                for (int i = 0; i < data.Length; ++i)
                    data[i] = Color.White;
                texture.SetData(data);
                return texture;
            }
        }

        public class Pixel : Shape
        {
            public Pixel(Vector2 position, Color color)
                : base(position, color, false) { }
            public override void Draw(SpriteBatch spriteBatch, PrimitiveBatch primitiveBatch)
            {
                spriteBatch.Draw(primitiveBatch.whitePixel, position, color);
            }
        }
    }
}