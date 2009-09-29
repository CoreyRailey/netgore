﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetGore.RPGComponents
{
    public class BodyInfoManager
    {
        static readonly BodyInfoManager _instance;
        static BodyInfo[] _bodyInfo;

        /// <summary>
        /// Initializes the <see cref="BodyInfoManager"/> class.
        /// </summary>
        static BodyInfoManager()
        {
            _instance = new BodyInfoManager();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BodyInfoManager"/> class.
        /// </summary>
        BodyInfoManager()
        {
            PathString path = ContentPaths.Build.Data.Join("bodies.xml");
            _bodyInfo = BodyInfo.Load(path);
        }

        public static BodyInfoManager Instance { get { return _instance; } }

        /// <summary>
        /// Gets the <see cref="BodyInfo"/> at the given <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index of the <see cref="BodyInfo"/> to get.</param>
        /// <returns>The <see cref="BodyInfo"/> at the given <paramref name="index"/>, or null if the
        /// <see cref="BodyInfo"/> did not exist or the <paramref name="index"/> was invalid.</returns>
        public BodyInfo this[BodyIndex index]
        {
            get
            {
                int i = (int)index;
                if (i < 0 || i >= _bodyInfo.Length)
                    return null;

                return _bodyInfo[i];
            }
        }
    }
}
