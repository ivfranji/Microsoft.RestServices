﻿namespace Microsoft.RestServices.Tests.Service.Entity
{
    using Microsoft.RestServices.Exchange;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ItemIdTests 
    {
        [TestMethod]
        public void TestMessageItemIdProperties()
        {
            ItemId itemId = new MessageId(
                "abcd", 
                "test@domain.com");

            Assert.AreEqual(
                itemId.MailboxId.Id, 
                "test@domain.com");

            Assert.AreEqual(
                itemId.MailboxId,
                new MailboxId("test@domain.com"));

            Assert.AreEqual(
                itemId.Id, 
                "abcd");

            Assert.AreEqual(
                itemId.IdPath, 
                "messages/abcd");
        }
    }
}
