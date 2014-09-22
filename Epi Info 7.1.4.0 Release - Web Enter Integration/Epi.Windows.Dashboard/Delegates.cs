using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Epi;
using Epi.Data;
using Epi.WPF.Dashboard;
//using Epi.WPF.Dashboard.NutStat;
using Epi.WPF.Dashboard.Controls;
//using EpiDasEpi.WPF.Dashboardhboard.Gadgets.Reporting;

namespace Epi.WPF.Dashboard
{
    public delegate void NotificationButtonHandler();

    public delegate void StatusStripButtonHandler();

    public delegate void GadgetCloseButtonHandler();
    public delegate void GadgetConfigButtonHandler();
    public delegate void GadgetDescriptionButtonHandler();
    public delegate void GadgetOutputExpandButtonHandler();
    public delegate void GadgetOutputCollapseButtonHandler();
    public delegate void GadgetFilterButtonHandler();

    public delegate void StrataGridRowClickedHandler(UIElement control, string strataValue);
    public delegate void StrataGridExpandAllClickedHandler();

    public delegate void TwoByTwoValuesUpdatedHandler(UserControl control);
    public delegate void GadgetSetAdornerHandler(UserControl gadget);
    public delegate void GadgetRefreshedHandler(UserControl gadget);
    public delegate void GadgetClosingHandler(UserControl gadget);
    public delegate void GadgetProcessingFinishedHandler(UserControl gadget);

    public delegate void NotificationEventHandler(object sender, string message);
    //public delegate DashboardHelper DashboardHelperRequestedHandler();
    //public delegate RelatedConnection RelatedDataRequestedHandler();

    public delegate void SendNotificationMessageHandler(string message, bool showButtons, NotificationButtonType buttonType, bool requiresInteraction, int duration);

    public delegate void HTMLGeneratedHandler();
    public delegate void RecordCountChangedHandler(int recordCount, string dataSourceName);
    public delegate void CanvasChangedHandler(string canvasFilePath);
    public delegate void GadgetStatusUpdateHandler(string statusMessage);
    public delegate void GadgetProgressUpdateHandler(string statusMessage, double progress);
    public delegate bool GadgetCheckForCancellationHandler();

    public delegate void SimpleCallback();

    public delegate void GadgetRepositionEventHandler(object sender, GadgetRepositionEventArgs e);
}
