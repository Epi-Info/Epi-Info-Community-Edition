using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Epi.Enter.Controls
{
    class CustomRadioButton : System.Windows.Forms.RadioButton
    {
        protected override bool IsInputKey(System.Windows.Forms.Keys keyData)
        {
            if (keyData == Keys.Tab)
            {
                return true;
            }

            return base.IsInputKey(keyData);
        }
    }
}
