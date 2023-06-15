using System.Security.AccessControl;

namespace WinPass.Shared.Helpers;

public static class FileAccessHelper
{
    #region Public methods

    public static void PrepareTempFileAccess(string tmpFilePath)
    {
        File.SetAttributes(tmpFilePath, FileAttributes.Hidden | FileAttributes.Temporary);

#pragma warning disable CA1416
        var accessControl = new FileInfo(tmpFilePath).GetAccessControl();
#pragma warning restore CA1416
        ChangeAccess(accessControl, "Administrators");
        ChangeAccess(accessControl, "Users");
    }

    public static void ResetTempFileAccess(string tmpFilePath)
    {
#pragma warning disable CA1416
        var accessControl = new FileInfo(tmpFilePath).GetAccessControl();
        ChangeAccess(accessControl, "Administrators", AccessControlType.Allow);
        ChangeAccess(accessControl, "Users", AccessControlType.Allow);
#pragma warning restore CA1416
    }

    #endregion

    #region Private methods

    private static void ChangeAccess(FileSecurity accessControl, string identity,
        AccessControlType type = AccessControlType.Deny)
    {
#pragma warning disable CA1416
        accessControl.AddAccessRule(new FileSystemAccessRule(identity, FileSystemRights.FullControl,
            AccessControlType.Deny));
        accessControl.AddAccessRule(new FileSystemAccessRule(identity, FileSystemRights.Delete,
            AccessControlType.Deny));
        accessControl.AddAccessRule(new FileSystemAccessRule(identity, FileSystemRights.ChangePermissions,
            AccessControlType.Deny));
        accessControl.AddAccessRule(new FileSystemAccessRule(identity, FileSystemRights.ExecuteFile,
            AccessControlType.Deny));
        accessControl.AddAccessRule(new FileSystemAccessRule(identity, FileSystemRights.ReadPermissions,
            AccessControlType.Deny));
        accessControl.AddAccessRule(new FileSystemAccessRule(identity, FileSystemRights.TakeOwnership,
            AccessControlType.Deny));
        accessControl.AddAccessRule(new FileSystemAccessRule(identity, FileSystemRights.WriteAttributes,
            AccessControlType.Deny));
        accessControl.AddAccessRule(new FileSystemAccessRule(identity, FileSystemRights.WriteExtendedAttributes,
            AccessControlType.Deny));
        accessControl.AddAccessRule(new FileSystemAccessRule(identity, FileSystemRights.ReadExtendedAttributes,
            AccessControlType.Deny));
        accessControl.AddAccessRule(new FileSystemAccessRule(identity, FileSystemRights.WriteAttributes,
            AccessControlType.Deny));
#pragma warning restore CA1416
    }

    #endregion
}