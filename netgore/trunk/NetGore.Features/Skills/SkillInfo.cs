﻿using System;
using System.Linq;
using NetGore;
using NetGore.IO;

namespace NetGore.Features.Skills
{
    /// <summary>
    /// Contains the information about individual skills.
    /// </summary>
    public class SkillInfo<T> : IPersistable
    {
        /// <summary>
        /// Gets or sets (protected) the cooldown group of skills the skill belongs to.
        /// </summary>
        [SyncValue]
        public byte CooldownGroup { get; protected set; }

        /// <summary>
        /// Gets or sets (protected) the description of the skill.
        /// </summary>
        [SyncValue]
        public string Description { get; protected set; }

        /// <summary>
        /// Gets or sets (protected) the name to display for the skill.
        /// </summary>
        [SyncValue]
        public string DisplayName { get; protected set; }

        /// <summary>
        /// Gets or sets (protected) the icon to display for the skill.
        /// </summary>
        [SyncValue]
        public GrhIndex Icon { get; protected set; }

        /// <summary>
        /// Gets or sets (protected) the actual type of skill that this information is for.
        /// </summary>
        [SyncValue]
        public T Value { get; protected set; }

        /// <summary>
        /// Reads the state of the object from an <see cref="IValueReader"/>. Values should be read in the exact
        /// same order as they were written.
        /// </summary>
        /// <param name="reader">The <see cref="IValueReader"/> to read the values from.</param>
        public virtual void ReadState(IValueReader reader)
        {
            PersistableHelper.Read(this, reader);
        }

        /// <summary>
        /// Writes the state of the object to an <see cref="IValueWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="IValueWriter"/> to write the values to.</param>
        public virtual void WriteState(IValueWriter writer)
        {
            PersistableHelper.Write(this, writer);
        }
    }
}