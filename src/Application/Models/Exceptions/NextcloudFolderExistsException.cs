namespace Kundenportal.AdminUi.Application.Models.Exceptions;

public class NextcloudFolderExistsException(string path)
    : ApplicationException($"A folder exists at path {path}");