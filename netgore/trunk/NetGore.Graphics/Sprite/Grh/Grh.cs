using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using log4net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NetGore.Graphics
{
    /// <summary>
    /// Provides an instance of a single sprite defined by a <see cref="GrhData"/>.
    /// </summary>
    public class Grh : ISprite
    {
        static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Type of animation the Grh uses
        /// </summary>
        AnimType _anim;

        /// <summary>
        /// Current frame (if animated)
        /// </summary>
        float _frame = 0;

        /// <summary>
        /// Root GrhData referenced by this Grh
        /// </summary>
        GrhData _grhData;

        /// <summary>
        /// Tick count at which the Grh was last updated (only needed if animated)
        /// </summary>
        int _lastUpdated = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="Grh"/> class.
        /// </summary>
        /// <param name="grhIndex">Index of the stationary Grh.</param>
        public Grh(GrhIndex grhIndex)
        {
            SetGrh(grhIndex);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Grh"/> class.
        /// </summary>
        /// <param name="grhData">GrhData to create from.</param>
        public Grh(GrhData grhData)
        {
            SetGrh(grhData);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Grh"/> class.
        /// </summary>
        public Grh()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Grh"/> class.
        /// </summary>
        /// <param name="grhIndex">Index of the Grh.</param>
        /// <param name="anim">Animation type.</param>
        /// <param name="currentTime">Current time.</param>
        public Grh(GrhIndex grhIndex, AnimType anim, int currentTime)
        {
            SetGrh(grhIndex, anim, currentTime);
        }

        /// <summary>
        /// Creates a Grh.
        /// </summary>
        /// <param name="grhData">GrhData to create from.</param>
        /// <param name="anim">Animation type.</param>
        /// <param name="currentTime">Current time.</param>
        public Grh(GrhData grhData, AnimType anim, int currentTime)
        {
            SetGrh(grhData, anim, currentTime);
        }

        /// <summary>
        /// Gets or sets the animation type for the Grh.
        /// </summary>
        public AnimType AnimType
        {
            get { return _anim; }
            set { _anim = value; }
        }

        /// <summary>
        /// Gets the GrhData to use for drawing based on the current frame. 
        /// </summary>
        public StationaryGrhData CurrentGrhData
        {
            get
            {
                if (GrhData == null)
                    return null;

                if (GrhData is AnimatedGrhData)
                    return ((AnimatedGrhData)GrhData).GetFrame((int)_frame);
                else if (GrhData is StationaryGrhData)
                    return (StationaryGrhData)GrhData;
                else if (GrhData is AutomaticAnimatedGrhData)
                    return ((AutomaticAnimatedGrhData)GrhData).GetFrame((int)_frame);
                else
                    throw new UnsupportedGrhDataTypeException(GrhData);
            }
        }

        /// <summary>
        /// Gets the current frame of the animation.
        /// </summary>
        public float Frame
        {
            get { return _frame; }
        }

        /// <summary>
        /// Gets the root GrhData referenced by this Grh.
        /// </summary>
        public GrhData GrhData
        {
            get { return _grhData; }
        }

        /// <summary>
        /// Gets the tick count at which the Grh was last updated (only for animated Grhs).
        /// </summary>
        public int LastUpdated
        {
            get { return _lastUpdated; }
        }

        /// <summary>
        /// Gets the size of the current frame in pixels.
        /// </summary>
        public Vector2 Size
        {
            get { return CurrentGrhData != null ? CurrentGrhData.Size : Vector2.Zero; }
        }

        /// <summary>
        /// Performs a detailed check to ensure the Grh can be drawn without problem. This should be called before
        /// any drawing is done!
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch to draw to.</param>
        /// <returns>True if it is safe for the Grh to draw to the <paramref name="spriteBatch"/>, else false.</returns>
        bool CanDrawGrh(SpriteBatch spriteBatch)
        {
            // Invalid GrhData
            if (GrhData == null)
            {
                const string errmsg = "Failed to render Grh - GrhData is null!";
                if (log.IsWarnEnabled)
                    log.Warn(errmsg);
                return false;
            }

            // Invalid texture
            if (Texture == null)
            {
                const string errmsg = "Failed to render Grh `{0}` - GrhData returning null texture for `{3}`!";
                if (log.IsWarnEnabled)
                    log.WarnFormat(errmsg, this, ((StationaryGrhData)GrhData).TextureName);
                return false;
            }

            // Invalid SpriteBatch
            if (spriteBatch == null)
            {
                const string errmsg = "Failed to render Grh `{0}` - SpriteBatch is null!";
                if (log.IsWarnEnabled)
                    log.WarnFormat(errmsg, this, GrhData.GrhIndex);
                return false;
            }

            if (spriteBatch.IsDisposed)
            {
                const string errmsg = "Failed to render Grh `{0}` - SpriteBatch is disposed!";
                if (log.IsWarnEnabled)
                    log.WarnFormat(errmsg, this, GrhData.GrhIndex);
                return false;
            }

            // All is good
            return true;
        }

        /// <summary>
        /// Draws the current frame of a Grh to an existing SpriteBatch.
        /// </summary>
        /// <param name="sb">SpriteBatch to add the draw to.</param>
        /// <param name="dest">Top-left corner pixel of the destination.</param>
        public void Draw(SpriteBatch sb, Vector2 dest)
        {
            if (!CanDrawGrh(sb))
                return;

            sb.Draw(Texture, dest, Source, Color.White);
        }

        /// <summary>
        /// Draws the current frame of a Grh to an existing SpriteBatch.
        /// </summary>
        /// <param name="sb">SpriteBatch to add the draw to.</param>
        /// <param name="dest">Top-left corner pixel of the destination.</param>
        /// <param name="color">Color of the sprite (default Color.White).</param>
        /// <param name="effect">Sprite effect to use (default SpriteEffects.None).</param>
        public void Draw(SpriteBatch sb, Vector2 dest, Color color, SpriteEffects effect)
        {
            if (!CanDrawGrh(sb))
                return;

            sb.Draw(Texture, dest, Source, color, 0, Vector2.Zero, 1.0f, effect, 0);
        }

        /// <summary>
        /// Draws the current frame of a Grh to an existing SpriteBatch.
        /// </summary>
        /// <param name="sb">SpriteBatch to add the draw to.</param>
        /// <param name="dest">Top-left corner pixel of the destination.</param>
        /// <param name="color">Color of the sprite (default Color.White).</param>
        /// <param name="effect">Sprite effect to use (default SpriteEffects.None).</param>
        /// <param name="rotation">The angle, in radians, to rotate the sprite around the origin (default 0).</param>
        /// <param name="origin">The origin of the sprite to rotate around (default Vector2.Zero).</param>
        /// <param name="scale">Uniform multiply by which to scale the width and height.</param>
        public void Draw(SpriteBatch sb, Vector2 dest, Color color, SpriteEffects effect, float rotation, Vector2 origin,
                         float scale)
        {
            if (!CanDrawGrh(sb))
                return;

            sb.Draw(Texture, dest, Source, color, rotation, origin, scale, effect, 0);
        }

        /// <summary>
        /// Draws the current frame of a Grh to an existing SpriteBatch.
        /// </summary>
        /// <param name="sb">SpriteBatch to add the draw to.</param>
        /// <param name="dest">Top-left corner pixel of the destination.</param>
        /// <param name="color">Color of the sprite (default Color.White).</param>
        /// <param name="effect">Sprite effect to use (default SpriteEffects.None).</param>
        /// <param name="rotation">The angle, in radians, to rotate the sprite around the origin (default 0).</param>
        /// <param name="origin">The origin of the sprite to rotate around (default Vector2.Zero).</param>
        /// <param name="scale">Vector2 defining the scale.</param>
        public void Draw(SpriteBatch sb, Vector2 dest, Color color, SpriteEffects effect, float rotation, Vector2 origin,
                         Vector2 scale)
        {
            if (!CanDrawGrh(sb))
                return;

            sb.Draw(Texture, dest, Source, color, rotation, origin, scale, effect, 0);
        }

        /// <summary>
        /// Draws the current frame of a Grh to an existing SpriteBatch.
        /// </summary>
        /// <param name="sb">SpriteBatch to add the draw to.</param>
        /// <param name="dest">Destination rectangle.</param>
        /// <param name="color">Color of the sprite (default Color.White).</param>
        /// <param name="effect">Sprite effect to use (default SpriteEffects.None).</param>
        /// <param name="rotation">The angle, in radians, to rotate the sprite around the origin (default 0).</param>
        /// <param name="origin">The origin of the sprite to rotate around (default Vector2.Zero).</param>
        public void Draw(SpriteBatch sb, Rectangle dest, Color color, SpriteEffects effect, float rotation, Vector2 origin)
        {
            if (!CanDrawGrh(sb))
                return;

            sb.Draw(Texture, dest, Source, color, rotation, origin, effect, 0);
        }

        /// <summary>
        /// Creates a duplicate (deep copy) of the Grh.
        /// </summary>
        /// <returns>Duplicate of the Grh.</returns>
        public Grh Duplicate()
        {
            return new Grh(_grhData, _anim, _lastUpdated) { _frame = _frame };
        }

        /// <summary>
        /// Sets the Grh to a new index.
        /// </summary>
        /// <param name="grhIndex">New Grh index to use for the stationary Grh.</param>
        public void SetGrh(GrhIndex grhIndex)
        {
            SetGrh(grhIndex, AnimType.None, 0);
        }

        /// <summary>
        /// Sets the Grh to a new index.
        /// </summary>
        /// <param name="grhData">New GrhData to use for the Grh.</param>
        /// <param name="anim">Type of animation.</param>
        /// <param name="currentTime">Current time.</param>
        public void SetGrh(GrhData grhData, AnimType anim, int currentTime)
        {
            _grhData = grhData;
            _frame = 0;
            _anim = anim;
            _lastUpdated = currentTime;
        }

        /// <summary>
        /// Sets the Grh to a new index.
        /// </summary>
        /// <param name="grhData">New GrhData to use for the stationary Grh.</param>
        public void SetGrh(GrhData grhData)
        {
            SetGrh(grhData, AnimType.None, 0);
        }

        /// <summary>
        /// Sets the Grh to a new index.
        /// </summary>
        /// <param name="grhIndex">New Grh index to use.</param>
        /// <param name="anim">Type of animation.</param>
        /// <param name="currentTime">Current time.</param>
        public void SetGrh(GrhIndex grhIndex, AnimType anim, int currentTime)
        {
            GrhData grhData = GrhInfo.GetData(grhIndex);
            if (grhData == null && grhIndex != 0)
            {
                const string errmsg = "Failed to set Grh - GrhIndex `{0}` does not exist.";
                Debug.Fail(string.Format(errmsg, grhIndex));
                if (log.IsErrorEnabled)
                    log.ErrorFormat(errmsg, grhIndex);
                return;
            }
            SetGrh(grhData, anim, currentTime);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return GrhData.Categorization + " [" + GrhData.GrhIndex + "]";
        }

        #region ISprite Members

        /// <summary>
        /// Performs updating for when the <see cref="GrhData"/> is an <see cref="AnimatedGrhData"/>.
        /// </summary>
        /// <param name="currentTime">The current time.</param>
        void UpdateAnimatedGrhData(int currentTime)
        {
            var c = (AnimatedGrhData)GrhData;
            UpdateFrameIndex(currentTime, c.Speed, c.FramesCount);
        }

        /// <summary>
        /// Performs updating for when the <see cref="GrhData"/> is an <see cref="AutomaticAnimatedGrhData"/>.
        /// </summary>
        /// <param name="currentTime">The current time.</param>
        void UpdateAutomaticAnimatedGrhData(int currentTime)
        {
            var c = (AutomaticAnimatedGrhData)GrhData;
            UpdateFrameIndex(currentTime, c.Speed, c.FramesCount);
        }

        /// <summary>
        /// Updates the current frame.
        /// </summary>
        /// <param name="currentTime">The current time.</param>
        /// <param name="animateSpeed">The animation speed.</param>
        /// <param name="framesCount">The number of frames.</param>
        void UpdateFrameIndex(int currentTime, float animateSpeed, int framesCount)
        {
            // Store the temporary new frame
            float tmpFrame = _frame + ((currentTime - _lastUpdated) * animateSpeed);

            // Check if the frame limit has been exceeded
            if (tmpFrame >= framesCount)
            {
                if (_anim == AnimType.LoopOnce)
                {
                    // The animation was only looping once, so end it and set at the first frame
                    _anim = AnimType.None;
                    _frame = 0;
                    return;
                }
                else
                {
                    // Animation is looping so get the frame back into range
                    tmpFrame = tmpFrame % framesCount;
                }
            }

            // Set the new frame
            _frame = tmpFrame;
        }

        /// <summary>
        /// Updates the Grh if it is animated.
        /// </summary>
        /// <param name="currentTime">Current total real time in total milliseconds.</param>
        public virtual void Update(int currentTime)
        {
            // Update by type
            if (_anim != AnimType.None)
            {
                if (GrhData is AnimatedGrhData)
                    UpdateAnimatedGrhData(currentTime);
                else if (GrhData is AutomaticAnimatedGrhData)
                    UpdateAutomaticAnimatedGrhData(currentTime);
            }

            // Set the last updated time to now
            _lastUpdated = currentTime;
        }

        /// <summary>
        /// Gets the source rectangle for the current frame.
        /// </summary>
        public Rectangle Source
        {
            get
            {
                var asStationary = CurrentGrhData;
                if (asStationary == null)
                    return Rectangle.Empty;

                return asStationary.SourceRect;
            }
        }

        /// <summary>
        /// Gets the texture for the current frame.
        /// </summary>
        public Texture2D Texture
        {
            get
            {
                var asStationary = CurrentGrhData;
                if (asStationary == null)
                    return null;

                return asStationary.Texture;
            }
        }

        /// <summary>
        /// Draws the current frame of a Grh to an existing SpriteBatch.
        /// </summary>
        /// <param name="sb">SpriteBatch to add the draw to.</param>
        /// <param name="dest">Top-left corner pixel of the destination.</param>
        /// <param name="color">Color of the sprite (default Color.White).</param>
        public void Draw(SpriteBatch sb, Vector2 dest, Color color)
        {
            if (!CanDrawGrh(sb))
                return;

            sb.Draw(Texture, dest, Source, color);
        }

        /// <summary>
        /// Draws the current frame of a Grh to an existing SpriteBatch.
        /// </summary>
        /// <param name="sb">SpriteBatch to add the draw to.</param>
        /// <param name="dest">Destination rectangle to draw to.</param>
        /// <param name="color">Color of the sprite (default Color.White).</param>
        public void Draw(SpriteBatch sb, Rectangle dest, Color color)
        {
            if (!CanDrawGrh(sb))
                return;

            sb.Draw(Texture, dest, Source, color);
        }

        #endregion
    }
}