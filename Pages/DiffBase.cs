using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace Diffle
{
    public class DiffBase : ComponentBase
    {
        public object? LeftDirectory { get; private set; }
        public object? RightDirectory { get; private set; }
        public bool? DirectoriesMatch { get; private set; }
        public bool DifferentFiles { get; private set; } = false;
        public bool MissingInfo { get; private set; } = false;
        private IJSObjectReference? module;

        [Inject]
        private IJSRuntime JSRuntime { get; set; } = default!;

        [Inject]
        private ILogger<DiffBase> _logger { get; set; } = default!;

        public async Task SetLeftDirectory()
        {
            try
            {
                LeftDirectory = await ShowDirectoryPicker();
            }
            catch (System.Exception ex)
            {

                _logger.LogError("Error: ", ex);
            }
        }

        public async Task SetRightDirectory()
        {
            try
            {
                RightDirectory = await ShowDirectoryPicker();
            }
            catch (System.Exception ex)
            {

                _logger.LogError("Error: {0}", ex);
            }
        }

        public async Task CompareDirectories()
        {
            // DirectoriesMatch = await Compare();

            var leftFiles = await GetFiles(LeftDirectory);
            var rightFiles = await GetFiles(RightDirectory);

            if (leftFiles == null || rightFiles == null)
            {
                MissingInfo = true;
                return;
            }

            var onlyInLeft = leftFiles.Except(rightFiles);
            var onlyInRight = rightFiles.Except(leftFiles);
            var both = leftFiles.Intersect(rightFiles);
            var sameFileNames = !onlyInLeft.Any() && !onlyInRight.Any();

            if (sameFileNames)
            {
                await foreach (var file in GetFilesString(leftFiles, rightFiles))
                {
                    var diffResult = InlineDiffBuilder.Diff(file.Item1?.Text, file.Item2?.Text);
                    var diffString = "";
                    foreach (var line in diffResult.Lines)
                    {
                        switch (line.Type)
                        {
                            case ChangeType.Inserted:
                                diffString += "+ ";
                                break;
                            case ChangeType.Deleted:
                                diffString += "- ";
                                break;
                            default:
                                diffString += "  ";
                                break;
                        }

                        diffString += line.Text + "\n";
                    }

                    if (diffResult.HasDifferences)
                    {
                        _logger.LogInformation("Found a diff in {0}", file.Item1?.Name);
                        _logger.LogInformation("The diff is {0}", diffString);
                        break;
                    }
                    else
                    {
                        _logger.LogInformation("No diff found in {0}", file.Item1?.Name);
                    }
                }
            }
            else
            {
                DifferentFiles = true;
                return;
            }
        }

        private async IAsyncEnumerable<(FileRef?, FileRef?)> GetFilesString(string[] leftFiles, string[] rightFiles)
        {
            for (var i = 0; i < leftFiles.Length; i++)
            {
                var left = await GetFileContents(LeftDirectory, leftFiles[i]);
                var right = await GetFileContents(RightDirectory, rightFiles[i]);
                yield return (left, right);
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                module = await JSRuntime.InvokeAsync<IJSObjectReference>(
                    "import", "./js/open-dir.js");
            }
        }

        public async ValueTask<IJSObjectReference?> ShowDirectoryPicker() =>
        module is not null ?
            await module.InvokeAsync<IJSObjectReference>("openDirectory") : null;

        public async ValueTask<bool?> Compare() =>
        module is not null ?
            await module.InvokeAsync<bool>("compare", LeftDirectory, RightDirectory) : null;

        public async ValueTask<string[]?> GetFiles(object? dir) =>
        module is not null ?
            await module.InvokeAsync<string[]>("getFiles", dir) : null;

        public async ValueTask<FileRef?> GetFileContents(object? dir, string fileName) =>
        module is not null ?
            await module.InvokeAsync<FileRef>("getFileContents", dir, fileName) : null;
    }
}

public class FileRef
{
    public string Name { get; set; } = "";
    public string Text { get; set; } = "";
}

