using Kundenportal.AdminUi.WebApp.Resources;
using MassTransit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Kundenportal.AdminUi.WebApp.Components.Pages.Structures;

public partial class EditStructureGroup
{
    public const string RouteEditBase = $"{StructureGroups.Route}/edit";
    public const string RouteEdit = $"{RouteEditBase}/{{id:guid}}";
    public const string RouteCreate = $"{StructureGroups.Route}/create";

    [Parameter] public Guid? Id { get; set; }

    [Inject]
    public ILogger<EditStructureGroup>? Logger { get; set; }

    [Inject]
    public IPublishEndpoint? PublishEndpoint { get; init; }

    [Inject]
    public NavigationManager? NavigationManager { get; init; }

    private readonly Model _model = new();
    private readonly EditContext _editContext;
    private readonly ValidationMessageStore _validationMessageStore;

    public EditStructureGroup()
    {
        _editContext = new EditContext(_model);
        _validationMessageStore = new ValidationMessageStore(_editContext);
    }

    private bool _validating = false;

    private void AddPathPart()
    {
        _model.PathParts.Add("");
    }

    private async Task OnSubmitAsync()
    {
        _validationMessageStore.Clear();
        _validating = true;

        Logger!.LogInformation(string.Join(" > ", _model.PathPartsUntilFirstEmpty));

        await Task.Delay(200);

        bool dataAnnotationsResult = _editContext.Validate();
        bool pathPartValidationResult = ValidatePathParts();

        if (!dataAnnotationsResult || !pathPartValidationResult)
        {
            _validating = false;
            return;
        }

        await Task.Delay(5000);

        _validationMessageStore.Add(() => _model.PathParts, "Test message");
        await InvokeAsync(StateHasChanged);

        _validating = false;
        //Logger!.LogInformation("Name: {Name}", _model.Name);
        //await PublishEndpoint!.Publish(new CreateStructureGroup.Command
        //{
        //    Name = _model.Name
        //});
        //NavigationManager!.NavigateTo(StructureGroups.Route);
    }

    private bool ValidatePathParts()
    {
        bool result = true;

        foreach (var (value, index) in _model.PathParts.Select((value, index) => (value, index)))
        {
            bool pathPartValidationResult = ValidatePathPart(value, index);
            if (!pathPartValidationResult)
            {
                result = false;
            }
        }

        _editContext.NotifyValidationStateChanged();

        return result;
    }

    private bool ValidatePathPart(string pathPart, int index)
    {
        string fieldIdName = GetFieldIdNameForPathPart(index);

        return
            ValidateContainsInvalidFolderCharacters(pathPart, fieldIdName) &&
            ValidateIsTooLong(pathPart, fieldIdName);
    }

    [GeneratedRegex(@"[<>?"":|/\\\*\.’#%]", RegexOptions.Compiled, 100)]
    private static partial Regex ContainsInvalidCharacters();

    private bool ValidateContainsInvalidFolderCharacters(string pathPart, string fieldIdName)
    {
        MatchCollection matches = ContainsInvalidCharacters().Matches(pathPart);

        if (matches.Count > 0)
        {
            string invalidCharacters = string.Join(", ", matches.Select(x => x.Captures[0].Value));
            _validationMessageStore.Add(_editContext.Field(fieldIdName), string.Format(Texts.ValidationErrorPathPartContainsInvalidCharacters, invalidCharacters));
        }

        return matches.Count == 0;
    }

    private bool ValidateIsTooLong(string pathPart, string fieldIdName)
    {
        const int maxLength = 64;
        bool isTooLong = pathPart.Length > maxLength;

        if (isTooLong)
        {
            _validationMessageStore.Add(_editContext.Field(fieldIdName), string.Format(Texts.ValidationErrorPathPartTooLong, maxLength));
        }

        return isTooLong;
    }

    private static string GetFieldIdNameForPathPart(int index)
    {
        return $"fieldid_pathpart_{index}";
    }

    public class Model
    {
        [Required]
        [MinLength(1)]
        public List<string> PathParts { get; set; } = [""];

        public IEnumerable<string> PathPartsUntilFirstEmpty
        {
            get
            {
                foreach (string pathPart in PathParts)
                {
                    if (string.IsNullOrEmpty(pathPart))
                    {
                        break;
                    }

                    yield return pathPart;
                }
            }
        }
    }
}