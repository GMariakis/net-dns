﻿using SimpleBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Makaretu.Dns
{
    /// <summary>
    ///   Parameters needed by authoritative servers to calculate hashed owner names.
    /// </summary>
    /// <remarks>
    ///   Defined by <see href="https://tools.ietf.org/html/rfc5155#section-4">RFC 5155 - DNS Security (DNSSEC) Hashed Authenticated Denial of Existence</see>.
    /// </remarks>
    public class NSEC3PARAMRecord : ResourceRecord
    {
        /// <summary>
        ///   Creates a new instance of the <see cref="NSEC3PARAMRecord"/> class.
        /// </summary>
        public NSEC3PARAMRecord() : base()
        {
            Type = DnsType.NSEC3PARAM;
        }

        /// <summary>
        ///   The cryptographic hash algorithm used to create the hashed owner name.
        /// </summary>
        /// <value>
        ///   One of the <see cref="DigestType"/> value.
        /// </value>
        public DigestType HashAlgorithm { get; set; }

        /// <summary>
        ///   Not used, must be zero.
        /// </summary>
        public byte Flags { get; set; }

        /// <summary>
        ///   Number of times to perform the <see cref="HashAlgorithm"/>.
        /// </summary>
        public ushort Iterations { get; set; }

        /// <summary>
        ///   Appended to the original owner name before hashing.
        /// </summary>
        /// <remarks>
        ///   Used to defend against pre-calculated dictionary attacks.
        /// </remarks>
        public byte[] Salt { get; set; }

        /// <inheritdoc />
        public override void ReadData(DnsReader reader, int length)
        {
            var end = reader.Position + length;

            HashAlgorithm = (DigestType)reader.ReadByte();
            Flags = reader.ReadByte();
            Iterations = reader.ReadUInt16();
            Salt = reader.ReadByteLengthPrefixedBytes();
        }

        /// <inheritdoc />
        public override void WriteData(DnsWriter writer)
        {
            writer.WriteByte((byte)HashAlgorithm);
            writer.WriteByte(Flags);
            writer.WriteUInt16(Iterations);
            writer.WriteByteLengthPrefixedBytes(Salt);
        }

        /// <inheritdoc />
        public override void ReadData(MasterReader reader)
        {
            HashAlgorithm = (DigestType)reader.ReadByte();
            Flags = reader.ReadByte();
            Iterations = reader.ReadUInt16();

            var salt = reader.ReadString();
            if (salt != "-")
                Salt = Base16.Decode(salt);
        }

        /// <inheritdoc />
        public override void WriteData(TextWriter writer)
        {
            writer.Write((ushort)HashAlgorithm);
            writer.Write(' ');
            writer.Write((byte)Flags);
            writer.Write(' ');
            writer.Write(Iterations);
            writer.Write(' ');

            if (Salt == null || Salt.Length == 0)
            {
                writer.Write('-');
            }
            else
            {
                writer.Write(Base16.EncodeLower(Salt));
            }
        }
    }
}
