﻿using GERMAG.DataModel.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GERMAG.Shared;

public class CoordinateParameters
{
    public TypeOfData? Type { get; set; }
    public int? ParameterKey { get; set; }
    public string? Parameter { get; set; }
    public ParameterRoot? JsonDataParameter { get; set; }
}
