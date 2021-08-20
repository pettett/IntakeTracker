using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveCharts.Configurations;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using LiveCharts;
namespace IntakeTrackerApp.Controls
{
    public class ReferralPoint : ObservablePoint
    {
        public static void Init()
        {
            Charting.For<ReferralPoint>(Mappers.Xy<ReferralPoint>().X(value => value.X).Y(value => value.Y));
        }
        public ReferralPoint(double x, double y, PatientReferral pointReferral) : base(x, y)
        {
            PointReferral = pointReferral;
        }

        public PatientReferral PointReferral { get; set; }

        public void OnClicked()
        {
            MainWindow.Singleton.OpenReferral(PointReferral);
        }
    }

}
