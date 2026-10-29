using DataForge.Modules.Onboarding.Application.UseCases.Datasets.Shared;
using DataForge.Modules.Onboarding.Application.UseCases.Projects.Shared;
using DataForge.Modules.Onboarding.Contracts.Dtos;
using DataForge.Modules.Onboarding.Domain.Entities;
using DataForge.Modules.Onboarding.Domain.ReadModels;

namespace DataForge.Modules.Onboarding.Application.Mappings;

public static class OnboardingMappingExtensions
{
    public static DatasetItem ToItem(this DatasetEntity dataset)
    {
        ArgumentNullException.ThrowIfNull(dataset);

        return new DatasetItem(
            dataset.PublicId,
            dataset.ProjectPublicId,
            dataset.FileName,
            dataset.SizeBytes,
            dataset.Status.ToString(),
            dataset.RowCount,
            dataset.Columns,
            dataset.FailureReason,
            dataset.CreatedAt,
            dataset.ImportedAt);
    }

    public static DatasetSummaryDto ToSummaryDto(this DatasetEntity dataset)
    {
        ArgumentNullException.ThrowIfNull(dataset);

        return new DatasetSummaryDto(
            dataset.PublicId,
            dataset.ProjectPublicId,
            dataset.FileName,
            dataset.Status.ToString(),
            dataset.RowCount,
            dataset.Columns);
    }

    public static ProjectItem ToItem(this ProjectListItem project)
    {
        ArgumentNullException.ThrowIfNull(project);

        return new ProjectItem(project.PublicId, project.Name, project.Description, project.DatasetCount, project.TotalRows, project.CreatedAt);
    }
}
