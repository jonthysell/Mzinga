// 
// UpdateUtils.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2016, 2017, 2018 Jon Thysell <http://jonthysell.com>
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Cache;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml;

using GalaSoft.MvvmLight.Messaging;

using Mzinga.SharedUX;
using Mzinga.SharedUX.ViewModel;

namespace Mzinga.Viewer
{
    public class UpdateUtils
    {
        public static AppViewModel AppVM
        {
            get
            {
                return AppViewModel.Instance;
            }
        }

        public static bool IsCheckingforUpdate { get; private set; }

        public static Task UpdateCheckAsync(bool confirmUpdate, bool showUpToDate)
        {
            return Task.Factory.StartNew(() =>
            {
                UpdateCheck(confirmUpdate, showUpToDate);
            });
        }

        public static void UpdateCheck(bool confirmUpdate, bool showUpToDate)
        {
            try
            {
                IsCheckingforUpdate = true;

                List<UpdateInfo> updateInfos = GetLatestUpdateInfos();

                ReleaseChannel targetReleaseChannel = GetReleaseChannel();

                ulong maxVersion = LongVersion(AppVM.FullVersion);

                UpdateInfo latestVersion = null;

                bool updateAvailable = false;
                foreach (UpdateInfo updateInfo in updateInfos)
                {
                    if (updateInfo.ReleaseChannel == targetReleaseChannel)
                    {
                        ulong updateVersion = LongVersion(updateInfo.Version);

                        if (updateVersion > maxVersion)
                        {
                            updateAvailable = true;
                            latestVersion = updateInfo;
                            maxVersion = updateVersion;
                        }
                    }
                }

                SetLastUpdateCheck(DateTime.Now);

                if (updateAvailable)
                {
                    if (confirmUpdate)
                    {
                        string message = string.Format("Mzinga v{0} is available. Would you like to go to the download page?", latestVersion.Version);
                        AppVM.DoOnUIThread(() =>
                        {
                            Messenger.Default.Send(new ConfirmationMessage(message, (confirmed) =>
                            {
                                try
                                {
                                    if (confirmed)
                                    {
                                        Messenger.Default.Send(new LaunchUrlMessage(latestVersion.Url));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    ExceptionUtils.HandleException(new UpdateException(ex));
                                }
                            }));
                        });
                    }
                    else
                    {
                        AppVM.DoOnUIThread(() =>
                        {
                            Messenger.Default.Send(new LaunchUrlMessage(latestVersion.Url));
                        });
                    }
                }
                else
                {
                    if (showUpToDate)
                    {
                        AppVM.DoOnUIThread(() =>
                        {
                            Messenger.Default.Send(new InformationMessage("Mzinga is up-to-date."));
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionUtils.HandleException(new UpdateException(ex));
            }
            finally
            {
                IsCheckingforUpdate = false;
            }
        }

        public static List<UpdateInfo> GetLatestUpdateInfos()
        {
            if (!IsConnectedToInternet)
            {
                throw new UpdateNoInternetException();
            }

            List<UpdateInfo> updateInfos = new List<UpdateInfo>();

            HttpWebRequest request = WebRequest.CreateHttp(_updateUrl);
            request.UserAgent = _userAgent;
            request.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);

            using (XmlReader reader = XmlReader.Create(request.GetResponse().GetResponseStream()))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        if (reader.Name == "update")
                        {
                            string version = reader.GetAttribute("version");
                            string url = reader.GetAttribute("url");
                            ReleaseChannel releaseChannel = (ReleaseChannel)Enum.Parse(typeof(ReleaseChannel), reader.GetAttribute("channel"));
                            updateInfos.Add(new UpdateInfo(version, url, releaseChannel));
                        }
                    }
                }
            }

            return updateInfos;
        }

        public static ulong LongVersion(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
            {
                throw new ArgumentNullException("version");
            }

            ulong vers = 0;

            string[] parts = version.Trim().Split('.');

            for (int i = 0; i < parts.Length; i++)
            {
                vers |= (ulong.Parse(parts[i]) << ((4 - (i + 1)) * 16));
            }

            return vers;
        }

        public static ReleaseChannel GetReleaseChannel()
        {
            // TODO: Get saved value from config

            return ReleaseChannel.Official;
        }

        public static bool GetCheckUpdateOnStart()
        {
            // TODO: Get saved value from config

            return false;
        }

        public static void SetCheckUpdateOnStart(bool value)
        {
            // TODO: Set saved value in config
        }

        public static DateTime GetLastUpdateCheck()
        {
            // TODO: Get saved value from config

            return DateTime.MinValue;
        }

        public static void SetLastUpdateCheck(DateTime value)
        {
            // TODO: Set saved value in config
        }

        public static bool IsConnectedToInternet
        {
            get
            {
                int Description;
                return NativeMethods.InternetGetConnectedState(out Description, 0);
            }
        }

        private const string _updateUrl = "http://mzingaupdate.jonthysell.com";
        private const string _userAgent = "Mozilla/5.0";
    }

    public class UpdateInfo
    {
        public string Version { get; private set; }

        public string Url { get; private set; }

        public ReleaseChannel ReleaseChannel { get; private set; }

        public UpdateInfo(string version, string url, ReleaseChannel releaseChannel)
        {
            if (string.IsNullOrWhiteSpace(version))
            {
                throw new ArgumentNullException("version");
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException("url");
            }

            Version = version.Trim();
            Url = url.Trim();
            ReleaseChannel = releaseChannel;
        }
    }

    public enum ReleaseChannel
    {
        Official,
        Preview
    }

    [Serializable]
    public class UpdateException : Exception
    {
        public override string Message
        {
            get
            {
                return "Unable to update Mzinga at this time.";
            }
        }

        public UpdateException(Exception innerException) : base("", innerException) { }
    }

    [Serializable]
    public class UpdateNoInternetException : Exception
    {
        public override string Message
        {
            get
            {
                return "Mzinga failed to detect an active internet connection.";
            }
        }

        public UpdateNoInternetException() : base() { }
    }

    internal static partial class NativeMethods
    {
        [DllImport("wininet.dll")]
        internal extern static bool InternetGetConnectedState(out int Description, int ReservedValue);
    }
}
