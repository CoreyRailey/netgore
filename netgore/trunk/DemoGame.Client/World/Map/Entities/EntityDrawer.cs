using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NetGore;
using NetGore.Graphics;

namespace DemoGame.Client
{
    /// <summary>
    /// Assists in drawing different types of Entities.
    /// </summary>
    public static class EntityDrawer
    {
        /// <summary>
        /// Color for an arrow
        /// </summary>
        static readonly Color _arrowColor = new Color(255, 255, 255, 150);

        /// <summary>
        /// Border color of the Entity
        /// </summary>
        static readonly Color _borderColor = new Color(0, 0, 0, 255);

        /// <summary>
        /// Basic Entity color
        /// </summary>
        static readonly Color _entityColor = new Color(0, 0, 255, 150);

        /// <summary>
        /// Color of the destination of a TeleportEntity
        /// </summary>
        static readonly Color _teleDestColor = new Color(255, 0, 0, 75);

        /// <summary>
        /// Color of the source TeleportEntity
        /// </summary>
        static readonly Color _teleSourceColor = new Color(0, 255, 0, 150);

        /// <summary>
        /// Color of WallEntities
        /// </summary>
        static readonly Color _wallColor = new Color(255, 255, 255, 100);

        /// <summary>
        /// Draws an Entity
        /// </summary>
        /// <param name="sb">SpriteBatch to draw to</param>
        /// <param name="entity">Entity to draw</param>
        public static void Draw(SpriteBatch sb, Entity entity)
        {
            WallEntityBase wallEntity;
            TeleportEntity teleportEntity;

            // Check for a different entity type
            if ((wallEntity = entity as WallEntityBase) != null)
                Draw(sb, wallEntity);
            else if ((teleportEntity = entity as TeleportEntity) != null)
                Draw(sb, teleportEntity);
            else
            {
                // Draw a normal entity using the CollisionBox
                Draw(sb, entity.ToRectangle(), _entityColor);
            }
        }

        /// <summary>
        /// Draws a TeleportEntity
        /// </summary>
        /// <param name="sb">SpriteBatch to draw to</param>
        /// <param name="tele">TeleportEntity to draw</param>
        public static void Draw(SpriteBatch sb, TeleportEntity tele)
        {
            // Source
            Draw(sb, tele.ToRectangle(), _teleSourceColor);

            // Dest
            Rectangle destRect = new Rectangle((int)tele.Destination.X, (int)tele.Destination.Y, (int)tele.Size.X, (int)tele.Size.Y);
            Draw(sb, destRect, _teleDestColor);

            // Arrow
            Vector2 centerOffset = tele.Size / 2;
            XNAArrow.Draw(sb, tele.Position + centerOffset, tele.Destination + centerOffset, _arrowColor);
        }

        /// <summary>
        /// Draws a WallEntity
        /// </summary>
        /// <param name="sb">SpriteBatch to draw to</param>
        /// <param name="wall">WallEntity to draw</param>
        public static void Draw(SpriteBatch sb, WallEntityBase wall)
        {
            Draw(sb, wall, Vector2.Zero);
        }

        /// <summary>
        /// Draws a WallEntity
        /// </summary>
        /// <param name="sb">SpriteBatch to draw to</param>
        /// <param name="wall">WallEntity to draw</param>
        /// <param name="offset">Offset to draw the WallEntity at from the original position</param>
        public static void Draw(SpriteBatch sb, WallEntityBase wall, Vector2 offset)
        {
            // Find the positon to draw to
            Vector2 p = wall.Position + offset;
            Rectangle dest = new Rectangle((int)p.X, (int)p.Y, (int)wall.Size.X, (int)wall.Size.Y);

            // Draw the collision area
            XNARectangle.Draw(sb, dest, _wallColor);
        }

        /// <summary>
        /// Draws a <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="sb">SpriteBatch to draw to.</param>
        /// <param name="rect">The <see cref="Rectangle"/> to draw.</param>
        /// <param name="color">Color to draw the CollisionBox.</param>
        static void Draw(SpriteBatch sb, Rectangle rect, Color color)
        {
            XNARectangle.Draw(sb, rect, color, _borderColor);
        }
    }
}