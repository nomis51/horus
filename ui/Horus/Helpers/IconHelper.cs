using System.Collections.Generic;
using Material.Icons;

namespace Horus.Helpers;

public static class IconHelper
{
    #region Constants

    private static readonly Dictionary<string, MaterialIconKind> EntryToIcon = new()
    {
        { "aws", MaterialIconKind.Aws },
        { "amazon", MaterialIconKind.Aws },
        { "github", MaterialIconKind.Github },
        { "facebook", MaterialIconKind.Facebook },
        { "microsoft", MaterialIconKind.Microsoft },
        { "windows", MaterialIconKind.MicrosoftWindows },
        { "outlook", MaterialIconKind.MicrosoftOutlook },
        { "hotmail", MaterialIconKind.MicrosoftOutlook },
        { "devops", MaterialIconKind.MicrosoftAzureDevops },
        { "azure", MaterialIconKind.MicrosoftAzure },
        { "twitter", MaterialIconKind.Twitter },
        { "gmail", MaterialIconKind.Gmail },
        { "hangout", MaterialIconKind.GoogleHangouts },
        { "pinterest", MaterialIconKind.Pinterest },
        { "steam", MaterialIconKind.Steam },
        { "reddit", MaterialIconKind.Reddit },
        { "apple", MaterialIconKind.Apple },
        { "ios", MaterialIconKind.AppleIos },
        { "icloud", MaterialIconKind.AppleIcloud },
        { "atlassian", MaterialIconKind.Atlassian },
        { "bitbucket", MaterialIconKind.Bitbucket },
        { "docker", MaterialIconKind.Docker },
        { "dropbox", MaterialIconKind.Dropbox },
        { "messenger", MaterialIconKind.FacebookMessenger },
        { "firebase", MaterialIconKind.Firebase },
        { "firefox", MaterialIconKind.Firefox },
        { "gitlab", MaterialIconKind.Gitlab },
        { "git", MaterialIconKind.Git },
        { "gog", MaterialIconKind.GogCom },
        { "chrome", MaterialIconKind.GoogleChrome },
        { "jira", MaterialIconKind.Jira },
        { "linkedin", MaterialIconKind.Linkedin },
        { "linux", MaterialIconKind.Linux },
        { "mastodon", MaterialIconKind.Mastodon },
        { "edge", MaterialIconKind.MicrosoftEdge },
        { "office", MaterialIconKind.MicrosoftOffice },
        { "onedrive", MaterialIconKind.MicrosoftOnedrive },
        { "xbox", MaterialIconKind.MicrosoftXbox },
        { "netflix", MaterialIconKind.Netflix },
        { "npm", MaterialIconKind.Npm },
        { "opera", MaterialIconKind.Opera },
        { "origin", MaterialIconKind.Origin },
        { "google", MaterialIconKind.Google },
        { "patreon", MaterialIconKind.Patreon },
        { "plex", MaterialIconKind.Plex },
        { "redhat", MaterialIconKind.Redhat },
        { "salesforce", MaterialIconKind.Salesforce },
        { "skype", MaterialIconKind.Skype },
        { "slack", MaterialIconKind.Slack },
        { "snapchat", MaterialIconKind.Snapchat },
        { "playstation", MaterialIconKind.SonyPlaystation },
        { "psn", MaterialIconKind.PlaystationNetwork },
        { "sony", MaterialIconKind.SonyPlaystation },
        { "spotify", MaterialIconKind.Spotify },
        { "stack-overflow", MaterialIconKind.StackOverflow },
        { "stackoverflow", MaterialIconKind.StackOverflow },
        { "stack overflow", MaterialIconKind.StackOverflow },
        { "teamviewer", MaterialIconKind.Teamviewer },
        { "trello", MaterialIconKind.Trello },
        { "twitch", MaterialIconKind.Twitch },
        { "ubisoft", MaterialIconKind.Ubisoft },
        { "unity", MaterialIconKind.Unity },
        { "unreal", MaterialIconKind.Unreal },
        { "vimeo", MaterialIconKind.Vimeo },
        { "waze", MaterialIconKind.Waze },
        { "whatsapp", MaterialIconKind.Whatsapp },
        { "wordpress", MaterialIconKind.Wordpress },
    };

    #endregion

    #region Public methods

    public static MaterialIconKind GetIconFromEntryName(string name)
    {
        var lowerName = name.ToLower();

        if (EntryToIcon.TryGetValue(lowerName, out var icon)) return icon;

        foreach (var (key, value) in EntryToIcon)
        {
            if (lowerName.Contains(key)) return value;
        }

        return MaterialIconKind.Key;
    }

    #endregion
}