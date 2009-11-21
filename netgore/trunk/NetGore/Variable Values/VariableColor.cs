﻿using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NetGore
{
    [TypeConverter(typeof(VariableColorConverter))]
    public struct VariableColor : IVariableValue<Color>
    {
        Color _max;
        Color _min;

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableColor"/> struct.
        /// </summary>
        /// <param name="value">The value for both the <see cref="Min"/> and <see cref="Max"/>.</param>
        public VariableColor(Color value)
        {
            _min = value;
            _max = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableColor"/> struct.
        /// </summary>
        /// <param name="min">The min.</param>
        /// <param name="max">The max.</param>
        public VariableColor(Color min, Color max)
        {
            _min = min;
            _max = max;

            if (min.R > max.R)
            {
                _min.R = max.R;
                _max.R = min.R;
            }

            if (min.G > max.G)
            {
                _min.G = max.G;
                _max.G = min.G;
            }

            if (min.B > max.B)
            {
                _min.B = max.B;
                _max.B = min.B;
            }

            if (min.A > max.A)
            {
                _min.A = max.A;
                _max.A = min.A;
            }
        }

        #region IVariableValue<Color> Members

        /// <summary>
        /// Gets or sets the inclusive maximum possible value. If this value is set to less than <see cref="IVariableValue{T}.Min"/>,
        /// then <see cref="IVariableValue{T}.Min"/> will be lowered to equal this value.
        /// </summary>
        public Color Max
        {
            get { return _max; }
            set
            {
                _max = value;

                if (_min.A > _max.A)
                    _min.A = _max.A;

                if (_min.R > _max.R)
                    _min.R = _max.R;

                if (_min.G > _max.G)
                    _min.G = _max.G;

                if (_min.B > _max.B)
                    _min.B = _max.B;
            }
        }

        /// <summary>
        /// Gets or sets the inclusive minimum possible value. If this value is set to greater than <see cref="IVariableValue{T}.Max"/>,
        /// then <see cref="IVariableValue{T}.Max"/> will be raised to equal this value.
        /// </summary>
        public Color Min
        {
            get { return _min; }
            set
            {
                _min = value;

                if (_max.A < _min.A)
                    _max.A = _min.A;

                if (_max.R < _min.R)
                    _max.R = _min.R;

                if (_max.G < _min.G)
                    _max.G = _min.G;

                if (_max.B < _min.B)
                    _max.B = _min.B;
            }
        }

        /// <summary>
        /// Gets the next value, based off of the <see cref="IVariableValue{T}.Min"/> and <see cref="IVariableValue{T}.Max"/>.
        /// </summary>
        /// <returns>The next value, based off of the <see cref="IVariableValue{T}.Min"/> and <see cref="IVariableValue{T}.Max"/>.</returns>
        public Color GetNext()
        {
            byte a = (byte)RandomHelper.NextInt(_min.A, _max.A);
            byte r = (byte)RandomHelper.NextInt(_min.R, _max.R);
            byte g = (byte)RandomHelper.NextInt(_min.G, _max.G);
            byte b = (byte)RandomHelper.NextInt(_min.B, _max.B);
            return new Color(r, g, b, a);
        }

        #endregion

        /// <summary>
        /// Gets the next value, based off of the <see cref="IVariableValue{T}.Min"/> and <see cref="IVariableValue{T}.Max"/>.
        /// </summary>
        /// <returns>The next value, based off of the <see cref="IVariableValue{T}.Min"/> and <see cref="IVariableValue{T}.Max"/>.</returns>
        public Vector4 GetNextAsVector4()
        {
            var a = RandomHelper.NextInt(_min.A, _max.A);
            var r = RandomHelper.NextInt(_min.R, _max.R);
            var g = RandomHelper.NextInt(_min.G, _max.G);
            var b = RandomHelper.NextInt(_min.B, _max.B);
            return new Vector4(r, g, b, a);
        }
    }
}