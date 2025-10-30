using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

public class EvidenceUploadManager(IFileService fileService, IModelExpressionProvider modelExpressionProvider)
{
    public async Task ValidateAndUploadAsync<TModel>(Expression<Func<TModel, EvidenceUploadModel>> evidenceExpression, ViewDataDictionary viewData)
    {
        var expressionBuilder = new ModelExpressionBuilder<TModel, EvidenceUploadModel>(modelExpressionProvider, evidenceExpression, viewData);
        var evidence = expressionBuilder.Model;

        if (evidence.UploadEvidence == true && evidence.EvidenceFile is null && evidence.UploadedEvidenceFile is null)
        {
            var expression = expressionBuilder.GetModelExpressionFor(e => e.EvidenceFile);
            viewData.ModelState.AddModelError(expression.Name, "Select a file");
        }

        // Delete any previously uploaded file if they're uploading a new one,
        // or choosing not to upload evidence (check for UploadEvidence != true because if
        // UploadEvidence somehow got set to null we still want to delete the file)
        if (evidence.UploadedEvidenceFile is UploadedEvidenceFile file && (evidence.EvidenceFile is not null || evidence.UploadEvidence != true))
        {
            await fileService!.DeleteFileAsync(file.FileId);
            evidence.UploadedEvidenceFile = null;
        }

        // Upload the file and set the display fields even if the rest of the form is invalid
        // otherwise the user will have to re-upload every time they re-submit
        if (evidence.UploadEvidence == true && evidence.EvidenceFile is IFormFile uploadedFile)
        {
            using var stream = evidence.EvidenceFile.OpenReadStream();
            var fileId = await fileService!.UploadFileAsync(stream, evidence.EvidenceFile.ContentType);
            evidence.UploadedEvidenceFile = new(fileId, uploadedFile.FileName, uploadedFile.Length);
            evidence.EvidenceFile = null;
        }
    }

    public async Task DeleteUploadedFileAsync(UploadedEvidenceFile? evidenceFile)
    {
        if (evidenceFile is UploadedEvidenceFile file)
        {
            await fileService.DeleteFileAsync(file.FileId);
        }
    }
}

public class ModelExpressionBuilder<TContainer, TModel>(
    IModelExpressionProvider modelExpressionProvider,
    Expression<Func<TContainer, TModel>> modelExpressionFromContainer,
    ViewDataDictionary viewData)
{
    private ViewDataDictionary<TContainer> _typedViewData = new(viewData);
    private Func<TContainer, TModel> _getModel = modelExpressionFromContainer.Compile();

    public TModel Model => _getModel(_typedViewData.Model);

    public ModelExpression GetModelExpressionFor<TValue>(Expression<Func<TModel, TValue>> propertyExpressionFromModel)
    {
        var propertyName = ((MemberExpression)propertyExpressionFromModel.Body).Member.Name;
        var modelParameter = modelExpressionFromContainer.Parameters[0];
        var propertyExpressionFromContainer = Expression.Lambda<Func<TContainer, TValue>>(
            Expression.Property(modelExpressionFromContainer.Body, propertyName),
            modelParameter);

        return modelExpressionProvider.CreateModelExpression(_typedViewData, propertyExpressionFromContainer);
    }
}
