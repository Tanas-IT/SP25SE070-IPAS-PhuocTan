﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapstoneProject_SP25_IPAS_BussinessObject.RequestModel.ReportOfUserRequest
{
    public class CreateReportOfUserModel
    {
        public string? QuestionOfUser { get; set; }
        public string? Description { get; set; }
        public string? ImageFile { get; set; }
        public int? QuestionerID { get; set; }
    }
}
