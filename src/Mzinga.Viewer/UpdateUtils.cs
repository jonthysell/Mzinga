// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

using GalaSoft.MvvmLight.Messaging;

using Mzinga.Viewer.ViewModels;

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
        
        public static bool IsConnectedToInternet
        {
            get
            {
                try
                {
                    using var client = new WebClient();
                    using (client.OpenRead("http://google.com/generate_204"))
                    {
                        return true;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public static bool IsCheckingforUpdate { get; private set; }

        internal static int TimeoutMS = 3000;

        public const int MaxTimeoutMS = 100000;

        public static async Task UpdateCheckAsync(bool confirmUpdate, bool showUpToDate)
        {
            try
            {
                IsCheckingforUpdate = true;

                var latestRelease = await GetLatestGitHubReleaseInfoAsync("jonthysell", AppInfo.Product);

                if (null == latestRelease)
                {
                    if (showUpToDate)
                    {
                        Messenger.Default.Send(new InformationMessage("Unable to check for updates at this time. Please try again later."));
                    }
                }
                else if (latestRelease.LongVersion <= AppInfo.LongVersion)
                {
                    if (showUpToDate)
                    {
                        Messenger.Default.Send(new InformationMessage($"{AppInfo.Product} is already up-to-date."));
                    }
                }
                else
                {
                    // Update available
                    if (confirmUpdate)
                    {
                        Messenger.Default.Send(new ConfirmationMessage($"{latestRelease.Name} is now avaliable. Would you like to open the release page?", (result) =>
                        {
                            try
                            {
                                if (result)
                                {
                                    Messenger.Default.Send(new LaunchUrlMessage(latestRelease.HtmlUrl.AbsoluteUri));
                                }
                            }
                            catch (Exception ex)
                            {
                                ExceptionUtils.HandleException(ex);
                            }

                        }));
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.Timeout)
                {
                    TimeoutMS = (int)Math.Min(TimeoutMS * 1.5, MaxTimeoutMS);
                }

                if (showUpToDate)
                {
                    ExceptionUtils.HandleException(new UpdateException(ex));
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

        private static async Task<GitHubReleaseInfo> GetLatestGitHubReleaseInfoAsync(string owner, string repo)
        {
            var releaseInfos = await GetGitHubReleaseInfosAsync(owner, repo);

            return releaseInfos
                .OrderByDescending(info => info.LongVersion)
                .ThenBy(info => info.Name)
                .FirstOrDefault();
        }

        private static async Task<IList<GitHubReleaseInfo>> GetGitHubReleaseInfosAsync(string owner, string repo)
        {
            if (!IsConnectedToInternet)
            {
                throw new UpdateNoInternetException();
            }

            var releaseInfos = new List<GitHubReleaseInfo>();

            try
            {
                var request = WebRequest.CreateHttp($"https://api.github.com/repos/{owner}/{repo}/releases");
                request.Headers.Add("Accept: application/vnd.github.v3+json");
                request.UserAgent = _userAgent;
                request.Timeout = TimeoutMS;

                using var response = await request.GetResponseAsync();
                var responseStream = response.GetResponseStream();
                var jsonDocument = await JsonDocument.ParseAsync(responseStream);

                foreach (var releaseObject in jsonDocument.RootElement.EnumerateArray())
                {
                    string name = releaseObject.GetProperty("name").GetString();
                    string tagName = releaseObject.GetProperty("tag_name").GetString();
                    string htmlUrl = releaseObject.GetProperty("html_url").GetString();
                    bool draft = releaseObject.GetProperty("draft").GetBoolean();
                    bool prerelease = releaseObject.GetProperty("prerelease").GetBoolean();

                    releaseInfos.Add(new GitHubReleaseInfo(name, tagName, htmlUrl, draft, prerelease));
                }
            }
            catch (Exception) { }

            return releaseInfos;
        }

        private const string _userAgent = "Mozilla/5.0";
    }

    public class GitHubReleaseInfo
    {
        public readonly string Name;
        public readonly string TagName;
        public readonly Uri HtmlUrl;
        public readonly bool Draft;
        public readonly bool Prerelease;

        public ulong LongVersion
        {
            get
            {
                if (!_longVersion.HasValue && VersionUtils.TryParseLongVersion(TagName, out ulong result))
                {
                    _longVersion = result;
                }
                return _longVersion.Value;
            }
        }
        private ulong? _longVersion;

        public GitHubReleaseInfo(string name, string tagName, string htmlUrl, bool draft, bool prerelease)
        {
            Name = name?.Trim() ?? throw new ArgumentNullException(nameof(name));
            TagName = tagName?.Trim() ?? throw new ArgumentNullException(nameof(tagName));
            HtmlUrl = new Uri(htmlUrl);
            Draft = draft;
            Prerelease = prerelease;
        }
    }

    [Serializable]
    public class UpdateException : Exception
    {
        public override string Message
        {
            get
            {
                string message = "Unable to update at this time. Please try again later.";
                if (InnerException is WebException wex)
                {
                    message = $"{message} ({wex.Status})";
                }
                return message;
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
                return "Failed to detect an active internet connection.";
            }
        }

        public UpdateNoInternetException() : base() { }
    }
}