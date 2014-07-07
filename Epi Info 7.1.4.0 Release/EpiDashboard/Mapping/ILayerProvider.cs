using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EpiDashboard.Mapping
{
    public interface ILayerProvider
    {
        void CloseLayer();
        void MoveUp();
        void MoveDown();
    }
}
