﻿using Q.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Q.Proxy
{
    public class Listener
    {
        private TcpListener m_tcpListener;

        private Repeater m_repeater;


        public IPEndPoint Proxy { get; private set; }

        public bool DecryptSSL { get; private set; }

        public Listener(string ip, int port, bool decryptSSL = false)
            : this(new IPEndPoint(IPAddress.Parse(ip), port), decryptSSL)
        { }

        public Listener(string ip, int port, string proxyIP, int proxyPort, bool decryptSSL = false)
            : this(new IPEndPoint(IPAddress.Parse(ip), port), new IPEndPoint(IPAddress.Parse(proxyIP), proxyPort), decryptSSL)
        { }

        public Listener(IPEndPoint endPoint, bool decryptSSL = false) :
            this(endPoint, null, decryptSSL)
        { }

        public Listener(IPEndPoint endPoint, IPEndPoint proxy, bool decryptSSL = false)
        {
            m_tcpListener = new TcpListener(endPoint);
            this.Proxy = proxy;
            m_repeater = new Repeater(this.Proxy);
        }

        public Listener Start()
        {
            this.m_tcpListener.Start(100);
            Logger.Info(this.ToString());
            this.m_tcpListener.BeginAcceptTcpClient(new AsyncCallback(DoAccept), m_tcpListener);
            return this;
        }

        public Listener Stop()
        {
            this.m_tcpListener.Stop();
            return this;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder()
                .AppendLine("----------------------------------------------------------------------")
                .AppendFormat("Listen Address\t:\t{0}\n", this.m_tcpListener.LocalEndpoint)
                .AppendFormat("Repeater Type\t:\t{0}\n", m_repeater);
            if (this.Proxy != null)
            {
                sb.AppendFormat("Proxy Address\t:\t{0}\n", this.Proxy);
            }
            sb.AppendLine("----------------------------------------------------------------------");
            return sb.ToString();
        }

        private void DoAccept(IAsyncResult ar)
        {
            TcpListener tcp = (ar.AsyncState as TcpListener);
            tcp.BeginAcceptTcpClient(new AsyncCallback(DoAccept), tcp);
            using (TcpClient client = tcp.EndAcceptTcpClient(ar))
            using (NetworkStream networkStream = client.GetStream())
            {
                Stream localStream = networkStream;
                Stream remoteStream = null;
                try
                {
                    while (localStream.CanRead)
                    {
                        m_repeater.Relay(ref localStream, ref remoteStream);

                        //HttpHeader header;
                        //BufferPool bufferPool;
                        //if (!new HttpAcceptor().TryAccept(networkStream, out bufferPool, out header))
                        //{
                        //    break;
                        //}
                        //var requestHeader = header as Http.HttpRequestHeader;
                        //m_repeater.Relay(stream, bufferPool, requestHeader);
                    }


                    /*
                    for (request = HttpPackage.Read(stream);
                        request != null;
                        request = keepAlive ? HttpPackage.Read(stream) : null, response = null, keepAlive = false)
                    {
                        DateTime startTime = DateTime.Now;
                        if (isSsl)
                        {
                            request.Host = host;
                            request.Port = port;
                            request.IsSSL = true;
                        }
                        if (request.HttpMethod == "CONNECT")
                        {
                            stream = SwitchToSslStream(stream, request);
                            isSsl = keepAlive = true;
                            host = request.Host;
                            port = request.Port;
                        }
                        else
                        {
                            this.pendingRequestsCount++;
                            response = this.miner.Fetch(request, this.Proxy, this.Proxy != null);
                            this.pendingRequestsCount--;
                            if (response != null && stream.CanWrite)
                            {
                                stream.Write(response.Binary, 0, response.Length);
                                stream.Flush();
                                // Proxy-Connection: keep-alive
                                keepAlive = request.HeaderItems.ContainsKey("Proxy-Connection")
                                    && String.Compare(request.HeaderItems["Proxy-Connection"], "keep-alive", true) == 0;
                            }
                        }
                    }
                    */
                }
                catch (Exception ex)
                {
                    throw ex;
                    //Logger.PublishException(ex, request != null ? String.Format("{0}:{1}\n{2}", request.Host, request.Port, request.StartLine) : null);
                }
                finally
                {
                }
            }
        }
    }
}
