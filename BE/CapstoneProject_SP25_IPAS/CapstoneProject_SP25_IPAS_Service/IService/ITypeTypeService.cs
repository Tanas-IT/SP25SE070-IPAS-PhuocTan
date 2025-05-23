﻿using CapstoneProject_SP25_IPAS_BussinessObject.RequestModel.ProductCriteriaSetRequest;
using CapstoneProject_SP25_IPAS_Service.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapstoneProject_SP25_IPAS_Service.IService
{
    public interface ITypeTypeService
    {
        public Task<BusinessResult> GetCriteriaSetOfProduct(GetProductCriteriaRequest request);
        public Task<BusinessResult> ApplyCriteriaSetToProduct(int productId, List<int> criteriaSetIds);
        public Task<BusinessResult> DeleteCriteriaSetFromProduct(int productId, int criteriaSetId);
        public Task<BusinessResult> UpdateCriteriaSetStatus(int productId, int criteriaSetId, bool isActive);
        public Task<BusinessResult> getCriteriaSetForSelectedProduct(int productId, int farmId, string target);


    }
}
