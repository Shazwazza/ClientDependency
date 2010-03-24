using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;
using System.Web.UI;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using ClientDependency.Core.Controls;
using ClientDependency.Core.FileRegistration.Providers;
using ClientDependency.Core.Config;
using ClientDependency.Core;
using System.Net;

namespace ClientDependency.Core.Module
{

    /// <summary>
    /// Used as an http response filter to modify the contents of the output html.
    /// This filter is used to intercept js and css rogue registrations on the html page.
    /// </summary>
    internal class RogueFileFilter : Stream
    {

        public RogueFileFilter(Stream inputStream)
        {
            m_ResponseStream = inputStream;
            m_ResponseHtml = new StringBuilder();
        }

        #region Private members

        private Stream m_ResponseStream;
        private long m_Position;
        private StringBuilder m_ResponseHtml;
        private string m_MatchScript = "<script(?:(?:.*(?<src>(?<=src=\")[^\"]*(?=\"))[^>]*)|[^>]*)>(?<content>(?:(?:\n|.)(?!(?:\n|.)<script))*)</script>";

        #endregion

        #region Basic Stream implementation overrides
        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get { return 0; }
        }
        #endregion

        #region Stream wrapper implementation
        public override void Close()
        {
            m_ResponseStream.Close();
        }

        public override long Position
        {
            get { return m_Position; }
            set { m_Position = value; }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return m_ResponseStream.Seek(offset, origin);
        }

        public override void SetLength(long length)
        {
            m_ResponseStream.SetLength(length);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return m_ResponseStream.Read(buffer, offset, count);
        }
        #endregion

        #region Stream implemenation that does stuff

        /// <summary>
        /// Appends the bytes written to our string builder
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            byte[] data = new byte[count];
            Buffer.BlockCopy(buffer, offset, data, 0, count);
            m_ResponseHtml.Append(System.Text.Encoding.Default.GetString(buffer));
        }

        /// <summary>
        /// Before the contents are flushed to the stream, the output is inspected and altered
        /// and then written to the stream.
        /// </summary>
        public override void Flush()
        {
            UpdateOutputHtml();
            m_ResponseStream.Flush();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Modifies the output html content:
        /// 
        /// 1.
        /// Replaces any rogue script tag's with calls to the compression handler instead 
        /// of just the script.
        /// 
        /// </summary>
        private void UpdateOutputHtml()
        {
            var output = m_ResponseHtml.ToString();

            output = ReplaceScripts(output);

            byte[] outputBytes = System.Text.Encoding.Default.GetBytes(output);
            m_ResponseStream.Write(outputBytes, 0, outputBytes.GetLength(0));
        }

        /// <summary>
        /// Replaces all src attribute values for a script tag with their corresponding 
        /// URLs as a composite script.
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        /// <remarks>
        /// TODO: Need to add caching to the src match found and what we returned for it so this doesn't
        /// get processed every request.
        /// </remarks>
        private string ReplaceScripts(string html)
        {
            //check if we should be processing!
            if (ClientDependencySettings.Instance.ProcessRogueJSFiles)
            {
                html = Regex.Replace(html, m_MatchScript,
                    (m) =>
                    {
                        var src = m.Groups["src"];

                        //if there is no src group name or it doesn't end with a js extension or it's already using the composite handler,
                        //the return the existing string.
                        if (src == null
                            || string.IsNullOrEmpty(src.ToString())
                            || !src.ToString().EndsWith(".js")
                            || src.ToString().StartsWith(ClientDependencySettings.Instance.CompositeFileHandlerPath))
                            return m.ToString();

                        //make sure that it's an internal request, though we can deal with external 
                        //requests, we'll leave that up to the developer to register an external request
                        //explicitly if they want to include in the composite scripts.
                        try
                        {
                            var url = new Uri(src.ToString(), UriKind.RelativeOrAbsolute);
                            if (!url.IsLocalUri())
                                return m.ToString(); //not a local uri                       
                        }
                        catch (UriFormatException)
                        {
                            //malformed url, let's exit
                            return m.ToString();
                        }

                        return m.ToString().Replace(src.ToString(),
                            BaseFileRegistrationProvider.GetCompositeFileUrl(src.ToString(), ClientDependencyType.Javascript));
                    },
                    RegexOptions.Compiled);
            }
            
            return html;
        }


        #endregion

    }
}
