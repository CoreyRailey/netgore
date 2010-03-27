﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using NetGore;
using NetGore.EditorTools;

namespace DemoGame.EditorTools
{
    public partial class AllianceIDListForm : Form
    {
        readonly List<AllianceID> _list;

        AllianceID? _selectedItem;

        /// <summary>
        /// Initializes a new instance of the <see cref="AllianceIDListForm"/> class.
        /// </summary>
        /// <param name="list">The list of <see cref="AllianceID"/>s and amounts.</param>
        /// <exception cref="ArgumentNullException"><paramref name="list"/> is null.</exception>
        public AllianceIDListForm(List<AllianceID> list)
        {
            if (DesignMode)
                return;

            if (list == null)
                throw new ArgumentNullException("list");

            _list = list;

            RequireDistinct = true;

            InitializeComponent();

            lstItems.Items.AddRange(list.Cast<object>().ToArray());
        }

        /// <summary>
        /// Gets or sets if the <see cref="ItemTemplateID"/>s must be distinct. Default value is true.
        /// </summary>
        public bool RequireDistinct { get; set; }

        /// <summary>
        /// Handles the Click event of the btnAdd control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void btnAdd_Click(object sender, EventArgs e)
        {
            // Validate
            if (!_selectedItem.HasValue)
            {
                MessageBox.Show("You must select an item to add first.");
                return;
            }

            if (RequireDistinct)
            {
                if (lstItems.Items.OfType<AllianceID>().Any(x => x == _selectedItem.Value))
                {
                    MessageBox.Show("That item is already in the list.");
                    _selectedItem = null;
                    return;
                }
            }

            // Add
            lstItems.Items.Add(_selectedItem.Value);

            // Select the new item
            lstItems.SelectedItem = _selectedItem.Value;

            if (RequireDistinct)
            {
                _selectedItem = null;
                txtItem.Text = string.Empty;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnBrowse control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var f = new AllianceUITypeEditorForm(null))
            {
                // If we require distinct, skip items we already have in the list
                if (RequireDistinct)
                {
                    var listItems = lstItems.Items.OfType<AllianceID>().ToImmutable();
                    f.SkipItems = (x => listItems.Any(y => y == x.ID));
                }

                if (f.ShowDialog(this) != DialogResult.OK)
                    return;

                var item = f.SelectedItem;
                _selectedItem = item.ID;

                txtItem.Text = item.ID + " [" + item.Name + "]";
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Closing"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.ComponentModel.CancelEventArgs"/> that contains the event data.</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            if (DesignMode)
                return;

            _list.Clear();
            _list.AddRange(lstItems.Items.OfType<AllianceID>());

            base.OnClosing(e);
        }
    }
}