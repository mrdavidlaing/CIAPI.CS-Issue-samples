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
            // 1) Logon to the Flex ITP and configure your account (a CFD only account) so that you have only one open position with an attached stop and limit order
            // https://ciapi.cityindex.com/flexitp

            // 2) Grab the open positions for this account
            var client = new CIAPI.Rpc.Client(new Uri(serviceUrl));
            client.LogIn(username, password);
            var accountInfo = client.AccountInformation.GetClientAndTradingAccount();

            var openpositions = client.TradesAndOrders.ListOpenPositions(accountInfo.TradingAccounts[0].TradingAccountId);
            
            // 3) Grab the related stop and limit orders fot this open position
            var stopOrder = client.TradesAndOrders.GetActiveStopLimitOrder(openpositions.OpenPositions[0].StopOrder.OrderId.ToString());
            /*
                Request URI     : https://ciapi.cityindex.com/tradingapi/order/473037563/activestoplimitorder
                ResponseText    : {"ActiveStopLimitOrder":{"Applicability":2,"Currency":"USD","Direction":"sell","ExpiryDateTimeUTC":null,"LastChangedDateTimeUTC":"\/Date(1327946960482)\/","LimitOrder":null,"MarketId":400481134,"MarketName":"EUR\/USD",
             *                                              "OcoOrder":{"OrderId":473037507,"Quantity":5000,"TriggerPrice":1.5},
             *                                              "OrderId":473037563,"ParentOrderId":473037506,"Quantity":5000,"Status":2,"StopOrder":null,"TradingAccountId":400258216,"TriggerPrice":1.2,"Type":2}}
            */
            var limitOrder = client.TradesAndOrders.GetActiveStopLimitOrder(openpositions.OpenPositions[0].LimitOrder.OrderId.ToString());
            /*
                Request URI     : https://ciapi.cityindex.com/tradingapi/order/473037507/activestoplimitorder
                ResponseText    : {"ActiveStopLimitOrder":{"Applicability":2,"Currency":"USD","Direction":"sell","ExpiryDateTimeUTC":null,"LastChangedDateTimeUTC":"\/Date(1327946876905)\/","LimitOrder":null,"MarketId":400481134,"MarketName":"EUR\/USD",
             *                                              "OcoOrder":null,
             *                                              "OrderId":473037507,"ParentOrderId":473037506,"Quantity":5000,"Status":2,"StopOrder":null,"TradingAccountId":400258216,"TriggerPrice":1.5,"Type":3}}
            */

            // 4) Now, check that stopOrder is related to the limitOrder via the OcoOrder
            Assert.IsNotNull(stopOrder.ActiveStopLimitOrder.OcoOrder, "stopOrder has no OcoOrder");
            Assert.AreEqual(stopOrder.ActiveStopLimitOrder.OcoOrder.OrderId, limitOrder.ActiveStopLimitOrder.OrderId, "stopOrder's Oco is not the limitOrder as expected");

            // 5) Now, check that reverse is also true - ie., the limitOrder is related to the stopOrder via the OcoOrder
            Assert.IsNotNull(limitOrder.ActiveStopLimitOrder.OcoOrder, "limitOrder has no OcoOrder");
            Assert.AreEqual(limitOrder.ActiveStopLimitOrder.OcoOrder.OrderId, stopOrder.ActiveStopLimitOrder.OrderId, "limitOrder's Oco is not the stopOrder as expected");

            /**
             * Replicate this behaviour using the JS test console at https://ciapi.cityindex.com/tradingapi
             *  
                    var userName = "xx747649";
                    doPost('/session',{ "UserName": userName, "Password": "password"}, function (data, textCode) {
                       setRequestHeader("UserName", userName);
                       setRequestHeader("Session", data.Session);

                       doGet('/UserAccount/ClientAndTradingAccount');  //TradingAccountId is 400258216
                    //grab all open positions
                       doGet('/order/openpositions?TradingAccountId=400258216');
                    //Which gives us position 473037506, that has a stop (473037563) & limit order (473037507)

                    // Get all orders - there should be 2. 
                       doGet('/order/activestoplimitorders?tradingaccountid=400258216');

                    // grab the stop - note that the attached OcoOrder is the limit
                       doGet('/order/473037563/activestoplimitorder');

                    //grab the limit - note that the limit has no attached OcoOrder
                       doGet('/order/473037507/activestoplimitorder');

                    }); 
             */
        }
    }
}
