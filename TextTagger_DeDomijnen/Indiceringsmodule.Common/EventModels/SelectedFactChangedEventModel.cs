﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace Indiceringsmodule.Common.EventModels
{
    public class SelectedFactChangedEventModel
    {
        public FlowDocument Data { get; set; }
        public Enums.direction Direction { get; set; }
    }
}
