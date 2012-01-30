using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace issue_102
{
    [TestFixture]
    public class FindRelatedOcoFixture
    {
        public const string username = "xx747649";
        public const string password = "password";
        public const string serviceUrl = "https://ciapi.cityindex.com/tradingapi";

        [Test]
        public void FindRelatedOco()
        {
            var client = new CIAPI.Rpc.Client(new Uri(serviceUrl));
            client.LogIn(username, password);

            var accountInfo = client.AccountInformation.GetClientAndTradingAccount();
            var openpositions = client.TradesAndOrders.ListOpenPositions(accountInfo.TradingAccounts[0].TradingAccountId);
            var stopOrder = client.TradesAndOrders.GetActiveStopLimitOrder(openpositions.OpenPositions[0].StopOrder.OrderId.ToString());
            var limitOrder = client.TradesAndOrders.GetActiveStopLimitOrder(openpositions.OpenPositions[0].LimitOrder.OrderId.ToString());

            //We assume that the stopOrder will be related to the limitOrder via OcoOrder
            Assert.IsNotNull(stopOrder.ActiveStopLimitOrder.OcoOrder);
            Assert.AreEqual(stopOrder.ActiveStopLimitOrder.OcoOrder.OrderId, limitOrder.ActiveStopLimitOrder.OrderId);

            //And that the limitOrder will be related to the stopOrder via OcoOrder
            Assert.IsNotNull(limitOrder.ActiveStopLimitOrder.OcoOrder);
            Assert.AreEqual(limitOrder.ActiveStopLimitOrder.OcoOrder.OrderId, stopOrder.ActiveStopLimitOrder.OrderId);

        }
    }
}
