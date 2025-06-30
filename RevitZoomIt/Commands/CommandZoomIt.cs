using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using RevitZoomIt.Models;

namespace RevitZoomIt.Commands;

public class CommandZoomIt_Availability : IExternalCommandAvailability
{
    public bool IsCommandAvailable(UIApplication uiApp, CategorySet categorySet) => uiApp.ActiveUIDocument != null;
}

[UsedImplicitly]
[Transaction(TransactionMode.Manual)]
public class CommandZoomIt : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var UIDoc = commandData.Application.ActiveUIDocument;
        var set = UIDoc.Selection.GetElementIds();

        string input = ZoomRibbonFactor.ZoomInputBox?.Value?.ToString() ?? "1.0";
        if (!double.TryParse(input, out double zoomFactor) || zoomFactor <= 0)
        {
            TaskDialog.Show("Zoom It", "Invalid zoom value. Please enter a positive number.");
            return Result.Succeeded;
        }

        if (0 == set.Count)
        {
            List<ElementId> elementIds = [];
            var objects = UIDoc.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element, "Select elements to zoom to");
            foreach (var element in objects)
                if (element != null && element.ElementId != ElementId.InvalidElementId && !elementIds.Contains(element.ElementId))
                    elementIds.Add(element.ElementId);

            set = elementIds;
        }

        if (set.Count <= 0)
        {
            TaskDialog.Show("Zoom It", "No elements selected for export.");
            return Result.Succeeded;
        }

        //ZoomToElementWithScale(commandData.Application.ActiveUIDocument, elementId, zoomFactor);
        ZoomToSelectionWithViewZoom(commandData.Application.ActiveUIDocument, set, zoomFactor / 100.0);
        return Result.Succeeded;
    }

    public void ZoomToElementWithScale(UIDocument uiDoc, ElementId elementId, double zoomFactor)
    {
        Document doc = uiDoc.Document;
        Element element = doc.GetElement(elementId);

        BoundingBoxXYZ boundingBox = element.get_BoundingBox(uiDoc.ActiveView);
        if (boundingBox == null) return;

        XYZ min = boundingBox.Min;
        XYZ max = boundingBox.Max;

        XYZ center = (min + max) / 2;
        XYZ halfSize = (max - min) / 2;

        halfSize *= zoomFactor;

        XYZ newMin = center - halfSize;
        XYZ newMax = center + halfSize;

        UIView uiview = uiDoc.GetOpenUIViews().FirstOrDefault(view => view.ViewId == uiDoc.ActiveView.Id);

        uiview?.ZoomAndCenterRectangle(newMin, newMax);
    }

    public void ZoomToSelectionWithViewZoom(UIDocument uiDoc, ICollection<ElementId> selectedIds, double zoomPercentage = 1.0)
    {
        Document doc = uiDoc.Document;
        if (selectedIds.Count == 0) return;

        BoundingBoxXYZ totalBox = null;

        foreach (ElementId id in selectedIds)
        {
            Element el = doc.GetElement(id);
            if (el == null) continue;

            BoundingBoxXYZ bbox = el.get_BoundingBox(uiDoc.ActiveView);
            if (bbox == null) continue;

            if (totalBox == null)
                totalBox = new BoundingBoxXYZ
                {
                    Min = bbox.Min,
                    Max = bbox.Max
                };
            else
            {
                totalBox.Min = new XYZ(
                    Math.Min(totalBox.Min.X, bbox.Min.X),
                    Math.Min(totalBox.Min.Y, bbox.Min.Y),
                    Math.Min(totalBox.Min.Z, bbox.Min.Z));

                totalBox.Max = new XYZ(
                    Math.Max(totalBox.Max.X, bbox.Max.X),
                    Math.Max(totalBox.Max.Y, bbox.Max.Y),
                    Math.Max(totalBox.Max.Z, bbox.Max.Z));
            }
        }

        if (totalBox == null) return;

        XYZ center = (totalBox.Min + totalBox.Max) / 2;
        XYZ halfSize = (totalBox.Max - totalBox.Min) / 2;

        halfSize /= zoomPercentage;

        XYZ newMin = center - halfSize;
        XYZ newMax = center + halfSize;

        UIView uiview = uiDoc.GetOpenUIViews().FirstOrDefault(v => v.ViewId == uiDoc.ActiveView.Id);

        uiview?.ZoomAndCenterRectangle(newMin, newMax);
    }
}