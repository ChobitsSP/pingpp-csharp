using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pingpp.Net;
using Pingpp.Models;

namespace Pingpp.Models
{
    /// <summary>
    /// 充值退款
    /// </summary>
    public class RechargeRefund:Pingpp
    {
        private const string BaseUrl = "/v1/apps/{0}/recharges/{1}/refunds";

        /// <summary>
        /// 创建充值退款
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="id"></param>
        /// <param name="refundParams"></param>
        /// <returns></returns>
        public Refund Create(string appId, string id, Dictionary<string,object> refundParams = null) 
        {
            var refund = base.DoRequest(string.Format(BaseUrl, appId, id), "POST", refundParams);
            return Mapper<Refund>.MapFromJson(refund);
        }

        /// <summary>
        /// 查询充值退款对象
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="id"></param>
        /// <param name="refunId"></param>
        /// <returns></returns>
        public Refund Retrieve(string appId,string id, string refunId)
        {
            var refund = base.DoRequest(string.Format("{0}/{1}", string.Format(BaseUrl, appId, id), refunId), "GET");
            return Mapper<Refund>.MapFromJson(refund);
        }

        /// <summary>
        /// 查询充值退款列表
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="id"></param>
        /// <param name="listParams"></param>
        /// <returns></returns>
        public RefundList List(string appId, string id, Dictionary<string, object> listParams = null) 
        {
            var refundList = base.DoRequest(Requestor.FormatUrl(string.Format(BaseUrl, appId, id), Requestor.CreateQuery(listParams)), "GET");
            return Mapper<RefundList>.MapFromJson(refundList);
        }
    }
}
