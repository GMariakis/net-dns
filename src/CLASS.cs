﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Makaretu.Dns
{
    /// <summary>
    ///   CLASS fields appear in resource records.
    /// </summary>
    public enum Class : ushort
    {
        /// <summary>
        ///   The Internet.
        /// </summary>
        IN = 1,

        /// <summary>
        ///   The CSNET class (Obsolete - used only for examples insome obsolete RFCs).
        /// </summary>
        CS = 2,

        /// <summary>
        ///   The CHAOS class.
        /// </summary>
        CH = 3,

        /// <summary>
        ///   Hesiod[Dyer 87].
        /// </summary>
        HS = 4,

        /// <summary>
        ///   Used in UPDATE message to signify no class.
        /// </summary>
        None = 254,

        /// <summary>
        ///   Only used in QCLASS.
        /// </summary>
        /// <seealso cref="Question.Class"/>
        ANY = 255
    }
}
