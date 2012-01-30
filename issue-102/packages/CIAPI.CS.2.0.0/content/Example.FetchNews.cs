﻿using System;
using System.Threading;
using CIAPI;
using CIAPI.DTO;
using CIAPI.Streaming;

namespace ConsoleSpikes
{
    class Example
    {
        private static readonly Uri RPC_URI = new Uri("https://ciapipreprod.cityindextest9.co.uk/TradingApi");
        private static readonly Uri STREAMING_URI = new Uri("https://pushpreprod.cityindextest9.co.uk");
        private const string USERNAME = "xx189949";
        private const string PASSWORD = "password";

        public void FetchNews_sync()
        {
            var ctx = new CIAPI.Rpc.Client(RPC_URI);
            ctx.LogIn(USERNAME, PASSWORD);

            ListNewsHeadlinesResponseDTO news = ctx.News.ListNewsHeadlinesWithSource("dj", "UK", 10);
            
            //do something with the news

            ctx.LogOut();
        }

        public void FetchNews_async()
        {
            var ctx = new CIAPI.Rpc.Client(RPC_URI);
            var gate = new AutoResetEvent(false);
            ctx.BeginLogIn(USERNAME, PASSWORD, a =>
            {
                ctx.EndLogIn(a);

                ctx.News.BeginListNewsHeadlinesWithSource("dj", "UK", 10, newsResult =>
                {

                    ListNewsHeadlinesResponseDTO news = ctx.News.EndListNewsHeadlinesWithSource(newsResult);

                    //do something with the news

                    ctx.BeginLogOut(result=>
                        {
                            gate.Set();
                        
                        }, null);
                }, null);
            }, null);

            gate.WaitOne();
        }

        public void SubscribeToNewsHeadlineStream()
        {
            //First we need a valid session, obtained from the Rpc client
            var ctx = new CIAPI.Rpc.Client(RPC_URI);
            ctx.LogIn(USERNAME, PASSWORD);

            //Next we create a connection to the streaming api, using the authenticated session
            //You application should only ever have one of these
            var streamingClient = StreamingClientFactory.CreateStreamingClient(STREAMING_URI, USERNAME, ctx.Session);
            

            //And instantiate a listener for news headlines on the appropriate topic
            //You can have multiple listeners on one connection
            var newsListener = streamingClient.BuildNewsHeadlinesListener("UK");
            

            //The MessageReceived event will be triggered every time a new News headline is available,
            //so attach a handler for that event, and wait until something comes through
            var gate = new ManualResetEvent(false);
            NewsDTO receivedNewsHeadline = null;
            newsListener.MessageReceived += (s, e) =>
            {
                receivedNewsHeadline = e.Data;
                //Do something with the new News headline data - perhaps update a news ticker?
                gate.Set();
            };
            gate.WaitOne();

            //Shut down the connection
            streamingClient.Dispose();

            //Destroy your session
            ctx.LogOut();
        }
    }
}
