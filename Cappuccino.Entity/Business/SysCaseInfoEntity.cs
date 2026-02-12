using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Cappuccino.Entity
{
    [Table("SysCaseInfo")]
    public class SysCaseInfoEntity : BaseEntity
    {
        /// <summary>
        /// 客户编号
        /// </summary>
        /// <returns></returns>
        public string CustomerId { get; set; }
        /// <summary>
        /// 客户姓名
        /// </summary>
        /// <returns></returns>
        public string CustName { get; set; }
        /// <summary>
        /// 客户身份证号
        /// </summary>
        /// <returns></returns>
        public string CustIDNumber { get; set; }
        /// <summary>
        /// 客户银行卡号
        /// </summary>
        /// <returns></returns>
        public string CustCardNo { get; set; }
        /// <summary>
        /// 客户贷款账号
        /// </summary>
        /// <returns></returns>
        public string CustLoanAccountNo { get; set; }
        /// <summary>
        /// 客户贷款城市
        /// </summary>
        /// <returns></returns>
        public string CustLoanCity { get; set; }
        /// <summary>
        /// 客户邮箱
        /// </summary>
        /// <returns></returns>
        public string CustEmail { get; set; }
        /// <summary>
        /// 案件编号（格式：机构代码+日期+序列号）
        /// </summary>
        /// <returns></returns>
        public string CaseNo { get; set; }
        /// <summary>
        /// 案件合同编号
        /// </summary>
        /// <returns></returns>
        public string CaseContractNo { get; set; }
        /// <summary>
        /// 案件状态（1-待分配，2-调解中，3-已结案）
        /// </summary>
        /// <returns></returns>
        public byte? CaseStatus { get; set; }
        /// <summary>
        /// 案件状态变更时间
        /// </summary>
        /// <returns></returns>
        public DateTime? CaseStatusChangeTime { get; set; }
        /// <summary>
        /// 案件类型Id（关联SysCaseType表Id）
        /// </summary>
        /// <returns></returns>
        public long? CaseTypeId { get; set; }
        /// <summary>
        /// 案件欠款金额
        /// </summary>
        /// <returns></returns>
        public decimal? CaseDebtAmount { get; set; }
        /// <summary>
        /// 案件本金
        /// </summary>
        /// <returns></returns>
        public decimal? CaseCapital { get; set; }
        /// <summary>
        /// 案件利息
        /// </summary>
        /// <returns></returns>
        public decimal? CaseInterest { get; set; }
        /// <summary>
        /// 案件手别
        /// </summary>
        /// <returns></returns>
        public byte? CaseLevel { get; set; }
        /// <summary>
        /// 案件详情（案由）
        /// </summary>
        /// <returns></returns>
        public string CaseDetails { get; set; }
        /// <summary>
        /// 调解案号（调解书字号）
        /// </summary>
        /// <returns></returns>
        public string CaseMediationWmn { get; set; }
        /// <summary>
        /// 逾期金额
        /// </summary>
        /// <returns></returns>
        public decimal? OverdueAmount { get; set; }
        /// <summary>
        /// 逾期天数
        /// </summary>
        /// <returns></returns>
        public byte? OverdueDays { get; set; }
        /// <summary>
        /// 逾期时段
        /// </summary>
        /// <returns></returns>
        public string OverduePeriod { get; set; }
        /// <summary>
        /// 已还金额
        /// </summary>
        /// <returns></returns>
        public decimal? PaidAmount { get; set; }
        /// <summary>
        /// 罚息
        /// </summary>
        /// <returns></returns>
        public decimal? PenaltyInterest { get; set; }
        /// <summary>
        /// 违约金
        /// </summary>
        /// <returns></returns>
        public decimal? Dedit { get; set; }
        /// <summary>
        /// 贷款还款日
        /// </summary>
        /// <returns></returns>
        public DateTime? RepaymentDate { get; set; }
        /// <summary>
        /// 分配方式（自动/手动）
        /// </summary>
        /// <returns></returns>
        public string AssignmentMethod { get; set; }
        /// <summary>
        /// 产品Id（关联SysProduct表Id）
        /// </summary>
        /// <returns></returns>
        public long? ProductId { get; set; }
        /// <summary>
        /// 调解开始日期
        /// </summary>
        /// <returns></returns>
        public DateTime? MediationStartDate { get; set; }
        /// <summary>
        /// 调解结束日期
        /// </summary>
        /// <returns></returns>
        public DateTime? MediationEndDate { get; set; }
        /// <summary>
        /// 案件所在部门Id（关联SysDepartment表Id）
        /// </summary>
        /// <returns></returns>
        public long? DepartmentId { get; set; }
        /// <summary>
        /// 所在部门变更时间
        /// </summary>
        /// <returns></returns>
        public DateTime? DepartmentChangeTime { get; set; }
        /// <summary>
        /// 调解员Id（关联SysUser表Id）
        /// </summary>
        /// <returns></returns>
        public long? MediatorId { get; set; }
        /// <summary>
        /// 调解员变更时间
        /// </summary>
        /// <returns></returns>
        public DateTime? MediatorChangeTime { get; set; }
        /// <summary>
        /// 助理调解员Id（关联SysUser表Id）
        /// </summary>
        /// <returns></returns>
        public long? AssistantMediatorId { get; set; }
        /// <summary>
        /// 助理调解员变更时间
        /// </summary>
        /// <returns></returns>
        public DateTime? AssistantMediatorChangeTime { get; set; }
        /// <summary>
        /// 业务批次Id
        /// </summary>
        public string BatchId { get; set; }
        /// <summary>
        /// 案件备注信息1
        /// </summary>
        /// <returns></returns>
        public string Remark1 { get; set; }
        /// <summary>
        /// 案件备注信息2
        /// </summary>
        /// <returns></returns>
        public string Remark2 { get; set; }
        /// <summary>
        /// 案件备注信息3
        /// </summary>
        /// <returns></returns>
        public string Remark3 { get; set; }
    }
}
