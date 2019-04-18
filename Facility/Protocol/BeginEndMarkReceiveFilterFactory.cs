using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.Facility.Protocol;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Facility.Protocol
{
    /// <summary>
    /// 
    /// </summary>
    public class BeginEndMarkReceiveFilterFactory : IReceiveFilterFactory<StringRequestInfo>
    {
        private readonly byte[] m_BeginMark;
        private readonly byte[] m_EndMark;
        private readonly Encoding m_Encoding;
        private readonly IRequestInfoParser<StringRequestInfo> m_RequestInfoParser;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="beginMark"></param>
        /// <param name="endMark"></param>
        public BeginEndMarkReceiveFilterFactory(string beginMark, string endMark)
            : this(beginMark, endMark, Encoding.ASCII, BasicRequestInfoParser.DefaultInstance)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="beginMark"></param>
        /// <param name="endMark"></param>
        /// <param name="encoding"></param>
        public BeginEndMarkReceiveFilterFactory(string beginMark, string endMark, Encoding encoding)
            : this(beginMark, endMark, encoding, BasicRequestInfoParser.DefaultInstance)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="beginMark"></param>
        /// <param name="endMark"></param>
        /// <param name="encoding"></param>
        /// <param name="requestInfoParser"></param>
        public BeginEndMarkReceiveFilterFactory(string beginMark, string endMark, Encoding encoding, IRequestInfoParser<StringRequestInfo> requestInfoParser)
        {
            m_BeginMark = encoding.GetBytes(beginMark);
            m_EndMark = encoding.GetBytes(endMark);
            m_Encoding = encoding;
            m_RequestInfoParser = requestInfoParser;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appServer"></param>
        /// <param name="appSession"></param>
        /// <param name="remoteEndPoint"></param>
        /// <returns></returns>
        public virtual IReceiveFilter<StringRequestInfo> CreateFilter(IAppServer appServer, IAppSession appSession, IPEndPoint remoteEndPoint)
        {
           return new BeginEndMarkReceiveFilter(m_BeginMark, m_EndMark, m_Encoding, m_RequestInfoParser);
        }

    }
}
