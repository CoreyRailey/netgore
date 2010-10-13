﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using DemoGame.Client;
using DemoGame.Server.Queries;
using log4net;
using NetGore.IO;

namespace DemoGame.Editor
{
    public static class MapHelper
    {
        static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Deletes a map.
        /// </summary>
        /// <param name="map">The map to delete.</param>
        /// <param name="showConfirmation">If true, a confirmation will be shown to make sure the user wants to
        /// perform this operation.</param>
        public static void DeleteMap(Map map, bool showConfirmation = true)
        {
            try
            {
                if (map == null)
                    return;

                // Show the confirmation message
                if (showConfirmation)
                {
                    const string confirmMsg = "Are you sure you wish to delete map `{0}`? This cannot be undone!";
                    if (MessageBox.Show(string.Format(confirmMsg, map), "Delete map?", MessageBoxButtons.YesNo) == DialogResult.No)
                        return;
                }

                // Delete the map file
                var path = MapBase.GetMapFilePath(ContentPaths.Dev, map.ID);
                if (File.Exists(path))
                    File.Delete(path);

                // Update the database
                GlobalState.Instance.DbController.GetQuery<DeleteMapQuery>().Execute(map.ID);

                // Delete successful
                if (showConfirmation)
                {
                    const string deletedMsg = "Successfully deleted map `{0}`!";
                    MessageBox.Show(string.Format(deletedMsg, map), "Map deleted", MessageBoxButtons.OK);
                }
            }
            catch (Exception ex)
            {
                const string errmsg = "Failed to delete map `{0}`. Exception: {1}";
                if (log.IsErrorEnabled)
                    log.ErrorFormat(errmsg, map, ex);
                Debug.Fail(string.Format(errmsg, map, ex));
            }
        }

        /// <summary>
        /// Saves a map.
        /// </summary>
        /// <param name="map">The map to save.</param>
        /// <param name="showConfirmation">If true, a confirmation will be shown to make sure the user wants to
        /// perform this operation.</param>
        public static void SaveMap(Map map, bool showConfirmation = true)
        {
            try
            {
                if (map == null)
                    return;

                // Show confirmation
                if (showConfirmation)
                {
                    const string confirmMsg = "Are you sure you wish to save the changes to map `{0}`?";
                    if (MessageBox.Show(string.Format(confirmMsg, map), "Save map?", MessageBoxButtons.YesNo) == DialogResult.No)
                        return;
                }

                // Add the MapGrh-bound walls
                var extraWalls = GlobalState.Instance.MapGrhWalls.CreateWallList(map.MapGrhs);
                foreach (var wall in extraWalls)
                {
                    map.AddEntity(wall);
                }

                // Save the map
                map.Save(ContentPaths.Dev, MapEditorDynamicEntityFactory.Instance);

                // Update the database
                GlobalState.Instance.DbController.GetQuery<InsertMapQuery>().Execute(map);

                // Pull the MapGrh-bound walls back out
                foreach (var wall in extraWalls)
                {
                    map.RemoveEntity(wall);
                }

                // Save successful
                if (showConfirmation)
                {
                    const string savedMsg = "Successfully saved the changes to map `{0}`!";
                    MessageBox.Show(string.Format(savedMsg, map), "Map saved", MessageBoxButtons.OK);
                }
            }
            catch (Exception ex)
            {
                const string errmsg = "Failed to save map `{0}`. Exception: {1}";
                if (log.IsErrorEnabled)
                    log.ErrorFormat(errmsg, map, ex);
                Debug.Fail(string.Format(errmsg, map, ex));
            }
        }

        /// <summary>
        /// Saves a map as a new map.
        /// </summary>
        /// <param name="map">The map to save.</param>
        /// <param name="showConfirmation">If true, a confirmation will be shown to make sure the user wants to
        /// perform this operation.</param>
        public static void SaveMapAs(Map map, bool showConfirmation = true)
        {
            try
            {
                if (map == null)
                    return;

                var newID = MapBase.GetNextFreeIndex(ContentPaths.Dev);

                // Confirm save
                if (showConfirmation)
                {
                    const string confirmMsg = "Are you sure you wish to save map `{0}` as a new map (with ID `{1}`)?";
                    if (MessageBox.Show(string.Format(confirmMsg, map, newID), "Save map as?", MessageBoxButtons.YesNo) == DialogResult.No)
                        return;
                }

                // Change the map ID
                map.ChangeID(newID);

                // Save
                SaveMap(map, false);

                // Save successful
                if (showConfirmation)
                {
                    const string savedMsg = "Successfully saved the map `{0}` as a new map!";
                    MessageBox.Show(string.Format(savedMsg, map), "Map successfully saved as a new map", MessageBoxButtons.OK);
                }
            }
            catch (Exception ex)
            {
                const string errmsg = "Failed to save map `{0}` as a new map. Exception: {1}";
                if (log.IsErrorEnabled)
                    log.ErrorFormat(errmsg, map, ex);
                Debug.Fail(string.Format(errmsg, map, ex));
            }
        }
    }
}