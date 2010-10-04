﻿using System;
using System.Collections.Generic;
using NetGore.EditorTools;
using NetGore.Graphics;

namespace DemoGame.Editor
{
    /// <summary>
    /// A <see cref="ToolBase"/> that displays the <see cref="ScreenGrid"/> for an <see cref="IDrawableMap"/>.
    /// </summary>
    public class MapGridTool : Tool
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapGridTool"/> class.
        /// </summary>
        /// <param name="toolManager">The <see cref="ToolManager"/>.</param>
        public MapGridTool(ToolManager toolManager) : base(toolManager, "Map Grid", ToolBarControlType.Button, ToolBarVisibility.Map)
        {
            var btn = ToolBarControl.ControlSettings.AsButtonSettings();
            btn.CheckOnClick = true;
            btn.Checked = IsEnabled;
            btn.CheckedChanged += btn_CheckedChanged;
        }

        /// <summary>
        /// When overridden in the derived class, allows for handling the <see cref="ToolBase.IsEnabledChanged"/> event.
        /// </summary>
        /// <param name="oldValue">The old (previous) value.</param>
        /// <param name="newValue">The new (current) value.</param>
        protected override void OnIsEnabledChanged(bool oldValue, bool newValue)
        {
            base.OnIsEnabledChanged(oldValue, newValue);

            // Ensure the Checked value on the control is the same as the IsEnabled value
            ToolBarControl.ControlSettings.AsButtonSettings().Checked = newValue;
        }

        /// <summary>
        /// When overridden in the derived class, gets the <see cref="IMapDrawingExtension"/>s that are used by this
        /// <see cref="ToolBase"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="IMapDrawingExtension"/>s used by this <see cref="ToolBase"/>. Can be null or empty if none
        /// are used. Default is null.
        /// </returns>
        protected override IEnumerable<IMapDrawingExtension> GetMapDrawingExtensions()
        {
            return new IMapDrawingExtension[] { new MapGridDrawingExtension() };
        }

        /// <summary>
        /// Handles the CheckedChanged event of the <see cref="ToolBase.ToolBarControl"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void btn_CheckedChanged(object sender, EventArgs e)
        {
            IsEnabled = ToolBarControl.ControlSettings.AsButtonSettings().Checked;
        }

        class MapGridDrawingExtension : MapDrawingExtension
        {
            readonly ScreenGrid _grid = new ScreenGrid();

            /// <summary>
            /// When overridden in the derived class, handles drawing to the map after all of the map drawing finishes.
            /// </summary>
            /// <param name="map">The map the drawing is taking place on.</param>
            /// <param name="spriteBatch">The <see cref="ISpriteBatch"/> to draw to.</param>
            /// <param name="camera">The <see cref="ICamera2D"/> that describes the view of the map being drawn.</param>
            protected override void HandleDrawAfterMap(IDrawableMap map, ISpriteBatch spriteBatch, ICamera2D camera)
            {
                _grid.Draw(spriteBatch, camera);
            }
        }
    }
}