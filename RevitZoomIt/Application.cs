using Autodesk.Revit.UI;
using RevitZoomIt.Models;
using System.Reflection;
using System.Windows.Media.Imaging;
using adWin = Autodesk.Windows;

namespace RevitZoomIt;
[UsedImplicitly]
public class Application : IExternalApplication
{
    string SystemTabId = "Modify";
    string SystemPanelId = "modify_shr";

    string ApiTabName = "SHSS Tools";
    string ApiPanelName = "Zoom";
    string ApiButtonName = "RevitZoomIt.Commands.CommandZoomIt";
    string ApiButtonText = "ZoomIt";


    public Result OnStartup(UIControlledApplication UIControlledApplication)
    {
        RibbonPanel ribbonPanel0 = GetRibbonPanel(UIControlledApplication, ApiPanelName, ApiTabName);
        var resourceString = "pack://application:,,,/RevitZoomIt;component/Resources/Icons/";
        PushButtonData buttonDataTest = new(ApiButtonName, ApiButtonText, Assembly.GetExecutingAssembly().Location, "RevitZoomIt.Commands.CommandZoomIt")
        {
            AvailabilityClassName = "RevitZoomIt.Commands.CommandZoomIt_Availability",
            LargeImage = new BitmapImage(new Uri(string.Concat(resourceString, "zoomit32.png"))),
            Image = new BitmapImage(new Uri(string.Concat(resourceString, "zoomit16.png"))),
            ToolTip = "Zoom to selected elements using given zoom factor."
        };

        ribbonPanel0.AddItem(buttonDataTest);

        RibbonPanel ribbonPanel1 = GetRibbonPanel(UIControlledApplication, "Zoom Factor", ApiTabName);
        TextBoxData textBoxData = new("ZoomInputBox");
        TextBox zoomTextBox = ribbonPanel1.AddItem(textBoxData) as TextBox;

        zoomTextBox.PromptText = "Zoom % (e.g. 100)";
        zoomTextBox.ToolTip = "Enter zoom percentage (e.g., 100 = fit, 50 = zoom in, 200 = zoom out)";
        zoomTextBox.Value = "100";
        zoomTextBox.Width = 100;

        ZoomRibbonFactor.ZoomInputBox = zoomTextBox;

        UIControlledApplication.ControlledApplication.ApplicationInitialized += OnApplicationInitialized;

        return Result.Succeeded;
    }

    void OnApplicationInitialized(object sender, Autodesk.Revit.DB.Events.ApplicationInitializedEventArgs e)
    {
        adWin.RibbonControl adWinRibbon = adWin.ComponentManager.Ribbon;

        adWin.RibbonTab adWinSysTab = null;
        adWin.RibbonPanel adWinSysPanel = null;

        adWin.RibbonTab adWinApiTab = null;
        adWin.RibbonPanel adWinApiPanel = null;
        adWin.RibbonItem adWinApiItem = null;

        foreach (adWin.RibbonTab ribbonTab in adWinRibbon.Tabs)
        {

            if (ribbonTab.Id == SystemTabId)
            {
                adWinSysTab = ribbonTab;

                foreach (adWin.RibbonPanel ribbonPanel in ribbonTab.Panels)
                    if (ribbonPanel.Source.Id == SystemPanelId)
                        adWinSysPanel = ribbonPanel;
            }
            else
            {
                if (ribbonTab.Id == ApiTabName)
                {
                    adWinApiTab = ribbonTab;

                    foreach (adWin.RibbonPanel ribbonPanel in ribbonTab.Panels)
                    {
                        if (ribbonPanel.Source.Id == "CustomCtrl_%" + ApiTabName + "%" + ApiPanelName)
                        {
                            adWinApiPanel = ribbonPanel;

                            foreach (adWin.RibbonItem ribbonItem in ribbonPanel.Source.Items)
                                if (ribbonItem.Id == "CustomCtrl_%CustomCtrl_%" + ApiTabName + "%" + ApiPanelName + "%" + ApiButtonName)
                                    adWinApiItem = ribbonItem;
                        }
                    }
                }
            }
        }

        if (adWinSysTab != null && adWinSysPanel != null && adWinApiTab != null && adWinApiPanel != null && adWinApiItem != null)
        {
            adWinSysTab.Panels.Add(adWinApiPanel);
            adWinSysPanel.Source.Items.Add(adWinApiItem);
        }
    }

    private RibbonPanel GetRibbonPanel(UIControlledApplication application, string ribbonName, string tabName = null)
    {
        IList<RibbonPanel> ribbonPanels;
        if (!string.IsNullOrEmpty(tabName))
        {
            try
            {
                application.CreateRibbonTab(tabName);
            }
            catch { }
            ribbonPanels = application.GetRibbonPanels(tabName);
        }
        else
            ribbonPanels = application.GetRibbonPanels();

        RibbonPanel ribbonPanel = null;
        foreach (RibbonPanel rp in ribbonPanels)
        {
            if (rp.Name.Equals(ribbonName))
            {
                ribbonPanel = rp;
                break;
            }
        }

        if (ribbonPanel == null)
            ribbonPanel = (string.IsNullOrEmpty(tabName)) ?
                application.CreateRibbonPanel(ribbonName) :
                application.CreateRibbonPanel(tabName, ribbonName);

        return ribbonPanel;
    }

    public Result OnShutdown(UIControlledApplication a)
    {
        return Result.Succeeded;
    }
}