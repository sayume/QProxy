﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Q.Http;

namespace Q.Proxy
{
    public abstract class Repeater
    {
        protected IPEndPoint Proxy { get; set; }

        public Repeater(IPEndPoint proxy = null)
        {
            this.Proxy = proxy;
        }

        public abstract Stream BeginRelay(Stream localStream, string host, int port, bool SSL);
    }

    public class LocalRepeater : Repeater
    {
        public LocalRepeater(IPEndPoint proxy = null) : base(proxy) { }

        public override Stream BeginRelay(Stream localStream, string host, int port, bool SSL)
        {
            bool byProxy = this.Proxy != null;
            IPEndPoint endPoint = byProxy ? this.Proxy : new IPEndPoint(DnsHelper.GetHostAddress(host), port);
            Stream remoteStream = SSL ? new HttpsStream(endPoint, host, port, byProxy) : new HttpStream(endPoint);
            localStream.CopyToAsync(remoteStream);
            remoteStream.CopyToAsync(remoteStream);
            return remoteStream;
        }
    }

    //public class HttpRepeater : Repeater
    //{
    //}
}
