using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NetGore;
using NetGore.Graphics;

namespace DemoGame.Client
{
    /// <summary>
    /// Client character
    /// </summary>
    public class Character : CharacterEntity, IGetTime, IDrawableEntity
    {
        readonly EntityInterpolator _interpolator = new EntityInterpolator();
        string _currSkelSet;

        /// <summary>
        /// The time that Draw() was last called.
        /// </summary>
        int _lastDrawnTime;

        CharacterState _lastState = CharacterState.Idle;
        Map _map;
        EventHandler _onLoopHandler;
        SkeletonAnimation _skelAnim = null;
        SkeletonManager _skelManager;

        /// <summary>
        /// Gets the location at which the character is to be drawn
        /// </summary>
        public Vector2 DrawPosition
        {
            get { return Interpolator.DrawPosition; }
        }

        public byte HPPercent { get; set; }

        public EntityInterpolator Interpolator
        {
            get { return _interpolator; }
        }

        public byte MPPercent { get; set; }

        /// <summary>
        /// Gets the map the character belongs to
        /// </summary>
        public Map Parent
        {
            get { return _map; }
        }

        public Character()
        {
        }

        /// <summary>
        /// Character constructor
        /// </summary>
        /// <param name="map">Map the character belongs to</param>
        /// <param name="position">Position of the character</param>
        /// <param name="bodyInfo">BodyInfo defining the character's body</param>
        /// <param name="skelManager">SkeletonManager used to load skeleton information</param>
        public Character(Map map, Vector2 position, BodyInfo bodyInfo, SkeletonManager skelManager)
        {
            _map = map;
            _skelManager = skelManager;
            _onLoopHandler = skelAnim_OnLoop;
            BodyInfo = bodyInfo;

            // Set up the skeleton
            _currSkelSet = BodyInfo.Stand;
            _skelAnim = new SkeletonAnimation(GetTime(), _skelManager.LoadSet(_currSkelSet));
            _skelAnim.SkeletonBody = new SkeletonBody(_skelManager.LoadBodyInfo(BodyInfo.Body), _skelAnim.Skeleton);

            // Set the collision box and position
            CB = new CollisionBox(BodyInfo.Width, BodyInfo.Height);
            Position = position;
        }

        /// <summary>
        /// Makes the character show an attack
        /// </summary>
        public void Attack()
        {
            SkeletonSet set = _skelManager.LoadSet(BodyInfo.Punch);
            set = SkeletonAnimation.CreateSmoothedSet(set, _skelAnim.Skeleton);
            SkeletonAnimation mod = new SkeletonAnimation(GetTime(), set);
            _skelAnim.AddModifier(mod);
        }

        /// <summary>
        /// Changes the SkeletonSet used if different
        /// </summary>
        /// <param name="setName">Name of the set to use</param>
        void ChangeSet(string setName)
        {
            // Check that the set has changed
            if (setName == _currSkelSet)
                return;

            _skelAnim.ChangeSet(_skelManager.LoadSet(setName));
            _currSkelSet = setName;
        }

        void DrawSPBar(SpriteBatch sb, byte percent, byte index, Color color)
        {
            const float spBarWidth = 55;
            const float spBarHeight = 6;

            Vector2 pos = new Vector2(Center.X - spBarWidth / 2f, Position.Y + CB.Height + (spBarHeight * index));

            Rectangle border = new Rectangle((int)pos.X, (int)pos.Y, (int)spBarWidth, (int)spBarHeight);
            Rectangle bar = border;
            bar.Width = (int)((spBarWidth * (percent / 100.0f))).Clamp(0, spBarWidth);

            XNARectangle.Draw(sb, border, new Color(0, 0, 0, 0), Color.Black);
            XNARectangle.Draw(sb, bar, color);
        }

        /// <summary>
        /// Gets the position for the Camera to focus on this Character.
        /// </summary>
        /// <returns>Position for the Camera to focus on this Character.</returns>
        public Vector2 GetCameraPos()
        {
            Vector2 p1 = DrawPosition + (Size / 2.0f) - (GameData.ScreenSize / 2.0f);
            return new Vector2((float)Math.Round(p1.X), (float)Math.Round(p1.Y));
        }

        /// <summary>
        /// Gets the map interface used by the Character, primarily for when referencing by the CharacterEntity.
        /// Can return null if the Character is not on a map, and null returns should be supported.
        /// </summary>
        /// <returns>Map interface used by the Character. Can be null.</returns>
        protected override IMap GetIMap()
        {
            return _map;
        }

        public void Initialize(Map map, SkeletonManager skelManager)
        {
            // HACK: This is quite a dirty way to do this
            _map = map;
            _skelManager = skelManager;
            _onLoopHandler = skelAnim_OnLoop;
            Interpolator.Teleport(Position);

            // Set up the skeleton
            _currSkelSet = BodyInfo.Stand;
            _skelAnim = new SkeletonAnimation(GetTime(), _skelManager.LoadSet(_currSkelSet));
            _skelAnim.SkeletonBody = new SkeletonBody(_skelManager.LoadBodyInfo(BodyInfo.Body), _skelAnim.Skeleton);
        }

        /// <summary>
        /// Sets the character's heading
        /// </summary>
        /// <param name="newHeading">New heading for the character</param>
        public override void SetHeading(Direction newHeading)
        {
            if (Heading == newHeading)
                return;

            base.SetHeading(newHeading);
        }

        /// <summary>
        /// Handles OnLoop events for the SkeletonAnimation to set the character's animation back to normal
        /// </summary>
        void skelAnim_OnLoop(object sender, EventArgs e)
        {
            _skelAnim.OnLoop -= _onLoopHandler;
            UpdateAnimation();
        }

        /// <summary>
        /// Updates the character's sprites
        /// </summary>
        void UpdateAnimation()
        {
            // Only update if the state has changed
            if (_lastState == State)
                return;

            _lastState = State;
            switch (State)
            {
                case CharacterState.Idle:
                    ChangeSet(BodyInfo.Stand);
                    break;

                case CharacterState.Falling:
                case CharacterState.FallingLeft:
                case CharacterState.FallingRight:
                    ChangeSet(BodyInfo.Fall);
                    break;

                case CharacterState.Jumping:
                case CharacterState.JumpingLeft:
                case CharacterState.JumpingRight:
                    ChangeSet(BodyInfo.Jump);
                    break;

                case CharacterState.WalkingLeft:
                case CharacterState.WalkingRight:
                    ChangeSet(BodyInfo.Walk);
                    break;
            }
        }

        #region IDrawableEntity Members

        /// <summary>
        /// Draws the character
        /// </summary>
        /// <param name="sb">SpriteBatch to draw the character with</param>
        public void Draw(SpriteBatch sb)
        {
            if (_skelAnim == null)
                return;

            // Get the delta time
            int currentTime = GetTime();
            int deltaTime = Math.Min(currentTime - _lastDrawnTime, GameData.MaxDrawDeltaTime);
            _lastDrawnTime = currentTime;

            // Update the drawable stuff
            UpdateAnimation();
            _skelAnim.Update(currentTime);
            _interpolator.Update(this, deltaTime);

            // Draw the character body
            SpriteEffects se = (Heading == Direction.East ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
            Vector2 p = DrawPosition;
            p.X += BodyInfo.Width / 2f;
            p.Y += BodyInfo.Height;

            _skelAnim.Draw(sb, p, se);

            // Draw the HP/MP
            DrawSPBar(sb, HPPercent, 0, new Color(255, 0, 0, 175));
            DrawSPBar(sb, MPPercent, 1, new Color(0, 0, 255, 175));
        }

        /// <summary>
        /// Checks if in view of the specified camera
        /// </summary>
        /// <param name="camera">Camera to check if in view of</param>
        /// <returns>True if in view of the camera, else false</returns>
        public bool InView(Camera2D camera)
        {
            return camera.InView(this);
        }

        /// <summary>
        /// Notifies when the Character's render layer changes (which is never for Characters)
        /// </summary>
        public event MapRenderLayerChange OnChangeRenderLayer
        {
            add { }
            remove { }
        }

        /// <summary>
        /// Gets the MapRenderLayer for the Character
        /// </summary>
        public MapRenderLayer MapRenderLayer
        {
            get
            {
                // Characters are always on the Character layer
                return MapRenderLayer.Chararacter;
            }
        }

        #endregion

        #region IGetTime Members

        /// <summary>
        /// Gets the current game time where time 0 is when the application started
        /// </summary>
        /// <returns>Current game time in milliseconds</returns>
        public int GetTime()
        {
            return _map.GetTime();
        }

        #endregion
    }
}