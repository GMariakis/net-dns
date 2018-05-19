﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Makaretu.Dns
{

    [TestClass]
    public class MessageTest
    {
        /// <summary>
        ///   From https://en.wikipedia.org/wiki/Multicast_DNS
        /// </summary>
        [TestMethod]
        public void DecodeQuery()
        {
            var bytes = new byte[]
            {
                0x00, 0x00,             // Transaction ID
                0x00, 0x00,             // Flags
                0x00, 0x01,             // Number of questions
                0x00, 0x00,             // Number of answers
                0x00, 0x00,             // Number of authority resource records
                0x00, 0x00,             // Number of additional resource records
                0x07, 0x61, 0x70, 0x70, 0x6c, 0x65, 0x74, 0x76, // "appletv"
                0x05, 0x6c, 0x6f, 0x63, 0x61, 0x6c, // "local"
                0x00,                   // Terminator
                0x00, 0x01,             // Type (A record)
                0x00, 0x01              // Class
            };
            var msg = new Message();
            msg.Read(bytes, 0, bytes.Length);
            Assert.AreEqual(0, msg.Id);
            Assert.AreEqual(1, msg.Questions.Count);
            Assert.AreEqual(0, msg.Answers.Count);
            Assert.AreEqual(0, msg.AuthorityRecords.Count);
            Assert.AreEqual(0, msg.AdditionalRecords.Count);
            var question = msg.Questions.First();
            Assert.AreEqual("appletv.local", question.Name);
            Assert.AreEqual(DnsType.A, question.Type);
            Assert.AreEqual(Class.IN, question.Class);
        }

        /// <summary>
        ///   From https://en.wikipedia.org/wiki/Multicast_DNS
        /// </summary>
        [TestMethod]
        public void DecodeResponse()
        {
            var bytes = new byte[]
            {
                0x00, 0x00, 0x84, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x02, 0x07, 0x61, 0x70, 0x70,
                0x6c, 0x65, 0x74, 0x76, 0x05, 0x6c, 0x6f, 0x63, 0x61, 0x6c, 0x00, 0x00, 0x01, 0x80, 0x01, 0x00,
                0x00, 0x78, 0x00, 0x00, 0x04, 0x99, 0x6d, 0x07, 0x5a, 0xc0, 0x0c, 0x00, 0x1c, 0x80, 0x01, 0x00,
                0x00, 0x78, 0x00, 0x00, 0x10, 0xfe, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x23, 0x32,
                0xff, 0xfe, 0xb1, 0x21, 0x52, 0xc0, 0x0c, 0x00, 0x2f, 0x80, 0x01, 0x00, 0x00, 0x78, 0x00, 0x00,
                0x08, 0xc0, 0x0c, 0x00, 0x04, 0x40, 0x00, 0x00, 0x08
            };
            var msg = new Message();
            msg.Read(bytes, 0, bytes.Length);

            Assert.IsTrue(msg.IsResponse);
            Assert.IsTrue(msg.AA);
            Assert.AreEqual(0, msg.Questions.Count);
            Assert.AreEqual(1, msg.Answers.Count);
            Assert.AreEqual(0, msg.AuthorityRecords.Count);
            Assert.AreEqual(2, msg.AdditionalRecords.Count);

            Assert.AreEqual("appletv.local", msg.Answers[0].Name);
            Assert.AreEqual(DnsType.A, msg.Answers[0].Type);
            Assert.AreEqual(0x8001, (ushort)msg.Answers[0].Class);
            Assert.AreEqual(TimeSpan.FromSeconds(30720), msg.Answers[0].TTL);
            Assert.IsInstanceOfType(msg.Answers[0], typeof(ARecord));
            Assert.AreEqual(IPAddress.Parse("153.109.7.90"), ((ARecord)msg.Answers[0]).Address);

            var aaaa = (AAAARecord)msg.AdditionalRecords[0];
            Assert.AreEqual("appletv.local", aaaa.Name);
            Assert.AreEqual(DnsType.AAAA, aaaa.Type);
            Assert.AreEqual(0x8001, (ushort)aaaa.Class);
            Assert.AreEqual(TimeSpan.FromSeconds(30720), aaaa.TTL);
            Assert.AreEqual(IPAddress.Parse("fe80::223:32ff:feb1:2152"), aaaa.Address);

            var nsec = (NSECRecord)msg.AdditionalRecords[1];
            Assert.AreEqual("appletv.local", nsec.Name);
            Assert.AreEqual(DnsType.NSEC, nsec.Type);
            Assert.AreEqual(0x8001, (ushort)nsec.Class);
            Assert.AreEqual(TimeSpan.FromSeconds(30720), nsec.TTL);
            Assert.AreEqual("appletv.local", nsec.NextOwnerName);
        }

        [TestMethod]
        public void Flags()
        {
            var expected = new Message
            {
                QR = true,
                Opcode = MessageOperation.Status,
                AA = true,
                TC = true,
                RD = true,
                RA = true,
                Z = 7,
                Status = MessageStatus.Refused
            };
            var actual = new Message();
            actual.Read(expected.ToByteArray());
            Assert.AreEqual(expected.QR, actual.QR);
            Assert.AreEqual(expected.Opcode, actual.Opcode);
            Assert.AreEqual(expected.AA, actual.AA);
            Assert.AreEqual(expected.TC, actual.TC);
            Assert.AreEqual(expected.RD, actual.RD);
            Assert.AreEqual(expected.RA, actual.RA);
            Assert.AreEqual(expected.Z, actual.Z);
            Assert.AreEqual(expected.Status, actual.Status);
        }

        [TestMethod]
        public void Response()
        {
            var query = new Message { Id = 1234, Opcode = MessageOperation.InverseQuery };
            var response = query.CreateResponse();
            Assert.IsTrue(response.IsResponse);
            Assert.AreEqual(query.Id, response.Id);
            Assert.AreEqual(query.Opcode, response.Opcode);
        }

        [TestMethod]
        public void Roundtrip()
        {
            var expected = new Message
            {
                AA = true,
                QR = true,
                Id = 1234
            };
            expected.Questions.Add(new Question { Name = "emanon.org" });
            expected.Answers.Add(new ARecord { Name = "emanon.org", Address = IPAddress.Parse("127.0.0.1") });
            expected.AuthorityRecords.Add(new SOARecord
            {
                Name = "emanon.org",
                PrimaryName = "erehwon",
                Mailbox = "hostmaster.emanon.org"
            });
            expected.AdditionalRecords.Add(new ARecord { Name = "erehwon", Address = IPAddress.Parse("127.0.0.1") });
            var actual = (Message)new Message().Read(expected.ToByteArray());
            Assert.AreEqual(expected.AA, actual.AA);
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.IsQuery, actual.IsQuery);
            Assert.AreEqual(expected.IsResponse, actual.IsResponse);
            Assert.AreEqual(1, actual.Questions.Count);
            Assert.AreEqual(1, actual.Answers.Count);
            Assert.AreEqual(1, actual.AuthorityRecords.Count);
            Assert.AreEqual(1, actual.AdditionalRecords.Count);
        }

        [TestMethod]
        public void ExtendedOpcode()
        {
            var expected = new Message { Opcode = (MessageOperation)0xfff };
            Assert.AreEqual((MessageOperation)0xfff, expected.Opcode);
            Assert.AreEqual(1, expected.AdditionalRecords.OfType<OPTRecord>().Count());

            var actual = (Message)new Message().Read(expected.ToByteArray());
            Assert.AreEqual(expected.Opcode, actual.Opcode);
        }
    }
}
